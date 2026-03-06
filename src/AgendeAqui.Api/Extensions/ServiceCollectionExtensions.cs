using AgendeAqui.Application;
using AgendeAqui.Infrastructure;

namespace AgendeAqui.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgendeAqui(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOpenApi()
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddMediator();

        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        return services;
    }
}
