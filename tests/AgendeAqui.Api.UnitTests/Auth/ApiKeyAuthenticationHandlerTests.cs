using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Abstractions.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Encodings.Web;

namespace AgendeAqui.Api.UnitTests.Auth;

// Note: ApiKeyAuthenticationHandler.HandleAuthenticateAsync() uses Dapper with DbConnection
// which cannot be unit-tested without a real database. The DB-dependent paths (valid key → Success
// with claims, invalid key → Fail) are covered by integration tests.
// These tests verify the header-parsing early-exit paths.
public class ApiKeyAuthenticationHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ApiKeyAuthenticationHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
    }

    private async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(HttpContext? httpContext = null)
    {
        var options = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

        var handler = new ApiKeyAuthenticationHandler(
            options,
            loggerFactory,
            UrlEncoder.Default,
            _sqlConnectionFactory);

        var context = httpContext ?? new DefaultHttpContext();
        var scheme = new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler));

        await handler.InitializeAsync(scheme, context);

        return handler;
    }

    [Fact]
    public async Task HandleAuthenticate_WithNoApiKeyHeader_ShouldReturnNoResult()
    {
        var handler = await CreateHandlerAsync();

        var result = await handler.AuthenticateAsync();

        result.None.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAuthenticate_WithEmptyApiKeyHeader_ShouldReturnNoResult()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Api-Key"] = "   ";
        var handler = await CreateHandlerAsync(context);

        var result = await handler.AuthenticateAsync();

        result.None.Should().BeTrue();
    }
}
