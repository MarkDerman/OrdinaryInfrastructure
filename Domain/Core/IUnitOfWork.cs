
namespace Odin.Domain
{
    /// <summary>
    /// Intended to wrap DbContext.SaveChangesAsync to support domain event publishing
    /// at database persistence time.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Commit database changes and publish any unpublished domain events.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}