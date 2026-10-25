using FluentValidation;

using Teledok.Contracts.Clients.Requests;

namespace Teledok.Application.Clients.Commands.AddFounder;

public sealed class AddFounderValidator : AbstractValidator<AddFounderRequest>
{
    public AddFounderValidator()
    {
        RuleFor(x => x)
            .Must(HasExactlyOneVariant)
            .WithErrorCode("founder.input.invalid")
            .WithMessage("Укажите либо founderId существующего учредителя, либо ИНН и ФИО нового");
    }

    private static bool HasExactlyOneVariant(AddFounderRequest request)
    {
        bool byId = request.FounderId is not null;
        bool byDetails = request.Inn is not null
            || request.LastName is not null
            || request.FirstName is not null
            || request.MiddleName is not null;

        return byId != byDetails;
    }
}
