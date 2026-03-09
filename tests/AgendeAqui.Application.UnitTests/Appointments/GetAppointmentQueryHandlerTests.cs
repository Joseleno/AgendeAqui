using AgendeAqui.Application.Appointments.GetAppointment;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Appointments;

// Note: GetAppointmentQueryHandler.Handle() uses Dapper with DbConnection which cannot be
// unit-tested without a real database. Handler logic is covered by integration tests.
// These tests verify the query record and response DTO contracts.
public class GetAppointmentQueryHandlerTests
{
    [Fact]
    public void GetAppointmentQuery_ShouldStoreAppointmentId()
    {
        var appointmentId = Guid.NewGuid();
        var query = new GetAppointmentQuery(appointmentId);

        query.AppointmentId.Should().Be(appointmentId);
    }

    [Fact]
    public void AppointmentResponse_ShouldHaveExpectedProperties()
    {
        var id = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var response = new AppointmentResponse(
            id, professionalId, "Maria Silva",
            serviceId, "Corte de Cabelo",
            clientId, "Joao Santos",
            new DateOnly(2026, 3, 10),
            new TimeOnly(14, 0),
            new TimeOnly(14, 30),
            "Confirmed", "Primeira visita",
            createdAt);

        response.Id.Should().Be(id);
        response.ProfessionalId.Should().Be(professionalId);
        response.ProfessionalName.Should().Be("Maria Silva");
        response.ServiceId.Should().Be(serviceId);
        response.ServiceName.Should().Be("Corte de Cabelo");
        response.ClientId.Should().Be(clientId);
        response.ClientName.Should().Be("Joao Santos");
        response.Date.Should().Be(new DateOnly(2026, 3, 10));
        response.StartTime.Should().Be(new TimeOnly(14, 0));
        response.EndTime.Should().Be(new TimeOnly(14, 30));
        response.Status.Should().Be("Confirmed");
        response.Notes.Should().Be("Primeira visita");
        response.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void AppointmentResponse_WithNullNotes_ShouldAllowNull()
    {
        var response = new AppointmentResponse(
            Guid.NewGuid(), Guid.NewGuid(), "Pro",
            Guid.NewGuid(), "Service",
            Guid.NewGuid(), "Client",
            new DateOnly(2026, 3, 10),
            new TimeOnly(9, 0),
            new TimeOnly(9, 30),
            "Scheduled", null,
            DateTime.UtcNow);

        response.Notes.Should().BeNull();
    }
}
