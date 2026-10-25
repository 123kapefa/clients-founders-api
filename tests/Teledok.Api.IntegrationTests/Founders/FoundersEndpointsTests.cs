using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Teledok.Api.IntegrationTests.Infrastructure;

namespace Teledok.Api.IntegrationTests.Founders;

public sealed class FoundersEndpointsTests(TeledokApiFactory factory) : IntegrationTestBase(factory)
{
    private const string FOUNDERS_URL = "/api/founders";

    private const string CLIENTS_URL = "/api/clients";

    private const string LEGAL_ENTITY_INN = "7707083893";

    private const string FOUNDER_INN = "500100732259";

    private const string OTHER_FOUNDER_INN = "773403580096";

    [Fact]
    public async Task GetList_WithSearchByLastNameInOtherCase_ReturnsMatchingFounders()
    {
        string clientId = await CreateLegalEntityAsync();
        await AddFounderAsync(clientId, FOUNDER_INN, "Иванов", "Иван");
        await AddFounderAsync(clientId, OTHER_FOUNDER_INN, "Петрова", "Анна");

        JsonElement page = await GetPageAsync("?search=петров");

        JsonElement founder = Assert.Single(page.GetProperty("items").EnumerateArray());
        Assert.Equal("Петрова", founder.GetProperty("lastName").GetString());
    }

    [Fact]
    public async Task GetList_WithSearchByInnPrefix_ReturnsMatchingFounders()
    {
        string clientId = await CreateLegalEntityAsync();
        await AddFounderAsync(clientId, FOUNDER_INN, "Иванов", "Иван");
        await AddFounderAsync(clientId, OTHER_FOUNDER_INN, "Петрова", "Анна");

        JsonElement page = await GetPageAsync("?search=5001");

        JsonElement founder = Assert.Single(page.GetProperty("items").EnumerateArray());
        Assert.Equal(FOUNDER_INN, founder.GetProperty("inn").GetString());
    }

    [Fact]
    public async Task GetList_WithInvalidPageSize_ReturnsBadRequest()
    {
        HttpResponseMessage response = await Client.GetAsync($"{FOUNDERS_URL}?pageSize=0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("founders.page.size.invalid", "pageSize"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task GetById_WithLinkedFounder_ReturnsFounderWithClients()
    {
        string clientId = await CreateLegalEntityAsync();
        string founderId = await AddFounderAsync(clientId, FOUNDER_INN, "Иванов", "Иван");

        HttpResponseMessage response = await Client.GetAsync($"{FOUNDERS_URL}/{founderId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement founder = (await ReadEnvelopeAsync(response)).GetProperty("result");
        Assert.Equal(FOUNDER_INN, founder.GetProperty("inn").GetString());
        JsonElement client = Assert.Single(founder.GetProperty("clients").EnumerateArray());
        Assert.Equal(clientId, client.GetProperty("clientId").GetString());
        Assert.Equal(LEGAL_ENTITY_INN, client.GetProperty("inn").GetString());
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.GetAsync($"{FOUNDERS_URL}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("founder.not.found", "founderId"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Update_WithValidFullName_RenamesFounderEverywhere()
    {
        string clientId = await CreateLegalEntityAsync();
        string founderId = await AddFounderAsync(clientId, FOUNDER_INN, "Иванова", "Анна");

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{FOUNDERS_URL}/{founderId}",
            new { lastName = "Петрова", firstName = "Анна", middleName = "Сергеевна" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement updated = (await ReadEnvelopeAsync(response)).GetProperty("result");
        Assert.Equal("Петрова", updated.GetProperty("lastName").GetString());
        Assert.Equal(FOUNDER_INN, updated.GetProperty("inn").GetString());

        JsonElement clientFounder = Assert.Single((await GetClientFoundersAsync(clientId)).EnumerateArray());
        Assert.Equal("Петрова", clientFounder.GetProperty("lastName").GetString());
        Assert.Equal("Сергеевна", clientFounder.GetProperty("middleName").GetString());
    }

    [Fact]
    public async Task Update_WithInvalidFullName_ReturnsBadRequest()
    {
        string clientId = await CreateLegalEntityAsync();
        string founderId = await AddFounderAsync(clientId, FOUNDER_INN, "Иванов", "Иван");

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{FOUNDERS_URL}/{founderId}",
            new { lastName = "", firstName = "Иван" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("value.is.required", "lastName"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Delete_WithActiveLinks_ReturnsConflict()
    {
        string clientId = await CreateLegalEntityAsync();
        string founderId = await AddFounderAsync(clientId, FOUNDER_INN, "Иванов", "Иван");

        HttpResponseMessage response = await DeleteFounderAsync(founderId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(("founder.has.active.links", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Delete_AfterUnlinking_SoftDeletesFounderAndAllowsSameInnAgain()
    {
        string clientId = await CreateLegalEntityAsync();
        string founderId = await AddFounderAsync(clientId, FOUNDER_INN, "Иванов", "Иван");
        await Client.DeleteAsync($"{CLIENTS_URL}/{clientId}/founders/{founderId}", TestContext.Current.CancellationToken);

        HttpResponseMessage response = await DeleteFounderAsync(founderId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        HttpResponseMessage getResponse = await Client.GetAsync($"{FOUNDERS_URL}/{founderId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        string newFounderId = await AddFounderAsync(clientId, FOUNDER_INN, "Иванов", "Иван");
        Assert.NotEqual(founderId, newFounderId);
    }

    [Fact]
    public async Task Delete_WithUnknownId_ReturnsNotFound()
    {
        HttpResponseMessage response = await DeleteFounderAsync(Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> CreateLegalEntityAsync()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            CLIENTS_URL,
            new { type = "LEGAL_ENTITY", inn = LEGAL_ENTITY_INN, name = "ООО «Ромашка»" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetString()!;
    }

    private async Task<string> AddFounderAsync(string clientId, string inn, string lastName, string firstName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{CLIENTS_URL}/{clientId}/founders",
            new { inn, lastName, firstName },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetProperty("founderId").GetString()!;
    }

    private Task<HttpResponseMessage> DeleteFounderAsync(string founderId) =>
        Client.DeleteAsync($"{FOUNDERS_URL}/{founderId}", TestContext.Current.CancellationToken);

    private async Task<JsonElement> GetPageAsync(string queryString)
    {
        HttpResponseMessage response = await Client.GetAsync(FOUNDERS_URL + queryString, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result");
    }

    private async Task<JsonElement> GetClientFoundersAsync(string clientId)
    {
        HttpResponseMessage response = await Client.GetAsync($"{CLIENTS_URL}/{clientId}", TestContext.Current.CancellationToken);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetProperty("founders");
    }
}
