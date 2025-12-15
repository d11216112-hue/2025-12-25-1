# 電子公文安全傳輸系統 (Secure Document Transfer System)

## 專題摘要 (Abstract)

本專題開發了一套具備資訊安全機制的電子公文傳輸系統。系統採用 C# 語言與 .NET 框架，實作了基於 TCP/IP 網路協定的傳輸軟體。核心整合了 **AES (Advanced Encryption Standard) 進階加密標準**，確保公文檔案在網路傳輸過程中的安全性，達成電子公文「無紙化」與「高機密性」的雙重目標。

系統提供兩種使用方式：
- **主控台版本**：傳統命令列介面，適合伺服器環境
- **網頁版本**：現代化 HTML 介面，提供更友善的使用體驗

## 系統架構

### 🌐 Web 版本（推薦使用）

**SecureFileTransferWeb** - 基於 ASP.NET Core 的 Web 應用程式

提供完整的網頁介面，包含：
- **首頁**：系統簡介與功能導覽
- **發送端頁面**：檔案選擇、加密與傳送
- **接收端頁面**：監聽設定、接收狀態與檔案清單

**特色：**
- 響應式設計，支援各種裝置
- 即時狀態更新
- 直觀的使用者介面
- 無需命令列操作

詳細說明請參閱：[SecureFileTransferWeb/README.md](SecureFileTransferWeb/README.md)

### 💻 主控台版本

系統分為兩個獨立應用程式：

### 1. 發送端應用程式 (SecureFileSender)
**工作流程：**
```
選取檔案 → 讀取二進位資料 → AES 加密 → 建立 TCP 連線 → 發送加密資料流
```

**主要功能：**
- 檔案選擇與讀取
- AES-128 加密處理
- TCP 客戶端連線
- 加密資料傳輸

### 2. 接收端應用程式 (SecureFileReceiver)
**工作流程：**
```
監聽 Port → 接收資料流 → AES 解密 → 還原檔案 → 存入硬碟
```

**主要功能：**
- TCP 伺服器監聽
- 接收加密資料
- AES-128 解密處理
- 檔案儲存與管理

## 核心技術

### 2.1 AES 對稱式加密

- **演算法**：AES (Advanced Encryption Standard)
- **金鑰長度**：128 位元 (16 Bytes)
- **模式**：CBC (Cipher Block Chaining)
- **填充**：PKCS7
- **安全性**：發送端與接收端必須擁有相同的金鑰與初始向量 (IV)

**技術選擇原因：**
- AES 為國際公認的安全標準
- 運算速度快且安全性高
- 適合大量資料與大型檔案的加密

### 2.2 TCP Socket 網路通訊

- **協定**：TCP/IP
- **架構**：主從式 (Client-Server)
- **可靠性**：TCP 協定提供可靠的資料傳輸，確保檔案封包不會遺失或順序錯亂
- **完整性**：保證公文檔案的完整性

## 系統需求

- **.NET SDK 10.0** 或更新版本
- 支援 TCP/IP 網路的作業系統（Windows、Linux、macOS）
- 網路連線（發送端與接收端需能互相通訊）
- 現代化瀏覽器（用於 Web 版本：Chrome、Firefox、Edge、Safari）

## 快速開始

### 🌐 使用 Web 版本（推薦）

1. **啟動 Web 伺服器**
```bash
cd SecureFileTransferWeb
dotnet run
```

2. **開啟瀏覽器**
```
http://localhost:5000
```

3. **操作步驟**
   - 點選「接收端」→ 設定 Port (8000) → 啟動接收
   - 開啟新分頁 → 點選「發送端」→ 選擇檔案 → 加密並發送
   - 觀察接收狀態與已接收檔案清單

### 💻 使用主控台版本

## 安裝與建置

### 1. 複製專案
```bash
git clone <repository-url>
cd 2025-12-25-1
```

### 2. 建置專案
```bash
dotnet build SecureFileTransfer.sln
```

### 3. 執行應用程式

**接收端（先啟動）：**
```bash
cd SecureFileReceiver
dotnet run
```

**發送端：**
```bash
cd SecureFileSender
dotnet run
```

## 使用說明

### 接收端操作步驟

1. 啟動接收端程式
2. 輸入監聽 Port（預設：8000）
3. 輸入檔案儲存目錄（預設：./received）
4. 程式開始監聽，等待連線

**範例：**
```
請輸入監聽 Port (例如: 8000) [預設: 8000]: 8000
請輸入檔案儲存目錄 (例如: ./received) [預設: ./received]: ./received
✓ 接收服務已啟動！
等待發送端連線中...
```

### 發送端操作步驟

1. 確保接收端已啟動並處於監聽狀態
2. 啟動發送端程式
3. 輸入接收端 IP 位址（預設：127.0.0.1，本機測試用）
4. 輸入接收端 Port（預設：8000）
5. 輸入要傳送的檔案完整路徑
6. 程式自動執行加密與傳送

**範例：**
```
請輸入接收端 IP 位址 (例如: 127.0.0.1) [預設: 127.0.0.1]: 127.0.0.1
請輸入接收端 Port (例如: 8000) [預設: 8000]: 8000
請輸入要傳送的檔案路徑: /path/to/document.pdf

檔案資訊：
  檔案名稱: document.pdf
  檔案大小: 102400 bytes

正在加密檔案...
✓ 加密完成！加密後大小: 102416 bytes

正在連線至 127.0.0.1:8000...
✓ 連線成功！
正在傳送加密資料...
✓ 傳送完成！

發送成功！檔案已安全傳送至接收端。
```

## 測試驗證

### 建立測試檔案
```bash
# 建立測試目錄
mkdir -p test_files

# 建立測試文字檔
echo "這是機密公文測試內容" > test_files/test_document.txt

# 或建立測試圖片（使用現有圖片）
# cp /path/to/image.jpg test_files/test_image.jpg
```

### 執行端對端測試

**終端機 1（接收端）：**
```bash
cd SecureFileReceiver
dotnet run
# 使用預設值：Port 8000，儲存目錄 ./received
```

**終端機 2（發送端）：**
```bash
cd SecureFileSender
dotnet run
# IP: 127.0.0.1
# Port: 8000
# 檔案路徑: ../test_files/test_document.txt
```

### 驗證結果

1. 檢查接收端目錄中的檔案
2. 比對原始檔案與解密後檔案的內容
3. 確認檔案大小與內容完全一致

```bash
# 比對檔案內容
diff test_files/test_document.txt SecureFileReceiver/received/recv_*_test_document.txt
# 無輸出表示檔案完全相同
```

## 安全性說明

### 加密機制
- **對稱式加密**：使用相同的金鑰進行加密與解密
- **金鑰管理**：當前版本金鑰硬編碼於程式中，實際部署時應使用安全的金鑰管理機制
- **傳輸安全**：檔案在傳輸過程中完全加密，即使被攔截也無法讀取

### 安全性限制
⚠️ **注意事項：**
1. 目前金鑰與 IV 為示範用途，實際應用應使用更安全的金鑰管理方式
2. 建議在實際部署時使用 SSL/TLS 加密通道
3. 應實作身份驗證機制，確保連線來源的可信度

## 程式架構

```
SecureFileTransfer.sln
├── SecureFileTransferWeb/         # Web 版本（推薦）
│   ├── wwwroot/                   # 靜態網頁檔案
│   │   ├── index.html            # 首頁
│   │   ├── sender.html           # 發送端頁面
│   │   ├── receiver.html         # 接收端頁面
│   │   ├── css/style.css         # 樣式表
│   │   └── js/                   # JavaScript 檔案
│   ├── Controllers/              # API 控制器
│   ├── Services/                 # 業務邏輯服務
│   └── README.md                 # Web 版本說明文件
│
├── SecureFileSender/              # 主控台發送端
│   ├── Program.cs                # 主程式（發送端邏輯）
│   ├── AesEncryption.cs          # AES 加密模組
│   └── SecureFileSender.csproj   # 專案檔
│
└── SecureFileReceiver/            # 主控台接收端
    ├── Program.cs                 # 主程式（接收端邏輯）
    ├── AesEncryption.cs          # AES 解密模組
    └── SecureFileReceiver.csproj # 專案檔
```

## 關鍵程式碼說明

### AES 加密模組 (AesEncryption.cs)

```csharp
// 加密檔案
public static byte[] EncryptFile(string inputFilePath)
{
    byte[] fileBytes = File.ReadAllBytes(inputFilePath);
    using (Aes aes = Aes.Create())
    {
        aes.Key = Key;  // 128-bit 金鑰
        aes.IV = IV;    // 128-bit 初始向量
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        
        using (MemoryStream msEncrypt = new MemoryStream())
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            csEncrypt.Write(fileBytes, 0, fileBytes.Length);
            csEncrypt.FlushFinalBlock();
            return msEncrypt.ToArray();
        }
    }
}
```

### TCP 通訊流程

**發送端：**
1. 建立 TcpClient 連線
2. 傳送檔案名稱長度與檔案名稱
3. 傳送加密資料長度
4. 傳送加密資料

**接收端：**
1. TcpListener 監聽連線
2. 接收檔案名稱與長度
3. 接收加密資料
4. 解密並儲存檔案

## 測試結果

經過實際測試：
- ✅ 成功傳輸各種格式檔案（.txt, .jpg, .pdf, .doc 等）
- ✅ 加密與解密過程正常運作
- ✅ 解密後檔案內容與原始檔案完全一致
- ✅ 無資料損毀或雜訊
- ✅ 傳輸過程穩定可靠
- ✅ Web 介面運作正常，提供良好的使用體驗

## 系統截圖

### 網頁版介面

**首頁**

![首頁](https://github.com/user-attachments/assets/7d534dbc-8f7b-4892-b8fb-b7eb3691ffec)

**發送端頁面**

![發送端](https://github.com/user-attachments/assets/df46b18d-6898-4177-a35f-4b16cf06a0e5)

**接收端頁面**

![接收端](https://github.com/user-attachments/assets/0bacb7a5-dc6b-4da4-bb48-3a5267ad357a)

## 未來展望

### 1. 數位簽章 (Digital Signature)
- 加入 RSA 非對稱加密技術
- 驗證發送者真實身分
- 防止公文遭到偽造

### 2. 資料庫整合
- 建立公文管理資料庫
- 記錄傳輸時間與經手人員
- 完善公文稽核軌跡

### 3. 使用者介面改進
- ✅ 已開發 HTML 網頁介面 (SecureFileTransferWeb)
- 提供更友善的操作體驗
- 支援批次檔案傳輸

### 4. 進階安全功能
- 實作 SSL/TLS 通道加密
- 加入使用者身份驗證
- 支援金鑰交換協定

## 授權

本專題為教育用途開發，供學習與研究使用。

## 聯絡資訊

如有任何問題或建議，歡迎提出 Issue 或 Pull Request。

---

**開發環境：**
- C# / .NET 8.0
- Visual Studio Code / Visual Studio
- TCP/IP 網路協定
- AES-128 加密演算法