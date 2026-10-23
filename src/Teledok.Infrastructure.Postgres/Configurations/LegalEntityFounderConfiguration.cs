using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Teledok.Domain.Entities;

namespace Teledok.Infrastructure.Postgres.Configurations;

internal sealed class LegalEntityFounderConfiguration : IEntityTypeConfiguration<LegalEntityFounder>
{
    public void Configure(EntityTypeBuilder<LegalEntityFounder> builder)
    {
        builder.ToTable("legal_entity_founders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.HasOne(x => x.Founder)
            .WithMany()
            .HasForeignKey(x => x.FounderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LegalEntityId, x.FounderId })
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
