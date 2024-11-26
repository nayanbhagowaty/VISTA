using LLama;
using LLama.Common;
using System.Collections.Generic;

namespace Vista.ApiService.Services
{
    public class EmbeddingService
    {
        private readonly ILogger<EmbeddingService> _logger;
        private bool _continue = false;
        private string modelPath = "";
        public EmbeddingService(IConfiguration configuration, ILogger<EmbeddingService> logger)
        {
            if (!File.Exists(configuration["EmbeddingModelPath"]))
                throw new Exception("LLM File not found");
            modelPath = configuration["EmbeddingModelPath"];
            _logger = logger;
        }
        public async Task<float[]> GetEmbeddings(string text)
        {
            var @params = new ModelParams(modelPath) {
                Embeddings = true
            };
            using var weights = LLamaWeights.LoadFromFile(@params);
            var embedder = new LLamaEmbedder(weights, @params);
            var readOnlyList = await embedder.GetEmbeddings(text);
            float[] embeddings = readOnlyList.SelectMany(arr => arr).ToArray();
            return embeddings;
        }
    }
}
