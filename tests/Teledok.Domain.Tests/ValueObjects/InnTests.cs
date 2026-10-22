using CSharpFunctionalExtensions;

using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Tests.ValueObjects;

public sealed class InnTests
{
    [Theory]
    [InlineData("7707083893")]
    [InlineData("7830002293")]
    [InlineData("7736207543")]
    [InlineData("500100732259")]
    public void Of_WithValidInn_ReturnsInn(string raw)
    {
        Result<Inn, Error> result = Inn.Of(raw);

        Assert.True(result.IsSuccess);
        Assert.Equal(raw, result.Value.Value);
    }

    [Fact]
    public void Of_WithSurroundingWhitespace_ReturnsTrimmedInn()
    {
        Result<Inn, Error> result = Inn.Of(" 7707083893 ");

        Assert.True(result.IsSuccess);
        Assert.Equal("7707083893", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Of_WithEmptyValue_ReturnsRequiredError(string? raw)
    {
        Result<Inn, Error> result = Inn.Of(raw);

        AssertErrorCode(result, "value.is.required");
    }

    [Theory]
    [InlineData("77O7083893")]
    [InlineData("7707-083893")]
    [InlineData("770708389a")]
    public void Of_WithNonDigitCharacters_ReturnsFormatError(string raw)
    {
        Result<Inn, Error> result = Inn.Of(raw);

        AssertErrorCode(result, "inn.invalid.format");
    }

    [Theory]
    [InlineData("77070838")]
    [InlineData("77070838931")]
    [InlineData("7707083893123")]
    public void Of_WithWrongLength_ReturnsLengthError(string raw)
    {
        Result<Inn, Error> result = Inn.Of(raw);

        AssertErrorCode(result, "inn.invalid.length");
    }

    [Theory]
    [InlineData("7707083894")]
    [InlineData("500100732258")]
    [InlineData("500100732249")]
    public void Of_WithInvalidChecksum_ReturnsChecksumError(string raw)
    {
        Result<Inn, Error> result = Inn.Of(raw);

        AssertErrorCode(result, "inn.invalid.checksum");
    }

    [Fact]
    public void Of_WithTenDigits_IsLegalEntity()
    {
        Inn inn = Inn.Of("7707083893").Value;

        Assert.True(inn.IsLegalEntity);
        Assert.False(inn.IsPerson);
    }

    [Fact]
    public void Of_WithTwelveDigits_IsPerson()
    {
        Inn inn = Inn.Of("500100732259").Value;

        Assert.True(inn.IsPerson);
        Assert.False(inn.IsLegalEntity);
    }

    [Fact]
    public void Of_WithSameValue_ReturnsEqualInns()
    {
        Inn first = Inn.Of("7707083893").Value;
        Inn second = Inn.Of(" 7707083893 ").Value;

        Assert.Equal(first, second);
    }

    private static void AssertErrorCode(Result<Inn, Error> result, string expectedCode)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Messages[0].Code);
    }
}
