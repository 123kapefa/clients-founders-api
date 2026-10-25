using System.Data.Common;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Respawn;

using Teledok.Infrastructure.Postgres;

using Testcontainers.PostgreSql;

namespace Teledok.Api.IntegrationTests.Infrastructure;

public sealed class TeledokApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string POSTGRES_IMAGE = "postgres:17";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(POSTGRES_IMAGE).Build();

    private DbConnection _connection = null!;

    private Respawner _respawner = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        using (IServiceScope scope = Services.CreateScope())
        {
            TeledokDbContext dbContext = scope.ServiceProvider.GetRequiredService<TeledokDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        _connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [TeledokDbContext.SCHEMA],
            TablesToIgnore = [new Respawn.Graph.Table(TeledokDbContext.SCHEMA, TeledokDbContext.MIGRATIONS_HISTORY_TABLE)],
        });
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_connection);

    public override async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(
            $"ConnectionStrings:{DependencyInjectionExtensions.CONNECTION_STRING_NAME}",
            _postgres.GetConnectionString());
    }
}
