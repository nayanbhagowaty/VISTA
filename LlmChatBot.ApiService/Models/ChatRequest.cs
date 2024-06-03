namespace LlmChatBot.ApiService.Models
{
    public record ChatRequest(string Model, List<Message> Messages);

    public record Message(string Role, string Content);

    public record ChatResponse
    {
        public string Id { get; init; }
        public string Object { get; init; }
        public long Created { get; init; }
        public string Model { get; init; }
        public string SystemFingerprint { get; init; }
        public List<Choice> Choices { get; init; }
        public Usage Usage { get; init; }

    }

    public record Choice
    {
        public int Index { get; init; }
        public Message Message { get; init; }
        public object Logprobs { get; init; }
        public string FinishReason { get; init; }
    }

    public record Usage
    {
        public int PromptTokens { get; init; }
        public int CompletionTokens { get; init; }
        public int TotalTokens { get; init; }
    }
}
