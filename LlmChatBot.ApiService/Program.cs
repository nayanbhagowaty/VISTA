using LlmChatBot.ApiService.Models;
using LlmChatBot.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ChatService>();
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();

app.MapPost("/chat", async ([FromBody] UserMessage input, [FromServices] ChatService _service, CancellationToken cancellationToken, HttpContext httpContext) =>
{
    httpContext.Response.ContentType = "text/event-stream";

    await foreach (var r in _service.SendStream(input))
    {
        await httpContext.Response.WriteAsync("data:" + r + "\n\n", cancellationToken);
        await httpContext.Response.Body.FlushAsync(cancellationToken);
    }

    await httpContext.Response.CompleteAsync();
});
app.MapPost("/v1/chat/completions", async ([FromBody] ChatRequest request, [FromServices] ChatService _service, CancellationToken cancellationToken, HttpContext httpContext) =>
{
    //var request1 = await httpContext.Request.ReadFromJsonAsync<ChatRequest>();

    // Here you would call your model and generate a response.
    // This is just a placeholder response.
    var chatresponse = await _service.Send(new UserMessage { Text = request.Messages.First().Content });

    var response = new ChatResponse
    {
        Id = "chatcmpl-123",
        Object = "chat.completion",
        Created = 1677652288,
        Model = "gpt-3.5-turbo-0125",
        SystemFingerprint = "fp_44709d6fcb",
        Choices = new List<Choice>
        {
            new Choice
            {
                Index = 0,
                Message = new Message("assistant",chatresponse),
                //Message = new Message
                //{
                //    Role = "assistant",
                //    Content = "\n\nHello there, how may I assist you today?",
                //},
                Logprobs = null,
                FinishReason = "stop"
            }
        },
        Usage = new Usage
        {
            PromptTokens = 9,
            CompletionTokens = 12,
            TotalTokens = 21
        }
    };

    await httpContext.Response.WriteAsJsonAsync(response);
});

app.MapDefaultEndpoints();

app.Run();
