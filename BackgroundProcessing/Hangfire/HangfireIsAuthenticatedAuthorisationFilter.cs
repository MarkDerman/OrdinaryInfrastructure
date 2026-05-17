using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Simply requires User to be authenticated
    /// </summary>
    public sealed class HangfireIsAuthenticatedAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Returns HttpContext.User.Identity.IsAuthenticated
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize([NotNull] DashboardContext context)
        {
            HttpContext httpContext = context.GetHttpContext();
            if (httpContext.User == null) return false;
            if (httpContext.User.Identity == null) return false;
            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}