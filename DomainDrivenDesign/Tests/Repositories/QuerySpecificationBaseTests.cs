using Odin.DDD;
using Odin.DDD.Repositories;
using System.Linq.Expressions;

namespace Tests.Odin.DDD.Repositories;

public sealed class QuerySpecificationBaseTests
{
    [Test]
    public void QuerySpecification_defaults_to_no_filtering_or_paging()
    {
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        Assert.That(sut.Criteria, Is.Null);
        Assert.That(sut.Includes, Is.Null);
        Assert.That(sut.Orderings, Is.Null);
        Assert.That(sut.Page, Is.Null);
    }

    [Test]
    public void QuerySpecification_stores_criteria()
    {
        Expression<Func<TestAggregateRoot, bool>> criteria = aggregateRoot => aggregateRoot.Id == 123;

        TestQuerySpecificationBase sut = new TestQuerySpecificationBase(criteria);

        Assert.That(sut.Criteria, Is.SameAs(criteria));
        Assert.That(sut.Criteria!.Compile()(new TestAggregateRoot { Id = 123 }), Is.True);
    }

    [Test]
    public void QuerySpecification_adds_include_expressions()
    {
        Expression<Func<TestAggregateRoot, object>> include = aggregateRoot => aggregateRoot.Name;
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.Include(include);

        Assert.That(sut.Includes, Is.Not.Null);
        sut.Includes.Single();
        Assert.That(sut.Includes[0], Is.SameAs(include));
    }

    [Test]
    public void QuerySpecification_applies_incremental_ordering()
    {
        Expression<Func<TestAggregateRoot, object>> orderBy = aggregateRoot => aggregateRoot.Name;
        Expression<Func<TestAggregateRoot, object>> thenByDescending = aggregateRoot => aggregateRoot.Id;
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.OrderAscending(orderBy);
        sut.ThenDescending(thenByDescending);

        Assert.That(sut.Orderings, Is.Not.Null);
        Assert.That(sut.Orderings.Count, Is.EqualTo(2));
        Assert.That(sut.Orderings[0].Expression, Is.SameAs(orderBy));
        Assert.That(sut.Orderings[0].Direction, Is.EqualTo(SortDirection.Ascending));
        Assert.That(sut.Orderings[1].Expression, Is.SameAs(thenByDescending));
        Assert.That(sut.Orderings[1].Direction, Is.EqualTo(SortDirection.Descending));
    }

    [Test]
    public void QuerySpecification_applies_batch_ordering()
    {
        Expression<Func<TestAggregateRoot, object>> orderByDescending = aggregateRoot => aggregateRoot.Name;
        Expression<Func<TestAggregateRoot, object>> thenBy = aggregateRoot => aggregateRoot.Id;
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.Order(
            QueryOrdering<TestAggregateRoot>.Descending(orderByDescending),
            QueryOrdering<TestAggregateRoot>.Ascending(thenBy));

        Assert.That(sut.Orderings, Is.Not.Null);
        Assert.That(sut.Orderings.Count, Is.EqualTo(2));
        Assert.That(sut.Orderings[0].Expression, Is.SameAs(orderByDescending));
        Assert.That(sut.Orderings[0].Direction, Is.EqualTo(SortDirection.Descending));
        Assert.That(sut.Orderings[1].Expression, Is.SameAs(thenBy));
        Assert.That(sut.Orderings[1].Direction, Is.EqualTo(SortDirection.Ascending));
    }

    [Test]
    public void QuerySpecification_applies_paging()
    {
        TestQuerySpecificationBase sut = new TestQuerySpecificationBase();

        sut.ApplyPage(new Pagination(pageNumber: 3, pageSize: 25));

        Assert.That(sut.Page, Is.Not.Null);
        Assert.That(sut.Page.PageNumber, Is.EqualTo(3));
        Assert.That(sut.Page.PageSize, Is.EqualTo(25));
    }

    [Test]
    public void Pagination_stores_page_number_and_page_size()
    {
        Pagination sut = new Pagination(pageNumber: 3, pageSize: 25);

        Assert.That(sut.PageNumber, Is.EqualTo(3));
        Assert.That(sut.PageSize, Is.EqualTo(25));
    }

    [Test]
    public void Pagination_calculates_skip_and_take_from_page_number_and_page_size()
    {
        Pagination sut = new Pagination(pageNumber: 3, pageSize: 25);

        Assert.That(sut.Skip, Is.EqualTo(50));
        Assert.That(sut.Take, Is.EqualTo(25));
    }
    [TestCase(0, 25)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
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
