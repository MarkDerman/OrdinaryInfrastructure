using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Dependency injection methods to support ILogger2 of T 
    /// </summary>
    public static class Logger2Extensions
    {
        /// <summary>
        /// Adds the Odin ILoggerWrapper of T implementation into dependency injection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static void AddOdinLoggerWrapper(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();
            serviceCollection.TryAddSingleton(typeof(ILoggerWrapper<>), typeof(LoggerWrapper<>));
        }
    }
}