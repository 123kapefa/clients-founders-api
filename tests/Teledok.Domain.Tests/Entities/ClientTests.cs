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
        Inn inn = CreateInn(PERSON_INN);
        ClientName name = CreateName("ООО «Ромашка»");

        Result<LegalEntity, Error> result = LegalEntity.Create(inn, name);

        Assert.True(result.IsFailure);
        ErrorMessage message = Assert.Single(result.Error.Messages);
        Assert.Equal("client.legal.entity.inn.length", message.Code);
        Assert.Equal("inn", message.InvalidField);
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
    public void Rename_WithNewName_ChangesNameAndKeepsInnAndType()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Inn originalInn = legalEntity.Inn;
        ClientName newName = CreateName("ООО «Лютик»");

        legalEntity.Rename(newName);

        Assert.Equal(newName, legalEntity.Name);
        Assert.Equal(originalInn, legalEntity.Inn);
        Assert.Equal(ClientType.LEGAL_ENTITY, legalEntity.Type);
    }

    private static LegalEntity CreateLegalEntity() =>
        LegalEntity.Create(CreateInn(LEGAL_ENTITY_INN), CreateName("ООО «Ромашка»")).Value;

    private static Inn CreateInn(string value) => Inn.Of(value).Value;

    private static ClientName CreateName(string value) => ClientName.Of(value).Value;
}
