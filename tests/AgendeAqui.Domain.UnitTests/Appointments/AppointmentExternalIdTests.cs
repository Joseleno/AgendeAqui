using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Appointments;

public class AppointmentExternalIdTests
{
    [Fact]
    public void SetExternalId_ShouldSetValueAndUpdateTimestamp()
    {
        var timeSlot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;
        var appointment = Appointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today), timeSlot).Value;

        appointment.SetExternalId("partner-system-123");

        appointment.ExternalId.Should().Be("partner-system-123");
        appointment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void NewAppointment_ShouldHaveNullExternalId()
    {
        var timeSlot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;
        var appointment = Appointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today), timeSlot).Value;

        appointment.ExternalId.Should().BeNull();
    }
}
