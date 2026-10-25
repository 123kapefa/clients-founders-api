using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Teledok.Application.Abstractions;
using Teledok.Infrastructure.Postgres.Database;
using Teledok.Infrastructure.Postgres.Interceptors;
using Teledok.Infrastructure.Postgres.Repositories;
using Teledok.Shared.Core.Database;

namespace Teledok.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
    public const string CONNECTION_STRING_NAME = "Postgres";

    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString(CONNECTION_STRING_NAME)
            ?? throw new InvalidOperationException($"Connection string '{CONNECTION_STRING_NAME}' is not configured.");

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditableInterceptor>();

        services.AddDbContext<TeledokDbContext>((serviceProvider, options) =>
        {
            ConfigureDbContext(options, connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableInterceptor>());
        });

        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IReadDbContext>(serviceProvider => serviceProvider.GetRequiredService<TeledokDbContext>());
        services.AddScoped<IClientsRepository, ClientsRepository>();
        services.AddScoped<IFoundersRepository, FoundersRepository>();

        return services;
    }

    internal static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(TeledokDbContext.MIGRATIONS_HISTORY_TABLE, TeledokDbContext.SCHEMA))
            .UseSnakeCaseNamingConvention();
}

