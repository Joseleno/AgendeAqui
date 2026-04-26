using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.AddTeamMember;

public sealed record AddTeamMemberCommand(Guid TeamId, Guid ProfessionalId) : ICommand;
