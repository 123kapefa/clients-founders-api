using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Teledok.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
    public const string CONNECTION_STRING_NAME = "Postgres";

    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString(CONNECTION_STRING_NAME)
            ?? throw new InvalidOperationException($"Connection string '{CONNECTION_STRING_NAME}' is not configured.");

        services.AddDbContext<TeledokDbContext>(options => ConfigureDbContext(options, connectionString));
        return services;
    }

    internal static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(TeledokDbContext.MIGRATIONS_HISTORY_TABLE, TeledokDbContext.SCHEMA))
            .UseSnakeCaseNamingConvention();
}
