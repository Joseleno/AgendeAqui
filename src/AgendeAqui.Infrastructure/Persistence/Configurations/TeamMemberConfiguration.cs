using AgendeAqui.Domain.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("team_members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(m => m.TeamId).HasColumnName("team_id").IsRequired();
        builder.Property(m => m.ProfessionalId).HasColumnName("professional_id").IsRequired();
        builder.Property(m => m.JoinedAt).HasColumnName("joined_at").IsRequired();
        builder.Property(m => m.LeftAt).HasColumnName("left_at");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        // Unique active membership per professional per team
        builder.HasIndex(m => new { m.TeamId, m.ProfessionalId })
            .HasFilter("left_at IS NULL")
            .IsUnique()
            .HasDatabaseName("ix_team_members_active_unique");

        // Lookup: which teams does a professional belong to?
        builder.HasIndex(m => new { m.TenantId, m.ProfessionalId })
            .HasFilter("left_at IS NULL")
            .HasDatabaseName("ix_team_members_tenant_professional");

        // Reports: aggregate appointments by team in date range
        builder.HasIndex(m => new { m.TenantId, m.TeamId, m.JoinedAt, m.LeftAt })
            .HasDatabaseName("ix_team_members_tenant_team_dates");
    }
}
