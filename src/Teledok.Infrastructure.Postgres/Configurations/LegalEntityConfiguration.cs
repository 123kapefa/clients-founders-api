using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Teledok.Domain.Entities;

namespace Teledok.Infrastructure.Postgres.Configurations;

internal sealed class LegalEntityConfiguration : IEntityTypeConfiguration<LegalEntity>
{
    public void Configure(EntityTypeBuilder<LegalEntity> builder)
    {
        builder.HasMany(x => x.Founders)
            .WithOne()
            .HasForeignKey(x => x.LegalEntityId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.Navigation(x => x.Founders)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
