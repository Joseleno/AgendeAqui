using AgendeAqui.Domain.DataProtection;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.DataProtection;

public class DataDeletionRequestTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var result = DataDeletionRequest.Create(tenantId, clientId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = DataDeletionRequest.Create(Guid.Empty, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DataDeletionRequestErrors.InvalidTenant);
    }

    [Fact]
    public void Create_WithEmptyClientId_ShouldFail()
    {
        var result = DataDeletionRequest.Create(Guid.NewGuid(), Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DataDeletionRequestErrors.InvalidClient);
    }

    [Fact]
    public void Create_ShouldSetStatusToPending()
    {
        var result = DataDeletionRequest.Create(Guid.NewGuid(), Guid.NewGuid());

        result.Value.Status.Should().Be(DeletionRequestStatus.Pending);
    }

    [Fact]
    public void Create_ShouldSetRequestedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var result = DataDeletionRequest.Create(Guid.NewGuid(), Guid.NewGuid());

        result.Value.RequestedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted()
    {
        var request = DataDeletionRequest.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        request.Complete();

        request.Status.Should().Be(DeletionRequestStatus.Completed);
    }

    [Fact]
    public void Complete_ShouldSetCompletedAt()
    {
        var request = DataDeletionRequest.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var before = DateTime.UtcNow.AddSeconds(-1);

        request.Complete();

        request.CompletedAt.Should().NotBeNull();
        request.CompletedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Complete_ShouldUpdateUpdatedAt()
    {
        var request = DataDeletionRequest.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var before = DateTime.UtcNow.AddSeconds(-1);

        request.Complete();

        request.UpdatedAt.Should().NotBeNull();
        request.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Complete_CalledTwice_ShouldBeIdempotent()
    {
        var request = DataDeletionRequest.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        request.Complete();
        var firstCompletedAt = request.CompletedAt;
        var firstUpdatedAt = request.UpdatedAt;

        request.Complete();

        request.Status.Should().Be(DeletionRequestStatus.Completed);
        request.CompletedAt.Should().Be(firstCompletedAt);
        request.UpdatedAt.Should().Be(firstUpdatedAt);
    }
}
