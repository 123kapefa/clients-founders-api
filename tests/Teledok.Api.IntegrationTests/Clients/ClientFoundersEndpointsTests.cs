using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Teledok.Api.IntegrationTests.Infrastructure;

namespace Teledok.Api.IntegrationTests.Clients;

public sealed class ClientFoundersEndpointsTests(TeledokApiFactory factory) : IntegrationTestBase(factory)
{
    private const string URL = "/api/clients";

    private const string FIRST_LEGAL_ENTITY_INN = "7707083893";

    private const string SECOND_LEGAL_ENTITY_INN = "7830002293";

    private const string FOUNDER_INN = "500100732259";

    private const string OTHER_FOUNDER_INN = "773403580096";

    [Fact]
    public async Task AddFounder_ByInnAndFullName_CreatesFounderAndShowsItInClient()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);

        HttpResponseMessage response = await AddFounderAsync(clientId, new
        {
            inn = FOUNDER_INN,
            lastName = "Иванов",
            firstName = "Иван",
            middleName = "Иванович",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"{URL}/{clientId}", response.Headers.Location?.ToString());
        JsonElement added = (await ReadEnvelopeAsync(response)).GetProperty("result");
        Assert.Equal(FOUNDER_INN, added.GetProperty("inn").GetString());

        JsonElement founder = Assert.Single((await GetFoundersAsync(clientId)).EnumerateArray());
        Assert.Equal(added.GetProperty("founderId").GetString(), founder.GetProperty("founderId").GetString());
        Assert.Equal("Иванов", founder.GetProperty("lastName").GetString());
        Assert.Equal("Иван", founder.GetProperty("firstName").GetString());
        Assert.Equal("Иванович", founder.GetProperty("middleName").GetString());
    }

    [Fact]
    public async Task AddFounder_ExistingByIdToAnotherLegalEntity_SharesFounder()
    {
        string firstId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);
        string secondId = await CreateLegalEntityAsync(SECOND_LEGAL_ENTITY_INN);
        string founderId = await AddFounderByInnAsync(firstId, FOUNDER_INN, "Иванов");

        HttpResponseMessage response = await AddFounderAsync(secondId, new { founderId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        JsonElement founder = Assert.Single((await GetFoundersAsync(secondId)).EnumerateArray());
        Assert.Equal(founderId, founder.GetProperty("founderId").GetString());
    }

    [Fact]
    public async Task AddFounder_ByInnOfExistingFounderWithSameFullName_ReusesFounder()
    {
        string firstId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);
        string secondId = await CreateLegalEntityAsync(SECOND_LEGAL_ENTITY_INN);
        string founderId = await AddFounderByInnAsync(firstId, FOUNDER_INN, "Иванов");

        HttpResponseMessage response = await AddFounderAsync(secondId, new { inn = FOUNDER_INN, lastName = " Иванов ", firstName = "Иван" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(founderId, (await ReadEnvelopeAsync(response)).GetProperty("result").GetProperty("founderId").GetString());
    }

    [Fact]
    public async Task AddFounder_ByInnOfExistingFounderWithOtherFullName_ReturnsConflict()
    {
        string firstId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);
        string secondId = await CreateLegalEntityAsync(SECOND_LEGAL_ENTITY_INN);
        await AddFounderByInnAsync(firstId, FOUNDER_INN, "Иванов");

        HttpResponseMessage response = await AddFounderAsync(secondId, new { inn = FOUNDER_INN, lastName = "Петров", firstName = "Иван" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(("founder.full.name.mismatch", "inn"), ReadErrors(await ReadEnvelopeAsync(response)));
        Assert.Empty((await GetFoundersAsync(secondId)).EnumerateArray());
    }

    [Fact]
    public async Task AddFounder_Twice_ReturnsConflict()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);
        string founderId = await AddFounderByInnAsync(clientId, FOUNDER_INN, "Иванов");

        HttpResponseMessage response = await AddFounderAsync(clientId, new { founderId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(("legal.entity.founder.already.added", "founderId"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task AddFounder_WithUnknownFounderId_ReturnsNotFound()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);

        HttpResponseMessage response = await AddFounderAsync(clientId, new { founderId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("founder.not.found", "founderId"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task AddFounder_ToIndividualEntrepreneur_ReturnsBadRequest()
    {
        HttpResponseMessage createResponse = await Client.PostAsJsonAsync(
            URL,
            new { type = "INDIVIDUAL_ENTREPRENEUR", inn = OTHER_FOUNDER_INN, name = "ИП Петров П.П." },
            TestContext.Current.CancellationToken);
        string clientId = (await ReadEnvelopeAsync(createResponse)).GetProperty("result").GetString()!;

        HttpResponseMessage response = await AddFounderAsync(clientId, new { inn = FOUNDER_INN, lastName = "Иванов", firstName = "Иван" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("client.founders.not.supported", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task AddFounder_WithLegalEntityInn_ReturnsBadRequest()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);

        HttpResponseMessage response = await AddFounderAsync(clientId, new { inn = SECOND_LEGAL_ENTITY_INN, lastName = "Иванов", firstName = "Иван" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("founder.inn.must.be.person", "inn"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task AddFounder_WithInvalidDetails_ReturnsAllErrors()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);

        HttpResponseMessage response = await AddFounderAsync(clientId, new { inn = "123", lastName = "", firstName = "Иван1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            [("inn.invalid.length", "inn"), ("value.is.required", "lastName"), ("full.name.invalid.characters", "firstName")],
            ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Theory]
    [MemberData(nameof(AmbiguousBodies))]
    public async Task AddFounder_WithoutExactlyOneVariant_ReturnsBadRequest(object body)
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);

        HttpResponseMessage response = await AddFounderAsync(clientId, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("founder.input.invalid", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task AddFounder_ToUnknownClient_ReturnsNotFound()
    {
        HttpResponseMessage response = await AddFounderAsync(Guid.NewGuid().ToString(), new { founderId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("client.not.found", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task RemoveFounder_WithLinkedFounder_RemovesItFromClient()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);
        string founderId = await AddFounderByInnAsync(clientId, FOUNDER_INN, "Иванов");

        HttpResponseMessage response = await RemoveFounderAsync(clientId, founderId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await GetFoundersAsync(clientId)).EnumerateArray());
    }

    [Fact]
    public async Task RemoveFounder_Twice_ReturnsNotFound()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);
        string founderId = await AddFounderByInnAsync(clientId, FOUNDER_INN, "Иванов");
        await RemoveFounderAsync(clientId, founderId);

        HttpResponseMessage response = await RemoveFounderAsync(clientId, founderId);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("legal.entity.founder.not.found", "founderId"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task RemoveFounder_ThenAddAgain_ReturnsCreated()
    {
        string clientId = await CreateLegalEntityAsync(FIRST_LEGAL_ENTITY_INN);
        string founderId = await AddFounderByInnAsync(clientId, FOUNDER_INN, "Иванов");
        await RemoveFounderAsync(clientId, founderId);

        HttpResponseMessage response = await AddFounderAsync(clientId, new { founderId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single((await GetFoundersAsync(clientId)).EnumerateArray());
    }

    public static TheoryData<object> AmbiguousBodies() =>
    [
        new { },
        new { founderId = Guid.NewGuid(), inn = FOUNDER_INN },
    ];

    private async Task<string> CreateLegalEntityAsync(string inn)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            URL,
            new { type = "LEGAL_ENTITY", inn, name = $"ООО «Компания {inn}»" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetString()!;
    }

    private async Task<string> AddFounderByInnAsync(string clientId, string inn, string lastName)
    {
        HttpResponseMessage response = await AddFounderAsync(clientId, new { inn, lastName, firstName = "Иван" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetProperty("founderId").GetString()!;
    }

    private Task<HttpResponseMessage> AddFounderAsync(string clientId, object body) =>
        Client.PostAsJsonAsync($"{URL}/{clientId}/founders", body, TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> RemoveFounderAsync(string clientId, string founderId) =>
        Client.DeleteAsync($"{URL}/{clientId}/founders/{founderId}", TestContext.Current.CancellationToken);

    private async Task<JsonElement> GetFoundersAsync(string clientId)
    {
        HttpResponseMessage response = await Client.GetAsync($"{URL}/{clientId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetProperty("founders");
    }
}
