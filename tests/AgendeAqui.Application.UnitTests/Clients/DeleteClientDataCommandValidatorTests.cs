using AgendeAqui.Application.Clients.DeleteClientData;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Clients;

public class DeleteClientDataCommandValidatorTests
{
    private readonly DeleteClientDataCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidClientId_ShouldHaveNoErrors()
    {
        var command = new DeleteClientDataCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyClientId_ShouldHaveError()
    {
        var command = new DeleteClientDataCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }
}
