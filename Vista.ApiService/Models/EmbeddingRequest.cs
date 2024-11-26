using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vista.ApiService.Models
{
    public class EmbeddingRequest
    {
        public string[] Input { get; set; }
        public string Model { get; set; } = "Phi-2";
        //public string Encoding_format { get; set; } = "float";
    }
    public class EmbeddingResponse
    {
        public string @object { get; set; } = "list";
        public List<Datum> data { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
    }
    public class Datum
    {
        public string @object { get; set; } = "embedding";
        public float[] Embedding { get; set; }
        public int index { get; set; } = 0;
    }
    //public class Usage
    //{
    //    public int prompt_tokens { get; set; }
    //    public int total_tokens { get; set; }
    //}
}
