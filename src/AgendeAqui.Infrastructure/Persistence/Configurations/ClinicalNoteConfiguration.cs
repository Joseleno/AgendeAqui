using AgendeAqui.Domain.ClinicalNotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.ToTable("clinical_notes");

        builder.HasKey(cn => cn.Id);

        builder.Property(cn => cn.Id)
            .HasColumnName("id");

        builder.Property(cn => cn.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(cn => cn.ProfessionalId)
            .HasColumnName("professional_id")
            .IsRequired();

        builder.Property(cn => cn.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(cn => cn.AppointmentId)
            .HasColumnName("appointment_id");

        builder.Property(cn => cn.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cn => cn.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(cn => cn.IsPrivate)
            .HasColumnName("is_private")
            .IsRequired();

        builder.Property(cn => cn.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cn => cn.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(cn => new { cn.TenantId, cn.ClientId })
            .HasDatabaseName("ix_clinical_notes_tenant_client");

        builder.HasIndex(cn => new { cn.TenantId, cn.ProfessionalId })
            .HasDatabaseName("ix_clinical_notes_tenant_professional");
    }
}
