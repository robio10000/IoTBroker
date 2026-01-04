using System.Text;

namespace IoTBroker.Rules.Actions;

/// <summary>
///     Action to call a WebHook URL.
/// </summary>
public class WebHookAction : IRuleAction
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "POST"; // GET, POST, PUT
    public string? PayloadTemplate { get; set; }

    public async void Execute(IServiceProvider serviceProvider, string clientId)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient();

        try
        {
            var request = new HttpRequestMessage(new HttpMethod(Method), Url);

            if (!string.IsNullOrEmpty(PayloadTemplate) && Method != "GET")
            {
                request.Content = new StringContent(PayloadTemplate, Encoding.UTF8, "application/json");
            }

            // Send it and log errors only internally (or via Console for testing)
            var response = await client.SendAsync(request);
            Console.WriteLine($"[WebHook] {Method} to {Url} returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebHook] Error calling {Url}: {ex.Message}");
        }
    }
}