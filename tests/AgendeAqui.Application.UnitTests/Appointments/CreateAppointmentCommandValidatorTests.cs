using AgendeAqui.Application.Appointments.CreateAppointment;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class CreateAppointmentCommandValidatorTests
{
    private readonly CreateAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(9, 0), null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyProfessionalId_ShouldHaveError()
    {
        var command = new CreateAppointmentCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(9, 0), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProfessionalId);
    }

    [Fact]
    public void Validate_WithEmptyServiceId_ShouldHaveError()
    {
        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(9, 0), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ServiceId);
    }

    [Fact]
    public void Validate_WithEmptyClientId_ShouldHaveError()
    {
        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(9, 0), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public void Validate_WithNotesExceeding1000Chars_ShouldHaveError()
    {
        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(9, 0), new string('x', 1001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_WithPastDate_ShouldHaveError()
    {
        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), new TimeOnly(9, 0), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }
}
