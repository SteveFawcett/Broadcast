namespace Broadcast.Classes;

public class JsonFetcher
{
    private readonly HttpClient _httpClient;

    public JsonFetcher(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JsonFetcher/1.0");
    }

    public async Task<string> GetJsonAsync(string endpoint, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var response = await _httpClient.GetAsync(endpoint, cts.Token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}