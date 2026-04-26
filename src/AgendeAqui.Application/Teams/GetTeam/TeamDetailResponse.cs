namespace AgendeAqui.Application.Teams.GetTeam;

public sealed record TeamDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? LeaderId,
    string? LeaderName,
    bool IsActive,
    DateTime CreatedAt,
    List<TeamMemberResponse> Members);

public sealed record TeamMemberResponse(
    Guid ProfessionalId,
    string ProfessionalName,
    string? Specialty,
    DateTime JoinedAt);
