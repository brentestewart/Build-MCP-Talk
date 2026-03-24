using System.Net.Http.Json;
using TodoMcpServer.Models;

namespace TodoMcpServer.Services;

public class TodoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TodoApiClient> _logger;

    public TodoApiClient(HttpClient httpClient, ILogger<TodoApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AddTaskResult> AddTaskAsync(string description)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/tasks", new { Description = description });
            response.EnsureSuccessStatusCode();

            var task = await response.Content.ReadFromJsonAsync<TaskDto>();
            _logger.LogInformation("Added task via API: Id={Id}", task?.Id);

            return new AddTaskResult { Success = true, Task = task };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling add task API");
            return new AddTaskResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<CompleteTaskResult> CompleteTaskAsync(int id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/tasks/{id}/complete", null);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new CompleteTaskResult { Success = false, Error = $"Task with id {id} not found" };
            }

            response.EnsureSuccessStatusCode();

            var task = await response.Content.ReadFromJsonAsync<TaskDto>();
            _logger.LogInformation("Completed task via API: Id={Id}", task?.Id);

            return new CompleteTaskResult { Success = true, Task = task };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling complete task API");
            return new CompleteTaskResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<ListTasksResult> ListTasksAsync()
    {
        try
        {
            var tasks = await _httpClient.GetFromJsonAsync<List<TaskDto>>("/tasks");
            _logger.LogInformation("Retrieved {Count} tasks via API", tasks?.Count ?? 0);

            return new ListTasksResult { Success = true, Tasks = tasks ?? new List<TaskDto>(), Count = tasks?.Count ?? 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling list tasks API");
            return new ListTasksResult { Success = false, Error = ex.Message, Tasks = new List<TaskDto>(), Count = 0 };
        }
    }
}
