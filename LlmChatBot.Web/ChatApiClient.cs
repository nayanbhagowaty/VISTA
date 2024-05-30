using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LlmChatBot.Web;

public class ChatApiClient(HttpClient httpClient)
{
    public async Task<string> GetResponseAsync(string query)
    {
        string responseText = "";
        try
        {
            var payload = new { text = query };
            var Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            var response = await httpClient.PostAsync("/chatstream", Content);
            if (response.IsSuccessStatusCode)
            {
                // Read the response stream asynchronously and update responseText
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        responseText += line.Replace("data:", ""); // Add each line of the response to the responseText
                    }
                }
            }
            else
            {
                // Handle error response
                responseText = "Error: " + response.StatusCode;
            }
        }
        catch (Exception ex)
        {
            // Handle exception
            responseText = "Exception: " + ex.Message;
        }
        return responseText;
    }
    public async Task<HttpResponseMessage> GetResponseMessageAsync(string query)
    {
        var payload = new { text = query };
            var Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            return await httpClient.PostAsync("/chatstream", Content);
    }

    public async Task<Stream> GetResponseStreamAsync()
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        return await httpClient.GetStreamAsync("/chatstream");
    }
    public async Task<HttpResponseMessage> GetResponseStreamAsync2(string query)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var payload = new { text = query };
        var request = new HttpRequestMessage(HttpMethod.Post, "/chatstream2")
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        };
        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    public async Task<HttpResponseMessage> GetResponseStreamAsync(string query)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var payload = new { text = query };
        var request = new HttpRequestMessage(HttpMethod.Post, "/chatstream")
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        };
        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }
}
