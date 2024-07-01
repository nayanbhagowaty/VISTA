using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LlmChatBot.Web;

public class ChatApiClient(HttpClient httpClient)
{
    public async Task<HttpResponseMessage> GetResponseStreamAsync(string query)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var payload = new { text = query };
        var request = new HttpRequestMessage(HttpMethod.Post, "/chat")
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        };
        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }
    public async Task<HttpResponseMessage> GetCodeStreamAsync(string query)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var payload = new { text = query };
        var request = new HttpRequestMessage(HttpMethod.Post, "/codecompletion")
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        };
        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }
}
