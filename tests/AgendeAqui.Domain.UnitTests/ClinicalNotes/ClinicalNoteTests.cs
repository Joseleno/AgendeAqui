using AgendeAqui.Domain.ClinicalNotes;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.ClinicalNotes;

public sealed class ClinicalNoteTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _professionalId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateNote()
    {
        var note = ClinicalNote.Create(
            _tenantId,
            _professionalId,
            _clientId,
            appointmentId: null,
            "Initial Assessment",
            "Patient presents with mild symptoms.",
            isPrivate: false);

        note.TenantId.Should().Be(_tenantId);
        note.ProfessionalId.Should().Be(_professionalId);
        note.ClientId.Should().Be(_clientId);
        note.AppointmentId.Should().BeNull();
        note.Title.Should().Be("Initial Assessment");
        note.Content.Should().Be("Patient presents with mild symptoms.");
        note.IsPrivate.Should().BeFalse();
        note.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrow()
    {
        var act = () => ClinicalNote.Create(
            _tenantId, _professionalId, _clientId,
            null, null!, "Some content", false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullContent_ShouldThrow()
    {
        var act = () => ClinicalNote.Create(
            _tenantId, _professionalId, _clientId,
            null, "Title", null!, false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        var note = ClinicalNote.Create(
            _tenantId, _professionalId, _clientId,
            null, "Original Title", "Original Content", false);

        note.Update("Updated Title", "Updated Content", true);

        note.Title.Should().Be("Updated Title");
        note.Content.Should().Be("Updated Content");
        note.IsPrivate.Should().BeTrue();
        note.UpdatedAt.Should().NotBeNull();
        note.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_WithNullTitle_ShouldThrow()
    {
        var note = ClinicalNote.Create(
            _tenantId, _professionalId, _clientId,
            null, "Title", "Content", false);

        var act = () => note.Update(null!, "New content", false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithNullContent_ShouldThrow()
    {
        var note = ClinicalNote.Create(
            _tenantId, _professionalId, _clientId,
            null, "Title", "Content", false);

        var act = () => note.Update("New title", null!, false);

        act.Should().Throw<ArgumentException>();
    }
}
