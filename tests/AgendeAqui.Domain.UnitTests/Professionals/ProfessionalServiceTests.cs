using AgendeAqui.Domain.Professionals;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Professionals;

public class ProfessionalServiceTests
{
    [Fact]
    public void Create_WithValidGuids_ShouldSucceed()
    {
        var tenantId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var result = ProfessionalService.Create(tenantId, professionalId, serviceId);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.ProfessionalId.Should().Be(professionalId);
        result.Value.ServiceId.Should().Be(serviceId);
    }

    [Fact]
    public void Create_WithEmptyProfessionalId_ShouldFail()
    {
        var result = ProfessionalService.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyServiceId_ShouldFail()
    {
        var result = ProfessionalService.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        result.IsFailure.Should().BeTrue();
    }
}
