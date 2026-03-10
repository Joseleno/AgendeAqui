using AgendeAqui.Api.Endpoints;
using AgendeAqui.Api.Hubs;
using AgendeAqui.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;

namespace AgendeAqui.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAgendeAqui(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseForwardedHeaders();

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseRateLimiter();

        app.UseAuthentication();

        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseAuthorization();

        app.UseMiddleware<IdempotencyMiddleware>();

        app.UseHttpMetrics();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
        app.MapMetrics();
        app.MapAuthEndpoints();
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
        app.MapApiKeyEndpoints();
        app.MapReportEndpoints();
        app.MapHub<AppointmentHub>("/hubs/appointments");

        return app;
    }
}
