namespace Odin.Patterns.Notifications;

/// <summary>
/// Defines the publishing implementation for a notification request.
/// </summary>
/// <typeparam name="TNotification"></typeparam>
public interface INotificationHandler<in TNotification> 
    where TNotification : INotification
{
    /// <summary>
    /// Handles the command request
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task HandleAsync(TNotification command, CancellationToken ct = default);
}
