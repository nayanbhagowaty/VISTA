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
app.MapGet("/hello",() => { return "Hello"; });
app.MapPost("/chat", ([FromBody] UserMessage input, [FromServices]ChatService _service) =>
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