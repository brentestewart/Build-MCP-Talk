using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace TodoMcpServer.Resources;

[McpServerResourceType]
public class TodoResources
{
    [McpServerResource(UriTemplate = "project://context", Name = "Project Context", MimeType = "application/json")]
    [Description("Current project information including team, priorities, sprint, and task conventions")]
    public static async Task<TextResourceContents> GetProjectContext()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "project-context.json");
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        var content = await File.ReadAllTextAsync(filePath);
        
        return new TextResourceContents
        {
            Uri = "project://context",
            MimeType = "application/json",
            Text = content
        };
    }
}
