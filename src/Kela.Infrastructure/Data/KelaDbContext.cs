using Kela.Application.Repositories;
using Kela.Domain;
using Kela.Domain.Grades;
using Kela.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Data;

public sealed class KelaDbContext(DbContextOptions<KelaDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Subject> Subjects => Set<Subject>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KelaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
