using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class ClientErrors
{
    public static Error LegalEntityInnRequired() =>
        Error.Validation(
            "client.legal.entity.inn.length",
            "ИНН юридического лица должен содержать 10 цифр",
            "inn");
}
