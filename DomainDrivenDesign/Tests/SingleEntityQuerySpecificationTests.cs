using System.Linq.Expressions;
using Odin.DDD;

namespace Tests.Odin.DDD.Abstractions;

public sealed class SingleEntityQuerySpecificationTests
{
    [Fact]
    public void SingleEntityQuerySpecification_requires_criteria()
    {
        Assert.Throws<ArgumentNullException>(() => new SingleEntityQuerySpecification<TestAggregateRoot>(null!));
    }

    [Fact]
    public void SingleEntityQuerySpecification_stores_criteria()
    {
        Expression<Func<TestAggregateRoot, bool>> criteria = aggregateRoot => aggregateRoot.Name == "test";

        SingleEntityQuerySpecification<TestAggregateRoot> sut = new SingleEntityQuerySpecification<TestAggregateRoot>(criteria);

        Assert.Same(criteria, sut.Criteria);
        Assert.True(sut.Criteria.Compile()(new TestAggregateRoot { Name = "test" }));
        Assert.Null(sut.Includes);
    }

    [Fact]
    public void SingleEntityQuerySpecification_stores_constructor_includes()
    {
        Expression<Func<TestAggregateRoot, object>> nameInclude = aggregateRoot => aggregateRoot.Name;
        Expression<Func<TestAggregateRoot, object>> idInclude = aggregateRoot => aggregateRoot.Id;

        SingleEntityQuerySpecification<TestAggregateRoot> sut = new SingleEntityQuerySpecification<TestAggregateRoot>(
            aggregateRoot => aggregateRoot.Id == 1,
            [nameInclude, idInclude]);

        Assert.NotNull(sut.Includes);
        Assert.Equal([nameInclude, idInclude], sut.Includes);
    }

    [Fact]
    public void SingleEntityQuerySpecification_adds_includes()
    {
        Expression<Func<TestAggregateRoot, object>> include = aggregateRoot => aggregateRoot.Name;
        SingleEntityQuerySpecification<TestAggregateRoot> sut =
            new SingleEntityQuerySpecification<TestAggregateRoot>(aggregateRoot => aggregateRoot.Id == 1);

        sut.AddInclude(include);

        Assert.NotNull(sut.Includes);
        Assert.Single(sut.Includes);
        Assert.Same(include, sut.Includes[0]);
    }

    private sealed class TestAggregateRoot : IIdentityAggregateRoot<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
