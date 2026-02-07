using System.Text.Json.Serialization;
using IoTBroker.Features.Clients;
using IoTBroker.Features.Rules;
using IoTBroker.Features.Rules.Actions;
using IoTBroker.Features.Rules.Strategies;
using IoTBroker.Features.Sensors;
using IoTBroker.Infrastructure.Data;
using IoTBroker.Infrastructure.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.OpenApi.Models;

namespace IoTBroker.Extensions;

/// <summary>
///     Extension methods for configuring services in the IoTBroker application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Configures the necessary services for the IoTBroker application,
    ///     including controllers, HTTP clients, API key service, trigger strategies, sensor and rule services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration for accessing settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddIoTBrokerServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Allow enum values as strings in JSON
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddHttpClient("WebHookClient", c => { c.Timeout = TimeSpan.FromSeconds(10); });
        services.AddEndpointsApiExplorer();

        services.AddScoped<IApiKeyService, ApiKeyService>();

        services.AddScoped<ITriggerStrategy, NumericTriggerStrategy>();
        services.AddScoped<ITriggerStrategy, BooleanTriggerStrategy>();
        services.AddScoped<ITriggerStrategy, StringTriggerStrategy>();

        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<IRuleService, RuleService>();

        return services;
    }

    /// <summary>
    ///     Configures Swagger for API documentation, including setting up the API info,
    ///     handling polymorphism in the models, and adding security definitions for API key authentication.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddIoTBrokerSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IoTBroker API",
                Version = "v1",
                Description = "API for IoTBroker"
            });

            c.UseAllOfForInheritance();
            c.UseOneOfForPolymorphism();
            c.SchemaFilter<PolymorphismSchemaFilter>();

            c.SelectSubTypesUsing(baseType =>
            {
                if (baseType == typeof(RuleAction))
                    return new[] { typeof(SetDeviceValueAction), typeof(WebHookAction) };
                return Enumerable.Empty<Type>();
            });

            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "Enter your API key into the 'X-API-KEY' header",
                In = ParameterLocation.Header,
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        },
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    ///     Configures the database context for the IoTBroker application based on the specified
    ///     database provider in the configuration. Supports SQLite, PostgreSQL, MySQL, and In-Memory databases.
    ///     Also replaces the default migrations assembly with a provider-specific one to handle differences
    ///     in migration generation across providers.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration for accessing settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddIoTBrokerDb(this IServiceCollection services, IConfiguration configuration)
    {
        var dbProvider = configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
        var connectionString = configuration.GetConnectionString(
            dbProvider.ToLower() == "postgres" ? "PostgresConnection" :
            dbProvider.ToLower() == "mysql" ? "MySqlConnection" : "SQLiteConnection");

        services.AddDbContext<IoTContext>(options =>
        {
            switch (dbProvider?.ToLower())
            {
                case "sqlite":
                    options.UseSqlite(connectionString, x => x.MigrationsAssembly("IoTBroker"));
                    break;
                case "postgres":
                    options.UseNpgsql(connectionString, x => x.MigrationsAssembly("IoTBroker"));
                    break;
                case "mysql":
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                        x => x.MigrationsAssembly("IoTBroker"));
                    break;
                case "inmemory":
                    options.UseInMemoryDatabase("IoTBrokerTestDb", x => x.EnableNullChecks(false));
                    break;
                default:
                    throw new Exception(
                        $"Unsupported Database Provider: {dbProvider}. Supported providers are: SQLite, Postgres, MySQL, InMemory.");
            }

            options.ReplaceService<IMigrationsAssembly, ProviderSpecificMigrationsAssembly>();
        });

        return services;
    }
}