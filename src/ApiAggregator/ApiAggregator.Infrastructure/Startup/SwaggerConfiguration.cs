using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiAggregator.Infrastructure.Startup;

public static class SwaggerConfiguration
{
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SchemaFilter<EnumValueSchemaFilter>();
        });
        services.AddSwaggerGen();
    }

    public static void EnableSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
    }
}

public class EnumValueSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumValues = Enum.GetValues(context.Type);
            foreach (var enumValue in enumValues)
            {
                var name = Enum.GetName(context.Type, enumValue);
                var value = Convert.ToInt32(enumValue);
                schema.Enum.Add(new OpenApiInteger(value));
            }
            schema.Type = "integer";
            schema.Format = "int32";
            // Add enum value descriptions to the schema description
            var enumDescriptions = enumValues
                .Cast<object>()
                .Select(v => $"{Convert.ToInt32(v)} = {Enum.GetName(context.Type, v)}");
            schema.Description = $"Enum values: {string.Join(", ", enumDescriptions)}";
        }
    }
}