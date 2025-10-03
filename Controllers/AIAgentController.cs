using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyFirstSKApp.Services;
using MyFirstSKApp.MCP;
using System.Text.Json;

namespace MyFirstSKApp.Controllers;

/// <summary>
/// AI Agent Web API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AIAgentController : ControllerBase
{
    private readonly AIAgentService _aiAgentService;
    private readonly MCPServer _mcpServer;
    private readonly ILogger<AIAgentController> _logger;

    public AIAgentController(
        AIAgentService aiAgentService, 
        MCPServer mcpServer,
        ILogger<AIAgentController> logger)
    {
        _aiAgentService = aiAgentService;
        _mcpServer = mcpServer;
        _logger = logger;
    }

    /// <summary>
    /// 與 AI 助理對話
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.StudentName) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest(new { error = "學生姓名和訊息不能為空" });
            }

            var response = await _aiAgentService.ProcessStudentQueryAsync(
                request.StudentName, 
                request.Message);

            return Ok(new ChatResponse
            {
                StudentName = request.StudentName,
                Response = response,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理聊天請求時發生錯誤");
            return StatusCode(500, new { error = "內部伺服器錯誤" });
        }
    }

    /// <summary>
    /// 分析作業
    /// </summary>
    [HttpPost("analyze-homework")]
    public async Task<IActionResult> AnalyzeHomework([FromBody] HomeworkAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.StudentName) || 
                string.IsNullOrEmpty(request.HomeworkTitle) ||
                string.IsNullOrEmpty(request.SubmittedContent))
            {
                return BadRequest(new { error = "所有欄位都是必填的" });
            }

            var feedback = await _aiAgentService.AnalyzeHomeworkAsync(
                request.StudentName, 
                request.HomeworkTitle, 
                request.SubmittedContent);

            return Ok(new HomeworkAnalysisResponse
            {
                StudentName = request.StudentName,
                HomeworkTitle = request.HomeworkTitle,
                Feedback = feedback,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析作業時發生錯誤");
            return StatusCode(500, new { error = "內部伺服器錯誤" });
        }
    }

    /// <summary>
    /// 生成每日學習建議
    /// </summary>
    [HttpGet("daily-recommendation/{studentName}")]
    public async Task<IActionResult> GetDailyRecommendation(string studentName)
    {
        try
        {
            if (string.IsNullOrEmpty(studentName))
            {
                return BadRequest(new { error = "學生姓名不能為空" });
            }

            var recommendation = await _aiAgentService.GenerateDailyRecommendationAsync(studentName);

            return Ok(new DailyRecommendationResponse
            {
                StudentName = studentName,
                Recommendation = recommendation,
                Date = DateTime.Today,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成每日建議時發生錯誤");
            return StatusCode(500, new { error = "內部伺服器錯誤" });
        }
    }

    /// <summary>
    /// 取得 MCP 工具列表
    /// </summary>
    [HttpGet("mcp/tools")]
    public IActionResult GetMCPTools()
    {
        try
        {
            var tools = _mcpServer.GetAvailableTools();
            return Ok(new { tools });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 MCP 工具時發生錯誤");
            return StatusCode(500, new { error = "內部伺服器錯誤" });
        }
    }

    /// <summary>
    /// 調用 MCP 工具
    /// </summary>
    [HttpPost("mcp/call")]
    public async Task<IActionResult> CallMCPTool([FromBody] MCPToolCallRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.name))
            {
                return BadRequest(new { error = "工具名稱不能為空" });
            }

            var result = await _mcpServer.HandleToolCallAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調用 MCP 工具時發生錯誤");
            return StatusCode(500, new { error = "內部伺服器錯誤" });
        }
    }

    /// <summary>
    /// 取得學生聊天歷史摘要
    /// </summary>
    [HttpGet("chat-history/{studentName}")]
    public IActionResult GetChatHistorySummary(string studentName)
    {
        try
        {
            if (string.IsNullOrEmpty(studentName))
            {
                return BadRequest(new { error = "學生姓名不能為空" });
            }

            var summary = _aiAgentService.GetChatHistorySummary(studentName);

            return Ok(new ChatHistorySummaryResponse
            {
                StudentName = studentName,
                Summary = summary,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得聊天歷史時發生錯誤");
            return StatusCode(500, new { error = "內部伺服器錯誤" });
        }
    }
}

// API 請求/回應模型

public class ChatRequest
{
    public string StudentName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string StudentName { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class HomeworkAnalysisRequest
{
    public string StudentName { get; set; } = string.Empty;
    public string HomeworkTitle { get; set; } = string.Empty;
    public string SubmittedContent { get; set; } = string.Empty;
}

public class HomeworkAnalysisResponse
{
    public string StudentName { get; set; } = string.Empty;
    public string HomeworkTitle { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class DailyRecommendationResponse
{
    public string StudentName { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ChatHistorySummaryResponse
{
    public string StudentName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}