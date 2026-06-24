using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace EventosVivosBackNet.Api.Middleware;

[ExcludeFromCodeCoverage]
public static class SwaggerExtension
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();
        return services;
    }

    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
                options.RouteTemplate = "swagger/{documentName}/swagger.json")
            .UseSwaggerUI(c =>
            {
                c.DocumentTitle = "EventosVivos API";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventosVivos API v1.0");
                c.DisplayRequestDuration();
                c.DefaultModelsExpandDepth(-1);
                c.EnableDeepLinking();
                c.ConfigObject.AdditionalItems.Add("showCommonExtensions", true);
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(ModelRendering.Model);
                c.DisplayOperationId();
                c.DocExpansion(DocExpansion.None);
                c.EnableFilter();
                c.EnablePersistAuthorization();
                c.EnableTryItOutByDefault();
                c.MaxDisplayedTags(5);
                c.ShowExtensions();
                c.ShowCommonExtensions();
                c.EnableValidator();
                c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Patch, SubmitMethod.Head);
                c.UseRequestInterceptor("(request) => { request.headers['Cache-Control'] = 'no-cache, no-store'; request.headers['Pragma'] = 'no-cache'; return request; }");
                c.UseResponseInterceptor("(response) => { return response; }");
            });
        return app;
    }
}
