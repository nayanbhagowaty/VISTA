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
            if (!File.Exists(configuration["ModelPath"]))
                throw new Exception("LLM File not found");
            var @params = new LLama.Common.ModelParams(configuration["ModelPath"]!)
            {
                ContextSize = 4096, // The longest length of chat as memory.
                GpuLayerCount = -1 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
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
            var outputs = _session.ChatAsync(new LLama.Common.ChatHistory.Message(LLama.Common.AuthorRole.User, input.Text!)
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

        /// <summary>
        /// Creates a completion for the chat message
        /// </summary>
        //public async Task<ChatResponse> CreateChatCompletionAsync(ChatRequest request)
        //{
        //    //TODO: validate arguments, handle errors

        //    if (request.stream)
        //    {
        //        // Handled by CreateChatCompletionStream
        //        throw new InvalidOperationException("CreateChatCompletionAsync called, instead of CreateChatCompletionStream");
        //    }

        //    var choices = new List<ChatCompletionResponseChoice>();
        //    var chat_completions_tasks = new List<Task<CompletionResult>>();

        //    var genParams = GetGenerationParameters(
        //        request.model,
        //        request.messages.Select(m => new ConversationMessage { Role = m.role, Message = m.content }).ToList(),
        //        request.temperature,
        //        request.top_p,
        //        request.max_tokens ?? 512,
        //        false,
        //        [request.stop ?? string.Empty]);


        //    for (int i = 0; i < request.n; i++)
        //    {
        //        chat_completions_tasks.Add(Task.Run(() => GenerateCompletion(genParams)));
        //    }

        //    CompletionResult[] chat_completion_results;
        //    try
        //    {
        //        chat_completion_results = await Task.WhenAll(chat_completions_tasks);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception($"Error in CreateChatCompletionAsync: {e.Message}", e);
        //        //TODO: handle error
        //        //return create_error_response(ErrorCode.INTERNAL_ERROR, e);
        //    }

        //    var usage = new UsageInfo();
        //    for (int i = 0; i < request.n; i++)
        //    {
        //        var chat_completion_result = chat_completion_results[i];
        //        if (chat_completion_result.ErrorCode != 0)
        //        {
        //            throw new Exception($"Error in CreateChatCompletionAsync (ErrorCode: {chat_completion_result.ErrorCode})");
        //            //TODO: handle error
        //            //return create_error_response(content["error_code"], content["text"]);
        //        }
        //        choices.Add(new ChatCompletionResponseChoice()
        //        {
        //            index = i,
        //            message = new ChatCompletionMessage() { role = "assistant", content = chat_completion_result.Text },
        //            finish_reason = chat_completion_result.FinishReason ?? "stop"
        //        });

        //        usage.prompt_tokens = chat_completion_result.Usage.PromptTokens;
        //        usage.completion_tokens = chat_completion_result.Usage.CompletionTokens;
        //        usage.total_tokens = chat_completion_result.Usage.TotalTokens;
        //    }

        //    return new ChatCompletionResponse()
        //    {
        //        model = request.model,
        //        choices = [.. choices],
        //        usage = usage,
        //        created = DateTime.UtcNow.ToUnixTime()
        //    };
        //}
    }
}
