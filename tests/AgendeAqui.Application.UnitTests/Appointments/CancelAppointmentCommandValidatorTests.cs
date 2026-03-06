using AgendeAqui.Application.Appointments.CancelAppointment;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class CancelAppointmentCommandValidatorTests
{
    private readonly CancelAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CancelAppointmentCommand(Guid.NewGuid(), "Patient request");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new CancelAppointmentCommand(Guid.Empty, "Reason");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyReason_ShouldHaveError(string? reason)
    {
        var command = new CancelAppointmentCommand(Guid.NewGuid(), reason!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_WithReasonExceeding500Chars_ShouldHaveError()
    {
        var command = new CancelAppointmentCommand(Guid.NewGuid(), new string('x', 501));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
