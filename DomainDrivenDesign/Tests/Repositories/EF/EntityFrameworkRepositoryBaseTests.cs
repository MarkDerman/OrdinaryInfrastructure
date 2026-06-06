using Microsoft.EntityFrameworkCore;
using Odin.DDD.Repositories;
using Tests.Odin.DDD.Repositories.Database;
using Tests.Odin.DDD.Repositories.EF.Builders;
using Tests.Odin.DDD.Repositories.EF.Entities;
using Tests.Odin.DDD.Repositories.EF.Fixtures;
using Tests.Odin.DDD.Repositories.EF.TestRepositories;

namespace Tests.Odin.DDD.Repositories.EF;

[TestFixtureSource(typeof(DatabaseTestFixtureSource), nameof(DatabaseTestFixtureSource.SupportedDatabases))]
[Category("IntegrationTest")]
public sealed class EntityFrameworkRepositoryBaseTests : DatabaseTestBase
{
    public EntityFrameworkRepositoryBaseTests(DatabaseTestContainerFixture testDatabase)
        : base(testDatabase)
    {
    }

    [Test]
    public async Task FetchManyAsync_applies_criteria_ordering_and_paging()
    {
        BillingEntity alpha = new BillingEntityBuilder().WithName("Alpha").Build();
        BillingEntity bravo = new BillingEntityBuilder().WithName("Bravo").Build();
        BillingEntity charlie = new BillingEntityBuilder().WithName("Charlie").Build();
        BillingEntity inactive = new BillingEntityBuilder()
            .WithName("Inactive")
            .WithStatus(BillingEntityStatus.NotActive)
            .Build();
        await SeedBillingEntitiesAsync(alpha, charlie, inactive, bravo);

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);
        TestQuerySpecification<BillingEntity> specification =
            new TestQuerySpecification<BillingEntity>(billingEntity => billingEntity.Status == BillingEntityStatus.Active)
                .OrderAscending(billingEntity => billingEntity.Name)
                .ApplyPage(pageNumber: 1, pageSize: 2);

        IReadOnlyList<BillingEntity> results = await repository.ListAsync(specification);

        Assert.That(results.Select(billingEntity => billingEntity.Name), Is.EqualTo(new[] { "Alpha", "Bravo" }));
    }

    [Test]
    public async Task FetchManyAsync_applies_projection_expression()
    {
        await SeedBillingEntitiesAsync(
            new BillingEntityBuilder().WithName("Alpha").Build(),
            new BillingEntityBuilder().WithName("Bravo").Build());

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);
        TestQuerySpecification<BillingEntity> specification =
            new TestQuerySpecification<BillingEntity>().OrderDescending(billingEntity => billingEntity.Name);

        IReadOnlyList<string> names = await repository.ListAsync(specification, billingEntity => billingEntity.Name);

        Assert.That(names, Is.EqualTo(new[] { "Bravo", "Alpha" }));
    }

    [Test]
    public async Task FetchManyAsync_applies_projected_query_specification()
    {
        BillingEntity active = new BillingEntityBuilder().WithName("Active").Build();
        BillingEntity inactive = new BillingEntityBuilder()
            .WithName("Inactive")
            .WithStatus(BillingEntityStatus.NotActive)
            .Build();
        await SeedBillingEntitiesAsync(active, inactive);

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);
        TestProjectedBillingEntitySpecification specification =
            new TestProjectedBillingEntitySpecification(billingEntity => billingEntity.Status == BillingEntityStatus.Active);

        IReadOnlyList<BillingEntitySummary> summaries = await repository.ListAsync(specification);

        Assert.That(summaries, Has.Count.EqualTo(1));
        Assert.That(summaries[0], Is.EqualTo(new BillingEntitySummary(active.Id, "Active")));
    }

    [Test]
    public async Task FetchManyAsync_applies_includes()
    {
        BillingEntity billingEntity = new BillingEntityBuilder().WithName("Included Entity").Build();
        BillingPeriod billingPeriod = new BillingPeriodBuilder()
            .ForBillingEntity(billingEntity)
            .WithPeriod(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28))
            .Build();
        await SeedBillingPeriodsAsync(billingPeriod);

        await using TestDatabaseContext context = CreateContext();
        TestBillingPeriodReadOnlyRepository repository = new TestBillingPeriodReadOnlyRepository(context);
        TestQuerySpecification<BillingPeriod> specification =
            new TestQuerySpecification<BillingPeriod>().Include(period => period.BillingEntity);

        IReadOnlyList<BillingPeriod> results = await repository.ListAsync(specification);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].BillingEntity.Name, Is.EqualTo("Included Entity"));
    }

    [Test]
    public async Task FetchSingleAsync_returns_null_when_no_entity_matches()
    {
        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);

        BillingEntity? result = await repository.SingleAsync(
            new SingleEntityQuerySpecification<BillingEntity>(billingEntity => billingEntity.Name == "Missing"));

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task FetchSingleAsync_returns_matching_entity()
    {
        BillingEntity expected = new BillingEntityBuilder().WithName("Expected").Build();
        await SeedBillingEntitiesAsync(expected);

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);

        BillingEntity? result = await repository.SingleAsync(
            new SingleEntityQuerySpecification<BillingEntity>(billingEntity => billingEntity.Name == "Expected"));

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(expected.Id));
    }

    [Test]
    public async Task FetchSingleAsync_throws_when_multiple_entities_match()
    {
        await SeedBillingEntitiesAsync(
            new BillingEntityBuilder().WithName("Duplicate").Build(),
            new BillingEntityBuilder().WithName("Duplicate").Build());

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);

        InvalidOperationException? exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repository.SingleAsync(
                new SingleEntityQuerySpecification<BillingEntity>(billingEntity => billingEntity.Name == "Duplicate")));

        Assert.That(exception!.Message, Is.EqualTo("Expected single result, but found 2."));
    }

    [Test]
    public async Task FetchIdsAsync_returns_matching_identity_values()
    {
        BillingEntity alpha = new BillingEntityBuilder().WithName("Alpha").Build();
        BillingEntity inactive = new BillingEntityBuilder()
            .WithName("Inactive")
            .WithStatus(BillingEntityStatus.NotActive)
            .Build();
        await SeedBillingEntitiesAsync(alpha, inactive);

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);
        TestQuerySpecification<BillingEntity> specification =
            new TestQuerySpecification<BillingEntity>(billingEntity => billingEntity.Status == BillingEntityStatus.Active);

        IReadOnlyList<int> ids = await repository.ListIdsAsync(specification);

        Assert.That(ids, Is.EqualTo(new[] { alpha.Id }));
    }

    [Test]
    public async Task Read_only_queries_use_no_tracking()
    {
        await SeedBillingEntitiesAsync(new BillingEntityBuilder().WithName("Detached").Build());

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityReadOnlyRepository repository = new TestBillingEntityReadOnlyRepository(context);

        BillingEntity? result = await repository.SingleAsync(
            new SingleEntityQuerySpecification<BillingEntity>(billingEntity => billingEntity.Name == "Detached"));

        Assert.That(result, Is.Not.Null);
        Assert.That(context.Entry(result!).State, Is.EqualTo(EntityState.Detached));
    }

    [Test]
    public async Task Read_write_queries_use_tracking()
    {
        await SeedBillingEntitiesAsync(new BillingEntityBuilder().WithName("Tracked").Build());

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityRepository repository = new TestBillingEntityRepository(context);

        BillingEntity? result = await repository.SingleAsync(
            new SingleEntityQuerySpecification<BillingEntity>(billingEntity => billingEntity.Name == "Tracked"));

        Assert.That(result, Is.Not.Null);
        Assert.That(context.Entry(result!).State, Is.EqualTo(EntityState.Unchanged));
    }

    [Test]
    public async Task Add_persists_entity_when_unit_of_work_saves()
    {
        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityRepository repository = new TestBillingEntityRepository(context);
        BillingEntity billingEntity = new BillingEntityBuilder().WithName("Added").Build();

        repository.Add(billingEntity);
        await repository.UnitOfWork.SaveChangesAsync();

        await using TestDatabaseContext assertContext = CreateContext();
        bool exists = await assertContext.BillingEntities.AnyAsync(entity => entity.Name == "Added");
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task AddRange_persists_entities_when_unit_of_work_saves()
    {
        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityRepository repository = new TestBillingEntityRepository(context);
        BillingEntity[] billingEntities =
        [
            new BillingEntityBuilder().WithName("Range 1").Build(),
            new BillingEntityBuilder().WithName("Range 2").Build()
        ];

        repository.AddRange(billingEntities);
        await repository.UnitOfWork.SaveChangesAsync();

        await using TestDatabaseContext assertContext = CreateContext();
        int count = await assertContext.BillingEntities.CountAsync(entity => entity.Name.StartsWith("Range "));
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public async Task Update_persists_entity_changes_when_unit_of_work_saves()
    {
        BillingEntity billingEntity = new BillingEntityBuilder().WithName("Before Update").Build();
        await SeedBillingEntitiesAsync(billingEntity);

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityRepository repository = new TestBillingEntityRepository(context);
        BillingEntity entityToUpdate = (await repository.SingleAsync(
            new SingleEntityQuerySpecification<BillingEntity>(entity => entity.Id == billingEntity.Id)))!;
        entityToUpdate.BillingName = "Updated Billing Name";

        repository.Update(entityToUpdate);
        await repository.UnitOfWork.SaveChangesAsync();

        await using TestDatabaseContext assertContext = CreateContext();
        string? billingName = await assertContext.BillingEntities
            .Where(entity => entity.Id == billingEntity.Id)
            .Select(entity => entity.BillingName)
            .SingleAsync();
        Assert.That(billingName, Is.EqualTo("Updated Billing Name"));
    }

    [Test]
    public async Task Delete_removes_entity_when_unit_of_work_saves()
    {
        BillingEntity billingEntity = new BillingEntityBuilder().WithName("Delete Me").Build();
        await SeedBillingEntitiesAsync(billingEntity);

        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityRepository repository = new TestBillingEntityRepository(context);
        BillingEntity entityToDelete = (await repository.SingleAsync(
            new SingleEntityQuerySpecification<BillingEntity>(entity => entity.Id == billingEntity.Id)))!;

        repository.Delete(entityToDelete);
        await repository.UnitOfWork.SaveChangesAsync();

        await using TestDatabaseContext assertContext = CreateContext();
        bool exists = await assertContext.BillingEntities.AnyAsync(entity => entity.Id == billingEntity.Id);
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task UnitOfWork_returns_the_repository_context()
    {
        await using TestDatabaseContext context = CreateContext();
        TestBillingEntityRepository repository = new TestBillingEntityRepository(context);

        Assert.That(repository.UnitOfWork, Is.SameAs(context));
        await Task.CompletedTask;
    }

    [Test]
    public async Task Seeder_persists_aggregate_graphs_created_by_builders()
    {
        BillingPeriod billingPeriod = new BillingPeriodBuilder()
            .WithProperty("ExternalId", DataType.String, "external-123")
            .WithTask(
                new BillingPeriodTaskBuilder()
                    .WithTaskType(BillingPeriodTaskType.CreateEndOfBillingPeriodSageARCustomerInvoice)
                    .WithStage(BillingPeriodStage.PostCustomerInvoice)
                    .WithData("""{"invoice":"draft"}"""))
            .Build();

        await SeedBillingPeriodsAsync(billingPeriod);

        await using TestDatabaseContext context = CreateContext();
        BillingPeriod savedPeriod = await context.BillingPeriods
            .Include(period => period.Properties)
            .Include(period => period.Tasks)
            .SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(savedPeriod.Properties, Has.Count.EqualTo(1));
            Assert.That(savedPeriod.Properties.Single().PropertyName, Is.EqualTo("ExternalId"));
            Assert.That(savedPeriod.Tasks, Has.Count.EqualTo(1));
            Assert.That(savedPeriod.Tasks.Single().TaskType,
                Is.EqualTo(BillingPeriodTaskType.CreateEndOfBillingPeriodSageARCustomerInvoice));
        });
    }

    private async Task SeedBillingEntitiesAsync(params BillingEntity[] billingEntities)
    {
        await using TestDatabaseContext context = CreateContext();
        await CreateSeeder(context)
            .AddMany(billingEntities)
            .SaveChangesAsync();
    }

    private async Task SeedBillingPeriodsAsync(params BillingPeriod[] billingPeriods)
    {
        await using TestDatabaseContext context = CreateContext();
        await CreateSeeder(context)
            .AddMany(billingPeriods)
            .SaveChangesAsync();
    }
}
