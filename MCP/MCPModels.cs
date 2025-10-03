using System.Text.Json;

namespace MyFirstSKApp.MCP;

/// <summary>
/// MCP 通訊協定的基礎訊息結構
/// </summary>
public class MCPMessage
{
    public string jsonrpc { get; set; } = "2.0";
    public string? id { get; set; }
    public string method { get; set; } = string.Empty;
    public JsonElement? @params { get; set; }
    public JsonElement? result { get; set; }
    public MCPError? error { get; set; }
}

public class MCPError
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public JsonElement? data { get; set; }
}

/// <summary>
/// MCP 工具定義
/// </summary>
public class MCPTool
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public MCPToolInputSchema inputSchema { get; set; } = new();
}

public class MCPToolInputSchema
{
    public string type { get; set; } = "object";
    public Dictionary<string, MCPProperty> properties { get; set; } = new();
    public List<string> required { get; set; } = new();
}

public class MCPProperty
{
    public string type { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public List<string>? @enum { get; set; }
}

/// <summary>
/// MCP 工具調用請求
/// </summary>
public class MCPToolCallRequest
{
    public string name { get; set; } = string.Empty;
    public Dictionary<string, object> arguments { get; set; } = new();
}

/// <summary>
/// MCP 工具調用結果
/// </summary>
public class MCPToolCallResult
{
    public bool isError { get; set; } = false;
    public List<MCPContent> content { get; set; } = new();
}

public class MCPContent
{
    public string type { get; set; } = "text";
    public string text { get; set; } = string.Empty;
}