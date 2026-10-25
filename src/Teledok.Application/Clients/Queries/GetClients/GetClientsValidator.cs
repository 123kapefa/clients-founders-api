using FluentValidation;

using Teledok.Contracts.Clients.Requests;
using Teledok.Domain.Enums;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Queries.GetClients;

public sealed class GetClientsValidator : AbstractValidator<GetClientsRequest>
{
    public const int SEARCH_MAX_LENGTH = 200;

    public GetClientsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .When(x => x.Page is not null)
            .WithErrorCode("clients.page.invalid")
            .WithMessage("Номер страницы должен быть не меньше 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PageResult.MAX_PAGE_SIZE)
            .When(x => x.PageSize is not null)
            .WithErrorCode("clients.page.size.invalid")
            .WithMessage($"Размер страницы должен быть от 1 до {PageResult.MAX_PAGE_SIZE}");

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
