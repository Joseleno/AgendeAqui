using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Domain.Abstractions;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Team?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Team team, CancellationToken cancellationToken = default);
    void Update(Team team);
}
