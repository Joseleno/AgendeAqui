using AgendeAqui.Domain.Abstractions;
using System.Security.Claims;

namespace AgendeAqui.Api.Auth;

internal sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public Guid TenantId
    {
        get
        {
            var value = User?.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Role => User?.FindFirstValue(ClaimTypes.Role)
        ?? User?.FindFirstValue("role")
        ?? string.Empty;

    public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

    public bool IsProfessional => Role.Equals("Professional", StringComparison.OrdinalIgnoreCase)
        || Role.Equals("ApiKey", StringComparison.OrdinalIgnoreCase)
        || IsAdmin;

    public bool IsClient => Role.Equals("Client", StringComparison.OrdinalIgnoreCase)
        || IsProfessional;
}
