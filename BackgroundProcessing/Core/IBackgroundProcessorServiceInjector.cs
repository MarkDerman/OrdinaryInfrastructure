using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Handles dependency injection of BackgroundProcessor services.
    /// </summary>
    public interface IBackgroundProcessorServiceInjector
    {
        /// <summary>
        /// Adds BackgroundProcessing services, such as Hangfire's server.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="configSection"></param>
        /// <param name="connectionStringFactory">Constructs the SQL Server connection string</param>
        void TryAddBackgroundProcessor(IServiceCollection serviceCollection, IConfiguration configuration, 
            IConfigurationSection configSection, Func<IServiceProvider, string>? connectionStringFactory = null);

        /// <summary>
        /// Adds BackgroundProcessing features (such as Hangfire's dashboard) to the HTTP pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appServices"></param>
        /// <returns></returns>
        IApplicationBuilder UseBackgroundProcessing(IApplicationBuilder app, IServiceProvider appServices);
        
    }
}