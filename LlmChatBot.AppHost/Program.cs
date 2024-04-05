var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.LlmChatBot_ApiService>("apiservice");

builder.AddProject<Projects.LlmChatBot_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
