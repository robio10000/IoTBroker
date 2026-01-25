using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IoTBroker.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using System.Reflection;

#pragma warning disable EF1001 // We are using internal EF Core APIs for the filter
/// <summary>
/// A custom MigrationsAssembly that filters migrations based on the current database provider.
/// </summary>
public class ProviderSpecificMigrationsAssembly : MigrationsAssembly
{
    private readonly ICurrentDbContext _currentContext;

    public ProviderSpecificMigrationsAssembly(
        ICurrentDbContext currentContext, 
        IDbContextOptions options, 
        IMigrationsIdGenerator idGenerator, 
        IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnosticSource) 
        : base(currentContext, options, idGenerator, diagnosticSource)
    {
        _currentContext = currentContext;
    }

    /// <summary>
    /// Returns only the migrations relevant to the current database provider.
    /// </summary>
    public override IReadOnlyDictionary<string, TypeInfo> Migrations 
    {
        get
        {
            var allMigrations = base.Migrations;
            var activeProvider = _currentContext.Context.Database.ProviderName?
                .Split('.').Last().ToLower(); // ergibt "sqlite", "mysql" oder "npgsql"

            // Map "npgsql" or "postgresql" to "postgres"
            if (activeProvider == "npgsql" || activeProvider == "postgresql") activeProvider = "postgres";

            // Only return migrations whose namespace contains the provider name
            return allMigrations
                .Where(m => m.Value.Namespace!.ToLower().Contains(activeProvider!))
                .ToDictionary(m => m.Key, m => m.Value);
        }
    }
}