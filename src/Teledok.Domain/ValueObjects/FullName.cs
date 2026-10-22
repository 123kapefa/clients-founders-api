using CSharpFunctionalExtensions;

using Teledok.Domain.Errors;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.ValueObjects;

public sealed record FullName
{
    public const int PART_MAX_LENGTH = 100;

    public string FirstName { get; }

    public string LastName { get; }

    public string? MiddleName { get; } = null!;

    private FullName(string lastName, string firstName, string? middleName)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    public static Result<FullName, Error> Of(string? lastName, string? firstName, string? middleName)
    {
        List<ErrorMessage> errors = [];

        string? lastNameValidated = ValidatePart(lastName, "lastName", isRequired: true, errors);
        string? firstNameValidated = ValidatePart(firstName, "firstName", isRequired: true, errors);
        string? middleNameValidated = ValidatePart(middleName, "middleName", isRequired: false, errors);

        if (errors.Count > 0)
        {
            return Error.Validation(errors);
        }

        return new FullName(lastNameValidated!, firstNameValidated!, middleNameValidated);
    }

    public override string ToString() =>
        MiddleName is null
            ? $"{LastName} {FirstName}"
            : $"{LastName} {FirstName} {MiddleName}";

    private static string? ValidatePart(string? raw, string fieldName, bool isRequired, List<ErrorMessage> errors)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (isRequired)
            {
                errors.AddRange(GeneralErrors.ValueIsRequired(fieldName).Messages);
            }

            return null;
        }

        string value = NormalizeSpaces(raw);

        if (value.Length > PART_MAX_LENGTH)
        {
            errors.AddRange(GeneralErrors.LengthIsInvalid(fieldName, max: PART_MAX_LENGTH).Messages);
            return null;
        }

        if (!HasValidCharacters(value))
        {
            errors.AddRange(FullNameErrors.InvalidCharacters(fieldName).Messages);
            return null;
        }

        return value;
    }

    private static bool HasValidCharacters(string value) =>
        char.IsLetter(value[0])
        && char.IsLetter(value[^1])
        && value.All(c => char.IsLetter(c) || c is '-' or ' ' or '\'' or '’');

    private static string NormalizeSpaces(string value) =>
        string.Join(' ', value.Split(' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

}
