using System.Text.Json.Serialization;
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
});

builder.Services.AddSingleton<ISensorService, SensorService>();

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

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Message = "IoTBroker is running" }))
    .WithName("Root");

app.Run();