using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Odin.DesignContracts;

namespace Odin.BackgroundProcessing
{
    /// <inheritdoc />
    public class HangfireServiceInjector : IBackgroundProcessorServiceInjector
    {
        /// <inheritdoc />
        public void TryAddBackgroundProcessor(IServiceCollection serviceCollection, IConfiguration configuration,
            IConfigurationSection backgroundProcessingSection, Func<IServiceProvider, string>? connectionStringFactory = null)
        {
            Contract.RequiresNotNull(serviceCollection);
            Contract.RequiresNotNull(backgroundProcessingSection);
            IConfigurationSection? providerSection =
                backgroundProcessingSection.GetSection(BackgroundProcessingProviders.Hangfire);
            if (providerSection == null)
            {
                throw new ApplicationException(
                    $"Section {BackgroundProcessingProviders.Hangfire} missing in BackgroundProcessing configuration.");
            }

            serviceCollection.AddOptions<HangfireOptions>().Bind(providerSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            HangfireOptions hangfireOptions = new HangfireOptions();
            providerSection.Bind(hangfireOptions);
            
            string GetConnectionString(IServiceProvider sp)
            {
                if (connectionStringFactory is not null)
                {
                    return connectionStringFactory(sp);
                }

                HangfireOptions opts = sp.GetRequiredService<IOptionsMonitor<HangfireOptions>>().CurrentValue;

                if (string.IsNullOrWhiteSpace(opts.ConnectionStringName))
                {
                    throw new ApplicationException($"Invalid Hangfire configuration. ConnectionString was not passed explicitly, and " +
                                                   $"no fallback connection string was named.");
                }
                
                string? namedConnString = sp.GetRequiredService<IConfiguration>().GetConnectionString(opts.ConnectionStringName);
                
                if (string.IsNullOrWhiteSpace(namedConnString))
                {
                    throw new ApplicationException($"Invalid Hangfire configuration. ConnectionString was not passed explicitly " +
                                                   $"and a connection string named \"{opts.ConnectionStringName}\" does not exist in configuration.");
                }

                return namedConnString;
            }

            SqlServerStorageOptions GetSqlServerStorageOptions(IServiceProvider sp)
            {
                HangfireOptions opts = sp.GetRequiredService<IOptionsMonitor<HangfireOptions>>().CurrentValue;
                return GetSqlServerOptions(opts);
            }

            serviceCollection.AddOdinLoggerWrapper();
            serviceCollection.AddTransient<IBackgroundProcessor, HangfireBackgroundProcessor>();
            
            serviceCollection.AddHangfire((sp, c) => c
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(GetConnectionString(sp), GetSqlServerStorageOptions(sp)));

            if (hangfireOptions.StartServer)
            {
                if (hangfireOptions.ServerWorkerCount.HasValue)
                {
                    serviceCollection.AddHangfireServer(options => options.WorkerCount = hangfireOptions.ServerWorkerCount.Value);
                }
                else
                {
                    serviceCollection.AddHangfireServer();
                }

                // Keep jobs for configured number of days if specced
                if (hangfireOptions.JobExpirationHours.HasValue)
                {
                    GlobalJobFilters.Filters.Add(
                        new HangfireExpirationPeriodAttribute(
                            TimeSpan.FromHours(hangfireOptions.JobExpirationHours.Value)));
                }

                // Automatically retry jobs setting...
                if (hangfireOptions.NumberOfAutomaticRetries.HasValue)
                {
                    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
                        { Attempts = hangfireOptions.NumberOfAutomaticRetries.Value });
                }
            }
        }

        private static SqlServerStorageOptions GetSqlServerOptions(HangfireOptions hangfireOptions)
        {
            SqlServerStorageOptions sqlOptions = new SqlServerStorageOptions();
            if (hangfireOptions.SqlServerCommandBatchMaxTimeoutSeconds.HasValue)
            {
                sqlOptions.CommandBatchMaxTimeout =
                    TimeSpan.FromSeconds(hangfireOptions.SqlServerCommandBatchMaxTimeoutSeconds.Value);
            }

            if (hangfireOptions.SqlServerSlidingInvisibilityTimeoutSeconds.HasValue)
            {
                sqlOptions.SlidingInvisibilityTimeout =
                    TimeSpan.FromSeconds(hangfireOptions.SqlServerSlidingInvisibilityTimeoutSeconds.Value);
            }

            if (hangfireOptions.SqlServerQueuePollIntervalSeconds.HasValue)
            {
                sqlOptions.QueuePollInterval =
                    TimeSpan.FromSeconds(hangfireOptions.SqlServerQueuePollIntervalSeconds.Value);
            }

            if (hangfireOptions.SqlServerUseRecommendedIsolationLevel.HasValue)
            {
                sqlOptions.UseRecommendedIsolationLevel =
                    hangfireOptions.SqlServerUseRecommendedIsolationLevel.Value;
            }

            if (hangfireOptions.SqlServerDisableGlobalLocks.HasValue)
            {
                sqlOptions.DisableGlobalLocks = hangfireOptions.SqlServerDisableGlobalLocks.Value;
            }

            return sqlOptions;
        }

        /// <summary>
        /// If configured, adds Hangfire dashboard to HTTP pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="appServices"></param>
        public IApplicationBuilder UseBackgroundProcessing(IApplicationBuilder builder, IServiceProvider appServices)
        {
            HangfireOptions hangfireOptions = appServices.GetRequiredService<IOptionsMonitor<HangfireOptions>>().CurrentValue;

            if (!hangfireOptions.StartDashboard)
            {
                return builder;
            }

            IEnumerable<string> filterStrings =
                hangfireOptions.DashboardAuthorizationFilters.Split(',', ';')
                    .Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim());
            IEnumerable<IDashboardAuthorizationFilter> filters =
                filterStrings.Select(TryCreateDashBoardAuthorizationFilter).Where(c => c != null)!;
            DashboardOptions options = new DashboardOptions()
            {
                Authorization = filters,
                StatsPollingInterval = hangfireOptions.StatsPollingInterval,
                IgnoreAntiforgeryToken = hangfireOptions.IgnoreAntiforgeryToken
            };
            if (!string.IsNullOrWhiteSpace(hangfireOptions.DashboardTitle))
            {
                options.DashboardTitle = hangfireOptions.DashboardTitle;
            }

            builder.UseHangfireDashboard(hangfireOptions.DashboardPath, options);

            return builder;
        }

        internal static IDashboardAuthorizationFilter? TryCreateDashBoardAuthorizationFilter(string? filterName)
        {
            if (string.IsNullOrWhiteSpace(filterName)) return null;
            string filterTrimmed = filterName.Trim();
            if (filterTrimmed.Equals(HangfireOptions.DefaultAuthorizationFilterNone,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new HangfireNoAuthorizationFilter();
            }

            if (filterName.Equals(HangfireOptions.DefaultAuthorizationFilterIsAuthenticated,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new HangfireIsAuthenticatedAuthorizationFilter();
            }

            return new HangfirePolicyAuthorisationFilter(filterTrimmed);
        }
    }
}