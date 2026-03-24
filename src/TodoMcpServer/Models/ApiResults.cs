namespace TodoMcpServer.Models;

public class AddTaskResult
{
    public bool Success { get; set; }
    public TaskDto? Task { get; set; }
    public string? Error { get; set; }
}

public class CompleteTaskResult
{
    public bool Success { get; set; }
    public TaskDto? Task { get; set; }
    public string? Error { get; set; }
}

public class ListTasksResult
{
    public bool Success { get; set; }
    public List<TaskDto> Tasks { get; set; } = new();
    public int Count { get; set; }
    public string? Error { get; set; }
}
