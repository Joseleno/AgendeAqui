using AgendeAqui.Application.Notifications.ListNotifications;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Notifications;

public class ListNotificationsQueryValidatorTests
{
    private readonly ListNotificationsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_ShouldHaveNoErrors()
    {
        var query = new ListNotificationsQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyAppointmentId_ShouldHaveError()
    {
        var query = new ListNotificationsQuery(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}
