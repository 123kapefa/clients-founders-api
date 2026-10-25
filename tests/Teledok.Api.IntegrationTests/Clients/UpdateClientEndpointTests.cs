using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Teledok.Api.IntegrationTests.Infrastructure;

namespace Teledok.Api.IntegrationTests.Clients;

public sealed class UpdateClientEndpointTests(TeledokApiFactory factory) : IntegrationTestBase(factory)
{
    private const string URL = "/api/clients";

    private const string LEGAL_ENTITY_INN = "7707083893";

    [Fact]
    public async Task Update_WithValidName_RenamesClientAndKeepsInnAndType()
    {
        string id = await CreateClientAsync();

        HttpResponseMessage response = await PutAsync(id, new { name = "  ООО   «Лютик» " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement updated = (await ReadEnvelopeAsync(response)).GetProperty("result");
        Assert.Equal(id, updated.GetProperty("id").GetString());
        Assert.Equal("ООО «Лютик»", updated.GetProperty("name").GetString());
        Assert.Equal(LEGAL_ENTITY_INN, updated.GetProperty("inn").GetString());
        Assert.Equal("LEGAL_ENTITY", updated.GetProperty("type").GetString());
        Assert.True(updated.GetProperty("updatedAt").GetDateTime() > updated.GetProperty("createdAt").GetDateTime());

        JsonElement stored = await GetClientAsync(id);
        Assert.Equal("ООО «Лютик»", stored.GetProperty("name").GetString());
    }

    [Theory]
    [InlineData(null, "client.name.required")]
    [InlineData("   ", "client.name.required")]
    [InlineData("ООО\n«Лютик»", "client.name.invalid.characters")]
    public async Task Update_WithInvalidName_ReturnsBadRequestAndKeepsName(string? name, string code)
    {
        string id = await CreateClientAsync();

        HttpResponseMessage response = await PutAsync(id, new { name });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains((code, "name"), ReadErrors(await ReadEnvelopeAsync(response)));
        Assert.Equal("ООО «Ромашка»", (await GetClientAsync(id)).GetProperty("name").GetString());
    }

    [Fact]
    public async Task Update_WithUnknownId_ReturnsNotFound()
    {
        HttpResponseMessage response = await PutAsync(Guid.NewGuid().ToString(), new { name = "ООО «Лютик»" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("client.not.found", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    private async Task<string> CreateClientAsync()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            URL,
            new { type = "LEGAL_ENTITY", inn = LEGAL_ENTITY_INN, name = "ООО «Ромашка»" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetString()!;
    }

    private Task<HttpResponseMessage> PutAsync(string id, object body) =>
        Client.PutAsJsonAsync($"{URL}/{id}", body, TestContext.Current.CancellationToken);

    private async Task<JsonElement> GetClientAsync(string id)
    {
        HttpResponseMessage response = await Client.GetAsync($"{URL}/{id}", TestContext.Current.CancellationToken);

        return (await ReadEnvelopeAsync(response)).GetProperty("result");
    }
}
