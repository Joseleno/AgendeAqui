using AgendeAqui.Application.Appointments.RescheduleAppointment;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class RescheduleAppointmentCommandValidatorTests
{
    private readonly RescheduleAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(9, 0));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyAppointmentId_ShouldHaveError()
    {
        var command = new RescheduleAppointmentCommand(
            Guid.Empty, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(9, 0));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_WithPastDate_ShouldHaveError()
    {
        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), new TimeOnly(9, 0));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewDate);
    }

    [Fact]
    public void Validate_WithDefaultNewStartTime_ShouldHaveError()
    {
        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), default);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewStartTime);
    }
}
