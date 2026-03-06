using AgendeAqui.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAgendeAqui(builder.Configuration);

var app = builder.Build();

app.UseAgendeAqui();

app.Run();
