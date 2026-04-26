using AgendeAqui.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(u => u.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
        builder.Property(u => u.ProfessionalId).HasColumnName("professional_id");
        builder.Property(u => u.ClientId).HasColumnName("client_id");
        builder.Property(u => u.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(64);
        builder.Property(u => u.RefreshTokenExpiresAt).HasColumnName("refresh_token_expires_at");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(u => new { u.TenantId, u.Email })
            .HasDatabaseName("ix_users_tenant_email")
            .IsUnique();

        builder.HasIndex(u => u.ProfessionalId)
            .HasDatabaseName("ix_users_professional_id")
            .IsUnique()
            .HasFilter("professional_id IS NOT NULL");

        builder.HasIndex(u => u.ClientId)
            .HasDatabaseName("ix_users_client_id")
            .IsUnique()
            .HasFilter("client_id IS NOT NULL");
    }
}
