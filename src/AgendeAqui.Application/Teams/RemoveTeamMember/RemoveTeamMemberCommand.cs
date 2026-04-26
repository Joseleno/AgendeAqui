using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand(Guid TeamId, Guid ProfessionalId) : ICommand;
