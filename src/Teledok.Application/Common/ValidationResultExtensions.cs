using FluentValidation.Results;

using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Common;

internal static class ValidationResultExtensions
{
    public static Error ToError(this ValidationResult result) =>
        Error.Validation(result.Errors.Select(failure =>
            new ErrorMessage(failure.ErrorCode, failure.ErrorMessage, ToCamelCase(failure.PropertyName))));

    private static string? ToCamelCase(string value) =>
        string.IsNullOrEmpty(value) ? null : char.ToLowerInvariant(value[0]) + value[1..];
}
