using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Skips ASP.NET Core auth by always returning true
    /// </summary>
    public sealed class HangfireNoAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// IDashboardAuthorizationFilter.Authorize implementation
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
        }
    }
}