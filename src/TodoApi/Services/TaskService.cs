using System.Collections.Concurrent;
using TodoApi.Models;

namespace TodoApi.Services;

public class TaskService
{
    private readonly ConcurrentDictionary<int, TaskItem> _tasks = new();
    private int _nextId = 0;

    public TaskItem AddTask(string description)
    {
        var task = new TaskItem
        {
            Id = Interlocked.Increment(ref _nextId),
            Description = description,
            Completed = false
        };
        _tasks[task.Id] = task;
        return task;
    }

    public TaskItem? CompleteTask(int id)
    {
        if (_tasks.TryGetValue(id, out var task))
        {
            task.Completed = true;
            return task;
        }
        return null;
    }

    public List<TaskItem> GetOpenTasks()
    {
        return _tasks.Values
            .Where(t => !t.Completed)
            .OrderBy(t => t.Id)
            .ToList();
    }
}
