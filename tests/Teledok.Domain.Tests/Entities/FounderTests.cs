using CSharpFunctionalExtensions;

using Teledok.Domain.Entities;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Tests.Entities;

public sealed class FounderTests
{
    private const string PERSON_INN = "500100732259";

    private const string LEGAL_ENTITY_INN = "7707083893";

    [Fact]
    public void Create_WithPersonInn_ReturnsFounder()
    {
        Inn inn = CreateInn(PERSON_INN);
        FullName fullName = CreateFullName("Иванов", "Иван", "Иванович");

        Result<Founder, Error> result = Founder.Create(inn, fullName);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(inn, result.Value.Inn);
        Assert.Equal(fullName, result.Value.FullName);
    }

    [Fact]
    public void Create_WithLegalEntityInn_ReturnsInnMustBelongToPersonError()
    {
        Inn inn = CreateInn(LEGAL_ENTITY_INN);
        FullName fullName = CreateFullName("Иванов", "Иван", null);

        Result<Founder, Error> result = Founder.Create(inn, fullName);

        Assert.True(result.IsFailure);
        ErrorMessage message = Assert.Single(result.Error.Messages);
        Assert.Equal("founder.inn.must.be.person", message.Code);
        Assert.Equal("inn", message.InvalidField);
    }

    [Fact]
    public void Create_NewFounder_IsNotDeleted()
    {
        Founder founder = CreateFounder();

        Assert.False(founder.IsDeleted);
        Assert.Null(founder.DeletedAt);
    }

    [Fact]
    public void Create_TwoFounders_HaveDifferentIds()
    {
        Founder first = CreateFounder();
        Founder second = CreateFounder();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Rename_WithNewFullName_ChangesFullNameAndKeepsInn()
    {
        Founder founder = CreateFounder();
        Inn originalInn = founder.Inn;
        FullName newFullName = CreateFullName("Петрова", "Анна", null);

        founder.Rename(newFullName);

        Assert.Equal(newFullName, founder.FullName);
        Assert.Equal(originalInn, founder.Inn);
    }

    private static Founder CreateFounder() =>
        Founder.Create(CreateInn(PERSON_INN), CreateFullName("Иванов", "Иван", null)).Value;

    private static Inn CreateInn(string value) => Inn.Of(value).Value;

    private static FullName CreateFullName(string lastName, string firstName, string? middleName) =>
        FullName.Of(lastName, firstName, middleName).Value;
}
