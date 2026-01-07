using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Odin.DesignContracts;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Simply requires User to be authenticated
    /// </summary>
    public sealed class HangfirePolicyAuthorisationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _policyName;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="policyName"></param>
        public HangfirePolicyAuthorisationFilter(string policyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(policyName));
            _policyName = policyName.Trim();
        }

        /// <summary>
        /// Returns HttpContext.User.Identity.IsAuthenticated
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize([NotNull] DashboardContext context)
        {
            HttpContext httpContext = context.GetHttpContext();
            if (httpContext.User == null!) return false;
            IAuthorizationService? authService = httpContext.RequestServices.GetService<IAuthorizationService>();
            if (authService == null) return false;
            Task<AuthorizationResult> task = authService.AuthorizeAsync(httpContext.User, _policyName);
            AuthorizationResult result = task.Result;
            return result.Succeeded;
        }
    }
}