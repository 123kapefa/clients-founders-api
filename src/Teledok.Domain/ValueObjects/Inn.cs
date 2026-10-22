using CSharpFunctionalExtensions;

using Teledok.Domain.Errors;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.ValueObjects;

public sealed record Inn
{
    public const int LEGAL_ENTITY_LENGTH = 10;

    public const int PERSON_LENGTH = 12;

    private static readonly int[] _legalEntityWeights = [2, 4, 10, 3, 5, 9, 4, 6, 8];

    private static readonly int[] _personFirstWeights = [7, 2, 4, 10, 3, 5, 9, 4, 6, 8];

    private static readonly int[] _personSecondWeights = [3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8];

    public string Value { get; }

    public bool IsLegalEntity => Value.Length == LEGAL_ENTITY_LENGTH;

    public bool IsPerson => Value.Length == PERSON_LENGTH;

    private Inn(string value)
    {
        Value = value;
    }

    public static Result<Inn, Error> Of(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return GeneralErrors.ValueIsRequired("inn");
        }

        string value = raw.Trim();

        if (!value.All(char.IsAsciiDigit))
        {
            return InnErrors.InvalidFormat();
        }

        if (value.Length is not (LEGAL_ENTITY_LENGTH or PERSON_LENGTH))
        {
            return InnErrors.InvalidLength();
        }

        if (!HasValidChecksum(value))
        {
            return InnErrors.InvalidChecksum();
        }

        return new Inn(value);
    }

    public override string ToString() => Value;

    private static bool HasValidChecksum(string value) =>
        value.Length == LEGAL_ENTITY_LENGTH
            ? ControlDigit(value, _legalEntityWeights) == Digit(value, 9)
            : ControlDigit(value, _personFirstWeights) == Digit(value, 10)
              && ControlDigit(value, _personSecondWeights) == Digit(value, 11);

    private static int ControlDigit(string value, int[] weights)
    {
        int sum = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i] * Digit(value, i);
        }

        return sum % 11 % 10;
    }

    private static int Digit(string value, int index) => value[index] - '0';
}
