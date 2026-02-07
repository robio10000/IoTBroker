using IoTBroker.Extensions;

/// <summary>
/// Main entry point for the IoTBroker application.
/// Initializes the web application and starts the application.
/// </summary>

// Override the default configuration to load environment variables from a .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIoTBrokerServices(builder.Configuration);

builder.Services.AddIoTBrokerSwagger();

builder.Services.AddIoTBrokerDb(builder.Configuration);

var app = builder.Build();

app.UseIoTBroker();

app.Run();