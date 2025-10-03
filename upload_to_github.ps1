# 🚀 GitHub 上傳腳本
# 使用方法：將下面的 YOUR_USERNAME 和 REPO_NAME 替換成實際值，然後逐行執行

Write-Host "🎓 智慧補習班 AI 助理系統 - GitHub 上傳腳本" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# 第一步：添加遠端儲存庫（請修改下面的 URL）
Write-Host "1️⃣ 設定遠端儲存庫..." -ForegroundColor Yellow
Write-Host "   請先在 GitHub 上建立新儲存庫，然後執行：" -ForegroundColor Green
Write-Host "   git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git" -ForegroundColor White

# 第二步：重命名分支
Write-Host "2️⃣ 重命名分支為 main..." -ForegroundColor Yellow
Write-Host "   git branch -M main" -ForegroundColor White

# 第三步：推送到 GitHub  
Write-Host "3️⃣ 推送程式碼到 GitHub..." -ForegroundColor Yellow
Write-Host "   git push -u origin main" -ForegroundColor White

Write-Host ""
Write-Host "✨ 上傳完成後，您的專案將包含：" -ForegroundColor Green
Write-Host "   📱 Web 應用程式（訪問 /）" -ForegroundColor White
Write-Host "   📖 API 文件（訪問 /swagger）" -ForegroundColor White
Write-Host "   🤖 AI 對話系統" -ForegroundColor White
Write-Host "   📚 完整測試資料" -ForegroundColor White
Write-Host "   📄 詳細說明文件" -ForegroundColor White

Write-Host ""
Write-Host "🎯 分享給補習班人員的連結：" -ForegroundColor Cyan
Write-Host "   https://github.com/YOUR_USERNAME/YOUR_REPO_NAME" -ForegroundColor White

Write-Host ""
Write-Host "💡 測試指令：" -ForegroundColor Yellow
Write-Host "   git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git" -ForegroundColor White
Write-Host "   cd YOUR_REPO_NAME" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host "   # 然後開啟 http://localhost:5000" -ForegroundColor Gray