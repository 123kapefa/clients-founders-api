using System.Net.Http.Json;
using System.Text.Json;

namespace Teledok.Api.IntegrationTests.Infrastructure;

[Collection(IntegrationTestsDefinition.NAME)]
public abstract class IntegrationTestBase(TeledokApiFactory factory) : IAsyncLifetime
{
    protected HttpClient Client { get; } = factory.CreateClient();

    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    protected static async Task<JsonElement> ReadEnvelopeAsync(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
    }

    protected static IReadOnlyList<(string Code, string? Field)> ReadErrors(JsonElement envelope) =>
        envelope.GetProperty("error").GetProperty("messages").EnumerateArray()
            .Select(message => (
                message.GetProperty("code").GetString()!,
                message.GetProperty("invalidField").GetString()))
            .ToList();
}
