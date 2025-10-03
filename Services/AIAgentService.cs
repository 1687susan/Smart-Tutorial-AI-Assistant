using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyFirstSKApp.Plugins;
using System.Text.Json;

namespace MyFirstSKApp.Services;

/// <summary>
/// 智慧協作 AI Agent 管理服務
/// </summary>
public class AIAgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly Dictionary<string, ChatHistory> _chatHistories;

    public AIAgentService(Kernel kernel, IChatCompletionService chatService)
    {
        _kernel = kernel;
        _chatService = chatService;
        _chatHistories = new Dictionary<string, ChatHistory>();
    }

    /// <summary>
    /// 取得或建立學生的聊天歷史記錄
    /// </summary>
    public ChatHistory GetOrCreateChatHistory(string studentName)
    {
        if (!_chatHistories.ContainsKey(studentName))
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage($@"
你是一位專業的補習班 AI 助理，專門協助學生學習。你的職責包括：

1. 協助學生查詢作業狀況和成績
2. 提供個人化的學習建議和指導
3. 推薦適合的課程
4. 解答學習相關問題
5. 記錄並追蹤學生的學習進度

目前服務的學生是：{studentName}

請保持友善、專業的態度，用繁體中文回應。當學生問到作業或課程相關問題時，
請主動使用相關的 Function 來查詢具體資料，然後提供個人化的建議。

可用的功能包括：
- GetStudentHomework：查詢學生作業狀況
- GetStudentProfile：查詢學生基本資料和選課狀況
- GetCourseList：查詢可選修課程
- GetCourseDetails：查詢特定課程詳情
- RecommendCourses：推薦適合的課程
- SubmitHomeworkFeedback：提供作業回饋建議
");
            _chatHistories[studentName] = chatHistory;
        }

        return _chatHistories[studentName];
    }

    /// <summary>
    /// 處理學生的問題並生成回應
    /// </summary>
    public async Task<string> ProcessStudentQueryAsync(string studentName, string query)
    {
        try
        {
            var chatHistory = GetOrCreateChatHistory(studentName);
            chatHistory.AddUserMessage(query);

            // 設定 OpenAI 執行設定，啟用自動函數調用
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.7,
                MaxTokens = 1000
            };

            // 生成回應
            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory, 
                executionSettings, 
                _kernel);

            // 將 AI 回應加入聊天歷史
            chatHistory.AddAssistantMessage(response.Content ?? "抱歉，我無法處理您的請求。");

            return response.Content ?? "抱歉，我無法處理您的請求。";
        }
        catch (Exception ex)
        {
            return $"處理請求時發生錯誤：{ex.Message}";
        }
    }

    /// <summary>
    /// 分析學生作業並提供 AI 回饋
    /// </summary>
    public async Task<string> AnalyzeHomeworkAsync(string studentName, string homeworkTitle, string submittedContent)
    {
        try
        {
            var analysisPrompt = $@"
請分析以下學生作業並提供建設性回饋：

學生：{studentName}
作業標題：{homeworkTitle}
學生作答內容：{submittedContent}

請提供：
1. 作答內容的優點
2. 需要改進的地方
3. 具體的學習建議
4. 鼓勵性的話語

回饋請用繁體中文，語氣要友善且具建設性。
";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("你是一位專業的教師，專門提供學生作業回饋和學習指導。");
            chatHistory.AddUserMessage(analysisPrompt);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.8,
                MaxTokens = 500
            };

            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory, 
                executionSettings, 
                _kernel);

            // 將回饋儲存到資料庫
            var studentPlugin = _kernel.Plugins["StudentPlugin"];
            await _kernel.InvokeAsync(studentPlugin["SubmitHomeworkFeedback"], new KernelArguments
            {
                ["studentName"] = studentName,
                ["homeworkTitle"] = homeworkTitle,
                ["aiFeedback"] = response.Content ?? ""
            });

            return response.Content ?? "無法生成回饋。";
        }
        catch (Exception ex)
        {
            return $"分析作業時發生錯誤：{ex.Message}";
        }
    }

    /// <summary>
    /// 取得學生的聊天歷史摘要
    /// </summary>
    public string GetChatHistorySummary(string studentName)
    {
        if (!_chatHistories.ContainsKey(studentName))
        {
            return "尚無聊天記錄。";
        }

        var chatHistory = _chatHistories[studentName];
        var userMessages = chatHistory.Where(m => m.Role == AuthorRole.User).Count();
        var assistantMessages = chatHistory.Where(m => m.Role == AuthorRole.Assistant).Count();

        return $"學生 {studentName} 的聊天記錄：共 {userMessages} 個問題，{assistantMessages} 個回應。";
    }

    /// <summary>
    /// 清除特定學生的聊天歷史
    /// </summary>
    public void ClearChatHistory(string studentName)
    {
        if (_chatHistories.ContainsKey(studentName))
        {
            _chatHistories.Remove(studentName);
        }
    }

    /// <summary>
    /// 生成每日學習建議
    /// </summary>
    public async Task<string> GenerateDailyRecommendationAsync(string studentName)
    {
        try
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage($@"
基於學生 {studentName} 的學習狀況，生成個人化的每日學習建議。
請先查詢學生的作業狀況和選課情形，然後提供：
1. 今日重點學習項目
2. 作業提醒
3. 複習建議
4. 鼓勵性話語

請用繁體中文回應，保持簡潔且實用。
");
            
            chatHistory.AddUserMessage($"請為學生 {studentName} 生成今日學習建議");

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.7,
                MaxTokens = 800
            };

            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory, 
                executionSettings, 
                _kernel);

            return response.Content ?? "無法生成每日建議。";
        }
        catch (Exception ex)
        {
            return $"生成每日建議時發生錯誤：{ex.Message}";
        }
    }
}