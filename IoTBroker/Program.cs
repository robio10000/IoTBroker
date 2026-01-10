using System.Text.Json.Serialization;
using IoTBroker.API.Middleware;
using IoTBroker.Features.Clients;
using IoTBroker.Features.Rules;
using IoTBroker.Features.Rules.Actions;
using IoTBroker.Features.Rules.Strategies;
using IoTBroker.Features.Sensors;
using Microsoft.OpenApi.Models;

/// <summary>
/// Main entry point for the IoTBroker application.
/// Configures services, middleware, and a simple health endpoints.
/// Also sets up Swagger for API documentation.
/// Reach the health endpoint at /health
/// Reach the Swagger UI at /doc
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow enum values as strings in JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IoTBroker API",
        Version = "v1",
        Description = "API for IoTBroker"
    });

    c.UseAllOfForInheritance();
    c.UseOneOfForPolymorphism();

    c.SelectSubTypesUsing(baseType =>
    {
        if (baseType == typeof(IRuleAction)) return new[] {typeof(SetDeviceValueAction), typeof(WebHookAction) };
        return Enumerable.Empty<Type>();
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        //Description = "In den Header 'X-API-KEY' eintragen",
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

builder.Services.AddSingleton<ISensorService, SensorService>();
builder.Services.AddSingleton<IApiKeyService, ApiKeyService>();

builder.Services.AddSingleton<ITriggerStrategy, NumericTriggerStrategy>();
builder.Services.AddSingleton<ITriggerStrategy, BooleanTriggerStrategy>();
builder.Services.AddSingleton<ITriggerStrategy, StringTriggerStrategy>();

builder.Services.AddSingleton<IRuleService, RuleService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IoTBroker v1");
        c.RoutePrefix = "doc";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Message = "IoTBroker is running" }))
    .WithName("Root");

app.Run();