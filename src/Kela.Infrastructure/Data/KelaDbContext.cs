using Kela.Application.Patterns;
using Kela.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Data;

public sealed class KelaDbContext(DbContextOptions<KelaDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options), IUnitOfWork
{
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<StudentPaymentTrack> StudentPaymentTracks => Set<StudentPaymentTrack>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KelaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
