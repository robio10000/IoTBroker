using IoTBroker.API.Middleware;

namespace IoTBroker.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    ///     Configures the middleware pipeline for the IoTBroker application, including exception handling,
    ///     Swagger UI for development, HTTPS redirection, authorization, API key authentication,
    ///     and mapping of controllers and a health endpoint.
    /// </summary>
    /// <param name="app"></param>
    public static void UseIoTBroker(this WebApplication app)
    {
        ///app.UseMiddleware<ExceptionMiddleware>(); // TODO: Create global exception handling middleware

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
    }
}