using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Infrastructure.Jobs;
using AgendeAqui.Infrastructure.Messaging;
using AgendeAqui.Infrastructure.Messaging.Consumers;
using AgendeAqui.Infrastructure.MultiTenancy;
using AgendeAqui.Infrastructure.Notifications;
using AgendeAqui.Infrastructure.Persistence;
using AgendeAqui.Infrastructure.Persistence.Interceptors;
using AgendeAqui.Infrastructure.Persistence.Repositories;
using AgendeAqui.Infrastructure.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace AgendeAqui.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

        var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        // Multi-tenancy
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<TenantInterceptor>();

        // DbContext
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<TenantInterceptor>();
            options.UseNpgsql(connectionString)
                   .AddInterceptors(interceptor);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IDataDeletionRequestRepository, DataDeletionRequestRepository>();

        // Dapper
        services.AddScoped<ISqlConnectionFactory>(sp =>
        {
            var tenantProvider = sp.GetRequiredService<ITenantProvider>();
            return new SqlConnectionFactory(connectionString, tenantProvider);
        });

        // RabbitMQ
        services.AddSingleton(rabbitMqSettings);
        services.AddSingleton<RabbitMqConnection>();
        services.AddScoped<IEventBus, RabbitMqEventBus>();
        services.AddHostedService<RabbitMqSetup>();

        // Notification Repository
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Consumers
        services.AddHostedService<AppointmentCreatedConsumer>();
        services.AddHostedService<AppointmentCancelledConsumer>();
        services.AddHostedService<AppointmentRescheduledConsumer>();
        services.AddHostedService<AppointmentReminderJob>();

        // Notification Service
        services.AddScoped<INotificationService, NotificationService>();

        // Webhook Repository
        services.AddScoped<IWebhookRepository, WebhookRepository>();

        // Webhook Consumer
        services.AddHostedService<WebhookDeliveryConsumer>();

        // Webhook Dispatcher (typed HttpClient with timeout + resilience)
        services.AddHttpClient<WebhookDispatcher>()
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

        // WhatsApp Client with resilience
        services.Configure<ChakraChatSettings>(configuration.GetSection("ChakraChat"));

        var chakraSettings = configuration.GetSection("ChakraChat").Get<ChakraChatSettings>()
            ?? new ChakraChatSettings();

        services.AddHttpClient<IWhatsAppClient, WhatsAppClient>(client =>
        {
            client.BaseAddress = new Uri(chakraSettings.BaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chakraSettings.ApiKey}");
        })
        .AddStandardResilienceHandler();

        return services;
    }
}
