using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Teledok.Api.IntegrationTests.Infrastructure;
using Teledok.Domain.Entities;

namespace Teledok.Api.IntegrationTests.Clients;

public sealed class ClientsEndpointsTests(TeledokApiFactory factory) : IntegrationTestBase(factory)
{
    private const string URL = "/api/clients";

    private const string LEGAL_ENTITY_INN = "7707083893";

    private const string PERSON_INN = "500100732259";

    [Fact]
    public async Task Create_WithValidLegalEntity_ReturnsCreatedAndCanBeRead()
    {
        HttpResponseMessage createResponse = await PostAsync(new { type = "LEGAL_ENTITY", inn = LEGAL_ENTITY_INN, name = "  ООО   «Ромашка» " });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        JsonElement created = await ReadEnvelopeAsync(createResponse);
        Assert.False(created.GetProperty("isError").GetBoolean());
        string id = created.GetProperty("result").GetString()!;
        Assert.Equal($"{URL}/{id}", createResponse.Headers.Location?.ToString());

        HttpResponseMessage getResponse = await Client.GetAsync(createResponse.Headers.Location, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        JsonElement client = (await ReadEnvelopeAsync(getResponse)).GetProperty("result");
        Assert.Equal(id, client.GetProperty("id").GetString());
        Assert.Equal("LEGAL_ENTITY", client.GetProperty("type").GetString());
        Assert.Equal(LEGAL_ENTITY_INN, client.GetProperty("inn").GetString());
        Assert.Equal("ООО «Ромашка»", client.GetProperty("name").GetString());
        Assert.Equal(client.GetProperty("createdAt").GetDateTime(), client.GetProperty("updatedAt").GetDateTime());
    }

    [Fact]
    public async Task Create_WithValidIndividualEntrepreneur_ReturnsCreated()
    {
        HttpResponseMessage response = await PostAsync(new { type = "INDIVIDUAL_ENTREPRENEUR", inn = PERSON_INN, name = "ИП Иванов И.И." });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateInn_ReturnsConflict()
    {
        await PostAsync(new { type = "LEGAL_ENTITY", inn = LEGAL_ENTITY_INN, name = "ООО «Ромашка»" });

        HttpResponseMessage response = await PostAsync(new { type = "LEGAL_ENTITY", inn = LEGAL_ENTITY_INN, name = "ООО «Лютик»" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        JsonElement envelope = await ReadEnvelopeAsync(response);
        Assert.Equal("CONFLICT", envelope.GetProperty("error").GetProperty("type").GetString());
        Assert.Contains(("unique.constraint.violation", "inn"), ReadErrors(envelope));
    }

    [Fact]
    public async Task Create_WithIndividualEntrepreneurAndLegalEntityInn_ReturnsBadRequest()
    {
        HttpResponseMessage response = await PostAsync(new { type = "INDIVIDUAL_ENTREPRENEUR", inn = LEGAL_ENTITY_INN, name = "ИП Иванов И.И." });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(("client.individual.entrepreneur.inn.length", "inn"), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Create_WithSeveralInvalidFields_ReturnsAllErrors()
    {
        HttpResponseMessage response = await PostAsync(new { type = "UNKNOWN", inn = "123", name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            [("client.type.invalid", "type"), ("inn.invalid.length", "inn"), ("client.name.required", "name")],
            ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Create_WithMalformedJson_ReturnsBadRequestEnvelope()
    {
        using StringContent content = new("{\"type\":", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await Client.PostAsync(URL, content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement envelope = await ReadEnvelopeAsync(response);
        Assert.True(envelope.GetProperty("isError").GetBoolean());
        Assert.All(ReadErrors(envelope), error => Assert.Equal("request.invalid", error.Code));
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.GetAsync($"{URL}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("client.not.found", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    private Task<HttpResponseMessage> PostAsync(object body) =>
        Client.PostAsJsonAsync(URL, body, TestContext.Current.CancellationToken);
}
