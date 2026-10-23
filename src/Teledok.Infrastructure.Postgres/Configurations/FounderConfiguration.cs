using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Teledok.Domain.Entities;
using Teledok.Domain.ValueObjects;

namespace Teledok.Infrastructure.Postgres.Configurations;

internal sealed class FounderConfiguration : IEntityTypeConfiguration<Founder>
{
    public void Configure(EntityTypeBuilder<Founder> builder)
    {
        builder.ToTable("founders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Inn)
            .HasConversion(inn => inn.Value, value => Inn.Of(value).Value)
            .HasMaxLength(Inn.PERSON_LENGTH)
            .IsRequired();

        builder.ComplexProperty(x => x.FullName, fullName =>
        {
            fullName.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(FullName.PART_MAX_LENGTH)
                .IsRequired();

            fullName.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(FullName.PART_MAX_LENGTH)
                .IsRequired();

            fullName.Property(x => x.MiddleName)
                .HasColumnName("middle_name")
                .HasMaxLength(FullName.PART_MAX_LENGTH);
        });

        builder.HasIndex(x => x.Inn)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
