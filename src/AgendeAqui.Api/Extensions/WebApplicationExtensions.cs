using AgendeAqui.Api.Endpoints;
using AgendeAqui.Api.Middleware;

namespace AgendeAqui.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAgendeAqui(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseHttpsRedirection();

        app.MapHealthChecks("/health");
        app.MapTenantEndpoints();
        app.MapProfessionalEndpoints();
        app.MapServiceEndpoints();
        app.MapClientEndpoints();

        return app;
    }
}
