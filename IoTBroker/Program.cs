using System.Text.Json.Serialization;
using IoTBroker.Middleware;
using IoTBroker.Services;
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IoTBroker API",
        Version = "v1",
        Description = "API for IoTBroker"
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