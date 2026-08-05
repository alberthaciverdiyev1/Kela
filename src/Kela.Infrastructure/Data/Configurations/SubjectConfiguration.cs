using Kela.Domain.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(s => s.Name).HasMaxLength(100);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);

        // Her tenant kendi "Matematik" dersini oluşturabilir
        builder.HasIndex(s => new { s.Name, s.TenantId }).IsUnique();
    }
}
