using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.ValueObjects;

public class AppointmentStatusTests
{
    [Theory]
    [InlineData("Scheduled")]
    [InlineData("Confirmed")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    [InlineData("NoShow")]
    public void FromName_WithValidName_ShouldSucceed(string name)
    {
        var result = AppointmentStatus.FromName(name);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    [InlineData("scheduled")]
    public void FromName_WithInvalidName_ShouldFail(string name)
    {
        var result = AppointmentStatus.FromName(name);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Scheduled_CanTransitionTo_Scheduled()
    {
        AppointmentStatus.Scheduled.CanTransitionTo(AppointmentStatus.Scheduled)
            .Should().BeTrue();
    }

    [Fact]
    public void Scheduled_CanTransitionTo_Confirmed()
    {
        AppointmentStatus.Scheduled.CanTransitionTo(AppointmentStatus.Confirmed)
            .Should().BeTrue();
    }

    [Fact]
    public void Scheduled_CanTransitionTo_Cancelled()
    {
        AppointmentStatus.Scheduled.CanTransitionTo(AppointmentStatus.Cancelled)
            .Should().BeTrue();
    }

    [Fact]
    public void Scheduled_CannotTransitionTo_InProgress()
    {
        AppointmentStatus.Scheduled.CanTransitionTo(AppointmentStatus.InProgress)
            .Should().BeFalse();
    }

    [Fact]
    public void Scheduled_CannotTransitionTo_Completed()
    {
        AppointmentStatus.Scheduled.CanTransitionTo(AppointmentStatus.Completed)
            .Should().BeFalse();
    }

    [Fact]
    public void Confirmed_CanTransitionTo_Scheduled()
    {
        AppointmentStatus.Confirmed.CanTransitionTo(AppointmentStatus.Scheduled)
            .Should().BeTrue();
    }

    [Fact]
    public void Confirmed_CanTransitionTo_InProgress()
    {
        AppointmentStatus.Confirmed.CanTransitionTo(AppointmentStatus.InProgress)
            .Should().BeTrue();
    }

    [Fact]
    public void Confirmed_CanTransitionTo_Cancelled()
    {
        AppointmentStatus.Confirmed.CanTransitionTo(AppointmentStatus.Cancelled)
            .Should().BeTrue();
    }

    [Fact]
    public void InProgress_CanTransitionTo_Completed()
    {
        AppointmentStatus.InProgress.CanTransitionTo(AppointmentStatus.Completed)
            .Should().BeTrue();
    }

    [Fact]
    public void InProgress_CanTransitionTo_NoShow()
    {
        AppointmentStatus.InProgress.CanTransitionTo(AppointmentStatus.NoShow)
            .Should().BeTrue();
    }

    [Fact]
    public void Completed_CannotTransitionToAnything()
    {
        AppointmentStatus.Completed.CanTransitionTo(AppointmentStatus.Scheduled).Should().BeFalse();
        AppointmentStatus.Completed.CanTransitionTo(AppointmentStatus.Cancelled).Should().BeFalse();
    }

    [Fact]
    public void Cancelled_CannotTransitionToAnything()
    {
        AppointmentStatus.Cancelled.CanTransitionTo(AppointmentStatus.Scheduled).Should().BeFalse();
        AppointmentStatus.Cancelled.CanTransitionTo(AppointmentStatus.Confirmed).Should().BeFalse();
    }

    [Fact]
    public void NoShow_CannotTransitionToAnything()
    {
        AppointmentStatus.NoShow.CanTransitionTo(AppointmentStatus.Scheduled).Should().BeFalse();
        AppointmentStatus.NoShow.CanTransitionTo(AppointmentStatus.Cancelled).Should().BeFalse();
    }

    [Fact]
    public void Equality_SameStatus_ShouldBeEqual()
    {
        var status1 = AppointmentStatus.FromName("Scheduled").Value;
        var status2 = AppointmentStatus.Scheduled;

        status1.Should().Be(status2);
    }

    [Fact]
    public void Equality_DifferentStatus_ShouldNotBeEqual()
    {
        AppointmentStatus.Scheduled.Should().NotBe(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        AppointmentStatus.Scheduled.ToString().Should().Be("Scheduled");
    }
}
