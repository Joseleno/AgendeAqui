using AgendeAqui.Application.Notifications.ListNotifications;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Notifications;

public class ListNotificationsQueryValidatorTests
{
    private readonly ListNotificationsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidAppointmentId_ShouldHaveNoErrors()
    {
        var query = new ListNotificationsQuery(AppointmentId: Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNoFilters_ShouldHaveNoErrors()
    {
        var query = new ListNotificationsQuery();
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyAppointmentId_ShouldHaveError()
    {
        var query = new ListNotificationsQuery(AppointmentId: Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_WithValidDateRange_ShouldHaveNoErrors()
    {
        var query = new ListNotificationsQuery(
            DateFrom: new DateOnly(2026, 1, 1),
            DateTo: new DateOnly(2026, 1, 31));
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidDateRange_ShouldHaveError()
    {
        var query = new ListNotificationsQuery(
            DateFrom: new DateOnly(2026, 2, 1),
            DateTo: new DateOnly(2026, 1, 1));
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.DateTo);
    }
}
