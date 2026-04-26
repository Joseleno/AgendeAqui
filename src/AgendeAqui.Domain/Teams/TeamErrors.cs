using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Teams;

public static class TeamErrors
{
    public static readonly Error NotFound = new("Team.NotFound", "Team not found.");
    public static readonly Error InvalidName = new("Team.InvalidName", "Team name is required.");
    public static readonly Error NameTooLong = new("Team.NameTooLong", $"Team name must be at most {Team.MaxNameLength} characters.");
    public static readonly Error LeaderMustBeMember = new("Team.LeaderMustBeMember", "The leader must be an active member of the team.");
    public static readonly Error CannotRemoveLeader = new("Team.CannotRemoveLeader", "Cannot remove the team leader. Assign a new leader first or clear the leader.");
    public static readonly Error MemberAlreadyExists = new("Team.MemberAlreadyExists", "This professional is already an active member of the team.");
    public static readonly Error MemberNotFound = new("Team.MemberNotFound", "This professional is not an active member of the team.");
    public static readonly Error CrossTenantMember = new("Team.CrossTenantMember", "Cannot add a professional from a different tenant.");
}
