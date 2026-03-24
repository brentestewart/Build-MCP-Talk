using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Endpoints;

public static class TodoEndpoints
{
    public static WebApplication MapTodoEndpoints(this WebApplication app)
    {
        app.MapGet("/tasks", GetTasks).WithName("GetTasks");
        app.MapPost("/tasks", AddTask).WithName("AddTask");
        app.MapPost("/tasks/{id}/complete", CompleteTask).WithName("CompleteTask");

        return app;
    }

    private static IResult GetTasks(TaskService taskService, [FromServices] ILogger<Program> logger)
    {
        var tasks = taskService.GetOpenTasks();
        logger.LogInformation("Retrieved {Count} open tasks", tasks.Count);
        return Results.Ok(tasks);
    }

    private static IResult AddTask(TaskCreateRequest request, TaskService taskService, [FromServices] ILogger<Program> logger)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return Results.BadRequest(new { error = "Description is required" });
        }

        if (request.Description.Length > 500)
        {
            return Results.BadRequest(new { error = "Description must be 500 characters or less" });
        }

        var task = taskService.AddTask(request.Description);
        logger.LogInformation("Added task: Id={Id}, Description={Description}", task.Id, task.Description);
        return Results.Created($"/tasks/{task.Id}", task);
    }

    private static IResult CompleteTask(int id, TaskService taskService, [FromServices] ILogger<Program> logger)
    {
        var task = taskService.CompleteTask(id);

        if (task == null)
        {
            return Results.NotFound(new { error = $"Task with id {id} not found" });
        }

        logger.LogInformation("Completed task: Id={Id}, Description={Description}", task.Id, task.Description);
        return Results.Ok(task);
    }
}
