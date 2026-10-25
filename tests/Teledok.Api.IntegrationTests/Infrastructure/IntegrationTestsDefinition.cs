namespace Teledok.Api.IntegrationTests.Infrastructure;

[CollectionDefinition(NAME)]
public sealed class IntegrationTestsDefinition : ICollectionFixture<TeledokApiFactory>
{
    public const string NAME = "Integration";
}
