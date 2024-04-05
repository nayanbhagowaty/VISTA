using Newtonsoft.Json;
using System.Text;

namespace LlmChatBot.Web.Service
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;

        public ChatService(string baseUrl, string apiKey)
        {
            _httpClient = new();
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<string> GetResponse(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress)
            {
                Content = new StringContent("{\"text\": " +
                                            query +
                                            "}",
                                            Encoding.UTF8,
                                            "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseString = JsonConvert.DeserializeObject<dynamic>(responseContent);

            return responseString!.choices[0].text;
        }
    }
}
