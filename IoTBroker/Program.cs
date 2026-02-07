using IoTBroker.Extensions;

/// <summary>
/// Main entry point for the IoTBroker application.
/// Initializes the web application and starts the application.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIoTBrokerServices(builder.Configuration);

builder.Services.AddIoTBrokerSwagger();

builder.Services.AddIoTBrokerDb(builder.Configuration);

var app = builder.Build();

app.UseIoTBroker();

app.Run();