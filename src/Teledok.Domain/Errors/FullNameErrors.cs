using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class FullNameErrors
{
    public static Error InvalidCharacters(string fieldName) =>
        Error.Validation(
            "full.name.invalid.characters",
            "Допустимы только буквы, дефис, пробел и апостроф; значение должно начинаться и заканчиваться буквой",
            fieldName);
}
