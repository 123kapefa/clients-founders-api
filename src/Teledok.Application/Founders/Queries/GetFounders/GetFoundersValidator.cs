using FluentValidation;

using Teledok.Application.Common;
using Teledok.Contracts.Founders.Requests;

namespace Teledok.Application.Founders.Queries.GetFounders;

public sealed class GetFoundersValidator : AbstractValidator<GetFoundersRequest>
{
    public const int SEARCH_MAX_LENGTH = 200;

    public GetFoundersValidator()
    {
        RuleFor(x => x.Page)
            .MustBeValidPage("founders.page.invalid")
            .When(x => x.Page is not null);

        RuleFor(x => x.PageSize)
            .MustBeValidPageSize("founders.page.size.invalid")
            .When(x => x.PageSize is not null);

        RuleFor(x => x.Search)
            .MaximumLength(SEARCH_MAX_LENGTH)
            .WithErrorCode("founders.search.too.long")
            .WithMessage($"Строка поиска не может быть длиннее {SEARCH_MAX_LENGTH} символов");
    }

}
