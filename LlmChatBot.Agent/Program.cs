using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;

namespace LlmChatBot.Agent
{
    internal class Program
    {
        private const string CompletionModel = "gpt-3.5-turbo";
        private const string EmbeddingsModel = "text-embedding-ada-002";
        private const string ChromeDbEndpoint = "http://localhost:8000";

        private static IConfiguration config;
        private static Kernel kernel;
        private static ISemanticTextMemory memory;

        public static async Task Main(string[] args)
        {
            config = CreateConfig();
            kernel = CreateKernel();
            memory = CreateMemory();
            var questionFunction = kernel.CreateFunctionFromPrompt(
                    "When did {{$artist}} release '{{$song}}'?"
            );
            var result = await questionFunction.InvokeAsync(kernel, new KernelArguments
            {
                ["song"] = "Blank Space",
                ["artist"] = "Taylor Swift",
            });
            Console.WriteLine("Answer: {0}", result.GetValue<string>());
        }

        private static IConfiguration CreateConfig() => new ConfigurationBuilder()
                //.AddJsonFile("appsettings.json").Build();
                .AddUserSecrets<Program>().Build();

        private static string GetOpenAiApiKey() => config["OpenAI:ApiKey"] ??
                                                   throw new Exception("OpenAI:ApiKey configuration required.");

        //private static string GetAssemblyAiApiKey() => config["AssemblyAI:ApiKey"] ??
        //                                               throw new Exception("AssemblyAI:ApiKey configuration required.");

        private static Kernel CreateKernel()
        {
            var builder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(CompletionModel, GetOpenAiApiKey());

            return builder.Build();
        }

        private static ISemanticTextMemory CreateMemory()
        {
            return new MemoryBuilder()
                .WithChromaMemoryStore(ChromeDbEndpoint)
                .WithOpenAITextEmbeddingGeneration(EmbeddingsModel, GetOpenAiApiKey())
                .Build();
        }
    }
}
