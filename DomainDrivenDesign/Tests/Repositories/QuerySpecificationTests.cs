using Odin.DDD;
using Odin.DDD.Repositories;
using System.Linq.Expressions;

namespace Tests.Odin.DDD.Repositories;

public sealed class QuerySpecificationTests
{
    [Fact]
    public void QuerySpecification_defaults_to_no_filtering_or_paging()
    {
        TestQuerySpecification sut = new TestQuerySpecification();

        Assert.Null(sut.Criteria);
        Assert.Null(sut.Includes);
        Assert.Null(sut.OrderBy);
        Assert.Null(sut.OrderByDescending);
        Assert.Equal(0, sut.Skip);
        Assert.Equal(0, sut.Take);
        Assert.False(sut.IsPagingEnabled);
    }

    [Fact]
    public void QuerySpecification_stores_criteria()
    {
        Expression<Func<TestAggregateRoot, bool>> criteria = aggregateRoot => aggregateRoot.Id == 123;

        TestQuerySpecification sut = new TestQuerySpecification(criteria);

        Assert.Same(criteria, sut.Criteria);
        Assert.True(sut.Criteria!.Compile()(new TestAggregateRoot { Id = 123 }));
    }

    [Fact]
    public void QuerySpecification_adds_include_expressions()
    {
        Expression<Func<TestAggregateRoot, object>> include = aggregateRoot => aggregateRoot.Name;
        TestQuerySpecification sut = new TestQuerySpecification();

        sut.Include(include);

        Assert.NotNull(sut.Includes);
        Assert.Single(sut.Includes);
        Assert.Same(include, sut.Includes[0]);
    }

    [Fact]
    public void QuerySpecification_applies_ordering()
    {
        Expression<Func<TestAggregateRoot, object>> orderBy = aggregateRoot => aggregateRoot.Name;
        Expression<Func<TestAggregateRoot, object>> orderByDescending = aggregateRoot => aggregateRoot.Id;
        TestQuerySpecification sut = new TestQuerySpecification();

        sut.OrderAscending(orderBy);
        sut.OrderDescending(orderByDescending);

        Assert.Same(orderBy, sut.OrderBy);
        Assert.Same(orderByDescending, sut.OrderByDescending);
    }

    [Fact]
    public void QuerySpecification_applies_paging()
    {
        TestQuerySpecification sut = new TestQuerySpecification();

        sut.Page(skip: 10, take: 25);

        Assert.Equal(10, sut.Skip);
        Assert.Equal(25, sut.Take);
        Assert.True(sut.IsPagingEnabled);
    }

    private sealed class TestQuerySpecification : QuerySpecification<TestAggregateRoot>
    {
        public TestQuerySpecification()
        {
        }

        public TestQuerySpecification(Expression<Func<TestAggregateRoot, bool>> criteria) : base(criteria)
        {
        }

        public void Include(Expression<Func<TestAggregateRoot, object>> includeExpression)
        {
            AddInclude(includeExpression);
        }

        public void OrderAscending(Expression<Func<TestAggregateRoot, object>> orderByExpression)
        {
            ApplyOrderBy(orderByExpression);
        }

        public void OrderDescending(Expression<Func<TestAggregateRoot, object>> orderByExpression)
        {
            ApplyOrderByDescending(orderByExpression);
        }

        public void Page(int skip, int take)
        {
            ApplyPaging(skip, take);
        }
    }

    private sealed class TestAggregateRoot : IIdentityAggregateRoot<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}