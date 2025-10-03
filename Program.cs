using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using MyFirstSKApp.Data;
using MyFirstSKApp.Services;
using MyFirstSKApp.Plugins;
using MyFirstSKApp.MCP;
using System.Text.Json;

namespace MyFirstSKApp;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎓 智慧補習班 AI 協作助理系統啟動中...\n");

        var builder = WebApplication.CreateBuilder(args);

        // 設定服務
        ConfigureWebServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // 設定中介軟體
        ConfigureWebApp(app);

        // 初始化資料庫
        await InitializeDatabaseAsync(app.Services);

        Console.WriteLine("🌐 Web 應用程式已啟動！");
        Console.WriteLine($"📱 請開啟瀏覽器訪問: http://localhost:5000");
        Console.WriteLine("🔗 API 文件: http://localhost:5000/swagger");
        Console.WriteLine("🎯 主頁面: http://localhost:5000");
        Console.WriteLine("\n按 Ctrl+C 停止服務器");

        await app.RunAsync();
    }

    private static void ConfigureWebServices(IServiceCollection services, IConfiguration configuration)
    {
        // Web 服務
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "智慧補習班 AI 助理 API", 
                Version = "v1",
                Description = "基於 Semantic Kernel 和 MCP 的智慧協作 AI Agent 系統"
            });
        });

        // CORS 設定
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });

        // 日誌服務
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // 資料庫服務
        services.AddDbContext<TutorialSchoolDbContext>(options =>
            options.UseInMemoryDatabase("TutorialSchoolDB"));

        // Semantic Kernel 設定
        var kernelBuilder = Kernel.CreateBuilder();
        
        // 從設定檔讀取 OpenAI API Key
        var openAIApiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(openAIApiKey) || openAIApiKey == "your-openai-api-key-here")
        {
            Console.WriteLine("⚠️  警告: 請在 appsettings.json 中設定您的 OpenAI API Key");
            Console.WriteLine("💡 提示: 將 'your-openai-api-key-here' 替換為您的實際 API Key");
            Console.WriteLine("🎭 目前使用模擬 AI 服務進行展示");
            
            // 使用模擬服務進行展示
            kernelBuilder.Services.AddSingleton<IChatCompletionService>(sp => 
                new MockChatCompletionService());
        }
        else
        {
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: configuration["OpenAI:Model"] ?? "gpt-4-turbo-preview",
                apiKey: openAIApiKey);
        }

        // 將 Kernel 註冊為 Scoped，確保 Plugin 可以正確取得相依服務
        services.AddScoped<Kernel>(provider =>
        {
            var kernel = kernelBuilder.Build();
            
            // 註冊 Plugins，從服務提供者取得相依項目
            var dbContext = provider.GetRequiredService<TutorialSchoolDbContext>();
            kernel.ImportPluginFromObject(new StudentPlugin(dbContext), "StudentPlugin");
            kernel.ImportPluginFromObject(new CoursePlugin(dbContext), "CoursePlugin");
            
            return kernel;
        });
        services.AddScoped<IChatCompletionService>(sp => 
            sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

        // AI Agent 服務
        services.AddScoped<AIAgentService>();

        // MCP 伺服器
        services.AddScoped<MCPServer>();
    }

    private static void ConfigureWebApp(WebApplication app)
    {
        // 開發環境設定
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "智慧補習班 AI 助理 API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseCors();
        
        // 靜態檔案服務
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // 路由設定
        app.UseRouting();
        app.MapControllers();

        // 預設路由到首頁
        app.MapFallbackToFile("index.html");
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("📚 初始化資料庫中...");
        
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TutorialSchoolDbContext>();
        
        await dbContext.Database.EnsureCreatedAsync();
        
        Console.WriteLine("✅ 資料庫初始化完成");
        Console.WriteLine($"   - 學生數量: {dbContext.Students.Count()}");
        Console.WriteLine($"   - 課程數量: {dbContext.Courses.Count()}");
        Console.WriteLine($"   - 作業數量: {dbContext.Homeworks.Count()}\n");
    }

    private static async Task RunInteractiveSystemAsync(ServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var aiAgentService = scope.ServiceProvider.GetRequiredService<AIAgentService>();
        var mcpServer = scope.ServiceProvider.GetRequiredService<MCPServer>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        Console.WriteLine("🤖 智慧補習班 AI 助理系統已就緒！");
        Console.WriteLine("=====================================");
        Console.WriteLine("可用功能：");
        Console.WriteLine("1. 與 AI 助理對話");
        Console.WriteLine("2. 查看 MCP 工具列表");
        Console.WriteLine("3. 測試 MCP 工具調用");
        Console.WriteLine("4. 生成每日學習建議");
        Console.WriteLine("5. 分析作業回饋");
        Console.WriteLine("6. 查看聊天歷史摘要");
        Console.WriteLine("輸入 'exit' 結束程式");
        Console.WriteLine("=====================================\n");

        while (true)
        {
            Console.Write("請選擇功能 (1-6) 或輸入 'exit': ");
            var choice = Console.ReadLine()?.Trim();

            if (choice?.ToLower() == "exit")
            {
                Console.WriteLine("👋 感謝使用智慧補習班 AI 助理系統！");
                break;
            }

            try
            {
                switch (choice)
                {
                    case "1":
                        await HandleChatInteraction(aiAgentService);
                        break;
                    case "2":
                        ShowMCPTools(mcpServer);
                        break;
                    case "3":
                        await TestMCPToolCall(mcpServer);
                        break;
                    case "4":
                        await GenerateDailyRecommendation(aiAgentService);
                        break;
                    case "5":
                        await AnalyzeHomework(aiAgentService);
                        break;
                    case "6":
                        ShowChatHistorySummary(aiAgentService);
                        break;
                    default:
                        Console.WriteLine("❌ 無效選擇，請輸入 1-6 或 'exit'\n");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "處理用戶請求時發生錯誤");
                Console.WriteLine($"❌ 發生錯誤: {ex.Message}\n");
            }
        }
    }

    private static async Task HandleChatInteraction(AIAgentService aiAgentService)
    {
        Console.WriteLine("\n💬 AI 助理對話模式");
        Console.WriteLine("請先輸入學生姓名：");
        var studentName = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(studentName))
        {
            Console.WriteLine("❌ 學生姓名不能為空\n");
            return;
        }

        Console.WriteLine($"\n歡迎 {studentName}！請輸入您的問題（輸入 'back' 返回主選單）：");

        while (true)
        {
            Console.Write($"{studentName} > ");
            var query = Console.ReadLine()?.Trim();

            if (query?.ToLower() == "back")
            {
                Console.WriteLine("返回主選單\n");
                break;
            }

            if (string.IsNullOrEmpty(query))
            {
                continue;
            }

            Console.WriteLine("\n🤖 AI 助理回覆：");
            var response = await aiAgentService.ProcessStudentQueryAsync(studentName, query);
            Console.WriteLine($"{response}\n");
        }
    }

    private static void ShowMCPTools(MCPServer mcpServer)
    {
        Console.WriteLine("\n🛠️  可用的 MCP 工具：");
        var tools = mcpServer.GetAvailableTools();
        
        foreach (var tool in tools)
        {
            Console.WriteLine($"- {tool.name}: {tool.description}");
        }
        Console.WriteLine();
    }

    private static async Task TestMCPToolCall(MCPServer mcpServer)
    {
        Console.WriteLine("\n🧪 MCP 工具調用測試");
        Console.WriteLine("可測試的工具：");
        Console.WriteLine("1. query_student_homework");
        Console.WriteLine("2. chat_with_student");
        
        Console.Write("請選擇要測試的工具 (1-2): ");
        var choice = Console.ReadLine()?.Trim();

        MCPToolCallRequest? request = choice switch
        {
            "1" => new MCPToolCallRequest
            {
                name = "query_student_homework",
                arguments = new Dictionary<string, object>
                {
                    ["student_name"] = "張小明"
                }
            },
            "2" => new MCPToolCallRequest
            {
                name = "chat_with_student",
                arguments = new Dictionary<string, object>
                {
                    ["student_name"] = "張小明",
                    ["query"] = "我的數學作業有什麼需要注意的地方？"
                }
            },
            _ => null
        };

        if (request == null)
        {
            Console.WriteLine("❌ 無效選擇\n");
            return;
        }

        Console.WriteLine("⏳ 執行中...");
        var result = await mcpServer.HandleToolCallAsync(request);
        
        Console.WriteLine("\n📋 MCP 工具調用結果：");
        Console.WriteLine($"是否錯誤: {result.isError}");
        foreach (var content in result.content)
        {
            Console.WriteLine($"內容: {content.text}");
        }
        Console.WriteLine();
    }

    private static async Task GenerateDailyRecommendation(AIAgentService aiAgentService)
    {
        Console.WriteLine("\n📅 生成每日學習建議");
        Console.Write("請輸入學生姓名: ");
        var studentName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(studentName))
        {
            Console.WriteLine("❌ 學生姓名不能為空\n");
            return;
        }

        Console.WriteLine("⏳ 生成中...");
        var recommendation = await aiAgentService.GenerateDailyRecommendationAsync(studentName);
        
        Console.WriteLine("\n📝 今日學習建議：");
        Console.WriteLine(recommendation);
        Console.WriteLine();
    }

    private static async Task AnalyzeHomework(AIAgentService aiAgentService)
    {
        Console.WriteLine("\n📝 作業分析回饋");
        Console.Write("請輸入學生姓名: ");
        var studentName = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(studentName))
        {
            Console.WriteLine("❌ 學生姓名不能為空\n");
            return;
        }

        Console.Write("請輸入作業標題: ");
        var homeworkTitle = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(homeworkTitle))
        {
            Console.WriteLine("❌ 作業標題不能為空\n");
            return;
        }

        Console.WriteLine("請輸入學生作答內容:");
        var submittedContent = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(submittedContent))
        {
            Console.WriteLine("❌ 作答內容不能為空\n");
            return;
        }

        Console.WriteLine("⏳ 分析中...");
        var feedback = await aiAgentService.AnalyzeHomeworkAsync(studentName, homeworkTitle, submittedContent);
        
        Console.WriteLine("\n🎯 AI 分析回饋：");
        Console.WriteLine(feedback);
        Console.WriteLine();
    }

    private static void ShowChatHistorySummary(AIAgentService aiAgentService)
    {
        Console.WriteLine("\n📊 聊天歷史摘要");
        Console.Write("請輸入學生姓名: ");
        var studentName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(studentName))
        {
            Console.WriteLine("❌ 學生姓名不能為空\n");
            return;
        }

        var summary = aiAgentService.GetChatHistorySummary(studentName);
        Console.WriteLine($"\n📈 {summary}\n");
    }
}

/// <summary>
/// 模擬 Chat Completion 服務（當沒有 OpenAI API Key 時使用）
/// </summary>
public class MockChatCompletionService : IChatCompletionService
{
    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
        
        var mockResponse = GenerateMockResponse(lastMessage);
        
        var result = new List<ChatMessageContent>
        {
            new ChatMessageContent(AuthorRole.Assistant, mockResponse)
        };

        return Task.FromResult<IReadOnlyList<ChatMessageContent>>(result);
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
        yield return new StreamingChatMessageContent(AuthorRole.Assistant, response.First().Content);
    }

    private string GenerateMockResponse(string userMessage)
    {
        // 模擬 AI 回應邏輯
        if (userMessage.Contains("作業") || userMessage.Contains("homework"))
        {
            return "我已經查詢了您的作業狀況。根據記錄，您有幾份作業需要注意：\n\n" +
                   "📚 二次函數練習 - 已評分 (85分)\n" +
                   "老師回饋：計算正確，但要注意圖形標示\n\n" +
                   "📚 三角函數應用 - 已繳交，等待評分\n" +
                   "您提到需要第6題的協助，建議複習三角函數的基本性質。\n\n" +
                   "繼續保持用功！如有問題歡迎隨時詢問。";
        }
        
        if (userMessage.Contains("課程") || userMessage.Contains("course"))
        {
            return "以下是為您推薦的課程：\n\n" +
                   "📖 國三數學總復習\n" +
                   "授課老師：陳老師\n" +
                   "學費：$12,000\n" +
                   "開課日期：適合會考準備\n\n" +
                   "這門課程很適合您目前的程度，建議盡早報名！";
        }

        return "您好！我是智慧補習班 AI 助理。我可以協助您：\n\n" +
               "✅ 查詢作業狀況和成績\n" +
               "✅ 推薦適合的課程\n" +
               "✅ 提供學習建議\n" +
               "✅ 解答學習相關問題\n\n" +
               "請告訴我您需要什麼協助？\n\n" +
               "💡 提示：由於未設定 OpenAI API Key，目前使用模擬回應。";
    }
}
