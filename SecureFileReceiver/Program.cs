using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SecureFileTransfer;

namespace SecureFileReceiver
{
    /// <summary>
    /// 接收端應用程式 (Receiver Application)
    /// 功能：監聽 Port → 接收資料流 → 執行 AES 解密 → 還原檔案 → 存入硬碟
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("   電子公文安全傳輸系統 - 接收端 (Receiver)");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            try
            {
                // 取得監聽通訊埠
                string portInput = GetInput("請輸入監聽 Port (例如: 8000)", "8000");
                int port = int.Parse(portInput);

                // 取得儲存目錄
                string saveDirectory = GetInput("請輸入檔案儲存目錄 (例如: ./received)", "./received");
                
                // 確保目錄存在
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                    Console.WriteLine($"✓ 已建立目錄: {saveDirectory}");
                }

                Console.WriteLine($"\n正在啟動接收服務...");
                Console.WriteLine($"監聽位址: 0.0.0.0:{port}");
                Console.WriteLine($"儲存目錄: {Path.GetFullPath(saveDirectory)}");

                // 建立 TCP 伺服器
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                Console.WriteLine("\n✓ 接收服務已啟動！");
                Console.WriteLine("等待發送端連線中...");
                Console.WriteLine("(按 Ctrl+C 可停止服務)\n");

                while (true)
                {
                    try
                    {
                        // 接受客戶端連線
                        using (TcpClient client = listener.AcceptTcpClient())
                        {
                            string clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();
                            Console.WriteLine($">>> 收到來自 {clientAddress} 的連線");

                            NetworkStream stream = client.GetStream();

                            // 接收檔案名稱長度
                            byte[] fileNameLengthBytes = new byte[4];
                            stream.Read(fileNameLengthBytes, 0, 4);
                            int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);

                            // 接收檔案名稱
                            byte[] fileNameBytes = new byte[fileNameLength];
                            stream.Read(fileNameBytes, 0, fileNameLength);
                            string originalFileName = System.Text.Encoding.UTF8.GetString(fileNameBytes);

                            // 接收資料長度
                            byte[] dataLengthBytes = new byte[4];
                            stream.Read(dataLengthBytes, 0, 4);
                            int dataLength = BitConverter.ToInt32(dataLengthBytes, 0);

                            Console.WriteLine($"原始檔案名稱: {originalFileName}");
                            Console.WriteLine($"加密資料大小: {dataLength} bytes");

                            // 接收加密資料
                            Console.WriteLine("正在接收加密資料...");
                            byte[] encryptedData = new byte[dataLength];
                            int totalRead = 0;
                            while (totalRead < dataLength)
                            {
                                int bytesRead = stream.Read(encryptedData, totalRead, dataLength - totalRead);
                                if (bytesRead == 0) break;
                                totalRead += bytesRead;
                            }

                            Console.WriteLine($"✓ 接收完成！共接收 {totalRead} bytes");

                            // 解密並儲存檔案
                            Console.WriteLine("正在解密檔案...");
                            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            string outputFileName = $"recv_{timestamp}_{originalFileName}";
                            string outputPath = Path.Combine(saveDirectory, outputFileName);

                            AesEncryption.DecryptFile(encryptedData, outputPath);

                            Console.WriteLine($"✓ 解密完成！");
                            Console.WriteLine($"✓ 檔案已儲存至: {Path.GetFullPath(outputPath)}");
                            Console.WriteLine($"檔案大小: {new FileInfo(outputPath).Length} bytes");
                            Console.WriteLine();
                            Console.WriteLine(">>> 等待下一個連線...\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"處理連線時發生錯誤: {ex.Message}");
                        Console.WriteLine(">>> 等待下一個連線...\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n嚴重錯誤: {ex.Message}");
            }
        }

        static string GetInput(string prompt, string defaultValue)
        {
            Console.Write($"{prompt} [預設: {defaultValue}]: ");
            string input = Console.ReadLine()?.Trim() ?? "";
            return string.IsNullOrEmpty(input) ? defaultValue : input;
        }
    }
}
