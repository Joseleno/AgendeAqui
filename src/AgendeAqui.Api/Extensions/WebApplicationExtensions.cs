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

        app.UseHttpsRedirection();

        app.UseRateLimiter();

        app.UseAuthentication();

        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapTenantEndpoints();
        app.MapProfessionalEndpoints();
        app.MapServiceEndpoints();
        app.MapClientEndpoints();
        app.MapAppointmentEndpoints();
        app.MapAvailabilityEndpoints();
        app.MapScheduleEndpoints();
        app.MapNotificationEndpoints();

        return app;
    }
}
