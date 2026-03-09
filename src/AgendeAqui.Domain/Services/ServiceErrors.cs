using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Services;

public static class ServiceErrors
{
    public static readonly Error NotFound = new("Service.NotFound", "Service not found.");
    public static readonly Error InvalidName = new("Service.InvalidName", "Service name is required.");
    public static readonly Error InvalidDuration = new("Service.InvalidDuration", "Service duration must be positive and at most 8 hours.");
    public static readonly Error InvalidPrice = new("Service.InvalidPrice", "Service price must not be negative.");
}
