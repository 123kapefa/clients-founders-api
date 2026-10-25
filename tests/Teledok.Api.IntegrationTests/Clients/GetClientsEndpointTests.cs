using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Teledok.Api.IntegrationTests.Infrastructure;

namespace Teledok.Api.IntegrationTests.Clients;

public sealed class GetClientsEndpointTests(TeledokApiFactory factory) : IntegrationTestBase(factory)
{
    private const string URL = "/api/clients";

    [Fact]
    public async Task GetList_WithoutClients_ReturnsEmptyPage()
    {
        JsonElement page = await GetPageAsync(string.Empty);

        Assert.Equal(0, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, page.GetProperty("totalPages").GetInt32());
        Assert.Empty(page.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task GetList_WithPaging_ReturnsRequestedPageInCreationOrder()
    {
        await SeedClientsAsync();

        JsonElement page = await GetPageAsync("?page=2&pageSize=2");

        Assert.Equal(2, page.GetProperty("page").GetInt32());
        Assert.Equal(2, page.GetProperty("pageSize").GetInt32());
        Assert.Equal(4, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, page.GetProperty("totalPages").GetInt32());
        Assert.True(page.GetProperty("hasPreviousPage").GetBoolean());
        Assert.False(page.GetProperty("hasNextPage").GetBoolean());
        Assert.Equal(["ИП Иванов И.И.", "ИП Петров П.П."], ReadNames(page));
    }

    [Fact]
    public async Task GetList_WithTypeFilter_ReturnsOnlyThatType()
    {
        await SeedClientsAsync();

        JsonElement page = await GetPageAsync("?type=INDIVIDUAL_ENTREPRENEUR");

        Assert.Equal(2, page.GetProperty("totalCount").GetInt32());
        Assert.All(
            page.GetProperty("items").EnumerateArray(),
            item => Assert.Equal("INDIVIDUAL_ENTREPRENEUR", item.GetProperty("type").GetString()));
    }

    [Fact]
    public async Task GetList_WithInnPrefix_ReturnsMatchingClients()
    {
        await SeedClientsAsync();

        JsonElement page = await GetPageAsync("?search=7707");

        Assert.Equal(["ООО «Ромашка»"], ReadNames(page));
    }

    [Fact]
    public async Task GetList_WithNamePartInOtherCase_ReturnsMatchingClients()
    {
        await SeedClientsAsync();

        JsonElement page = await GetPageAsync("?search=лютик");

        Assert.Equal(["ООО «Лютик»"], ReadNames(page));
    }

    [Fact]
    public async Task GetList_WithLikeWildcardInSearch_TreatsItAsText()
    {
        await SeedClientsAsync();

        JsonElement page = await GetPageAsync("?search=%25");

        Assert.Equal(0, page.GetProperty("totalCount").GetInt32());
    }

    [Theory]
    [InlineData("?page=0", "clients.page.invalid", "page")]
    [InlineData("?pageSize=101", "clients.page.size.invalid", "pageSize")]
    [InlineData("?type=UNKNOWN", "client.type.invalid", "type")]
    [InlineData("?type=1", "client.type.invalid", "type")]
    [InlineData("?page=abc", "request.invalid", "page")]
    public async Task GetList_WithInvalidParameters_ReturnsBadRequest(string queryString, string code, string field)
    {
        HttpResponseMessage response = await Client.GetAsync(URL + queryString, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains((code, field), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    private async Task SeedClientsAsync()
    {
        await CreateClientAsync("LEGAL_ENTITY", "7707083893", "ООО «Ромашка»");
        await CreateClientAsync("LEGAL_ENTITY", "7830002293", "ООО «Лютик»");
        await CreateClientAsync("INDIVIDUAL_ENTREPRENEUR", "500100732259", "ИП Иванов И.И.");
        await CreateClientAsync("INDIVIDUAL_ENTREPRENEUR", "773403580096", "ИП Петров П.П.");
    }

    private async Task CreateClientAsync(string type, string inn, string name)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            URL,
            new { type, inn, name },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<JsonElement> GetPageAsync(string queryString)
    {
        HttpResponseMessage response = await Client.GetAsync(URL + queryString, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result");
    }

    private static List<string> ReadNames(JsonElement page) =>
        page.GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("name").GetString()!)
            .ToList();
}
