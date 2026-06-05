using Microsoft.EntityFrameworkCore;
using Odin.DDD;
using Tests.Odin.DDD.Repositories.EF.Entities;

namespace Tests.Odin.DDD.Repositories.EF
{
    public interface ITestDatabaseContext : IDisposable, IAsyncDisposable
    {
        DbSet<BillingEntity> BillingEntities { get;  }
        DbSet<BillingPeriod> BillingPeriods { get;  }
        DbSet<BillingPeriodProperty> BillingPeriodProperties { get; }
        DbSet<BillingPeriodTask> BillingPeriodTasks { get; }
    }
    
    public class TestDatabaseContext : DbContext, ITestDatabaseContext, IUnitOfWork
    {
        public TestDatabaseContext(DbContextOptions<TestDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BillingEntity> BillingEntities { get; set; } = null!;
        public virtual DbSet<BillingPeriod> BillingPeriods { get; set; } = null!;
        public virtual DbSet<BillingPeriodProperty> BillingPeriodProperties { get; set; } = null!;
        public virtual DbSet<BillingPeriodTask> BillingPeriodTasks { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new BillingEntityConfiguration());
            modelBuilder.ApplyConfiguration(new BillingPeriodConfiguration());
            modelBuilder.ApplyConfiguration(new BillingPeriodPropertyConfiguration());
            modelBuilder.ApplyConfiguration(new BillingPeriodTaskConfiguration());
        }
    }
}
