using Microsoft.EntityFrameworkCore;

using Teledok.Domain.Entities;

namespace Teledok.Infrastructure.Postgres;

public sealed class TeledokDbContext(DbContextOptions<TeledokDbContext> options) : DbContext(options)
{
    public const string SCHEMA = "teledok";

    public const string MIGRATIONS_HISTORY_TABLE = "__ef_migrations_history";

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Founder> Founders => Set<Founder>();

    public DbSet<LegalEntityFounder> LegalEntityFounders => Set<LegalEntityFounder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SCHEMA);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeledokDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
