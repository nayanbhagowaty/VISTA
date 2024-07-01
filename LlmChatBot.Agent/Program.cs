using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

namespace LlmChatBot.Agent
{
    internal class Program
    {
        private const string CompletionModel = "fake";
        private const string EmbeddingsModel = "Phi-2";
        private const string ChromeDbEndpoint = "http://localhost:8000";
        private static HttpClient client = new HttpClient(new CustomHttpHandler());
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
                ["song"] = "my zero to hero",
                ["artist"] = "Taylor Swift",
            });
            Console.WriteLine("Answer: {0}", result.GetValue<string>());

            var result1 = await questionFunction.InvokeAsync(kernel, new KernelArguments
            {
                ["song"] = "Blank Space",
                ["artist"] = "Taylor Swift",
            });
            Console.WriteLine("Answer: {0}", result1.GetValue<string>());

            const string question = "When did Taylor Swift release 'my zero to hero'?";
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
                  Song name: Anti-zero
                  Artist: Taylor Swift
                  Release date: December 31, 2020
                  """,
                id: "antihero_taylorswift"
            );

            await memory.SaveInformationAsync(
               collection: collectionName,
               text: """
                  Song name: my-zero-to-hero
                  Artist: Taylor Swift
                  Release date: December 31, 2023
                  """,
               id: "antihero_taylorswift_zero"
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
            var result3 = await questionFunction.InvokeAsync(kernel, new KernelArguments
            {
                ["context"] = doc?.Metadata.Text,
                ["question"] = question
            });
            Console.WriteLine("Answer: {0}", result3.GetValue<string>());


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
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("fake-model-name", "fake-api-key", httpClient: client);

            return kernel.Build();
        }

        private static ISemanticTextMemory CreateMemory()
        {
            return new MemoryBuilder()
                .WithChromaMemoryStore(ChromeDbEndpoint) //ToDo: use MSSQL Memo
                //.WithOpenAITextEmbeddingGeneration(EmbeddingsModel,"", GetOpenAiApiKey())
                .WithOpenAITextEmbeddingGeneration(EmbeddingsModel,"fake", httpClient:client)
                .Build();
        }
    }
}
//https://www.assemblyai.com/blog/ask-dotnetrocks-questions-semantic-kernel/