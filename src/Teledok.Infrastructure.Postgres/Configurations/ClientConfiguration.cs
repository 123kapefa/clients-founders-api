using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Teledok.Domain.Entities;
using Teledok.Domain.Enums;
using Teledok.Domain.ValueObjects;

namespace Teledok.Infrastructure.Postgres.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    private const int TYPE_MAX_LENGTH = 32;

    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Inn)
            .HasConversion(inn => inn.Value, value => Inn.Of(value).Value)
            .HasMaxLength(Inn.PERSON_LENGTH)
            .IsRequired();

        builder.ComplexProperty(x => x.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("name")
                .HasColumnType("citext")
                .IsRequired();
        });

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(TYPE_MAX_LENGTH);

        builder.HasDiscriminator(x => x.Type)
            .HasValue<LegalEntity>(ClientType.LEGAL_ENTITY)
            .HasValue<IndividualEntrepreneur>(ClientType.INDIVIDUAL_ENTREPRENEUR);

        builder.HasIndex(x => x.Inn)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
