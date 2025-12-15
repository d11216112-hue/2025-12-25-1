# 電子公文安全傳輸系統 - Web 版本

## 系統簡介

本系統提供 HTML 網頁介面的電子公文安全傳輸功能，採用 **AES-128 加密標準** 與 **TCP/IP 網路協定**，實現安全的檔案傳輸。

## 技術架構

### 前端 (Frontend)
- **HTML5** - 網頁結構
- **CSS3** - 響應式設計與美化
- **JavaScript (ES6+)** - 互動邏輯與 API 呼叫

### 後端 (Backend)
- **ASP.NET Core 10.0** - Web API 框架
- **C#** - 業務邏輯實作
- **AES-128-CBC** - 加密演算法
- **TCP Socket** - 網路通訊

## 系統需求

- **.NET SDK 10.0** 或更新版本
- 支援現代瀏覽器（Chrome、Firefox、Edge、Safari）
- 網路連線（發送端與接收端需能互相通訊）

## 安裝與啟動

### 1. 建置專案

```bash
cd SecureFileTransferWeb
dotnet build
```

### 2. 啟動 Web 伺服器

```bash
dotnet run
```

伺服器將在 `http://localhost:5000` 啟動

### 3. 開啟瀏覽器

在瀏覽器中開啟：
```
http://localhost:5000
```

## 使用說明

### 首頁

首頁提供兩個主要功能選項：
- **發送端 (Sender)** - 加密並發送檔案
- **接收端 (Receiver)** - 接收並解密檔案

### 發送端操作流程

1. 點選「發送端」按鈕進入發送頁面
2. 輸入接收端資訊：
   - **IP 位址**：接收端的 IP（本機測試使用 `127.0.0.1`）
   - **Port**：接收端監聽的埠號（預設 `8000`）
3. 點選「選取檔案」瀏覽並選擇要傳送的檔案
4. 確認檔案資訊無誤後，點選「🔐 加密並發送」
5. 系統將自動執行：
   - 讀取檔案內容
   - AES-128 加密
   - 建立 TCP 連線
   - 傳送加密資料
6. 等待「發送成功」訊息

### 接收端操作流程

1. 點選「接收端」按鈕進入接收頁面
2. 設定接收參數：
   - **監聽 Port**：設定要監聽的埠號（預設 `8000`）
   - **儲存目錄**：設定接收檔案的儲存路徑（預設 `./received`）
3. 點選「▶️ 啟動接收」
4. 系統開始監聽，等待發送端連線
5. 接收到檔案時，系統會自動：
   - 接收加密資料
   - AES-128 解密
   - 儲存檔案（格式：`recv_時間戳記_原檔名`）
6. 已接收的檔案會顯示在「已接收檔案」清單中
7. 完成後可點選「⏹️ 停止接收」結束服務

## 完整測試範例

### 測試準備

開啟兩個瀏覽器分頁或視窗：

**分頁 1（接收端）：**
```
http://localhost:5000/receiver.html
```

**分頁 2（發送端）：**
```
http://localhost:5000/sender.html
```

### 測試步驟

1. **在接收端分頁**：
   - Port: `8000`
   - 儲存目錄: `./received`
   - 點選「啟動接收」
   - 等待顯示「監聽中」狀態

2. **在發送端分頁**：
   - IP: `127.0.0.1`
   - Port: `8000`
   - 選擇任意檔案（如圖片、文件、PDF 等）
   - 點選「加密並發送」

3. **觀察狀態**：
   - 發送端顯示加密與傳送進度
   - 接收端顯示接收與解密進度
   - 完成後，接收端的「已接收檔案」清單會更新

4. **驗證結果**：
   - 檢查 `SecureFileTransferWeb/received/` 目錄
   - 確認檔案已正確儲存
   - 開啟檔案確認內容完整

## 系統架構圖

```
┌─────────────────┐                    ┌─────────────────┐
│   瀏覽器 (前端)   │                    │   瀏覽器 (前端)   │
│  Sender 頁面     │                    │  Receiver 頁面   │
└────────┬────────┘                    └────────┬────────┘
         │ HTTP API                             │ HTTP API
         ▼                                      ▼
┌─────────────────────────────────────────────────────────┐
│         ASP.NET Core Web Server (後端)                   │
│  ┌──────────────┐              ┌──────────────┐        │
│  │ Sender API   │              │ Receiver API │        │
│  │  Controller  │              │  Controller  │        │
│  └──────┬───────┘              └──────┬───────┘        │
│         │                              │                │
│  ┌──────▼───────┐              ┌──────▼───────┐        │
│  │ AES          │              │ AES          │        │
│  │ Encryption   │              │ Decryption   │        │
│  └──────┬───────┘              └──────▲───────┘        │
│         │                              │                │
│  ┌──────▼──────────────────────────────┴───────┐        │
│  │         TCP Socket 網路通訊層               │        │
│  └────────────────────────────────────────────┘        │
└─────────────────────────────────────────────────────────┘
         │ TCP/IP                          ▲
         └─────────────────────────────────┘
```

## 核心功能說明

### AES 加密模組
- **演算法**：AES (Advanced Encryption Standard)
- **金鑰長度**：128 位元 (16 Bytes)
- **模式**：CBC (Cipher Block Chaining)
- **填充**：PKCS7
- **位置**：`Services/AesEncryption.cs`

### TCP 通訊協定
- **傳輸格式**：
  1. 檔案名稱長度（4 bytes）
  2. 檔案名稱（UTF-8 編碼）
  3. 加密資料長度（4 bytes）
  4. 加密資料（byte array）
- **位置**：`Services/TcpTransferService.cs`

### API 端點

**發送端 API：**
```
POST /api/sender/send
Content-Type: multipart/form-data
Body:
  - file: 檔案
  - receiverIp: 接收端 IP
  - receiverPort: 接收端 Port
```

**接收端 API：**
```
POST /api/receiver/start
Content-Type: application/json
Body: { "port": 8000, "saveDirectory": "./received" }

POST /api/receiver/stop

GET /api/receiver/status
Response: { "isListening": bool, "isReceiving": bool, ... }
```

## 安全性說明

### ⚠️ 重要提示

本系統為**教育示範用途**，實際部署時需注意：

1. **金鑰管理**：
   - 目前金鑰硬編碼於程式中
   - 實際應用應使用金鑰管理系統（KMS）
   - 或實作 Diffie-Hellman 金鑰交換

2. **傳輸安全**：
   - 建議使用 HTTPS 保護 Web API
   - 考慮在 TCP 層之上加入 TLS/SSL

3. **身份驗證**：
   - 加入使用者登入機制
   - 實作 JWT 或 OAuth 認證
   - 驗證發送端與接收端的身分

4. **存取控制**：
   - 限制允許連線的 IP 位址
   - 實作檔案上傳大小限制
   - 加入檔案類型檢查

## 專案結構

```
SecureFileTransferWeb/
├── wwwroot/                    # 靜態檔案目錄
│   ├── index.html             # 首頁
│   ├── sender.html            # 發送端頁面
│   ├── receiver.html          # 接收端頁面
│   ├── css/
│   │   └── style.css          # 樣式表
│   └── js/
│       ├── sender.js          # 發送端邏輯
│       └── receiver.js        # 接收端邏輯
├── Controllers/               # API 控制器
│   ├── SenderController.cs   # 發送端 API
│   └── ReceiverController.cs # 接收端 API
├── Services/                  # 業務邏輯服務
│   ├── AesEncryption.cs      # AES 加密模組
│   └── TcpTransferService.cs # TCP 通訊服務
├── Properties/
│   └── launchSettings.json   # 啟動設定
├── Program.cs                # 應用程式進入點
└── SecureFileTransferWeb.csproj
```

## 常見問題

### Q1: 為什麼無法連線？
**A:** 請確認：
- 接收端已啟動並顯示「監聽中」狀態
- IP 位址與 Port 設定正確
- 防火牆未阻擋該 Port
- 發送端與接收端在同一網路環境

### Q2: 接收端無法啟動？
**A:** 可能原因：
- Port 已被其他程式佔用，嘗試更換 Port
- 權限不足，某些系統需要管理員權限開啟低於 1024 的 Port
- 防火牆設定阻擋

### Q3: 檔案解密後損毀？
**A:** 請檢查：
- 發送端與接收端使用相同版本的程式
- 網路連線穩定
- 檔案在傳輸過程中未中斷

### Q4: 支援哪些檔案格式？
**A:** 支援所有檔案格式，包括：
- 文字檔（.txt, .doc, .docx, .pdf）
- 圖片（.jpg, .png, .gif, .bmp）
- 壓縮檔（.zip, .rar, .7z）
- 其他任意二進位檔案

## 效能說明

- **加密速度**：約 100 MB/s（視硬體效能而定）
- **傳輸速度**：取決於網路頻寬
- **檔案大小限制**：建議單檔不超過 100 MB
- **同時連線**：接收端可處理多個連續請求

## 授權

本專題為教育用途開發，供學習與研究使用。

## 聯絡資訊

如有任何問題或建議，歡迎提出 Issue 或 Pull Request。

---

**開發環境：**
- ASP.NET Core 10.0
- C# 13
- HTML5 / CSS3 / JavaScript ES6+
- TCP/IP 網路協定
- AES-128 加密演算法
