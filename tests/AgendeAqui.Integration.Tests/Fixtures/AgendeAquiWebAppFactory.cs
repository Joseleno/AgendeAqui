using AgendeAqui.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace AgendeAqui.Integration.Tests.Fixtures;

public sealed class AgendeAquiWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("agendeaqui_test")
        .WithUsername("agendeaqui_test")
        .WithPassword("agendeaqui_test")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Database", _postgres.GetConnectionString());

        builder.UseSetting("RabbitMq:HostName", _rabbitMq.Hostname);
        builder.UseSetting("RabbitMq:Port", _rabbitMq.GetMappedPublicPort(5672).ToString());
        builder.UseSetting("RabbitMq:UserName", "guest");
        builder.UseSetting("RabbitMq:Password", "guest");
        builder.UseSetting("RabbitMq:VirtualHost", "/");

        // JWT test settings
        builder.UseSetting("Jwt:Issuer", "AgendeAqui.Test");
        builder.UseSetting("Jwt:Audience", "AgendeAqui.Api.Test");
        builder.UseSetting("Jwt:SecretKey", "integration-test-secret-key-must-be-at-least-32-chars");
        builder.UseSetting("Jwt:ExpirationMinutes", "60");

        // ChakraChat stub
        builder.UseSetting("ChakraChat:BaseUrl", "http://localhost:9999/");
        builder.UseSetting("ChakraChat:ApiKey", "test-api-key");
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();

        // Create schema from EF Core model (no migrations exist yet)
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await base.DisposeAsync();
    }
}
