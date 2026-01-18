using System.Text;
using IoTBroker.Domain;
using IoTBroker.Features.Rules.Helper;
using IoTBroker.Features.Rules.Models;

namespace IoTBroker.Features.Rules.Actions;

/// <summary>
///     Action to call a WebHook URL.
/// </summary>
public class WebHookAction : RuleAction //IRuleAction
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "POST"; // GET, POST, PUT
    public string? PayloadTemplate { get; set; }

    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    ///     Executes the WebHook action.
    /// </summary>
    /// <param name="serviceProvider">Used to resolve services like ISensorService.</param>
    /// <param name="clientId">The context of the client who owns the rule.</param>
    /// <param name="triggerPayload">The payload that triggered the rule.</param>
    /// <param name="rule">The rule that triggered this action.</param>
    public override async Task ExecuteAsync(IServiceProvider serviceProvider, string clientId,
        SensorPayload triggerPayload, SensorRule rule)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient("WebHookClient");

        // Replace tokens in URL and Payload
        var finalUrl = TokenReplacer.Replace(Url, triggerPayload, rule);
        var finalPayload = !string.IsNullOrEmpty(PayloadTemplate)
            ? TokenReplacer.Replace(PayloadTemplate, triggerPayload, rule)
            : null;

        try
        {
            var request = new HttpRequestMessage(new HttpMethod(Method), finalUrl);

            foreach (var header in Headers)
            {
                var replacedValue = TokenReplacer.Replace(header.Value, triggerPayload, rule);
                request.Headers.TryAddWithoutValidation(header.Key, replacedValue);
            }

            if (finalPayload != null && Method != "GET")
                request.Content = new StringContent(finalPayload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            Console.WriteLine($"[WebHook] {Method} to {finalUrl} returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebHook] Error calling {finalUrl}: {ex.Message}");
        }
    }
}