using LlmChatBot.Web;
using LlmChatBot.Web.Components;
using LlmChatBot.Web.Service;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ChatApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });
builder.Services.AddSingleton<ChatService>(cp =>
{
    var config = cp.GetRequiredService<IConfiguration>();
    var apiUrl = config.GetValue<string>("ChatSettings:ApiURL");
    var apiKey = config.GetValue<string>("ChatSettings:ApiKey");
    return new ChatService(apiUrl, apiKey);
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    AzureOpenAI options = sp.GetRequiredService<IOptions<AzureOpenAI>>().Value;
    return new AzureOpenAIChatCompletionService(options.ChatDeploymentName, options.Endpoint, options.ApiKey);
});
//https://devblogs.microsoft.com/semantic-kernel/using-semantic-kernel-with-dependency-injection/
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
