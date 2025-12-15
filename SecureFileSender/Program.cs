using System;
using System.IO;
using System.Net.Sockets;
using SecureFileTransfer;

namespace SecureFileSender
{
    /// <summary>
    /// 發送端應用程式 (Sender Application)
    /// 功能：選取檔案 → 讀取二進位資料 → 執行 AES 加密 → 建立 TCP 連線 → 發送加密資料流
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("   電子公文安全傳輸系統 - 發送端 (Sender)");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            try
            {
                // 取得接收端 IP 位址
                string ipAddress = GetInput("請輸入接收端 IP 位址 (例如: 127.0.0.1)", "127.0.0.1");
                
                // 取得接收端通訊埠
                string portInput = GetInput("請輸入接收端 Port (例如: 8000)", "8000");
                int port = int.Parse(portInput);

                // 取得要傳送的檔案路徑
                Console.Write("請輸入要傳送的檔案路徑: ");
                string filePath = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    Console.WriteLine("錯誤：檔案不存在！");
                    return;
                }

                // 顯示檔案資訊
                FileInfo fileInfo = new FileInfo(filePath);
                Console.WriteLine($"\n檔案資訊：");
                Console.WriteLine($"  檔案名稱: {fileInfo.Name}");
                Console.WriteLine($"  檔案大小: {fileInfo.Length} bytes");

                Console.WriteLine("\n正在加密檔案...");
                
                // 步驟 1: 讀取檔案並進行 AES 加密
                byte[] encryptedData = AesEncryption.EncryptFile(filePath);
                Console.WriteLine($"✓ 加密完成！加密後大小: {encryptedData.Length} bytes");

                // 步驟 2: 建立 TCP 連線並發送
                Console.WriteLine($"\n正在連線至 {ipAddress}:{port}...");
                
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(ipAddress, port);
                    Console.WriteLine("✓ 連線成功！");

                    NetworkStream stream = client.GetStream();

                    // 先發送檔案名稱長度和檔案名稱
                    byte[] fileNameBytes = System.Text.Encoding.UTF8.GetBytes(fileInfo.Name);
                    byte[] fileNameLength = BitConverter.GetBytes(fileNameBytes.Length);
                    stream.Write(fileNameLength, 0, 4);
                    stream.Write(fileNameBytes, 0, fileNameBytes.Length);

                    // 發送加密資料長度
                    byte[] dataLength = BitConverter.GetBytes(encryptedData.Length);
                    stream.Write(dataLength, 0, 4);

                    // 發送加密資料
                    Console.WriteLine("正在傳送加密資料...");
                    stream.Write(encryptedData, 0, encryptedData.Length);
                    stream.Flush();

                    Console.WriteLine("✓ 傳送完成！");
                }

                Console.WriteLine("\n發送成功！檔案已安全傳送至接收端。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n錯誤發生: {ex.Message}");
            }

            // 只在非重定向模式下等待按鍵
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("\n按任意鍵結束程式...");
                Console.ReadKey();
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
