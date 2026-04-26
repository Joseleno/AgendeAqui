using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Teams;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class TeamRepository : ITeamRepository
{
    private readonly ApplicationDbContext _context;

    public TeamRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Teams
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Team?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default) =>
        await _context.Teams.AddAsync(team, cancellationToken);

    public void Update(Team team) =>
        _context.Teams.Update(team);
}
