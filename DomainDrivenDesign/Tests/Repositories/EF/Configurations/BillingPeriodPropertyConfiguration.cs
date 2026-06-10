using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tests.Odin.DDD.Repositories.TestDomain;

namespace Tests.Odin.DDD.Repositories.EF.Configurations
{
    public class BillingPeriodPropertyConfiguration : IEntityTypeConfiguration<BillingPeriodProperty>
    {
        public void Configure(EntityTypeBuilder<BillingPeriodProperty> builder)
        {
            builder.ToTable("BillingPeriodProperty", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.BillingPeriodId);
            builder.Property(x => x.PropertyName)
                .HasMaxLength(50)
                .IsUnicode();
            builder.Property(x => x.DataType)
                .HasConversion<short>()
                .HasColumnType("smallint");
            builder.Property(x => x.DataValue)
                .IsUnicode();
            builder.HasOne(x => x.BillingPeriod)
                .WithMany(x => x.Properties)
                .HasForeignKey(x => x.BillingPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.BillingPeriodId, x.PropertyName })
                .IsUnique()
                .HasDatabaseName("IX_BillingPeriodProperty_BillingPeriodId_PropertyName");
        }
    }
}
