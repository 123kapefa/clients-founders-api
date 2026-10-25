using FluentValidation;

using Teledok.Application.Clients.Commands.AddFounder;
using Teledok.Contracts.Clients.Requests;

namespace Teledok.Application.Clients.Commands.CreateClient;

public sealed class CreateClientValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientValidator()
    {
        RuleForEach(x => x.Founders)
            .NotNull()
            .WithErrorCode("founder.input.invalid")
            .WithMessage("Укажите либо founderId существующего учредителя, либо ИНН и ФИО нового")
            .SetValidator(new AddFounderValidator());

        RuleFor(x => x.Founders)
            .Must(HaveNoDuplicates)
            .WithErrorCode("client.founders.duplicated")
            .WithMessage("Один и тот же учредитель указан несколько раз");
    }

    private static bool HaveNoDuplicates(IReadOnlyList<AddFounderRequest>? founders)
    {
        if (founders is null)
        {
            return true;
        }

        List<AddFounderRequest> items = founders.Where(x => x is not null).ToList();

        List<Guid> ids = items
            .Where(x => x.FounderId is not null)
            .Select(x => x.FounderId!.Value)
            .ToList();

        List<string> inns = items
            .Where(x => x.FounderId is null && !string.IsNullOrWhiteSpace(x.Inn))
            .Select(x => x.Inn!.Trim())
            .ToList();

        return ids.Distinct().Count() == ids.Count && inns.Distinct(StringComparer.Ordinal).Count() == inns.Count;
    }
}
