using TodoApi.Endpoints;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register TaskService as singleton
builder.Services.AddSingleton<TaskService>();

// Add OpenAPI support
builder.Services.AddOpenApi();

// CORS: Allow all origins for development/demo
// In production, restrict to specific origins only
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.MapOpenApi();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck");

// Map endpoints
app.MapTodoEndpoints();

// Configure to listen on port 5000
app.Urls.Clear();
app.Urls.Add("http://localhost:5000");

logger.LogInformation("Starting Todo Backend API on http://localhost:5000");

app.Run();
