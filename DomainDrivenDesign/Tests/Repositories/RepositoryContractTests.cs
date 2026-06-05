using Odin.DDD.Repositories;
using Tests.Odin.DDD.Repositories.EF;
using Tests.Odin.DDD.Repositories.EF.Entities;

namespace Tests.Odin.DDD.Repositories;

public sealed class RepositoryContractTests
{
    [Test]
    public void Read_write_repository_is_a_read_only_repository()
    {
        Assert.That(
            typeof(IReadOnlyRepository<BillingPeriod>).IsAssignableFrom(typeof(IRepository<BillingPeriod>)),
            Is.True);
    }

    [Test]
    public void Entity_framework_read_only_repository_does_not_implement_read_write_contract()
    {
        Assert.That(
            typeof(IRepository<BillingPeriod>).IsAssignableFrom(typeof(ReadOnlyBillingPeriodRepository)),
            Is.False);
    }

    [Test]
    public void Entity_framework_repository_implements_read_write_and_read_only_contracts()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                typeof(IRepository<BillingPeriod>).IsAssignableFrom(typeof(ReadWriteBillingPeriodRepository)),
                Is.True);
            Assert.That(
                typeof(IReadOnlyRepository<BillingPeriod>).IsAssignableFrom(typeof(ReadWriteBillingPeriodRepository)),
                Is.True);
        });
    }

    [Test]
    public void Entity_framework_read_only_identity_repository_implements_read_only_contract()
    {
        Assert.That(
            typeof(IReadOnlyRepository<BillingEntity>).IsAssignableFrom(typeof(ReadOnlyBillingEntityRepository)),
            Is.True);
    }

    private sealed class ReadOnlyBillingPeriodRepository
        : EntityFrameworkReadOnlyRepositoryBase<BillingPeriod, TestDatabaseContext>
    {
        public ReadOnlyBillingPeriodRepository(TestDatabaseContext dbContext)
            : base(dbContext)
        {
        }
    }

    private sealed class ReadWriteBillingPeriodRepository
        : EntityFrameworkRepositoryBase<BillingPeriod, TestDatabaseContext>
    {
        public ReadWriteBillingPeriodRepository(TestDatabaseContext dbContext)
            : base(dbContext)
        {
        }
    }

    private sealed class ReadOnlyBillingEntityRepository
        : EntityFrameworkReadOnlyIdentityRepositoryBase<BillingEntity, int, TestDatabaseContext>
    {
        public ReadOnlyBillingEntityRepository(TestDatabaseContext dbContext)
            : base(dbContext)
        {
        }
    }
}
