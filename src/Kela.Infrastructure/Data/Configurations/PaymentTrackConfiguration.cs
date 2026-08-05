using Kela.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class PaymentTrackConfiguration : IEntityTypeConfiguration<PaymentTrack>
{
    public void Configure(EntityTypeBuilder<PaymentTrack> builder)
    {
        builder.ToTable("payment_tracks");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(p => p.Amount).HasPrecision(10, 2).IsRequired();
        builder.Property(p => p.Status).HasConversion<int>().IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);

        builder.HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.StudentId, p.Status });

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
