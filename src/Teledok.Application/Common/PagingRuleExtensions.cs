using FluentValidation;

using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Common;

internal static class PagingRuleExtensions
{
    public static IRuleBuilderOptions<T, int?> MustBeValidPage<T>(this IRuleBuilder<T, int?> rule, string errorCode) =>
        rule
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(errorCode)
            .WithMessage("Номер страницы должен быть не меньше 1");

    public static IRuleBuilderOptions<T, int?> MustBeValidPageSize<T>(this IRuleBuilder<T, int?> rule, string errorCode) =>
        rule
            .InclusiveBetween(1, PageResult.MAX_PAGE_SIZE)
            .WithErrorCode(errorCode)
            .WithMessage($"Размер страницы должен быть от 1 до {PageResult.MAX_PAGE_SIZE}");
}
