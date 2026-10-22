using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Errors;

public static class InnErrors
{
    public static Error InvalidFormat() =>
        Error.Validation("inn.invalid.format", "Некорректный формат ИНН", "inn");

    public static Error InvalidLength() =>
        Error.Validation("inn.invalid.length", "ИНН должен содержать 10 или 12 цифр", "inn");

    public static Error InvalidChecksum() =>
        Error.Validation("inn.invalid.checksum", "Неверная контрольная сумма ИНН", "inn");
}
