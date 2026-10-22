using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class ClientNameErrors
{
    public static Error Required() =>
        Error.Validation(
            "client.name.required",
            "Наименование клиента обязательно",
            "name");

    public static Error TooLong() =>
        Error.Validation(
            "client.name.too.long",
            $"Наименование клиента не может быть длиннее {ClientName.MAX_LENGTH} символов",
            "name");

    public static Error InvalidCharacters() =>
        Error.Validation(
            "client.name.invalid.characters",
            "Наименование клиента не должно содержать переводов строк и других управляющих символов",
            "name");
}
