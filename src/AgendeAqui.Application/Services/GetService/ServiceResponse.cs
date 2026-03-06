namespace AgendeAqui.Application.Services.GetService;

public sealed record ServiceResponse(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    bool IsActive,
    DateTime CreatedAt);
