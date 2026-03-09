using AgendeAqui.Domain.Webhooks;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Webhooks;

public class WebhookDeliveryTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var tenantId = Guid.NewGuid();
        var webhookId = Guid.NewGuid();
        const string eventType = "appointment.created";
        const string payload = """{"id":"abc"}""";

        var result = WebhookDelivery.Create(tenantId, webhookId, eventType, payload);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.WebhookId.Should().Be(webhookId);
        result.Value.EventType.Should().Be(eventType);
        result.Value.Payload.Should().Be(payload);
        result.Value.Attempt.Should().Be(1);
        result.Value.ResponseCode.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = WebhookDelivery.Create(Guid.Empty, Guid.NewGuid(), "appointment.created", "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidTenant);
    }

    [Fact]
    public void Create_WithEmptyWebhookId_ShouldFail()
    {
        var result = WebhookDelivery.Create(Guid.NewGuid(), Guid.Empty, "appointment.created", "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidWebhookId);
    }

    [Fact]
    public void Create_WithEmptyEventType_ShouldFail()
    {
        var result = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "", "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidEvents);
    }

    [Fact]
    public void Create_WithWhitespaceEventType_ShouldFail()
    {
        var result = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "   ", "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidEvents);
    }

    [Fact]
    public void Create_WithEmptyPayload_ShouldFail()
    {
        var result = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidPayload);
    }

    [Fact]
    public void Create_WithWhitespacePayload_ShouldFail()
    {
        var result = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidPayload);
    }

    [Fact]
    public void RecordResult_ShouldSetResponseCode()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.RecordResult(200);

        delivery.ResponseCode.Should().Be(200);
    }

    [Fact]
    public void RecordResult_ShouldUpdateUpdatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.RecordResult(201);

        delivery.UpdatedAt.Should().NotBeNull();
        delivery.UpdatedAt!.Value.Should().BeAfter(before);
    }

    [Fact]
    public void RecordFailure_ShouldIncrementAttempt()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;
        delivery.Attempt.Should().Be(1);

        delivery.RecordFailure();

        delivery.Attempt.Should().Be(2);
    }

    [Fact]
    public void RecordFailure_ShouldUpdateUpdatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.RecordFailure();

        delivery.UpdatedAt.Should().NotBeNull();
        delivery.UpdatedAt!.Value.Should().BeAfter(before);
    }

    [Fact]
    public void HasExceededMaxAttempts_WhenBelowMax_ShouldBeFalse()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.HasExceededMaxAttempts.Should().BeFalse();
    }

    [Fact]
    public void HasExceededMaxAttempts_WhenOneBelow_ShouldBeFalse()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        for (var i = 1; i < WebhookDelivery.MaxAttempts - 1; i++)
            delivery.RecordFailure();

        delivery.HasExceededMaxAttempts.Should().BeFalse();
    }

    [Fact]
    public void HasExceededMaxAttempts_WhenAtMax_ShouldBeTrue()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        for (var i = 1; i < WebhookDelivery.MaxAttempts; i++)
            delivery.RecordFailure();

        delivery.HasExceededMaxAttempts.Should().BeTrue();
    }

    [Fact]
    public void IsSuccess_WithSuccessCode_ShouldBeTrue()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.RecordResult(200);

        delivery.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void IsSuccess_With201_ShouldBeTrue()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.RecordResult(201);

        delivery.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void IsSuccess_With500_ShouldBeFalse()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.RecordResult(500);

        delivery.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void IsSuccess_WithNoResponseCode_ShouldBeFalse()
    {
        var delivery = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", "{}").Value;

        delivery.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithPayloadExceedingMaxLength_ShouldFail()
    {
        var largePayload = new string('x', WebhookDelivery.MaxPayloadLength + 1);

        var result = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", largePayload);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.PayloadTooLarge);
    }

    [Fact]
    public void Create_WithPayloadAtMaxLength_ShouldSucceed()
    {
        var payload = new string('x', WebhookDelivery.MaxPayloadLength);

        var result = WebhookDelivery.Create(Guid.NewGuid(), Guid.NewGuid(), "appointment.created", payload);

        result.IsSuccess.Should().BeTrue();
    }
}
