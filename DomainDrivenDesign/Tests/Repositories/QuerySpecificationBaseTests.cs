using Odin.DDD;
using Odin.DDD.Repositories;
using System.Linq.Expressions;

namespace Tests.Odin.DDD.Repositories;

public sealed class QuerySpecificationBaseTests
{
    [Fact]
    public void QuerySpecification_defaults_to_no_filtering_or_paging()
    {
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        Assert.Null(sut.Criteria);
        Assert.Null(sut.Includes);
        Assert.Null(sut.Orderings);
        Assert.Null(sut.Page);
    }

    [Fact]
    public void QuerySpecification_stores_criteria()
    {
        Expression<Func<TestAggregateRoot, bool>> criteria = aggregateRoot => aggregateRoot.Id == 123;

        TestQuerySpecificationBase sut = new TestQuerySpecificationBase(criteria);

        Assert.Same(criteria, sut.Criteria);
        Assert.True(sut.Criteria!.Compile()(new TestAggregateRoot { Id = 123 }));
    }

    [Fact]
    public void QuerySpecification_adds_include_expressions()
    {
        Expression<Func<TestAggregateRoot, object>> include = aggregateRoot => aggregateRoot.Name;
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.Include(include);

        Assert.NotNull(sut.Includes);
        Assert.Single(sut.Includes);
        Assert.Same(include, sut.Includes[0]);
    }

    [Fact]
    public void QuerySpecification_applies_incremental_ordering()
    {
        Expression<Func<TestAggregateRoot, object>> orderBy = aggregateRoot => aggregateRoot.Name;
        Expression<Func<TestAggregateRoot, object>> thenByDescending = aggregateRoot => aggregateRoot.Id;
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.OrderAscending(orderBy);
        sut.ThenDescending(thenByDescending);

        Assert.NotNull(sut.Orderings);
        Assert.Equal(2, sut.Orderings.Count);
        Assert.Same(orderBy, sut.Orderings[0].Expression);
        Assert.Equal(SortDirection.Ascending, sut.Orderings[0].Direction);
        Assert.Same(thenByDescending, sut.Orderings[1].Expression);
        Assert.Equal(SortDirection.Descending, sut.Orderings[1].Direction);
    }

    [Fact]
    public void QuerySpecification_applies_batch_ordering()
    {
        Expression<Func<TestAggregateRoot, object>> orderByDescending = aggregateRoot => aggregateRoot.Name;
        Expression<Func<TestAggregateRoot, object>> thenBy = aggregateRoot => aggregateRoot.Id;
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.Order(
            QueryOrdering<TestAggregateRoot>.Descending(orderByDescending),
            QueryOrdering<TestAggregateRoot>.Ascending(thenBy));

        Assert.NotNull(sut.Orderings);
        Assert.Equal(2, sut.Orderings.Count);
        Assert.Same(orderByDescending, sut.Orderings[0].Expression);
        Assert.Equal(SortDirection.Descending, sut.Orderings[0].Direction);
        Assert.Same(thenBy, sut.Orderings[1].Expression);
        Assert.Equal(SortDirection.Ascending, sut.Orderings[1].Direction);
    }

    [Fact]
    public void QuerySpecification_applies_paging()
    {
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.ApplyPage(new Pagination(pageNumber: 3, pageSize: 25));

        Assert.NotNull(sut.Page);
        Assert.Equal(3, sut.Page.PageNumber);
        Assert.Equal(25, sut.Page.PageSize);
    }

    [Fact]
    public void Pagination_stores_page_number_and_page_size()
    {
        Pagination sut = new Pagination(pageNumber: 3, pageSize: 25);

        Assert.Equal(3, sut.PageNumber);
        Assert.Equal(25, sut.PageSize);
    }

    [Fact]
    public void Pagination_calculates_skip_and_take_from_page_number_and_page_size()
    {
        Pagination sut = new Pagination(pageNumber: 3, pageSize: 25);

        Assert.Equal(50, sut.Skip);
        Assert.Equal(25, sut.Take);
    }

    [Theory]
    [InlineData(0, 25)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Pagination_rejects_invalid_page_number_or_page_size(int pageNumber, int pageSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Pagination(pageNumber, pageSize));
    }

    private sealed class TestQuerySpecificationBase : QuerySpecificationBase<TestAggregateRoot>
    {
        public TestQuerySpecificationBase()
        {
        }

        public TestQuerySpecificationBase(Expression<Func<TestAggregateRoot, bool>> criteria) 
        {
            ApplyCriteria(criteria);
        }

        public void Include(Expression<Func<TestAggregateRoot, object>> includeExpression)
        {
            AddInclude(includeExpression);
        }

        public void OrderAscending(Expression<Func<TestAggregateRoot, object>> orderByExpression)
        {
            AddOrderByAscending(orderByExpression);
        }

        public void OrderDescending(Expression<Func<TestAggregateRoot, object>> orderByExpression)
        {
            ApplyOrderByDescending(orderByExpression);
        }

        public void ThenAscending(Expression<Func<TestAggregateRoot, object>> thenByExpression)
        {
            ApplyThenByAscending(thenByExpression);
        }

        public void ThenDescending(Expression<Func<TestAggregateRoot, object>> thenByExpression)
        {
            ApplyThenByDescending(thenByExpression);
        }

        public void Order(params QueryOrdering<TestAggregateRoot>[] orderings)
        {
            ApplyOrdering(orderings);
        }

        public void ApplyPage(Pagination page)
        {
            ApplyPaging(page);
        }
    }

    private sealed class TestAggregateRoot : IIdentityAggregateRoot<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
