using CSharpFunctionalExtensions;

using Teledok.Domain.Entities;
using Teledok.Domain.Enums;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Tests.Entities;

public sealed class ClientTests
{
    private const string LEGAL_ENTITY_INN = "7707083893";

    private const string PERSON_INN = "500100732259";

    [Fact]
    public void LegalEntityCreate_WithLegalEntityInn_ReturnsLegalEntity()
    {
        Inn inn = CreateInn(LEGAL_ENTITY_INN);
        ClientName name = CreateName("ООО «Ромашка»");

        Result<LegalEntity, Error> result = LegalEntity.Create(inn, name);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(ClientType.LEGAL_ENTITY, result.Value.Type);
        Assert.Equal(inn, result.Value.Inn);
        Assert.Equal(name, result.Value.Name);
    }

    [Fact]
    public void LegalEntityCreate_WithPersonInn_ReturnsLegalEntityInnError()
    {
        Result<LegalEntity, Error> result = LegalEntity.Create(CreateInn(PERSON_INN), CreateName("ООО «Ромашка»"));

        AssertSingleError(result, "client.legal.entity.inn.length");
    }

    [Fact]
    public void LegalEntityCreate_NewLegalEntity_IsNotDeleted()
    {
        LegalEntity legalEntity = CreateLegalEntity();

        Assert.False(legalEntity.IsDeleted);
        Assert.Null(legalEntity.DeletedAt);
    }

    [Fact]
    public void LegalEntityCreate_TwoLegalEntities_HaveDifferentIds()
    {
        LegalEntity first = CreateLegalEntity();
        LegalEntity second = CreateLegalEntity();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void LegalEntityRename_WithNewName_ChangesNameAndKeepsInnAndType()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Inn originalInn = legalEntity.Inn;
        ClientName newName = CreateName("ООО «Лютик»");

        legalEntity.Rename(newName);

        Assert.Equal(newName, legalEntity.Name);
        Assert.Equal(originalInn, legalEntity.Inn);
        Assert.Equal(ClientType.LEGAL_ENTITY, legalEntity.Type);
    }

    [Fact]
    public void IndividualEntrepreneurCreate_WithPersonInn_ReturnsIndividualEntrepreneur()
    {
        Inn inn = CreateInn(PERSON_INN);
        ClientName name = CreateName("ИП Иванов И.И.");

        Result<IndividualEntrepreneur, Error> result = IndividualEntrepreneur.Create(inn, name);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(ClientType.INDIVIDUAL_ENTREPRENEUR, result.Value.Type);
        Assert.Equal(inn, result.Value.Inn);
        Assert.Equal(name, result.Value.Name);
    }

    [Fact]
    public void IndividualEntrepreneurCreate_WithLegalEntityInn_ReturnsIndividualEntrepreneurInnError()
    {
        Result<IndividualEntrepreneur, Error> result =
            IndividualEntrepreneur.Create(CreateInn(LEGAL_ENTITY_INN), CreateName("ИП Иванов И.И."));

        AssertSingleError(result, "client.individual.entrepreneur.inn.length");
    }

    [Fact]
    public void IndividualEntrepreneurCreate_NewIndividualEntrepreneur_IsNotDeleted()
    {
        IndividualEntrepreneur entrepreneur = CreateIndividualEntrepreneur();

        Assert.False(entrepreneur.IsDeleted);
        Assert.Null(entrepreneur.DeletedAt);
    }

    [Fact]
    public void IndividualEntrepreneurRename_WithNewName_ChangesNameAndKeepsInnAndType()
    {
        IndividualEntrepreneur entrepreneur = CreateIndividualEntrepreneur();
        Inn originalInn = entrepreneur.Inn;
        ClientName newName = CreateName("ИП Петров П.П.");

        entrepreneur.Rename(newName);

        Assert.Equal(newName, entrepreneur.Name);
        Assert.Equal(originalInn, entrepreneur.Inn);
        Assert.Equal(ClientType.INDIVIDUAL_ENTREPRENEUR, entrepreneur.Type);
    }

    private static LegalEntity CreateLegalEntity() =>
        LegalEntity.Create(CreateInn(LEGAL_ENTITY_INN), CreateName("ООО «Ромашка»")).Value;

    private static IndividualEntrepreneur CreateIndividualEntrepreneur() =>
        IndividualEntrepreneur.Create(CreateInn(PERSON_INN), CreateName("ИП Иванов И.И.")).Value;

    private static Inn CreateInn(string value) => Inn.Of(value).Value;

    private static ClientName CreateName(string value) => ClientName.Of(value).Value;

    private static void AssertSingleError<T>(Result<T, Error> result, string expectedCode)
    {
        Assert.True(result.IsFailure);
        ErrorMessage message = Assert.Single(result.Error.Messages);
        Assert.Equal(expectedCode, message.Code);
        Assert.Equal("inn", message.InvalidField);
    }
}
