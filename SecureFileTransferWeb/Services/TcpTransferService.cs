using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFileTransferWeb.Services
{
    /// <summary>
    /// TCP 傳輸服務
    /// 提供 TCP Socket 通訊功能
    /// </summary>
    public class TcpTransferService
    {
        /// <summary>
        /// 使用 TCP 傳送加密檔案
        /// </summary>
        public static async Task SendEncryptedFileAsync(string ip, int port, string fileName, byte[] encryptedData)
        {
            using (TcpClient client = new TcpClient())
            {
                // 連線至接收端
                await client.ConnectAsync(ip, port);
                
                using (NetworkStream stream = client.GetStream())
                {
                    // 1. 傳送檔案名稱長度 (4 bytes)
                    byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                    byte[] fileNameLength = BitConverter.GetBytes(fileNameBytes.Length);
                    await stream.WriteAsync(fileNameLength, 0, fileNameLength.Length);

                    // 2. 傳送檔案名稱
                    await stream.WriteAsync(fileNameBytes, 0, fileNameBytes.Length);

                    // 3. 傳送加密資料長度 (4 bytes)
                    byte[] dataLength = BitConverter.GetBytes(encryptedData.Length);
                    await stream.WriteAsync(dataLength, 0, dataLength.Length);

                    // 4. 傳送加密資料
                    await stream.WriteAsync(encryptedData, 0, encryptedData.Length);

                    await stream.FlushAsync();
                }
            }
        }
    }

    /// <summary>
    /// 接收服務
    /// </summary>
    public class ReceiverService
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isListening = false;
        private bool _isReceiving = false;
        private string _saveDirectory = "./received";
        private readonly List<ReceivedFileInfo> _receivedFiles = new List<ReceivedFileInfo>();
        private readonly Queue<StatusMessage> _messageQueue = new Queue<StatusMessage>();

        public bool IsListening => _isListening;
        public bool IsReceiving => _isReceiving;
        public IEnumerable<ReceivedFileInfo> ReceivedFiles => _receivedFiles;

        /// <summary>
        /// 啟動接收服務
        /// </summary>
        public void Start(int port, string saveDirectory)
        {
            if (_isListening)
            {
                throw new InvalidOperationException("接收服務已在執行中");
            }

            _saveDirectory = saveDirectory;
            
            // 確保儲存目錄存在
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isListening = true;

            _cancellationTokenSource = new CancellationTokenSource();
            
            // 在背景執行接收迴圈
            Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));

            AddMessage("接收服務已啟動", "success");
        }

        /// <summary>
        /// 停止接收服務
        /// </summary>
        public void Stop()
        {
            _isListening = false;
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            
            AddMessage("接收服務已停止", "warning");
        }

        /// <summary>
        /// 取得新訊息
        /// </summary>
        public IEnumerable<StatusMessage> GetNewMessages()
        {
            var messages = new List<StatusMessage>();
            lock (_messageQueue)
            {
                while (_messageQueue.Count > 0)
                {
                    messages.Add(_messageQueue.Dequeue());
                }
            }
            return messages;
        }

        /// <summary>
        /// 接收迴圈
        /// </summary>
        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            while (_isListening && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null) break;

                    // 等待連線 (非阻塞式檢查)
                    if (!_listener.Pending())
                    {
                        await Task.Delay(100, cancellationToken);
                        continue;
                    }

                    _isReceiving = true;
                    AddMessage("📡 偵測到連線請求", "info");

                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    AddMessage("✓ 連線已建立", "success");

                    await HandleClientAsync(client, cancellationToken);

                    _isReceiving = false;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AddMessage($"❌ 錯誤: {ex.Message}", "error");
                    _isReceiving = false;
                }
            }
        }

        /// <summary>
        /// 處理客戶端連線
        /// </summary>
        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    // 設定接收超時
                    stream.ReadTimeout = 30000; // 30 秒

                    // 1. 讀取檔案名稱長度
                    byte[] fileNameLengthBytes = new byte[4];
                    await ReadExactlyAsync(stream, fileNameLengthBytes, 4, cancellationToken);
                    int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);

                    // 2. 讀取檔案名稱
                    byte[] fileNameBytes = new byte[fileNameLength];
                    await ReadExactlyAsync(stream, fileNameBytes, fileNameLength, cancellationToken);
                    string fileName = Encoding.UTF8.GetString(fileNameBytes);

                    AddMessage($"📄 接收檔案: {fileName}", "info");

                    // 3. 讀取加密資料長度
                    byte[] dataLengthBytes = new byte[4];
                    await ReadExactlyAsync(stream, dataLengthBytes, 4, cancellationToken);
                    int dataLength = BitConverter.ToInt32(dataLengthBytes, 0);

                    AddMessage($"📦 資料大小: {dataLength} bytes", "info");

                    // 4. 讀取加密資料
                    AddMessage("📥 正在接收加密資料...", "info");
                    byte[] encryptedData = new byte[dataLength];
                    await ReadExactlyAsync(stream, encryptedData, dataLength, cancellationToken);

                    AddMessage("✓ 資料接收完成", "success");

                    // 5. 解密資料
                    AddMessage("🔓 正在解密檔案...", "info");
                    byte[] decryptedData = AesEncryption.Decrypt(encryptedData);
                    AddMessage("✓ 解密完成", "success");

                    // 6. 儲存檔案
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string outputFileName = $"recv_{timestamp}_{fileName}";
                    string outputPath = Path.Combine(_saveDirectory, outputFileName);

                    File.WriteAllBytes(outputPath, decryptedData);

                    AddMessage($"💾 檔案已儲存: {outputFileName}", "success");
                    AddMessage("🎉 接收完成！", "success");

                    // 記錄已接收檔案
                    _receivedFiles.Add(new ReceivedFileInfo
                    {
                        Name = outputFileName,
                        Size = decryptedData.Length,
                        ReceivedTime = DateTime.Now
                    });

                    AddMessage("⏳ 等待下一個連線...", "info");
                }
            }
            catch (Exception ex)
            {
                AddMessage($"❌ 處理連線時發生錯誤: {ex.Message}", "error");
            }
        }

        /// <summary>
        /// 精確讀取指定數量的位元組
        /// </summary>
        private async Task ReadExactlyAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, totalRead, count - totalRead, cancellationToken);
                if (read == 0)
                {
                    throw new IOException("連線意外中斷");
                }
                totalRead += read;
            }
        }

        /// <summary>
        /// 新增訊息到佇列
        /// </summary>
        private void AddMessage(string text, string type)
        {
            lock (_messageQueue)
            {
                _messageQueue.Enqueue(new StatusMessage { Text = text, Type = type });
            }
        }
    }

    /// <summary>
    /// 已接收檔案資訊
    /// </summary>
    public class ReceivedFileInfo
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime ReceivedTime { get; set; }
    }

    /// <summary>
    /// 狀態訊息
    /// </summary>
    public class StatusMessage
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
    }
}
