using IoTBroker.Features.Rules.Actions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IoTBroker.Infrastructure.Swagger;

public class PolymorphismSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(RuleAction))
        {
            // Add the discriminator manually to the schema if Swashbuckle forgets it
            if (!schema.Properties.ContainsKey("$type"))
            {
                schema.Properties.Add("$type", new OpenApiSchema { Type = "string", Description = "Discriminator (webhook | set_value)" });
            }
        }
    }
}