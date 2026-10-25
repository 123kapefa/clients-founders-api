using FluentValidation;

using Teledok.Application.Common;
using Teledok.Contracts.Clients.Requests;
using Teledok.Domain.Enums;

namespace Teledok.Application.Clients.Queries.GetClients;

public sealed class GetClientsValidator : AbstractValidator<GetClientsRequest>
{
    public const int SEARCH_MAX_LENGTH = 200;

    public GetClientsValidator()
    {
        RuleFor(x => x.Page)
            .MustBeValidPage("clients.page.invalid")
            .When(x => x.Page is not null);

        RuleFor(x => x.PageSize)
            .MustBeValidPageSize("clients.page.size.invalid")
            .When(x => x.PageSize is not null);

        RuleFor(x => x.Type)
            .Must(type => Enum.TryParse<ClientType>(type, ignoreCase: false, out _) && !int.TryParse(type, out _))
            .When(x => x.Type is not null)
            .WithErrorCode("client.type.invalid")
            .WithMessage("Тип клиента должен быть LEGAL_ENTITY или INDIVIDUAL_ENTREPRENEUR");

        RuleFor(x => x.Search)
            .MaximumLength(SEARCH_MAX_LENGTH)
            .WithErrorCode("clients.search.too.long")
            .WithMessage($"Строка поиска не может быть длиннее {SEARCH_MAX_LENGTH} символов");
    }
}
