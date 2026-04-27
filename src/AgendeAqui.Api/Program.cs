using AgendeAqui.Api;
using AgendeAqui.Api.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, loggerConfig) =>
        loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "AgendeAqui")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

    builder.Services.AddAgendeAqui(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseAgendeAqui();

    if (app.Environment.IsDevelopment())
        await SeedData.InitializeAsync(app);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Make Program accessible for WebApplicationFactory in integration tests
public static partial class Program { }
