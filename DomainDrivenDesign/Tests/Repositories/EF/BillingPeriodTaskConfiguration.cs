using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tests.Odin.DDD.Repositories.EF.Entities;

namespace Tests.Odin.DDD.Repositories.EF
{
    public class BillingPeriodTaskConfiguration : IEntityTypeConfiguration<BillingPeriodTask>
    {
        public void Configure(EntityTypeBuilder<BillingPeriodTask> builder)
        {
            builder.ToTable("BillingPeriodTask", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.BillingPeriodId);
            builder.Property(x => x.TaskType)
                .HasConversion<short>()
                .HasColumnType("smallint");
            builder.Property(x => x.Status)
                .HasConversion<short>()
                .HasColumnType("smallint");
            builder.Property(x => x.DependsOn)
                .HasMaxLength(1000)
                .IsUnicode(false);
            builder.Property(x => x.CreatedAt);
            builder.Property(x => x.LastAttemptedAt);
            builder.Property(x => x.WaitUntil);
            builder.Property(x => x.Data)
                .IsUnicode();
            builder.Property(x => x.Stage)
                .HasConversion<short>()
                .HasColumnType("smallint");
            builder.HasOne(x => x.BillingPeriod)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.BillingPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_BillingPeriodTask_Status");
            builder.HasIndex(x => x.WaitUntil)
                .HasDatabaseName("IX_BillingPeriodTask_WaitUntil");
            builder.HasIndex(x => x.TaskType)
                .HasDatabaseName("IX_BillingPeriodTask_Type");
        }
    }
}
