using LlmChatBot.ApiService.Models;
using LlmChatBot.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ChatService>();
// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});
app.MapPost("/chat", ([FromBody] UserMessage input, [FromServices] ChatService _service) =>
{
    return _service.Send(input);
});
app.MapPost("/chatstream", async ([FromBody] UserMessage input, [FromServices] ChatService _service, CancellationToken cancellationToken, HttpContext httpContext) =>
{
    httpContext.Response.ContentType = "text/event-stream";

    await foreach (var r in _service.SendStream(input))
    {
        await httpContext.Response.WriteAsync("data:" + r + "\n\n", cancellationToken);
        await httpContext.Response.Body.FlushAsync(cancellationToken);
    }

    await httpContext.Response.CompleteAsync();
});
app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
