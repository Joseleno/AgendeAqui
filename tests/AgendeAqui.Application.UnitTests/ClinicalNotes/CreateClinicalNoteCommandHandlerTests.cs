using AgendeAqui.Application.ClinicalNotes.CreateClinicalNote;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ClinicalNotes;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.ClinicalNotes;

public class CreateClinicalNoteCommandHandlerTests
{
    private readonly IClinicalNoteRepository _clinicalNoteRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUser _currentUser;
    private readonly CreateClinicalNoteCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _professionalId = Guid.NewGuid();

    public CreateClinicalNoteCommandHandlerTests()
    {
        _clinicalNoteRepository = Substitute.For<IClinicalNoteRepository>();
        _clientRepository = Substitute.For<IClientRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.ProfessionalId.Returns(_professionalId);

        _handler = new CreateClinicalNoteCommandHandler(
            _clinicalNoteRepository,
            _clientRepository,
            _unitOfWork,
            _tenantProvider,
            _currentUser);
    }

    private Client CreateClient() =>
        Client.Create(_tenantId, "Maria", Email.Create("maria@test.com").Value, PhoneNumber.Create("5521912345678").Value).Value;

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateNoteAndReturnId()
    {
        var client = CreateClient();
        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateClinicalNoteCommand(
            client.Id, null, "Assessment", "Patient is stable.", false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _clinicalNoteRepository.Received(1).AddAsync(Arg.Any<ClinicalNote>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetTenantIdAndProfessionalId()
    {
        var client = CreateClient();
        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        ClinicalNote? capturedNote = null;
        await _clinicalNoteRepository.AddAsync(
            Arg.Do<ClinicalNote>(n => capturedNote = n),
            Arg.Any<CancellationToken>());

        var command = new CreateClinicalNoteCommand(
            client.Id, null, "Note Title", "Note Content", true);

        await _handler.Handle(command, CancellationToken.None);

        capturedNote.Should().NotBeNull();
        capturedNote!.TenantId.Should().Be(_tenantId);
        capturedNote.ProfessionalId.Should().Be(_professionalId);
        capturedNote.IsPrivate.Should().BeTrue();
    }
}
