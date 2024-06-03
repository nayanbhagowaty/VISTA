namespace LlmChatBot.ApiService.Models
{
    public class UserMessage
    {
        public string Text { get; set; } = "";
    }
    public class HistoryInput
    {
        public List<HistoryItem> Messages { get; set; } = [];
        public class HistoryItem
        {
            public string Role { get; set; } = "User";
            public string Content { get; set; } = "";
        }
    }
 }
