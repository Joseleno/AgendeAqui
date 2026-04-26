using Microsoft.AspNetCore.Authorization;

namespace AgendeAqui.Api.Auth;

public static class AuthorizationPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireProfessional = "RequireProfessional";
    public const string RequireClient = "RequireClient";
    public const string RequireAuthenticated = "RequireAuthenticated";
    public const string RequirePlatformOperator = "RequirePlatformOperator";

    public static AuthorizationOptions AddAgendeAquiPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(RequireAdmin, policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy(RequireProfessional, policy =>
            policy.RequireRole("Admin", "Professional", "ApiKey"));

        options.AddPolicy(RequireClient, policy =>
            policy.RequireRole("Admin", "Professional", "Client", "ApiKey"));

        options.AddPolicy(RequireAuthenticated, policy =>
            policy.RequireAuthenticatedUser());

        options.AddPolicy(RequirePlatformOperator, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim("platform_role", "reports_read", "ops_admin"));

        return options;
    }
}
