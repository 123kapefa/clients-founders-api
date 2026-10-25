using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Teledok.Api.IntegrationTests.Infrastructure;

namespace Teledok.Api.IntegrationTests.Clients;

public sealed class CreateClientWithFoundersEndpointTests(TeledokApiFactory factory) : IntegrationTestBase(factory)
{
    private const string URL = "/api/clients";

    private const string LEGAL_ENTITY_INN = "7707083893";

    private const string OTHER_LEGAL_ENTITY_INN = "7830002293";

    private const string FOUNDER_INN = "500100732259";

    private const string OTHER_FOUNDER_INN = "773403580096";

    [Fact]
    public async Task Create_WithNewAndExistingFounders_LinksBoth()
    {
        string firstId = await CreateLegalEntityAsync(
            LEGAL_ENTITY_INN,
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });
        string existingFounderId = (await GetFoundersAsync(firstId))[0].GetProperty("founderId").GetString()!;

        string secondId = await CreateLegalEntityAsync(
            OTHER_LEGAL_ENTITY_INN,
            new { founderId = existingFounderId },
            new { inn = OTHER_FOUNDER_INN, lastName = "Петрова", firstName = "Анна", middleName = "Сергеевна" });

        IReadOnlyList<JsonElement> founders = await GetFoundersAsync(secondId);
        Assert.Equal(2, founders.Count);
        Assert.Contains(founders, founder => founder.GetProperty("founderId").GetString() == existingFounderId);
        Assert.Contains(founders, founder => founder.GetProperty("inn").GetString() == OTHER_FOUNDER_INN);
    }

    [Fact]
    public async Task Create_WithExistingFounderInnAndSameFullName_ReusesFounder()
    {
        string firstId = await CreateLegalEntityAsync(
            LEGAL_ENTITY_INN,
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });
        string secondId = await CreateLegalEntityAsync(
            OTHER_LEGAL_ENTITY_INN,
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });

        string first = (await GetFoundersAsync(firstId))[0].GetProperty("founderId").GetString()!;
        string second = (await GetFoundersAsync(secondId))[0].GetProperty("founderId").GetString()!;
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Create_IndividualEntrepreneurWithFounders_ReturnsBadRequest()
    {
        HttpResponseMessage response = await PostAsync(new
        {
            type = "INDIVIDUAL_ENTREPRENEUR",
            inn = FOUNDER_INN,
            name = "ИП Иванов И.И.",
            founders = new[] { new { inn = OTHER_FOUNDER_INN, lastName = "Петрова", firstName = "Анна" } },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("client.founders.not.supported", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Create_WithInvalidFounderInn_ReturnsBadRequestWithIndexedField()
    {
        HttpResponseMessage response = await PostLegalEntityAsync(
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" },
            new { inn = "123", lastName = "Петрова", firstName = "Анна" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        IReadOnlyList<(string Code, string? Field)> errors = ReadErrors(await ReadEnvelopeAsync(response));
        Assert.Contains(errors, error => error.Field == "founders[1].inn");
        await AssertNoClientsAsync();
    }

    [Fact]
    public async Task Create_WithUnknownFounderId_ReturnsNotFoundAndCreatesNothing()
    {
        HttpResponseMessage response = await PostLegalEntityAsync(new { founderId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("founder.not.found", "founders[0].founderId"), ReadErrors(await ReadEnvelopeAsync(response)));
        await AssertNoClientsAsync();
    }

    [Fact]
    public async Task Create_WithBothFounderVariants_ReturnsBadRequest()
    {
        HttpResponseMessage response = await PostLegalEntityAsync(
            new { founderId = Guid.NewGuid(), inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("founder.input.invalid", "founders[0]"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Create_WithNullFounder_ReturnsBadRequest()
    {
        HttpResponseMessage response = await PostAsync(new
        {
            type = "LEGAL_ENTITY",
            inn = LEGAL_ENTITY_INN,
            name = "ООО «Ромашка»",
            founders = new object?[] { null },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("founder.input.invalid", "founders[0]"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Create_WithDuplicateFounderInn_ReturnsBadRequest()
    {
        HttpResponseMessage response = await PostLegalEntityAsync(
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" },
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("client.founders.duplicated", "founders"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Create_WithSameFounderByIdAndByInn_ReturnsConflict()
    {
        string firstId = await CreateLegalEntityAsync(
            LEGAL_ENTITY_INN,
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });
        string founderId = (await GetFoundersAsync(firstId))[0].GetProperty("founderId").GetString()!;

        HttpResponseMessage response = await PostAsync(new
        {
            type = "LEGAL_ENTITY",
            inn = OTHER_LEGAL_ENTITY_INN,
            name = "ООО «Лютик»",
            founders = new object[]
            {
                new { founderId },
                new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" },
            },
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(("legal.entity.founder.already.added", "founderId"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Create_WithExistingFounderInnAndOtherFullName_ReturnsConflict()
    {
        await CreateLegalEntityAsync(
            LEGAL_ENTITY_INN,
            new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });

        HttpResponseMessage response = await PostAsync(new
        {
            type = "LEGAL_ENTITY",
            inn = OTHER_LEGAL_ENTITY_INN,
            name = "ООО «Лютик»",
            founders = new[] { new { inn = FOUNDER_INN, lastName = "Петров", firstName = "Иван" } },
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(("founder.full.name.mismatch", "founders[0].inn"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    private Task<HttpResponseMessage> PostAsync(object body) =>
        Client.PostAsJsonAsync(URL, body, TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> PostLegalEntityAsync(params object[] founders) =>
        PostAsync(new { type = "LEGAL_ENTITY", inn = LEGAL_ENTITY_INN, name = "ООО «Ромашка»", founders });

    private async Task<string> CreateLegalEntityAsync(string inn, params object[] founders)
    {
        HttpResponseMessage response = await PostAsync(new { type = "LEGAL_ENTITY", inn, name = "ООО «Ромашка»", founders });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetString()!;
    }

    private async Task<IReadOnlyList<JsonElement>> GetFoundersAsync(string clientId)
    {
        HttpResponseMessage response = await Client.GetAsync($"{URL}/{clientId}", TestContext.Current.CancellationToken);

        return [.. (await ReadEnvelopeAsync(response)).GetProperty("result").GetProperty("founders").EnumerateArray()];
    }

    private async Task AssertNoClientsAsync()
    {
        HttpResponseMessage response = await Client.GetAsync(URL, TestContext.Current.CancellationToken);

        Assert.Equal(0, (await ReadEnvelopeAsync(response)).GetProperty("result").GetProperty("totalCount").GetInt32());
    }
}
