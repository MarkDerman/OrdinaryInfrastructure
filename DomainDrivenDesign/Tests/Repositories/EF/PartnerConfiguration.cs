using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tests.Odin.DDD.Repositories.EF.Entities;

namespace Tests.Odin.DDD.Repositories.EF
{
    public class BillingEntityConfiguration : IEntityTypeConfiguration<BillingEntity>
    {
        public void Configure(EntityTypeBuilder<BillingEntity> builder)
        {
            builder.ToTable("BillingEntity", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Name)
                .HasMaxLength(120)
                .IsUnicode();
            builder.Property(x => x.BillingName)
                .HasMaxLength(120)
                .IsUnicode();
            builder.Property(x => x.BillingAddress)
                .HasMaxLength(500)
                .IsUnicode();
            builder.Property(x => x.VatNumber)
                .HasMaxLength(50)
                .IsUnicode();
            builder.Property(x => x.Status)
                .HasConversion<short>()
                .HasColumnType("smallint");
        }
    }
}
