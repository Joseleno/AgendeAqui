using AgendeAqui.Domain.Services;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Services;

public class ServiceTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Haircut");
        result.Value.Duration.Should().Be(TimeSpan.FromMinutes(30));
        result.Value.Price.Should().Be(50m);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = Service.Create(Guid.NewGuid(), "", TimeSpan.FromMinutes(30), 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidName);
    }

    [Fact]
    public void Create_WithZeroDuration_ShouldFail()
    {
        var result = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.Zero, 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidDuration);
    }

    [Fact]
    public void Create_WithNegativeDuration_ShouldFail()
    {
        var result = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(-10), 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidDuration);
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldFail()
    {
        var result = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), -10m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidPrice);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        var result = Service.Create(Guid.NewGuid(), "Free Consultation", TimeSpan.FromMinutes(15), 0m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Price.Should().Be(0m);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var result = Service.Create(Guid.NewGuid(), "  Haircut  ", TimeSpan.FromMinutes(30), 50m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Haircut");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveAndUpdatedAt()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        service.Deactivate();

        service.Activate();

        service.IsActive.Should().BeTrue();
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsInactiveAndUpdatedAt()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        service.Deactivate();

        service.IsActive.Should().BeFalse();
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithDurationOver8Hours_ShouldFail()
    {
        var result = Service.Create(Guid.NewGuid(), "Marathon", TimeSpan.FromHours(9), 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidDuration);
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldSucceed()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        var result = service.UpdateDetails("Premium Haircut", TimeSpan.FromMinutes(60), 100m);

        result.IsSuccess.Should().BeTrue();
        service.Name.Should().Be("Premium Haircut");
        service.Duration.Should().Be(TimeSpan.FromMinutes(60));
        service.Price.Should().Be(100m);
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ShouldFail()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        var result = service.UpdateDetails("", TimeSpan.FromMinutes(60), 100m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidName);
        service.Name.Should().Be("Haircut");
    }

    [Fact]
    public void UpdateDetails_WithZeroDuration_ShouldFail()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        var result = service.UpdateDetails("Haircut", TimeSpan.Zero, 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidDuration);
    }

    [Fact]
    public void UpdateDetails_WithNegativePrice_ShouldFail()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        var result = service.UpdateDetails("Haircut", TimeSpan.FromMinutes(30), -10m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidPrice);
    }

    [Fact]
    public void UpdateDetails_WithDurationExceeding8Hours_ShouldReturnInvalidDuration()
    {
        // Arrange
        var result = Service.Create(Guid.NewGuid(), "Test Service", TimeSpan.FromMinutes(60), 100m);
        var service = result.Value;

        // Act
        var updateResult = service.UpdateDetails("Updated", TimeSpan.FromHours(9), 200m);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Should().Be(ServiceErrors.InvalidDuration);
    }
}
