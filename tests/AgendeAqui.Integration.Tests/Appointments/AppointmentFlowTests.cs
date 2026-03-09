using AgendeAqui.Integration.Tests.Fixtures;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AgendeAqui.Integration.Tests.Appointments;

public sealed class AppointmentFlowTests : IntegrationTestBase
{
    public AppointmentFlowTests(AgendeAquiWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task FullAppointmentWorkflow_ShouldSucceed()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // 1. Create a tenant
        var tenantId = await CreateTenantAsync($"Barbearia {suffix}", $"barbearia-{suffix}");
        tenantId.Should().NotBeEmpty();

        // 2. Set admin JWT scoped to this tenant
        SetJwtToken(tenantId, "Admin");

        // 3. Create professional
        var profResponse = await Client.PostAsJsonAsync("/api/v1/professionals",
            new { name = "Carlos Silva", email = $"carlos-{suffix}@test.com", phone = "5511999990001" });
        profResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            await profResponse.Content.ReadAsStringAsync());
        var professionalId = (await profResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // 4. Create service
        var svcResponse = await Client.PostAsJsonAsync("/api/v1/services",
            new { name = "Corte de Cabelo", durationMinutes = 30, price = 35.00m });
        svcResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            await svcResponse.Content.ReadAsStringAsync());
        var serviceId = (await svcResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // 5. Create client
        var clientResponse = await Client.PostAsJsonAsync("/api/v1/clients",
            new { name = "Ana Lima", email = $"ana-{suffix}@test.com", phone = "5511888880002" });
        clientResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            await clientResponse.Content.ReadAsStringAsync());
        var clientId = (await clientResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // 6. Create schedule for Monday
        var appointmentDate = GetNextWeekday(DayOfWeek.Monday);
        var scheduleResponse = await Client.PostAsJsonAsync("/api/v1/schedules",
            new
            {
                professionalId,
                dayOfWeek = (int)DayOfWeek.Monday,
                startTime = "08:00:00",
                endTime = "18:00:00",
                slotDurationMinutes = 30
            });
        scheduleResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            await scheduleResponse.Content.ReadAsStringAsync());

        // 7. Create appointment
        var apptResponse = await Client.PostAsJsonAsync("/api/v1/appointments",
            new
            {
                professionalId,
                serviceId,
                clientId,
                date = appointmentDate.ToString("yyyy-MM-dd"),
                startTime = "10:00:00",
                notes = (string?)null
            });
        apptResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            await apptResponse.Content.ReadAsStringAsync());
        var appointmentId = (await apptResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // 8. Verify appointment
        var getResponse = await Client.GetAsync($"/api/v1/appointments/{appointmentId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 9. Cancel appointment
        var cancelResponse = await Client.PostAsJsonAsync(
            $"/api/v1/appointments/{appointmentId}/cancel",
            new { reason = "Client requested cancellation" });
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            await cancelResponse.Content.ReadAsStringAsync());

        // 10. Verify appointment is now Cancelled
        var afterCancelResponse = await Client.GetAsync($"/api/v1/appointments/{appointmentId}");
        afterCancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterCancelBody = await afterCancelResponse.Content.ReadFromJsonAsync<AppointmentResponse>();
        afterCancelBody!.Status.Should().Be("Cancelled");
    }

    private static DateOnly GetNextWeekday(DayOfWeek dayOfWeek)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysToAdd = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7;
        return today.AddDays(daysToAdd);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record AppointmentResponse(Guid Id, string Status);
}
