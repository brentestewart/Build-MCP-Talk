using System.Text.Json;
using TodoMcpServer.Models;

namespace TodoMcpServer.Services;

public static class ResourceDefinitions
{
    public const string UriProjectContext = "project://context";
    
    public static readonly List<ResourceDefinition> Resources = new()
    {
        new()
        {
            Uri = UriProjectContext,
            Name = "Project Context",
            Description = "Current project information including team, priorities, sprint, and task conventions",
            MimeType = "application/json"
        }
    };

    public static async Task<ResourceContent?> ReadResourceAsync(string uri)
    {
        return uri switch
        {
            UriProjectContext => await ReadProjectContextAsync(),
            _ => null
        };
    }

    private static async Task<ResourceContent> ReadProjectContextAsync()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "project-context.json");
        var content = await File.ReadAllTextAsync(filePath);
        
        return new ResourceContent
        {
            Uri = UriProjectContext,
            MimeType = "application/json",
            Text = content
        };
    }
}
