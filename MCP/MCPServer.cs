using System.Text.Json;
using Microsoft.Extensions.Logging;
using MyFirstSKApp.Services;

namespace MyFirstSKApp.MCP;

/// <summary>
/// MCP 伺服器實作，處理與 AI Agents 的通訊
/// </summary>
public class MCPServer
{
    private readonly AIAgentService _agentService;
    private readonly ILogger<MCPServer> _logger;

    public MCPServer(AIAgentService agentService, ILogger<MCPServer> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    /// <summary>
    /// 取得可用的 MCP 工具列表
    /// </summary>
    public List<MCPTool> GetAvailableTools()
    {
        return new List<MCPTool>
        {
            new MCPTool
            {
                name = "query_student_homework",
                description = "查詢學生的作業狀況、成績和老師回饋",
                inputSchema = new MCPToolInputSchema
                {
                    properties = new Dictionary<string, MCPProperty>
                    {
                        ["student_name"] = new MCPProperty { type = "string", description = "學生姓名" },
                        ["course_name"] = new MCPProperty { type = "string", description = "課程名稱（可選）" }
                    },
                    required = new List<string> { "student_name" }
                }
            },
            new MCPTool
            {
                name = "get_student_profile",
                description = "取得學生的基本資料和選課狀況",
                inputSchema = new MCPToolInputSchema
                {
                    properties = new Dictionary<string, MCPProperty>
                    {
                        ["student_name"] = new MCPProperty { type = "string", description = "學生姓名" }
                    },
                    required = new List<string> { "student_name" }
                }
            },
            new MCPTool
            {
                name = "list_courses",
                description = "列出可選修的課程，可依科目篩選",
                inputSchema = new MCPToolInputSchema
                {
                    properties = new Dictionary<string, MCPProperty>
                    {
                        ["subject"] = new MCPProperty { type = "string", description = "科目名稱（可選）" }
                    }
                }
            },
            new MCPTool
            {
                name = "recommend_courses",
                description = "根據學生年級和興趣推薦適合的課程",
                inputSchema = new MCPToolInputSchema
                {
                    properties = new Dictionary<string, MCPProperty>
                    {
                        ["grade"] = new MCPProperty { type = "string", description = "學生年級" },
                        ["preferred_subject"] = new MCPProperty { type = "string", description = "偏好科目（可選）" }
                    },
                    required = new List<string> { "grade" }
                }
            },
            new MCPTool
            {
                name = "chat_with_student",
                description = "與學生進行智慧對話，提供個人化學習指導",
                inputSchema = new MCPToolInputSchema
                {
                    properties = new Dictionary<string, MCPProperty>
                    {
                        ["student_name"] = new MCPProperty { type = "string", description = "學生姓名" },
                        ["query"] = new MCPProperty { type = "string", description = "學生的問題或需求" }
                    },
                    required = new List<string> { "student_name", "query" }
                }
            },
            new MCPTool
            {
                name = "analyze_homework",
                description = "分析學生作業並提供 AI 回饋",
                inputSchema = new MCPToolInputSchema
                {
                    properties = new Dictionary<string, MCPProperty>
                    {
                        ["student_name"] = new MCPProperty { type = "string", description = "學生姓名" },
                        ["homework_title"] = new MCPProperty { type = "string", description = "作業標題" },
                        ["submitted_content"] = new MCPProperty { type = "string", description = "學生提交的作業內容" }
                    },
                    required = new List<string> { "student_name", "homework_title", "submitted_content" }
                }
            }
        };
    }

    /// <summary>
    /// 處理 MCP 工具調用
    /// </summary>
    public async Task<MCPToolCallResult> HandleToolCallAsync(MCPToolCallRequest request)
    {
        try
        {
            _logger.LogInformation($"處理 MCP 工具調用: {request.name}");

            var result = request.name switch
            {
                "query_student_homework" => await HandleQueryStudentHomework(request.arguments),
                "get_student_profile" => await HandleGetStudentProfile(request.arguments),
                "list_courses" => await HandleListCourses(request.arguments),
                "recommend_courses" => await HandleRecommendCourses(request.arguments),
                "chat_with_student" => await HandleChatWithStudent(request.arguments),
                "analyze_homework" => await HandleAnalyzeHomework(request.arguments),
                _ => new MCPToolCallResult
                {
                    isError = true,
                    content = new List<MCPContent>
                    {
                        new MCPContent { text = $"未知的工具: {request.name}" }
                    }
                }
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"處理 MCP 工具調用時發生錯誤: {request.name}");
            return new MCPToolCallResult
            {
                isError = true,
                content = new List<MCPContent>
                {
                    new MCPContent { text = $"工具調用失敗: {ex.Message}" }
                }
            };
        }
    }

    private async Task<MCPToolCallResult> HandleQueryStudentHomework(Dictionary<string, object> arguments)
    {
        var studentName = arguments["student_name"].ToString() ?? "";
        var courseName = arguments.ContainsKey("course_name") ? arguments["course_name"].ToString() : null;
        
        var response = await _agentService.ProcessStudentQueryAsync(
            studentName, 
            $"請查詢我的作業狀況{(courseName != null ? $"，特別是{courseName}課程" : "")}");

        return new MCPToolCallResult
        {
            content = new List<MCPContent>
            {
                new MCPContent { text = response }
            }
        };
    }

    private async Task<MCPToolCallResult> HandleGetStudentProfile(Dictionary<string, object> arguments)
    {
        var studentName = arguments["student_name"].ToString() ?? "";
        
        var response = await _agentService.ProcessStudentQueryAsync(
            studentName, 
            "請查詢我的基本資料和選課狀況");

        return new MCPToolCallResult
        {
            content = new List<MCPContent>
            {
                new MCPContent { text = response }
            }
        };
    }

    private async Task<MCPToolCallResult> HandleListCourses(Dictionary<string, object> arguments)
    {
        var subject = arguments.ContainsKey("subject") ? arguments["subject"].ToString() : null;
        
        var response = await _agentService.ProcessStudentQueryAsync(
            "系統", 
            $"請列出可選修的課程{(subject != null ? $"，特別是{subject}相關課程" : "")}");

        return new MCPToolCallResult
        {
            content = new List<MCPContent>
            {
                new MCPContent { text = response }
            }
        };
    }

    private async Task<MCPToolCallResult> HandleRecommendCourses(Dictionary<string, object> arguments)
    {
        var grade = arguments["grade"].ToString() ?? "";
        var preferredSubject = arguments.ContainsKey("preferred_subject") ? arguments["preferred_subject"].ToString() : null;
        
        var response = await _agentService.ProcessStudentQueryAsync(
            "系統", 
            $"請為{grade}學生推薦適合的課程{(preferredSubject != null ? $"，偏好科目是{preferredSubject}" : "")}");

        return new MCPToolCallResult
        {
            content = new List<MCPContent>
            {
                new MCPContent { text = response }
            }
        };
    }

    private async Task<MCPToolCallResult> HandleChatWithStudent(Dictionary<string, object> arguments)
    {
        var studentName = arguments["student_name"].ToString() ?? "";
        var query = arguments["query"].ToString() ?? "";
        
        var response = await _agentService.ProcessStudentQueryAsync(studentName, query);

        return new MCPToolCallResult
        {
            content = new List<MCPContent>
            {
                new MCPContent { text = response }
            }
        };
    }

    private async Task<MCPToolCallResult> HandleAnalyzeHomework(Dictionary<string, object> arguments)
    {
        var studentName = arguments["student_name"].ToString() ?? "";
        var homeworkTitle = arguments["homework_title"].ToString() ?? "";
        var submittedContent = arguments["submitted_content"].ToString() ?? "";
        
        var response = await _agentService.AnalyzeHomeworkAsync(studentName, homeworkTitle, submittedContent);

        return new MCPToolCallResult
        {
            content = new List<MCPContent>
            {
                new MCPContent { text = response }
            }
        };
    }

    /// <summary>
    /// 處理 MCP 訊息
    /// </summary>
    public async Task<MCPMessage> ProcessMessageAsync(MCPMessage message)
    {
        try
        {
            return message.method switch
            {
                "initialize" => HandleInitialize(message),
                "tools/list" => HandleToolsList(message),
                "tools/call" => await HandleToolsCall(message),
                _ => new MCPMessage
                {
                    id = message.id,
                    error = new MCPError
                    {
                        code = -32601,
                        message = $"未知的方法: {message.method}"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"處理 MCP 訊息時發生錯誤: {message.method}");
            return new MCPMessage
            {
                id = message.id,
                error = new MCPError
                {
                    code = -32603,
                    message = $"內部錯誤: {ex.Message}"
                }
            };
        }
    }

    private MCPMessage HandleInitialize(MCPMessage message)
    {
        return new MCPMessage
        {
            id = message.id,
            result = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { },
                    logging = new { }
                },
                serverInfo = new
                {
                    name = "TutorialSchool-AI-Agent",
                    version = "1.0.0"
                }
            })).RootElement
        };
    }

    private MCPMessage HandleToolsList(MCPMessage message)
    {
        var tools = GetAvailableTools();
        return new MCPMessage
        {
            id = message.id,
            result = JsonDocument.Parse(JsonSerializer.Serialize(new { tools })).RootElement
        };
    }

    private async Task<MCPMessage> HandleToolsCall(MCPMessage message)
    {
        var request = JsonSerializer.Deserialize<MCPToolCallRequest>(message.@params?.GetRawText() ?? "{}");
        if (request == null)
        {
            return new MCPMessage
            {
                id = message.id,
                error = new MCPError { code = -32602, message = "無效的參數" }
            };
        }

        var result = await HandleToolCallAsync(request);
        return new MCPMessage
        {
            id = message.id,
            result = JsonDocument.Parse(JsonSerializer.Serialize(result)).RootElement
        };
    }
}