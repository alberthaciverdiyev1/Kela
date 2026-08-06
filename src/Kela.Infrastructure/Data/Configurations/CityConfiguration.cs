using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("cities");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        // Çeviriler Postgres'te native jsonb sütununda saklanır.
        builder.Property(c => c.NameTranslations)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
    }
}
