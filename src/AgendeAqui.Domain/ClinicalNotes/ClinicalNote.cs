using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ClinicalNotes;

public sealed class ClinicalNote : TenantEntity
{
    public Guid ProfessionalId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public bool IsPrivate { get; private set; }

    private ClinicalNote() { }

    public static ClinicalNote Create(
        Guid tenantId,
        Guid professionalId,
        Guid clientId,
        Guid? appointmentId,
        string title,
        string content,
        bool isPrivate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new ClinicalNote
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            ClientId = clientId,
            AppointmentId = appointmentId,
            Title = title,
            Content = content,
            IsPrivate = isPrivate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string content, bool isPrivate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Title = title;
        Content = content;
        IsPrivate = isPrivate;
        UpdatedAt = DateTime.UtcNow;
    }
}
