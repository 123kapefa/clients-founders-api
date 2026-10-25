using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Teledok.Api.IntegrationTests.Infrastructure;
using Teledok.Domain.Entities;
using Teledok.Domain.ValueObjects;
using Teledok.Infrastructure.Postgres;

namespace Teledok.Api.IntegrationTests.Clients;

public sealed class DeleteClientEndpointTests(TeledokApiFactory factory) : IntegrationTestBase(factory)
{
    private const string URL = "/api/clients";

    private const string LEGAL_ENTITY_INN = "7707083893";

    private const string PERSON_INN = "500100732259";

    private readonly TeledokApiFactory _factory = factory;

    [Fact]
    public async Task Delete_WithExistingClient_ReturnsOkAndHidesClient()
    {
        string id = await CreateClientAsync("LEGAL_ENTITY", LEGAL_ENTITY_INN, "ООО «Ромашка»");

        HttpResponseMessage response = await DeleteAsync(id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement envelope = await ReadEnvelopeAsync(response);
        Assert.False(envelope.GetProperty("isError").GetBoolean());

        HttpResponseMessage getResponse = await Client.GetAsync($"{URL}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        HttpResponseMessage listResponse = await Client.GetAsync(URL, TestContext.Current.CancellationToken);
        JsonElement page = (await ReadEnvelopeAsync(listResponse)).GetProperty("result");
        Assert.Equal(0, page.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Delete_WithExistingClient_KeepsRowAsSoftDeleted()
    {
        string id = await CreateClientAsync("INDIVIDUAL_ENTREPRENEUR", PERSON_INN, "ИП Иванов И.И.");

        await DeleteAsync(id);

        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        TeledokDbContext dbContext = scope.ServiceProvider.GetRequiredService<TeledokDbContext>();
        Client client = await dbContext.Clients
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == Guid.Parse(id), TestContext.Current.CancellationToken);

        Assert.True(client.IsDeleted);
        Assert.NotNull(client.DeletedAt);
    }

    [Fact]
    public async Task Delete_WithLegalEntityFounders_SoftDeletesLinksAndKeepsFounders()
    {
        string id = await CreateClientAsync("LEGAL_ENTITY", LEGAL_ENTITY_INN, "ООО «Ромашка»");
        Guid founderId = await AddFounderDirectlyAsync(Guid.Parse(id));

        await DeleteAsync(id);

        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        TeledokDbContext dbContext = scope.ServiceProvider.GetRequiredService<TeledokDbContext>();

        LegalEntityFounder link = await dbContext.LegalEntityFounders
            .IgnoreQueryFilters()
            .SingleAsync(x => x.FounderId == founderId, TestContext.Current.CancellationToken);
        Assert.True(link.IsDeleted);

        Founder founder = await dbContext.Founders.SingleAsync(x => x.Id == founderId, TestContext.Current.CancellationToken);
        Assert.False(founder.IsDeleted);
    }

    [Fact]
    public async Task Delete_ThenCreateWithSameInn_ReturnsCreated()
    {
        string id = await CreateClientAsync("LEGAL_ENTITY", LEGAL_ENTITY_INN, "ООО «Ромашка»");
        await DeleteAsync(id);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            URL,
            new { type = "LEGAL_ENTITY", inn = LEGAL_ENTITY_INN, name = "ООО «Ромашка-2»" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Twice_ReturnsNotFound()
    {
        string id = await CreateClientAsync("LEGAL_ENTITY", LEGAL_ENTITY_INN, "ООО «Ромашка»");
        await DeleteAsync(id);

        HttpResponseMessage response = await DeleteAsync(id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(("client.not.found", null), ReadErrors(await ReadEnvelopeAsync(response)));
    }

    [Fact]
    public async Task Delete_WithUnknownId_ReturnsNotFound()
    {
        HttpResponseMessage response = await DeleteAsync(Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> CreateClientAsync(string type, string inn, string name)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            URL,
            new { type, inn, name },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await ReadEnvelopeAsync(response)).GetProperty("result").GetString()!;
    }

    private Task<HttpResponseMessage> DeleteAsync(string id) =>
        Client.DeleteAsync($"{URL}/{id}", TestContext.Current.CancellationToken);

    private async Task<Guid> AddFounderDirectlyAsync(Guid legalEntityId)
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        TeledokDbContext dbContext = scope.ServiceProvider.GetRequiredService<TeledokDbContext>();

        LegalEntity legalEntity = await dbContext.Set<LegalEntity>()
            .Include(x => x.Founders)
            .SingleAsync(x => x.Id == legalEntityId, TestContext.Current.CancellationToken);

        Founder founder = Founder.Create(Inn.Of(PERSON_INN).Value, FullName.Of("Иванов", "Иван", null).Value).Value;
        dbContext.Founders.Add(founder);
        legalEntity.AddFounder(founder);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return founder.Id;
    }
}
