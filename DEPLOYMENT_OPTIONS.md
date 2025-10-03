# 🌐 智慧補習班 AI 協作助理系統 - 部署選項

## 📋 目前狀態
✅ 本地應用程式已成功啟動在 http://localhost:5000
✅ GitHub 儲存庫：https://github.com/1687susan/Smart-Tutorial-AI-Assistant

## 🚀 部署到雲端平台（讓補習班人員直接訪問）

### 方案 1：Azure Web Apps（推薦）
**優點**：免費額度、易於部署、自動 HTTPS
**預估成本**：免費（F1 方案）

1. **創建 Azure 帳戶**
   - 訪問：https://azure.microsoft.com/zh-tw/
   - 免費註冊（需要信用卡驗證，但不會收費）

2. **部署步驟**
   ```bash
   # 1. 安裝 Azure CLI
   # 2. 登入 Azure
   az login
   
   # 3. 創建資源群組
   az group create --name smart-tutorial-rg --location "East Asia"
   
   # 4. 創建 Web App
   az webapp create --resource-group smart-tutorial-rg --plan myAppServicePlan --name smart-tutorial-ai-assistant --runtime "DOTNETCORE|8.0"
   
   # 5. 部署代碼
   az webapp deployment source config --name smart-tutorial-ai-assistant --resource-group smart-tutorial-rg --repo-url https://github.com/1687susan/Smart-Tutorial-AI-Assistant --branch main --manual-integration
   ```

3. **部署後訪問網址**
   - **https://smart-tutorial-ai-assistant.azurewebsites.net**

### 方案 2：Render.com（最簡單）
**優點**：完全免費、自動部署、無需信用卡
**缺點**：冷啟動較慢

1. **訪問**：https://render.com
2. **註冊帳戶**（免費）
3. **連接 GitHub**：授權 Render 訪問您的 GitHub
4. **創建新 Web Service**：
   - Repository: `1687susan/Smart-Tutorial-AI-Assistant`
   - Branch: `main`
   - Runtime: `.NET`
   - Build Command: `dotnet publish -c Release -o out`
   - Start Command: `dotnet out/MyFirstSKApp.dll`

5. **部署後訪問網址**
   - **https://smart-tutorial-ai-assistant.onrender.com**

### 方案 3：Railway.app（推薦新手）
**優點**：部署簡單、免費額度、自動 SSL

1. **訪問**：https://railway.app
2. **GitHub 登入**
3. **Deploy from GitHub**
4. **選擇儲存庫**：Smart-Tutorial-AI-Assistant
5. **自動檢測 .NET 專案並部署**

## 🎯 快速部署指南（推薦 Render.com）

### 步驟一：準備專案
您的專案已經準備就緒，包含：
- ✅ Dockerfile（支援容器部署）
- ✅ GitHub Actions（CI/CD）
- ✅ 完整的 .NET 8 專案

### 步驟二：一鍵部署到 Render
1. 點選：https://render.com/deploy
2. 連接 GitHub 帳戶
3. 選擇 `Smart-Tutorial-AI-Assistant` 儲存庫
4. 設定部署參數：
   ```
   Service Type: Web Service
   Environment: Docker
   Plan: Free
   ```
5. 點擊「Deploy」

### 步驟三：獲得分享連結
部署完成後，您將獲得如下格式的網址：
**https://smart-tutorial-ai-assistant-[random].onrender.com**

## 📱 分享給補習班人員

一旦部署完成，您可以分享以下資訊：

### 🔗 直接訪問連結
```
主要網站：https://[您的部署網址]
API 文件：https://[您的部署網址]/swagger
```

### 📧 分享郵件範本
```
主旨：智慧補習班 AI 協作助理系統 - 測試邀請

親愛的同事們，

我們開發的智慧補習班 AI 協作助理系統已經完成並部署上線！
請點擊以下連結進行測試：

🌐 系統網址：https://[您的部署網址]
📚 使用說明：https://github.com/1687susan/Smart-Tutorial-AI-Assistant/blob/main/TESTING_GUIDE.md

主要功能：
✅ 學生問題智能回答
✅ 作業分析與建議
✅ 個人化課程推薦
✅ 每日學習建議

請測試各項功能並提供回饋意見。

謝謝！
```

## 🛠️ 緊急本地分享方案

如果需要立即展示，可以使用：

### 方案 A：ngrok（推薦）
```bash
# 1. 下載 ngrok: https://ngrok.com/download
# 2. 註冊帳戶並獲取 authtoken
# 3. 執行
ngrok http 5000
```
這將提供一個臨時的公開網址，如：https://abc123.ngrok.io

### 方案 B：Visual Studio Code Live Share
1. 安裝 Live Share 擴充功能
2. 分享工作階段
3. 邀請同事加入

## 💡 建議
1. **短期測試**：使用 ngrok 或 Render.com
2. **長期使用**：建議 Azure Web Apps
3. **預算考量**：Render.com 免費方案已足夠

## 📞 技術支援
如需協助部署，請提供：
- 選擇的部署平台
- 遇到的具體問題
- 錯誤訊息截圖