using AgendeAqui.Application.ClinicalNotes.DeleteClinicalNote;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ClinicalNotes;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.ClinicalNotes;

public class DeleteClinicalNoteCommandHandlerTests
{
    private readonly IClinicalNoteRepository _clinicalNoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly DeleteClinicalNoteCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _professionalId = Guid.NewGuid();

    public DeleteClinicalNoteCommandHandlerTests()
    {
        _clinicalNoteRepository = Substitute.For<IClinicalNoteRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.ProfessionalId.Returns(_professionalId);

        _handler = new DeleteClinicalNoteCommandHandler(
            _clinicalNoteRepository,
            _unitOfWork,
            _currentUser);
    }

    [Fact]
    public async Task Handle_ExistingNote_OwnedByUser_ShouldDelete()
    {
        var note = ClinicalNote.Create(
            _tenantId, _professionalId, Guid.NewGuid(),
            null, "Title", "Content", false);

        _clinicalNoteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new DeleteClinicalNoteCommand(note.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _clinicalNoteRepository.Received(1).Remove(note);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnError()
    {
        _clinicalNoteRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ClinicalNote?)null);

        var command = new DeleteClinicalNoteCommand(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClinicalNoteErrors.NotFound);
    }

    [Fact]
    public async Task Handle_NotOwner_NotAdmin_ShouldReturnUnauthorized()
    {
        var otherProfessionalId = Guid.NewGuid();
        var note = ClinicalNote.Create(
            _tenantId, otherProfessionalId, Guid.NewGuid(),
            null, "Title", "Content", false);

        _clinicalNoteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var command = new DeleteClinicalNoteCommand(note.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClinicalNoteErrors.NotAuthorized);
    }
}
