using Odin.DDD;

namespace Tests.Odin.DDD;

public sealed class DomainEventTests
{
    [Test]
    public void DomainEvent_sets_occurred_at_and_defaults_to_unpublished()
    {
        DateTimeOffset occurredAt = new DateTimeOffset(2026, 5, 14, 10, 30, 0, TimeSpan.Zero);

        TestDomainEvent sut = new TestDomainEvent(occurredAt);

        Assert.That(sut.OccurredAt, Is.EqualTo(occurredAt));
        Assert.That(sut.IsPublished, Is.False);
    }

    [Test]
    public void DomainEvent_allows_published_state_to_be_changed()
    {
        TestDomainEvent sut = new TestDomainEvent(DateTimeOffset.UtcNow);

        sut.IsPublished = true;

        Assert.That(sut.IsPublished, Is.True);
    }

    private sealed class TestDomainEvent : DomainEvent
    {
        public TestDomainEvent(DateTimeOffset now) : base(now)
        {
        }
    }
}