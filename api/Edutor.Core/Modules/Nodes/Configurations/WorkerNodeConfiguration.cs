using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Nodes.Configurations;

using WorkerNode = Edutor.Core.Modules.Nodes.Entities.WorkerNode;

public sealed class WorkerNodeConfiguration : IEntityTypeConfiguration<WorkerNode>
{
    public void Configure(EntityTypeBuilder<WorkerNode> builder)
    {
        builder.ToTable("worker_nodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(64);
        builder.Property(x => x.TokenIssuedAt).HasColumnName("token_issued_at");
        builder.Property(x => x.RegisteredAt).HasColumnName("registered_at");
        builder.Property(x => x.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(x => x.DeviceJson).HasColumnName("device_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.LimitsJson).HasColumnName("limits_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.BenchmarkJson).HasColumnName("benchmark_json").HasColumnType("jsonb");
        builder.Property(x => x.PerformanceScore).HasColumnName("performance_score");
        builder.Property(x => x.MaxConcurrentJobs).HasColumnName("max_concurrent_jobs");
        builder.Property(x => x.ActiveJobs).HasColumnName("active_jobs");
        builder.Property(x => x.CompletedJobs).HasColumnName("completed_jobs");
        builder.Property(x => x.FailedJobs).HasColumnName("failed_jobs");
        builder.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true);
        builder.Property(x => x.Version).HasColumnName("version").HasMaxLength(40);

        builder.HasIndex(x => x.TokenHash).IsUnique().HasFilter("token_hash IS NOT NULL");
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.LastSeenAt);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
