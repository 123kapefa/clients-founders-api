using CSharpFunctionalExtensions;

using Teledok.Domain.Errors;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.ValueObjects;

public sealed record ClientName
{
    public const int MAX_LENGTH = 500;

    public string Value { get; }

    private ClientName(string value)
    {
        Value = value;
    }

    public static Result<ClientName, Error> Of(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return ClientNameErrors.Required();
        }

        if (raw.Any(char.IsControl))
        {
            return ClientNameErrors.InvalidCharacters();
        }

        string value = NormalizeSpaces(raw);

        if (value.Length > MAX_LENGTH)
        {
            return ClientNameErrors.TooLong();
        }

        return new ClientName(value);
    }

    private static string NormalizeSpaces(string value) =>
        string.Join(' ', value.Split(' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    public override string ToString() => Value;
}
