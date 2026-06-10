using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tests.Odin.DDD.Repositories.EF.Entities;

namespace Tests.Odin.DDD.Repositories.EF
{
    public class BillingPeriodConfiguration : IEntityTypeConfiguration<BillingPeriod>
    {
        public void Configure(EntityTypeBuilder<BillingPeriod> builder)
        {
            builder.ToTable("BillingPeriod", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.BillingEntityId);
            builder.Property(x => x.PeriodStarting);
            builder.Property(x => x.PeriodEnding);
            builder.Property(x => x.Stage)
                .HasConversion<short>()
                .HasColumnType("smallint");
            builder.Property(x => x.BillingStatus)
                .HasColumnName("BillingStatus")
                .HasConversion<short>()
                .HasColumnType("smallint");
            builder.HasOne(x => x.BillingEntity)
                .WithMany()
                .HasForeignKey(x => x.BillingEntityId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Properties)
                .WithOne(x => x.BillingPeriod)
                .HasForeignKey(x => x.BillingPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Tasks)
                .WithOne(x => x.BillingPeriod)
                .HasForeignKey(x => x.BillingPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.BillingEntityId, x.PeriodEnding })
                .HasDatabaseName("IX_BillingPeriod_BillingEntityIdPeriodEnding");
        }
    }
}
