namespace IoTBroker.Domain;

/// <summary>
///     Represents an API client with associated roles and owned devices.
/// </summary>
public class ApiClient
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();

    // List of DeviceIds that this client is allowed to manage
    // e.g. ["device123", "device456"]
    public HashSet<string> OwnedDevices { get; set; } = new();
}