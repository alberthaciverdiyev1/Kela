using Kela.Application.Patterns;
using Kela.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Data;

public sealed class KelaDbContext(DbContextOptions<KelaDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options), IUnitOfWork
{
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<StudentPaymentTrack> StudentPaymentTracks => Set<StudentPaymentTrack>();
    public DbSet<BaseSiteConfiguration> BaseSiteConfigurations => Set<BaseSiteConfiguration>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KelaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
