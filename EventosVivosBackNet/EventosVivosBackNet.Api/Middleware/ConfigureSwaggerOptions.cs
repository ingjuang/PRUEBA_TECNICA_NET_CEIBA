using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventosVivosBackNet.Api.Middleware;

[ExcludeFromCodeCoverage]
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = $"v1 - {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}",
            Title = "EventosVivos API",
            Description = "API para gestión de eventos culturales y reservas - EventosVivos"
        });
    }
}
