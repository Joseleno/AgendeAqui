namespace AgendeAqui.Application.Teams.ListTeams;

public sealed record TeamResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? LeaderId,
    string? LeaderName,
    bool IsActive,
    int MemberCount,
    DateTime CreatedAt);
