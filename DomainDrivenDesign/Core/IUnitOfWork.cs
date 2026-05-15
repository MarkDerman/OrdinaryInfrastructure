
namespace Odin.DDD
{
    /// <summary>
    /// Intended to wrap DbContext.SaveChangesAsync to support domain event publishing
    /// at database persistence time.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Commits any changes made during the unit of work.
        /// This typically includes persisting entity changes to a database,
        /// firing unpublished domain events, and any other necessary post-save operations.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}