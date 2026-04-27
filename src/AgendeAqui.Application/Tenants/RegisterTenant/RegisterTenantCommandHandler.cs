using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;
using AgendeAqui.Domain.Users;

namespace AgendeAqui.Application.Tenants.RegisterTenant;

internal sealed class RegisterTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterTenantCommand, RegisterTenantResult>
{
    public async ValueTask<Result<RegisterTenantResult>> Handle(
        RegisterTenantCommand command,
        CancellationToken cancellationToken)
    {
        if (await tenantRepository.GetBySlugAsync(command.Slug, cancellationToken) is not null)
            return Result.Failure<RegisterTenantResult>(TenantErrors.SlugAlreadyExists);

        var emailLower = command.AdminEmail.ToLowerInvariant();
        if (await userRepository.ExistsByEmailAsync(emailLower, cancellationToken))
            return Result.Failure<RegisterTenantResult>(UserErrors.EmailAlreadyExists);

        var tenant = Tenant.CreateWithTrial(command.Name, command.Slug, trialDays: 14);
        await tenantRepository.AddAsync(tenant, cancellationToken);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.AdminPassword);
        var userResult = User.Create(tenant.Id, emailLower, passwordHash, command.AdminName, "Admin");
        if (userResult.IsFailure)
            return Result.Failure<RegisterTenantResult>(userResult.Error);

        await userRepository.AddAsync(userResult.Value, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            // Concurrent registration with same slug or email won the race
            return Result.Failure<RegisterTenantResult>(TenantErrors.SlugAlreadyExists);
        }

        return Result.Success(new RegisterTenantResult(
            tenant.Id,
            userResult.Value.Id,
            tenant.TrialEndsAt!.Value));
    }

    // Postgres unique violation SQLSTATE is 23505 — check without a hard EF Core / Npgsql dependency
    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        var current = ex;
        while (current is not null)
        {
            if (current.GetType().Name is "PostgresException" or "UniqueConstraintException"
                && current.Message.Contains("23505", StringComparison.Ordinal))
                return true;
            current = current.InnerException;
        }
        return false;
    }
}
