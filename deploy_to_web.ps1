# 🚀 一鍵部署到 Render.com

Write-Host "🎓 智慧補習班 AI 協作助理系統 - 部署助手" -ForegroundColor Green
Write-Host "=" * 50

# 檢查專案狀態
Write-Host "📋 檢查專案狀態..." -ForegroundColor Yellow

if (Test-Path "MyFirstSKApp.csproj") {
    Write-Host "✅ .NET 專案檔案已找到" -ForegroundColor Green
} else {
    Write-Host "❌ 找不到專案檔案" -ForegroundColor Red
    exit 1
}

if (Test-Path "Dockerfile") {
    Write-Host "✅ Docker 檔案已準備" -ForegroundColor Green
} else {
    Write-Host "❌ 找不到 Dockerfile" -ForegroundColor Red
    exit 1
}

# 檢查 Git 狀態
Write-Host "`n🔍 檢查 Git 狀態..." -ForegroundColor Yellow
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "⚠️  發現未提交的變更，正在提交..." -ForegroundColor Yellow
    git add .
    git commit -m "Update for deployment: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    git push
    Write-Host "✅ 變更已推送到 GitHub" -ForegroundColor Green
} else {
    Write-Host "✅ Git 狀態乾淨" -ForegroundColor Green
}

Write-Host "`n🌐 部署選項：" -ForegroundColor Cyan
Write-Host "1. 🎯 Render.com（推薦 - 免費且簡單）"
Write-Host "2. 🔥 Railway.app（快速部署）"  
Write-Host "3. ☁️  Azure Web Apps（企業級）"
Write-Host "4. 🚀 ngrok（臨時分享）"

$choice = Read-Host "`n請選擇部署方式 (1-4)"

switch ($choice) {
    "1" {
        Write-Host "`n🎯 準備 Render.com 部署..." -ForegroundColor Green
        Write-Host "請按照以下步驟操作：" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "1. 訪問：https://render.com" -ForegroundColor White
        Write-Host "2. 點擊 'Get Started for Free'" -ForegroundColor White
        Write-Host "3. 使用 GitHub 帳戶登入" -ForegroundColor White
        Write-Host "4. 點擊 'New +' -> 'Web Service'" -ForegroundColor White
        Write-Host "5. 連接儲存庫：1687susan/Smart-Tutorial-AI-Assistant" -ForegroundColor White
        Write-Host "6. 設定：" -ForegroundColor White
        Write-Host "   - Name: smart-tutorial-ai-assistant" -ForegroundColor Gray
        Write-Host "   - Environment: Docker" -ForegroundColor Gray
        Write-Host "   - Plan: Free" -ForegroundColor Gray
        Write-Host "7. 點擊 'Deploy Web Service'" -ForegroundColor White
        Write-Host ""
        Write-Host "⏱️  部署需要 5-10 分鐘，完成後您將獲得網址！" -ForegroundColor Cyan
        
        # 自動開啟瀏覽器
        Start-Process "https://render.com"
    }
    "2" {
        Write-Host "`n🔥 準備 Railway.app 部署..." -ForegroundColor Green
        Write-Host "請按照以下步驟操作：" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "1. 訪問：https://railway.app" -ForegroundColor White
        Write-Host "2. 點擊 'Start a New Project'" -ForegroundColor White
        Write-Host "3. 選擇 'Deploy from GitHub repo'" -ForegroundColor White
        Write-Host "4. 選擇：Smart-Tutorial-AI-Assistant" -ForegroundColor White
        Write-Host "5. 自動部署將開始" -ForegroundColor White
        
        Start-Process "https://railway.app"
    }
    "3" {
        Write-Host "`n☁️  準備 Azure 部署..." -ForegroundColor Green
        Write-Host "需要 Azure CLI，正在檢查..." -ForegroundColor Yellow
        
        try {
            $azVersion = az --version 2>$null
            if ($azVersion) {
                Write-Host "✅ Azure CLI 已安裝" -ForegroundColor Green
                Write-Host "請執行：az login" -ForegroundColor White
                Write-Host "然後使用 DEPLOYMENT_OPTIONS.md 中的 Azure 指令" -ForegroundColor White
            } else {
                throw "Azure CLI not found"
            }
        } catch {
            Write-Host "❌ 需要安裝 Azure CLI" -ForegroundColor Red
            Write-Host "請訪問：https://docs.microsoft.com/zh-tw/cli/azure/install-azure-cli" -ForegroundColor White
        }
    }
    "4" {
        Write-Host "`n🚀 準備 ngrok 臨時分享..." -ForegroundColor Green
        
        # 檢查 ngrok 是否已安裝
        try {
            $ngrokVersion = ngrok version 2>$null
            if ($ngrokVersion) {
                Write-Host "✅ ngrok 已安裝" -ForegroundColor Green
                Write-Host "正在啟動 ngrok..." -ForegroundColor Yellow
                Write-Host "⚠️  請確保您的應用程式正在 localhost:5000 運行" -ForegroundColor Yellow
                
                # 啟動 ngrok
                Start-Process -NoNewWindow ngrok -ArgumentList "http 5000"
                Write-Host "✅ ngrok 已啟動！請檢查終端機以獲取公開網址" -ForegroundColor Green
            } else {
                throw "ngrok not found"
            }
        } catch {
            Write-Host "❌ 需要安裝 ngrok" -ForegroundColor Red
            Write-Host "請訪問：https://ngrok.com/download" -ForegroundColor White
            Write-Host "或使用 chocolatey：choco install ngrok" -ForegroundColor Gray
        }
    }
    default {
        Write-Host "❌ 無效選擇" -ForegroundColor Red
    }
}

Write-Host "`n📋 部署後檢查清單：" -ForegroundColor Cyan
Write-Host "□ 網站是否可以正常訪問" -ForegroundColor White
Write-Host "□ 所有功能是否正常工作" -ForegroundColor White  
Write-Host "□ API 文檔是否可訪問 (/swagger)" -ForegroundColor White
Write-Host "□ 取得分享網址" -ForegroundColor White

Write-Host "`n🎉 部署完成後，您可以分享網址給補習班同事測試！" -ForegroundColor Green