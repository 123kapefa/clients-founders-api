using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class FounderErrors
{
    public static Error InnMustBelongToPerson() =>
        Error.Validation(
            "founder.inn.must.be.person",
            "ИНН учредителя должен принадлежать физическому лицу и содержать 12 цифр",
            "inn");
}
