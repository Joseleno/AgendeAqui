using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Infrastructure.Messaging;
using AgendeAqui.Infrastructure.MultiTenancy;
using AgendeAqui.Infrastructure.Persistence;
using AgendeAqui.Infrastructure.Persistence.Interceptors;
using AgendeAqui.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
