using System.Text.Json;
using IoTBroker.Domain;
using IoTBroker.Features.Rules.Actions;
using IoTBroker.Features.Rules.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTBroker.Infrastructure.Data;

public class IoTContext : DbContext
{
    public IoTContext(DbContextOptions<IoTContext> options) : base(options)
    {
    }

    public DbSet<ApiClient> Clients { get; set; }
    public DbSet<SensorPayload> Payloads { get; set; }
    public DbSet<SensorRule> Rules { get; set; }
    public DbSet<DeviceState> DeviceStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite key for DeviceState
        modelBuilder.Entity<DeviceState>()
            .HasKey(ds => new { ds.ClientId, ds.DeviceId });
        
        modelBuilder.Entity<ApiClient>().ToTable("api_clients");

        // Configure RuleAction inheritance using TPH
        modelBuilder.Entity<RuleAction>()
            .HasDiscriminator<string>("ActionType")
            .HasValue<WebHookAction>("webhook")
            .HasValue<SetDeviceValueAction>("set_device_value");

        // Configure conversion for Headers property in WebHookAction
        modelBuilder.Entity<WebHookAction>()
            .Property(b => b.Headers)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ??
                     new Dictionary<string, string>() // Aus DB als Dictionary laden
            );

        base.OnModelCreating(modelBuilder);
    }
}