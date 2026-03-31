using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace TodoMcpServer.Prompts;

[McpServerPromptType]
public class TodoPrompts
{
    [McpServerPrompt(Name = "sprint-planning")]
    [Description("Generate a breakdown of tasks for a sprint based on the project context and sprint goal")]
    public static GetPromptResult SprintPlanning(
        [Description("The goal or theme for this sprint (e.g., 'Implement checkout payment flow')")] string goal)
    {
        var promptText = $@"Based on the current project context (review the project://context resource), please help plan tasks for this sprint.

Sprint Goal: {goal}

Create a breakdown of tasks that:
- Aligns with our team structure and roles
- Follows our task conventions (BUG: prefix for urgent items, TECH: for tech debt)
- Addresses our key priorities (performance and security)
- Ensures all API endpoints are documented
- Requires code review before completion

After generating the task list, use the add_task tool to create each task in the system.";

        var messages = new List<PromptMessage>
        {
            new PromptMessage
            {
                Role = Role.User,
                Content = new TextContentBlock { Text = promptText }
            }
        };

        return new GetPromptResult
        {
            Messages = messages,
            Description = $"Sprint planning for: {goal}"
        };
    }
}
