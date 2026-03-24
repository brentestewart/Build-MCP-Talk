namespace TodoMcpServer.Models;

public class TaskDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
}
