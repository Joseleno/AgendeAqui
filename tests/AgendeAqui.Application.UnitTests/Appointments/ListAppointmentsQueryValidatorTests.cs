using AgendeAqui.Application.Appointments.ListAppointments;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class ListAppointmentsQueryValidatorTests
{
    private readonly ListAppointmentsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_ShouldHaveNoErrors()
    {
        var query = new ListAppointmentsQuery(1, 10, status: "Scheduled");

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveError()
    {
        var query = new ListAppointmentsQuery(1, 10, status: "Invalid");

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithNullStatus_ShouldHaveNoErrors()
    {
        var query = new ListAppointmentsQuery(1, 10);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithZeroPage_ShouldHaveError()
    {
        var query = new ListAppointmentsQuery(0, 10);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WithZeroPageSize_ShouldHaveError()
    {
        var query = new ListAppointmentsQuery(1, 0);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WithPageSizeAtMax_ShouldHaveNoErrors()
    {
        var query = new ListAppointmentsQuery(1, 50);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Scheduled")]
    [InlineData("Confirmed")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    [InlineData("NoShow")]
    public void Validate_WithAllValidStatuses_ShouldHaveNoErrors(string status)
    {
        var query = new ListAppointmentsQuery(1, 10, status: status);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
