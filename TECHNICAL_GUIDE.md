# 技術實作說明 (Technical Implementation Guide)

## 專題名稱
電子公文安全傳輸系統 - AES 加密檔案傳輸應用

## 第一章 緒論

### 1.1 研究背景
隨著資訊科技的快速發展，傳統組織單位的公文往來仍大量依賴紙本作業。這不僅消耗大量的紙張與碳粉資源，在公文遞送的人力成本及歸檔保存的空間需求上，皆造成組織沉重的負擔。此外，紙本公文在傳遞過程中，容易發生遺失或遭到未授權人員窺探的風險。

### 1.2 研究目的
本專題利用現代網路技術取代傳統人工遞送，並以數位儲存取代實體空間。研究重點在於引入「資訊安全」技術，開發一套電子公文系統。透過程式自動化與加密演算法的結合，達成節能減碳、提升行政效率，並確保機密公文在傳輸過程中的安全性。

## 第二章 核心技術與原理

### 2.1 AES 對稱式加密 (AES Encryption)

為確保機密公文（如：極機密等級）的安全性，本系統採用 AES (Advanced Encryption Standard) 演算法。

**技術選擇原因：**
- AES 為目前國際公認的安全標準
- 運算速度快且安全性高
- 非常適合用於大量資料或大型檔案的加密

**實作方式：**
- 系統設定了 128 位元 (16 Bytes) 的金鑰 (Key) 與初始向量 (IV)
- 發送端與接收端必須擁有一模一樣的金鑰組合，才能進行加解密運算
- 這保證了資料的機密性 (Confidentiality)

**關鍵程式碼：**
```csharp
// AES 金鑰與初始向量設定
private static readonly byte[] Key = new byte[16]
{
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
    0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
};

private static readonly byte[] IV = new byte[16]
{
    0x10, 0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09,
    0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01
};

// 加密函式
public static byte[] EncryptFile(string inputFilePath)
{
    byte[] fileBytes = File.ReadAllBytes(inputFilePath);
    
    using (Aes aes = Aes.Create())
    {
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                csEncrypt.FlushFinalBlock();
                return msEncrypt.ToArray();
            }
        }
    }
}
```

### 2.2 TCP Socket 網路通訊

為了在不同電腦間傳送檔案，系統採用 TCP/IP 協定中的 Socket (通訊端) 技術。

**可靠性：**
- TCP 協定提供可靠的資料傳輸
- 確保檔案封包在傳送過程中不會遺失或順序錯亂
- 這對於公文檔案的完整性至關重要

**架構：**
- 採用主從式架構 (Client-Server)
- 接收端作為伺服器 (Server) 監聽特定連接埠
- 發送端 (Client) 主動發起連線請求

## 第三章 系統設計與實作

### 3.1 系統架構圖

```
發送端 (Sender)：
選取檔案 → 讀取二進位資料 → 執行 AES 加密 → 建立 TCP 連線 → 發送加密資料流

接收端 (Receiver)：
監聽 Port → 接收資料流 → 執行 AES 解密 → 還原檔案 → 存入硬碟
```

### 3.2 關鍵功能實作

#### 檔案加密模組

使用 .NET 的 `System.Security.Cryptography` 函式庫。在檔案讀取後，透過 `ICryptoTransform` 介面將原始的 byte 陣列轉換為加密後的亂碼陣列。

```csharp
// 使用 CryptoStream 進行串流寫入
using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
{
    csEncrypt.Write(fileBytes, 0, fileBytes.Length);
    csEncrypt.FlushFinalBlock();
    return msEncrypt.ToArray();
}
```

#### 檔案解密模組

接收端在收到資料後，使用與發送端相同的 Key 與 IV 建立解密器 (Decryptor)。系統會自動將收到的加密串流還原為原始檔案格式（如 .jpg 或 .doc），並儲存至指定路徑。

```csharp
// 解密並儲存檔案
public static void DecryptFile(byte[] encryptedData, string outputFilePath)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (FileStream fsOutput = new FileStream(outputFilePath, FileMode.Create))
                {
                    csDecrypt.CopyTo(fsOutput);
                }
            }
        }
    }
}
```

#### TCP 通訊協定

**發送端實作：**
```csharp
using (TcpClient client = new TcpClient())
{
    client.Connect(ipAddress, port);
    NetworkStream stream = client.GetStream();

    // 1. 發送檔案名稱長度和檔案名稱
    byte[] fileNameBytes = System.Text.Encoding.UTF8.GetBytes(fileInfo.Name);
    byte[] fileNameLength = BitConverter.GetBytes(fileNameBytes.Length);
    stream.Write(fileNameLength, 0, 4);
    stream.Write(fileNameBytes, 0, fileNameBytes.Length);

    // 2. 發送加密資料長度
    byte[] dataLength = BitConverter.GetBytes(encryptedData.Length);
    stream.Write(dataLength, 0, 4);

    // 3. 發送加密資料
    stream.Write(encryptedData, 0, encryptedData.Length);
    stream.Flush();
}
```

**接收端實作：**
```csharp
TcpListener listener = new TcpListener(IPAddress.Any, port);
listener.Start();

while (true)
{
    using (TcpClient client = listener.AcceptTcpClient())
    {
        NetworkStream stream = client.GetStream();

        // 1. 接收檔案名稱
        byte[] fileNameLengthBytes = new byte[4];
        stream.Read(fileNameLengthBytes, 0, 4);
        int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);
        
        byte[] fileNameBytes = new byte[fileNameLength];
        stream.Read(fileNameBytes, 0, fileNameLength);
        string originalFileName = System.Text.Encoding.UTF8.GetString(fileNameBytes);

        // 2. 接收加密資料
        byte[] dataLengthBytes = new byte[4];
        stream.Read(dataLengthBytes, 0, 4);
        int dataLength = BitConverter.ToInt32(dataLengthBytes, 0);

        byte[] encryptedData = new byte[dataLength];
        int totalRead = 0;
        while (totalRead < dataLength)
        {
            int bytesRead = stream.Read(encryptedData, totalRead, dataLength - totalRead);
            if (bytesRead == 0) break;
            totalRead += bytesRead;
        }

        // 3. 解密並儲存
        AesEncryption.DecryptFile(encryptedData, outputPath);
    }
}
```

## 第四章 系統操作與測試結果

### 4.1 發送端操作

使用者開啟發送端介面：
1. 輸入接收方的 IP 位址（如 127.0.0.1）
2. 輸入通訊埠（Port 8000）
3. 選取欲傳送的公文圖片或文件
4. 按下「加密並發送」按鈕
5. 系統提示發送成功訊息

### 4.2 接收端操作

接收端開啟後：
1. 輸入監聽 Port（8000）
2. 按下「啟動接收」
3. 系統隨即進入背景監聽模式，等待連線

### 4.3 傳輸成果驗證

經過實際測試：
- 成功將測試檔案加密傳送
- 接收端在收到數據後，成功解密並儲存檔案
- 檔案內容與原始檔案完全一致，無任何損毀或雜訊
- 證明加密與傳輸邏輯運作正常

**測試結果：**
```
測試 1: 文字檔案 (test_document.txt)
  原始大小: 69 bytes
  加密後大小: 80 bytes
  接收後大小: 69 bytes
  ✓ 內容完全相同

測試 2: 圖片檔案 (test_image.jpg)
  原始大小: 626 bytes
  加密後大小: 640 bytes
  接收後大小: 626 bytes
  ✓ 內容完全相同
```

## 第五章 結論與未來展望

### 5.1 結論

本專題成功實作了一套具備 AES 高強度加密的電子公文傳輸系統。透過實際程式開發，我們驗證了以軟體加密取代實體封存的可行性。本系統不僅解決了公文傳遞的時效性問題，更透過密碼學技術解決了網路傳輸最擔心的安全疑慮。

**達成目標：**
- ✅ 實作 AES-128 對稱式加密
- ✅ 實作 TCP Socket 可靠傳輸
- ✅ 確保檔案完整性
- ✅ 保護傳輸過程機密性
- ✅ 支援多種檔案格式

### 5.2 未來展望

目前的系統已具備基礎的機密傳輸功能，未來可針對以下方向進行擴充：

#### 1. 數位簽章 (Digital Signature)
- 加入 RSA 非對稱加密技術
- 讓接收端能驗證發送者的真實身分
- 防止公文遭到偽造

#### 2. 資料庫整合
- 建立公文管理資料庫
- 記錄每一筆傳輸的送達時間與經手人員
- 完善公文的稽核軌跡

#### 3. 金鑰管理系統
- 實作安全的金鑰交換機制（如 Diffie-Hellman）
- 支援金鑰輪替機制
- 提供金鑰備份與復原功能

#### 4. 圖形化使用者介面
- 開發 Windows Forms 或 WPF 應用程式
- 提供更直觀的操作體驗
- 支援檔案拖放功能

#### 5. 進階安全功能
- 加入 SSL/TLS 通道加密
- 實作使用者身份驗證
- 支援多因素認證 (MFA)
- 加入傳輸日誌與稽核功能

## 附錄

### A. 系統需求
- .NET 8.0 SDK 或更新版本
- 支援 TCP/IP 的網路環境
- 至少 50MB 可用磁碟空間

### B. 安全性建議
1. 在生產環境中應使用更安全的金鑰管理方式
2. 建議啟用 SSL/TLS 加密通道
3. 實作使用者身份驗證機制
4. 定期更新加密金鑰
5. 記錄所有傳輸活動以供稽核

### C. 參考資料
- NIST FIPS 197: Advanced Encryption Standard (AES)
- RFC 793: Transmission Control Protocol
- Microsoft .NET Security Documentation
- OWASP Cryptographic Storage Cheat Sheet

---

**開發團隊**
- 開發語言：C# 12 / .NET 8.0
- 開發工具：Visual Studio Code
- 加密標準：AES-128-CBC
- 網路協定：TCP/IP
- 開發時間：2025年12月
