using AgendeAqui.Api.Endpoints;
using AgendeAqui.Api.Middleware;
using Prometheus;

namespace AgendeAqui.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAgendeAqui(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.UseRateLimiter();

        app.UseAuthentication();

        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseAuthorization();

        app.UseHttpMetrics();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");
        app.MapMetrics();
        app.MapLgpdEndpoints();
        app.MapTenantEndpoints();
        app.MapProfessionalEndpoints();
        app.MapServiceEndpoints();
        app.MapClientEndpoints();
        app.MapAppointmentEndpoints();
        app.MapAvailabilityEndpoints();
        app.MapScheduleEndpoints();
        app.MapNotificationEndpoints();
        app.MapWebhookEndpoints();
        app.MapReportEndpoints();

        return app;
    }
}
