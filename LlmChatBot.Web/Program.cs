using LlmChatBot.Web;
using LlmChatBot.Web.Components;
using LlmChatBot.Web.Service;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

//builder.Services.AddHttpClient<ChatApiClient>(client => client.BaseAddress = new("http://apiservice"));
builder.Services.AddHttpClient<ChatApiClient>(client =>
{
    client.BaseAddress = new Uri("http://apiservice");
    client.Timeout = TimeSpan.FromSeconds(60); // Adjust the timeout value as needed
});
builder.Services.AddSingleton<ChatService>(cp =>
{
    var config = cp.GetRequiredService<IConfiguration>();
    var apiUrl = config.GetValue<string>("ChatSettings:ApiURL");
    var apiKey = config.GetValue<string>("ChatSettings:ApiKey");
    return new ChatService(apiUrl, apiKey);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
