using AgendeAqui.Application.Clients.ExportClientData;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Clients;

public class ExportClientDataQueryValidatorTests
{
    private readonly ExportClientDataQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidClientId_ShouldHaveNoErrors()
    {
        var query = new ExportClientDataQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyClientId_ShouldHaveError()
    {
        var query = new ExportClientDataQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }
}
