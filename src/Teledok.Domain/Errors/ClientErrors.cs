using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class ClientErrors
{
    public static Error LegalEntityInnRequired() =>
        Error.Validation(
            "client.legal.entity.inn.length",
            "ИНН юридического лица должен содержать 10 цифр",
            "inn");

    public static Error IndividualEntrepreneurInnRequired() =>
        Error.Validation(
            "client.individual.entrepreneur.inn.length",
            "ИНН индивидуального предпринимателя должен содержать 12 цифр",
            "inn");

    public static Error NotFound() =>
        Error.NotFound("client.not.found", "Клиент не найден");

    public static Error TypeInvalid() =>
        Error.Validation(
            "client.type.invalid",
            "Тип клиента должен быть LEGAL_ENTITY или INDIVIDUAL_ENTREPRENEUR",
            "type");
}
