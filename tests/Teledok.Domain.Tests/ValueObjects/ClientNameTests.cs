using CSharpFunctionalExtensions;

using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Tests.ValueObjects;

public sealed class ClientNameTests
{
    [Theory]
    [InlineData("ООО «Ромашка»")]
    [InlineData("ООО «Альфа-1»")]
    [InlineData("ИП Иванов И.И.")]
    [InlineData("Рога & Копыта")]
    public void Of_WithValidName_ReturnsClientName(string raw)
    {
        Result<ClientName, Error> result = ClientName.Of(raw);

        Assert.True(result.IsSuccess);
        Assert.Equal(raw, result.Value.Value);
    }

    [Fact]
    public void Of_WithExtraSpaces_NormalizesValue()
    {
        Result<ClientName, Error> result = ClientName.Of("  ООО   «Ромашка»  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("ООО «Ромашка»", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Of_WithEmptyValue_ReturnsRequiredError(string? raw)
    {
        Result<ClientName, Error> result = ClientName.Of(raw);

        AssertErrorCode(result, "client.name.required");
    }

    [Theory]
    [InlineData("ООО\n«Ромашка»")]
    [InlineData("ООО «Ромашка»\t")]
    [InlineData("ООО\0«Ромашка»")]
    public void Of_WithControlCharacters_ReturnsInvalidCharactersError(string raw)
    {
        Result<ClientName, Error> result = ClientName.Of(raw);

        AssertErrorCode(result, "client.name.invalid.characters");
    }

    [Fact]
    public void Of_WithMaxLength_ReturnsClientName()
    {
        string raw = new('а', ClientName.MAX_LENGTH);

        Result<ClientName, Error> result = ClientName.Of(raw);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Of_WithLengthOverMax_ReturnsTooLongError()
    {
        string raw = new('а', ClientName.MAX_LENGTH + 1);

        Result<ClientName, Error> result = ClientName.Of(raw);

        AssertErrorCode(result, "client.name.too.long");
    }

    [Fact]
    public void Of_WithSameNormalizedValue_ReturnsEqualNames()
    {
        ClientName first = ClientName.Of("ООО «Ромашка»").Value;
        ClientName second = ClientName.Of("  ООО  «Ромашка» ").Value;

        Assert.Equal(first, second);
    }

    private static void AssertErrorCode(Result<ClientName, Error> result, string expectedCode)
    {
        Assert.True(result.IsFailure);
        ErrorMessage message = Assert.Single(result.Error.Messages);
        Assert.Equal(expectedCode, message.Code);
        Assert.Equal("name", message.InvalidField);
    }
}
