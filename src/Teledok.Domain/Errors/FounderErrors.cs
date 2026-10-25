using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class FounderErrors
{
    public static Error InnMustBelongToPerson() =>
        Error.Validation(
            "founder.inn.must.be.person",
            "ИНН учредителя должен принадлежать физическому лицу и содержать 12 цифр",
            "inn");

    public static Error NotFound() =>
        Error.NotFound("founder.not.found", "Учредитель не найден", "founderId");

    public static Error FullNameMismatch() =>
        Error.Conflict(
            "founder.full.name.mismatch",
            "Учредитель с этим ИНН уже существует с другим ФИО",
            "inn");

    public static Error HasActiveLinks() =>
        Error.Conflict(
            "founder.has.active.links",
            "Учредитель привязан к юридическим лицам, сначала отвяжите его");
}
