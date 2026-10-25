using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class LegalEntityErrors
{
    public static Error FounderAlreadyAdded() =>
        Error.Conflict(
            "legal.entity.founder.already.added",
            "Учредитель уже добавлен к юридическому лицу",
            "founderId");

    public static Error FounderNotFound() =>
        Error.NotFound(
            "legal.entity.founder.not.found",
            "Учредитель не привязан к юридическому лицу",
            "founderId");
}
