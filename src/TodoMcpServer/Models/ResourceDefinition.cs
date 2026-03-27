using System.Text.Json.Serialization;

namespace TodoMcpServer.Models;

public class ResourceDefinition
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "application/json";
}

public class ResourceContent
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
    
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "application/json";
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
