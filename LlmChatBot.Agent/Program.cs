using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

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
            kernel = CreateKernel2();
            memory = CreateMemory();


            // verify chat completion service works
            var questionFunction = kernel.CreateFunctionFromPrompt(
                "When did {{$artist}} release '{{$song}}'?"
            );
            var result = await questionFunction.InvokeAsync(kernel, new KernelArguments
            {
                ["song"] = "Blank Space",
                ["artist"] = "Taylor Swift",
            });
            Console.WriteLine("Answer: {0}", result.GetValue<string>());

            const string question = "When did Taylor Swift release 'Anti-Hero'?";
            // verify that embeddings generation and Chroma DB connector works
            const string collectionName = "Songs";
            await memory.SaveInformationAsync(
                collection: collectionName,
                text: """
                  Song name: Never Gonna Give You Up
                  Artist: Rick Astley
                  Release date: 27 July 1987
                  """,
                id: "nevergonnagiveyouup_ricksastley"
            );
            await memory.SaveInformationAsync(
                collection: collectionName,
                text: """
                  Song name: Anti-Hero
                  Artist: Taylor Swift
                  Release date: October 24, 2022
                  """,
                id: "antihero_taylorswift"
            );

            var docs = memory.SearchAsync(collectionName, query: question, limit: 1);
            var doc = docs.ToBlockingEnumerable().SingleOrDefault();
            Console.WriteLine("Chroma DB result: {0}", doc?.Metadata.Text);

            questionFunction = kernel.CreateFunctionFromPrompt(
                """
                Here is relevant context: {{$context}}
                ---
                {{$question}}
                """);
            result = await questionFunction.InvokeAsync(kernel, new KernelArguments
            {
                ["context"] = doc?.Metadata.Text,
                ["question"] = question
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

        private static Kernel CreateKernel2()
        {
            HttpClient client = new HttpClient(new CustomHttpHandler());

            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("fake-model-name", "fake-api-key", httpClient: client);

            return kernel.Build();
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
//https://www.assemblyai.com/blog/ask-dotnetrocks-questions-semantic-kernel/