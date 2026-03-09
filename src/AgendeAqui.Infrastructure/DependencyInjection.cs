using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Application.Webhooks;
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
using System.Net.Sockets;

namespace AgendeAqui.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddMessaging(configuration)
            .AddExternalServices(configuration)
            .AddBackgroundJobs(configuration);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<TenantInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<TenantInterceptor>();
            options.UseNpgsql(connectionString)
                   .AddInterceptors(interceptor);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IDataDeletionRequestRepository, DataDeletionRequestRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();

        services.AddScoped<ISqlConnectionFactory>(sp =>
        {
            var tenantProvider = sp.GetRequiredService<ITenantProvider>();
            return new SqlConnectionFactory(connectionString, tenantProvider);
        });

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

        var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddSingleton(rabbitMqSettings);
        services.AddSingleton<RabbitMqConnection>();
        services.AddScoped<IEventBus, RabbitMqEventBus>();
        services.AddHostedService<RabbitMqSetup>();

        services.AddHostedService<AppointmentCreatedConsumer>();
        services.AddHostedService<AppointmentCancelledConsumer>();
        services.AddHostedService<AppointmentRescheduledConsumer>();
        services.AddHostedService<WebhookDeliveryConsumer>();

        return services;
    }

    private static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<INotificationService, NotificationService>();

        services.AddHttpClient<WebhookDispatcher>()
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler
                {
                    ConnectCallback = SsrfProtectedHandler.ConnectCallback,
                };
            })
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

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

    private static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ReminderSettings>(configuration.GetSection(ReminderSettings.SectionName));
        services.AddHostedService<AppointmentReminderJob>();

        return services;
    }
}
