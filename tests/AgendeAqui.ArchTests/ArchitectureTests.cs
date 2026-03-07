using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace AgendeAqui.ArchTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly =
        typeof(AgendeAqui.Domain.Common.Entity).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(AgendeAqui.Application.Behaviors.ValidationBehavior<,>).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(AgendeAqui.Infrastructure.DependencyInjection).Assembly;

    // Api uses top-level statements; load by name
    private static readonly Assembly ApiAssembly =
        Assembly.Load("AgendeAqui.Api");

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnApplication()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("AgendeAqui.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Domain must not reference Application. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnInfrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("AgendeAqui.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Domain must not reference Infrastructure. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnApi()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("AgendeAqui.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Domain must not reference Api. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnInfrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("AgendeAqui.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Application must not reference Infrastructure. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("AgendeAqui.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Application must not reference Api. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_ShouldNot_HaveDependencyOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("AgendeAqui.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Infrastructure must not reference Api. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_HaveNoExternalDependencies()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "AgendeAqui.Application",
                "AgendeAqui.Infrastructure",
                "AgendeAqui.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Domain layer must be completely independent. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CommandHandlers_Should_ImplementICommandHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .HaveDependencyOn("AgendeAqui.Application.Abstractions.Messaging")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"All command handlers must use ICommandHandler. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void QueryHandlers_Should_ImplementIQueryHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .HaveDependencyOn("AgendeAqui.Application.Abstractions.Messaging")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"All query handlers must use IQueryHandler. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
