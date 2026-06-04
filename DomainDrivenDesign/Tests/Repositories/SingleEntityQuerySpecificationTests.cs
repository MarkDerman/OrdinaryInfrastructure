using Odin.DDD;
using Odin.DDD.Repositories;
using System.Linq.Expressions;

namespace Tests.Odin.DDD.Repositories;

public sealed class SingleEntityQuerySpecificationTests
{
    [Test]
    public void SingleEntityQuerySpecification_requires_criteria()
    {
        Assert.Throws<ArgumentNullException>(() => new SingleEntityQuerySpecification<TestAggregateRoot>(null!));
    }

    [Test]
    public void SingleEntityQuerySpecification_stores_criteria()
    {
        Expression<Func<TestAggregateRoot, bool>> criteria = aggregateRoot => aggregateRoot.Name == "test";

        SingleEntityQuerySpecification<TestAggregateRoot> sut = new SingleEntityQuerySpecification<TestAggregateRoot>(criteria);

        Assert.That(sut.Criteria, Is.SameAs(criteria));
        Assert.That(sut.Criteria.Compile()(new TestAggregateRoot { Name = "test" }), Is.True);
        Assert.That(sut.Includes, Is.Null);
    }

    [Test]
    public void SingleEntityQuerySpecification_stores_constructor_includes()
    {
        Expression<Func<TestAggregateRoot, object>> nameInclude = aggregateRoot => aggregateRoot.Name;
        Expression<Func<TestAggregateRoot, object>> idInclude = aggregateRoot => aggregateRoot.Id;

        SingleEntityQuerySpecification<TestAggregateRoot> sut = new SingleEntityQuerySpecification<TestAggregateRoot>(
            aggregateRoot => aggregateRoot.Id == 1,
            [nameInclude, idInclude]);

        Assert.That(sut.Includes, Is.Not.Null);
        Assert.That(sut.Includes, Is.EqualTo([nameInclude, idInclude]));
    }

    [Test]
    public void SingleEntityQuerySpecification_adds_includes()
    {
        Expression<Func<TestAggregateRoot, object>> include = aggregateRoot => aggregateRoot.Name;
        SingleEntityQuerySpecification<TestAggregateRoot> sut =
            new SingleEntityQuerySpecification<TestAggregateRoot>(aggregateRoot => aggregateRoot.Id == 1);

        sut.AddInclude(include);

        Assert.That(sut.Includes, Is.Not.Null);
        sut.Includes.Single();
        Assert.That(sut.Includes[0], Is.SameAs(include));
    }

    private sealed class TestAggregateRoot : IIdentityAggregateRoot<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}