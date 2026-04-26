using AgendeAqui.Domain.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(Team.MaxNameLength).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(Team.MaxDescriptionLength);
        builder.Property(t => t.LeaderId).HasColumnName("leader_id");
        builder.Property(t => t.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(t => t.Members)
            .WithOne()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Leader FK — same-tenant constraint enforced at application layer + DB compound FK
        builder.HasOne<Domain.Professionals.Professional>()
            .WithMany()
            .HasForeignKey(t => t.LeaderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("ix_teams_tenant_id");

        builder.HasIndex(t => new { t.TenantId, t.IsActive })
            .HasFilter("is_active = true")
            .HasDatabaseName("ix_teams_tenant_active");
    }
}
