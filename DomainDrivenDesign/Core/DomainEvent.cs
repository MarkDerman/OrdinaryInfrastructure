
namespace Odin.DDD
{
    /// <summary>
    /// Base class for a domain event.
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="occurredAt"></param>
        protected DomainEvent(DateTimeOffset occurredAt)
        {
            OccurredAt = occurredAt;
        }

        /// <summary>
        /// Whether the event has been published or not.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Gets the timestamp when the event occurred.
        /// </summary>
        public DateTimeOffset OccurredAt { get; protected set; }
    }
}