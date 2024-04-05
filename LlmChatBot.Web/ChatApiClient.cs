using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace LlmChatBot.Web;

public class ChatApiClient(HttpClient httpClient)
{
    public async Task<WeatherForecast[]> GetWeatherAsync()
    {
        return await httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast") ?? [];
    }
    public async Task<string> GetResponseAsync(string query)
    {
        string responseText = "";
        try
        {
            var payload = new { text = query };
            var Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
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
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
