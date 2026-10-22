using CSharpFunctionalExtensions;

using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Tests.ValueObjects;

public sealed class FullNameTests
{
    [Fact]
    public void Of_WithAllParts_ReturnsFullName()
    {
        Result<FullName, Error> result = FullName.Of("Иванов", "Иван", "Иванович");

        Assert.True(result.IsSuccess);
        Assert.Equal("Иванов", result.Value.LastName);
        Assert.Equal("Иван", result.Value.FirstName);
        Assert.Equal("Иванович", result.Value.MiddleName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Of_WithoutMiddleName_SetsMiddleNameToNull(string? middleName)
    {
        Result<FullName, Error> result = FullName.Of("Иванов", "Иван", middleName);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.MiddleName);
    }

    [Theory]
    [InlineData("Римская-Корсакова", "Анна")]
    [InlineData("O'Neil", "Shaquille")]
    [InlineData("O’Neil", "Shaquille")]
    [InlineData("Müller", "Jürgen")]
    [InlineData("де ла Круз", "Пенелопа")]
    public void Of_WithAllowedCharacters_KeepsValueAsIs(string lastName, string firstName)
    {
        Result<FullName, Error> result = FullName.Of(lastName, firstName, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(lastName, result.Value.LastName);
        Assert.Equal(firstName, result.Value.FirstName);
    }

    [Fact]
    public void Of_WithExtraSpaces_NormalizesParts()
    {
        Result<FullName, Error> result = FullName.Of("  Иванов  ", " Анна   Мария ", "  Ивановна  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Иванов", result.Value.LastName);
        Assert.Equal("Анна Мария", result.Value.FirstName);
        Assert.Equal("Ивановна", result.Value.MiddleName);
    }

    [Theory]
    [InlineData(null, "Иван", "lastName")]
    [InlineData("", "Иван", "lastName")]
    [InlineData("Иванов", "   ", "firstName")]
    public void Of_WithMissingRequiredPart_ReturnsRequiredError(string? lastName, string? firstName, string field)
    {
        Result<FullName, Error> result = FullName.Of(lastName, firstName, null);

        AssertSingleError(result, "value.is.required", field);
    }

    [Fact]
    public void Of_WithSeveralInvalidParts_ReturnsAllErrors()
    {
        Result<FullName, Error> result = FullName.Of("", "Иван1", "Петр0вич");

        Assert.True(result.IsFailure);
        Assert.Collection(
            result.Error.Messages,
            message => AssertMessage(message, "value.is.required", "lastName"),
            message => AssertMessage(message, "full.name.invalid.characters", "firstName"),
            message => AssertMessage(message, "full.name.invalid.characters", "middleName"));
    }

    [Theory]
    [InlineData("Иванов1")]
    [InlineData("Иванов@")]
    [InlineData("-")]
    [InlineData(" - ")]
    [InlineData("-Иванов")]
    [InlineData("Иванов'")]
    public void Of_WithInvalidCharacters_ReturnsInvalidCharactersError(string lastName)
    {
        Result<FullName, Error> result = FullName.Of(lastName, "Иван", null);

        AssertSingleError(result, "full.name.invalid.characters", "lastName");
    }

    [Fact]
    public void Of_WithPartAtMaxLength_ReturnsFullName()
    {
        string lastName = new('а', FullName.PART_MAX_LENGTH);

        Result<FullName, Error> result = FullName.Of(lastName, "Иван", null);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Of_WithPartOverMaxLength_ReturnsLengthError()
    {
        string lastName = new('а', FullName.PART_MAX_LENGTH + 1);

        Result<FullName, Error> result = FullName.Of(lastName, "Иван", null);

        AssertSingleError(result, "length.is.invalid", "lastName");
    }

    [Theory]
    [InlineData("Иванович", "Иванов Иван Иванович")]
    [InlineData(null, "Иванов Иван")]
    public void ToString_ReturnsLastFirstMiddle(string? middleName, string expected)
    {
        FullName fullName = FullName.Of("Иванов", "Иван", middleName).Value;

        Assert.Equal(expected, fullName.ToString());
    }

    [Fact]
    public void Of_WithSameNormalizedValues_ReturnsEqualFullNames()
    {
        FullName first = FullName.Of("Иванов", "Иван", null).Value;
        FullName second = FullName.Of(" Иванов ", "Иван", "   ").Value;

        Assert.Equal(first, second);
    }

    private static void AssertSingleError(Result<FullName, Error> result, string expectedCode, string expectedField)
    {
        Assert.True(result.IsFailure);
        ErrorMessage message = Assert.Single(result.Error.Messages);
        AssertMessage(message, expectedCode, expectedField);
    }

    private static void AssertMessage(ErrorMessage message, string expectedCode, string expectedField)
    {
        Assert.Equal(expectedCode, message.Code);
        Assert.Equal(expectedField, message.InvalidField);
    }
}
