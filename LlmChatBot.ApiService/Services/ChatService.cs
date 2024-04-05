using LLama;
using LlmChatBot.ApiService.Models;

namespace LlmChatBot.ApiService.Services
{
    public class ChatService: IDisposable
    {
        private readonly ChatSession _session;
        private readonly LLamaContext _context;
        private readonly ILogger<ChatService> _logger;
        private bool _continue = false;

        private const string SystemPrompt = "Transcript of a dialog, where the User interacts with an Assistant. Assistant is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.";

        public ChatService(IConfiguration configuration, ILogger<ChatService> logger)
        {
            var @params = new LLama.Common.ModelParams(configuration["ModelPath"]!)
            {
                ContextSize = 2048, // The longest length of chat as memory.
                GpuLayerCount = 10 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
            };

            // todo: share weights from a central service
            using var weights = LLamaWeights.LoadFromFile(@params);

            _logger = logger;
            _context = new LLamaContext(weights, @params);

            _session = new ChatSession(new InteractiveExecutor(_context));
            _session.History.AddMessage(LLama.Common.AuthorRole.System, SystemPrompt);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<string> Send(UserMessage input)
        {
            if (!_continue)
            {
                _logger.LogInformation("Prompt: {text}", SystemPrompt);
                _continue = true;
            }
            _logger.LogInformation("Input: {text}", input.Text);
            var outputs = _session.ChatAsync(
                new LLama.Common.ChatHistory.Message(LLama.Common.AuthorRole.User, input.Text),
                new LLama.Common.InferenceParams()
                {
                    RepeatPenalty = 1.0f,
                    AntiPrompts = new string[] { "User:" },
                });

            var result = "";
            await foreach (var output in outputs)
            {
                _logger.LogInformation("Message: {output}", output);
                result += output;
            }

            return result;
        }

        public async IAsyncEnumerable<string> SendStream(UserMessage input)
        {
            if (!_continue)
            {
                _logger.LogInformation(SystemPrompt);
                _continue = true;
            }

            _logger.LogInformation(input.Text);

            var outputs = _session.ChatAsync(
                new LLama.Common.ChatHistory.Message(LLama.Common.AuthorRole.User, input.Text!)
                , new LLama.Common.InferenceParams()
                {
                    RepeatPenalty = 1.0f,
                    AntiPrompts = new string[] { "User:" },
                });

            await foreach (var output in outputs)
            {
                _logger.LogInformation(output);
                yield return output;
            }
        }
    }
}
