using System.Text.Json;
using IoTBroker.Domain;
using IoTBroker.Features.Rules.Actions;
using IoTBroker.Features.Rules.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        // Helper variable for JSON options
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // ApiClient configuration with custom conversions and comparers
        modelBuilder.Entity<ApiClient>(entity =>
        {
            entity.ToTable("api_clients");

            // Roles: List<string> with Comparer
            entity.Property(e => e.Roles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // OwnedDevices: HashSet<string> with Comparer
            entity.Property(e => e.OwnedDevices)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<HashSet<string>>(v, jsonOptions) ?? new HashSet<string>())
                .Metadata.SetValueComparer(new ValueComparer<HashSet<string>>(
                    (c1, c2) => c1!.SetEquals(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToHashSet()));
        });

        // DeviceState (Composite Key)
        modelBuilder.Entity<DeviceState>()
            .HasKey(ds => new { ds.ClientId, ds.DeviceId });

        // SensorPayload (Index for performance)
        modelBuilder.Entity<SensorPayload>()
            .HasIndex(p => new { p.ClientId, p.DeviceId });
        
        // SensorRule Cascade Deletes for Conditions and Actions
        modelBuilder.Entity<SensorRule>(entity =>
        {
            // Conditions relation
            entity.HasMany(r => r.Conditions)
                .WithOne()
                .HasForeignKey("SensorRuleId")
                .OnDelete(DeleteBehavior.Cascade);

            // Actions relation
            entity.HasMany(r => r.Actions)
                .WithOne()
                .HasForeignKey("SensorRuleId")
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        
        
        // RuleActions (TPH)
        modelBuilder.Entity<RuleAction>()
            .HasDiscriminator<string>("ActionType")
            .HasValue<WebHookAction>("webhook")
            .HasValue<SetDeviceValueAction>("set_device_value");
        
        // WebHookAction Headers: Dictionary with Comparer
        modelBuilder.Entity<WebHookAction>()
            .Property(b => b.Headers)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions) ?? new Dictionary<string, string>())
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (d1, d2) => d1!.Count == d2!.Count && !d1.Except(d2).Any(),
                d => d.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                d => d.ToDictionary(entry => entry.Key, entry => entry.Value)));

        // Set default max length for string properties that are keys or end with "Id"
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(string)))
            {
                // If it is a key or foreign key or has "Id" in the name
                if (property.IsPrimaryKey() || property.IsForeignKey() || property.Name.EndsWith("Id"))
                {
                    property.SetMaxLength(255);
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}