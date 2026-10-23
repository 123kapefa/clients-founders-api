using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Teledok.Infrastructure.Postgres;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TeledokDbContext>
{
    private const string CONNECTION_STRING_ENVIRONMENT_VARIABLE = "ConnectionStrings__Postgres";

    private const string DEFAULT_CONNECTION_STRING =
        "Host=localhost;Port=5434;Database=teledok;Username=teledok;Password=teledok";

    public TeledokDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable(CONNECTION_STRING_ENVIRONMENT_VARIABLE)
            ?? DEFAULT_CONNECTION_STRING;

        var optionsBuilder = new DbContextOptionsBuilder<TeledokDbContext>();
        DependencyInjectionExtensions.ConfigureDbContext(optionsBuilder, connectionString);

        return new TeledokDbContext(optionsBuilder.Options);
    }
}
