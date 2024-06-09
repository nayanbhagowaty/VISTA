using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LlmChatBot.ApiService.Models
{
    public class EmbeddingRequest
    {
        public string Text { get; set; }
        public string Model { get; set; } = "fake";
        public string Encoding_format { get; set; } = "float";
        public EmbeddingRequest(string text)
        {
            Text = text;
        }
    }
    public class EmbeddingResponse
    {
        public string @object { get; set; }
        public List<Datum> data { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
    }
    public class Datum
    {
        public string @object { get; set; }
        public List<double> embedding { get; set; }
        public int index { get; set; }
    }
    //public class Usage
    //{
    //    public int prompt_tokens { get; set; }
    //    public int total_tokens { get; set; }
    //}
}
