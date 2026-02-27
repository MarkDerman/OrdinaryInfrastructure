
using System;

namespace Odin.Domain
{
    /// <summary>
    /// Base class for a domain event.
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="now"></param>
        protected DomainEvent(DateTimeOffset now)
        {
            OccurredAt = now;
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