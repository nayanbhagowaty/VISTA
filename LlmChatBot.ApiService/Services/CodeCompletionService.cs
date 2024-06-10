using LLama;
using LLama.Common;

namespace LlmChatBot.ApiService.Services
{
    public class CodeCompletionService
    {
        //private readonly ChatSession _session;
        private readonly LLamaContext _context;
        private readonly ILogger<CodeCompletionService> _logger;
        private bool _continue = false;
        const string InstructionPrefix = "[INST]";
        const string InstructionSuffix = "[/INST]";
        const string SystemInstruction = "You're an intelligent, concise coding assistant. " +
            "Wrap code in ``` for readability. Don't repeat yourself. " +
            "Use best practice and good coding standards.";
        public CodeCompletionService(IConfiguration configuration, ILogger<CodeCompletionService> logger)
        {
            if (!File.Exists(configuration["ModelPath"]))
                throw new Exception("LLM File not found");
            var @params = new LLama.Common.ModelParams(configuration["ModelPath"]!)
            {
                ContextSize = 4096, // The longest length of chat as memory.
                //GpuLayerCount = -1 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
            };

            // todo: share weights from a central service
            using var weights = LLamaWeights.LoadFromFile(@params);

            _logger = logger;
            _context = new LLamaContext(weights, @params);

            //_session = new ChatSession(new InteractiveExecutor(_context));
            //_session.History.AddMessage(LLama.Common.AuthorRole.System, SystemPrompt);
        }
        public async IAsyncEnumerable<string> GetCodeComplete(string userInstruction) {
            var executor = new InstructExecutor(_context, InstructionPrefix, InstructionSuffix, null);
            var inferenceParams = new InferenceParams()
            {
                Temperature = 0.8f,
                MaxTokens = -1,
            };
            string instruction = $"{SystemInstruction}\n\n";
            instruction += userInstruction;
            await foreach (var text in executor.InferAsync(instruction + Environment.NewLine, inferenceParams))
            {
                yield return text;
            }
        }
    }
}
