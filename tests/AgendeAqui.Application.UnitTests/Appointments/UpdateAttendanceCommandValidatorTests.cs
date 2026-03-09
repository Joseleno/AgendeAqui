using AgendeAqui.Application.Appointments.UpdateAttendance;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class UpdateAttendanceCommandValidatorTests
{
    private readonly UpdateAttendanceCommandValidator _validator = new();

    [Theory]
    [InlineData(AttendanceAction.Confirm)]
    [InlineData(AttendanceAction.Start)]
    [InlineData(AttendanceAction.Complete)]
    [InlineData(AttendanceAction.NoShow)]
    public void Validate_WithValidAction_ShouldHaveNoErrors(AttendanceAction action)
    {
        var command = new UpdateAttendanceCommand(Guid.NewGuid(), action);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateAttendanceCommand(Guid.Empty, AttendanceAction.Confirm);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_WithInvalidEnumValue_ShouldHaveError()
    {
        var command = new UpdateAttendanceCommand(Guid.NewGuid(), (AttendanceAction)999);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Action);
    }
}
