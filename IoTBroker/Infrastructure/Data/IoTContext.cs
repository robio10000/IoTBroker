using IoTBroker.Domain;
using IoTBroker.Features.Rules.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTBroker.Infrastructure.Data;

public class IoTContext : DbContext
{
    public IoTContext(DbContextOptions<IoTContext> options) : base(options) { }

    public DbSet<ApiClient> Clients { get; set; }
    public DbSet<SensorPayload> Payloads { get; set; }
    public DbSet<SensorRule> Rules { get; set; }
    public DbSet<DeviceState> DeviceStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite Key für DeviceState
        modelBuilder.Entity<DeviceState>()
            .HasKey(ds => new { ds.ClientId, ds.DeviceId });

        // Beispiel für Fluent API: Table Names
        modelBuilder.Entity<ApiClient>().ToTable("api_clients");
        
        base.OnModelCreating(modelBuilder);
    }
}