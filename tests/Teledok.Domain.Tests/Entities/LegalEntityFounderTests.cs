using CSharpFunctionalExtensions;

using Teledok.Domain.Entities;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Tests.Entities;

public sealed class LegalEntityFounderTests
{
    private const string LEGAL_ENTITY_INN = "7707083893";

    private const string PERSON_INN = "500100732259";

    [Fact]
    public void Create_NewLegalEntity_HasNoFounders()
    {
        LegalEntity legalEntity = CreateLegalEntity();

        Assert.Empty(legalEntity.Founders);
    }

    [Fact]
    public void Create_WithFounders_AddsLinks()
    {
        Founder founder = CreateFounder();

        Result<LegalEntity, Error> result = LegalEntity.Create(
            Inn.Of(LEGAL_ENTITY_INN).Value,
            ClientName.Of("ООО «Ромашка»").Value,
            [founder]);

        Assert.True(result.IsSuccess);
        LegalEntityFounder link = Assert.Single(result.Value.Founders);
        Assert.Equal(result.Value.Id, link.LegalEntityId);
        Assert.Equal(founder.Id, link.FounderId);
    }

    [Fact]
    public void Create_WithSameFounderTwice_ReturnsConflict()
    {
        Founder founder = CreateFounder();

        Result<LegalEntity, Error> result = LegalEntity.Create(
            Inn.Of(LEGAL_ENTITY_INN).Value,
            ClientName.Of("ООО «Ромашка»").Value,
            [founder, founder]);

        AssertError(result, ErrorType.CONFLICT, "legal.entity.founder.already.added");
    }

    [Fact]
    public void AddFounder_WithNewFounder_AddsLink()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Founder founder = CreateFounder();

        UnitResult<Error> result = legalEntity.AddFounder(founder);

        Assert.True(result.IsSuccess);
        LegalEntityFounder link = Assert.Single(legalEntity.Founders);
        Assert.Equal(legalEntity.Id, link.LegalEntityId);
        Assert.Equal(founder.Id, link.FounderId);
        Assert.Same(founder, link.Founder);
        Assert.NotEqual(Guid.Empty, link.Id);
    }

    [Fact]
    public void AddFounder_WithTwoDifferentFounders_AddsTwoLinks()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Founder first = CreateFounder();
        Founder second = CreateFounder();

        legalEntity.AddFounder(first);
        legalEntity.AddFounder(second);

        Assert.Collection(
            legalEntity.Founders,
            link => Assert.Equal(first.Id, link.FounderId),
            link => Assert.Equal(second.Id, link.FounderId));
    }

    [Fact]
    public void AddFounder_WithAlreadyAddedFounder_ReturnsConflictAndKeepsSingleLink()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Founder founder = CreateFounder();
        legalEntity.AddFounder(founder);

        UnitResult<Error> result = legalEntity.AddFounder(founder);

        AssertError(result, ErrorType.CONFLICT, "legal.entity.founder.already.added");
        Assert.Single(legalEntity.Founders);
    }

    [Fact]
    public void RemoveFounder_WithAddedFounder_RemovesLink()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Founder founder = CreateFounder();
        legalEntity.AddFounder(founder);

        UnitResult<Error> result = legalEntity.RemoveFounder(founder.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(legalEntity.Founders);
    }

    [Fact]
    public void RemoveFounder_WithOneOfTwoFounders_KeepsOtherLink()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Founder first = CreateFounder();
        Founder second = CreateFounder();
        legalEntity.AddFounder(first);
        legalEntity.AddFounder(second);

        legalEntity.RemoveFounder(first.Id);

        LegalEntityFounder link = Assert.Single(legalEntity.Founders);
        Assert.Equal(second.Id, link.FounderId);
    }

    [Fact]
    public void RemoveFounder_WithUnknownFounder_ReturnsNotFound()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        legalEntity.AddFounder(CreateFounder());

        UnitResult<Error> result = legalEntity.RemoveFounder(Guid.NewGuid());

        AssertError(result, ErrorType.NOT_FOUND, "legal.entity.founder.not.found");
        Assert.Single(legalEntity.Founders);
    }

    [Fact]
    public void AddFounder_AfterRemove_CreatesNewLink()
    {
        LegalEntity legalEntity = CreateLegalEntity();
        Founder founder = CreateFounder();
        legalEntity.AddFounder(founder);
        Guid removedLinkId = legalEntity.Founders[0].Id;
        legalEntity.RemoveFounder(founder.Id);

        UnitResult<Error> result = legalEntity.AddFounder(founder);

        Assert.True(result.IsSuccess);
        LegalEntityFounder link = Assert.Single(legalEntity.Founders);
        Assert.Equal(founder.Id, link.FounderId);
        Assert.NotEqual(removedLinkId, link.Id);
    }

    private static LegalEntity CreateLegalEntity() =>
        LegalEntity.Create(Inn.Of(LEGAL_ENTITY_INN).Value, ClientName.Of("ООО «Ромашка»").Value).Value;

    private static Founder CreateFounder() =>
        Founder.Create(Inn.Of(PERSON_INN).Value, FullName.Of("Иванов", "Иван", null).Value).Value;

    private static void AssertError(UnitResult<Error> result, ErrorType expectedType, string expectedCode)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(expectedType, result.Error.Type);
        ErrorMessage message = Assert.Single(result.Error.Messages);
        Assert.Equal(expectedCode, message.Code);
        Assert.Equal("founderId", message.InvalidField);
    }
}
