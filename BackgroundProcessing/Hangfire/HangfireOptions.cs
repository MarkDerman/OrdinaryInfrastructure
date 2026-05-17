namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Hangfire settings 
    /// </summary>
    public record HangfireOptions
    {
        private const string DefaultDashboardPath = "/system/background-processing";
        internal const string DefaultAuthorizationFilterNone = "None";
        internal const string DefaultAuthorizationFilterIsAuthenticated = "IsAuthenticated";
        private const int DefaultJobExpirationHours = 24 * 30;

        /// <summary>
        /// Name of the SQL connection string in ConnectionStrings in configuration
        /// </summary>
        public string? ConnectionStringName { get; set; }

        /// <summary>
        /// Whether to start the dashboard or not.
        /// Default is true.
        /// </summary>
        public bool StartDashboard { get; set; } = true;

        /// <summary>
        /// Whether to start HangfireServer or not.
        /// </summary>
        public bool StartServer { get; set; } = true;

        /// <summary>
        /// Path to the Hangfire dashboard.
        /// If NULL or empty then the Hangfire default of /hangfire will apply. 
        /// Default: /system/background-processing
        /// </summary>
        public string? DashboardPath { get; set; } = DefaultDashboardPath;

        /// <summary>
        /// Optional title to replace 'Hangfire Dashboard'.
        /// </summary>
        public string? DashboardTitle { get; set; }

        /// <summary>
        /// A comma or semi-colon delimited list of dashboard authorisation filters to use.
        /// Filters other than 'None' and 'IsAuthenticated' are assumed to be policy based filters.
        /// Default: 'None'
        /// </summary>
        public string DashboardAuthorizationFilters { get; set; } = DefaultAuthorizationFilterNone;

        /// <summary>
        /// Number of retry attempts for failed jobs. If null then the Hangfire default applies
        /// Default: 0
        /// </summary>
        public int? NumberOfAutomaticRetries { get; set; } = 0;

        /// <summary>
        /// Number of hours before jobs are archived\expired from Hangfire.
        /// If null then the Hangfire default of 24 hours applies.
        /// </summary>
        public int? JobExpirationHours { get; set; } = DefaultJobExpirationHours;

        /// <summary>
        /// The number of workers to start Hangfire up with. If set to NULL then Hangfire will start up using Environment.ProcessorCount as the number of workers.
        /// If null then the Hangfire default of 5 x processor count will be used.
        /// </summary>
        public int? ServerWorkerCount { get; set; }

        /// <summary>
        /// TBA
        /// </summary>
        public bool IgnoreAntiforgeryToken { get; set; } = false;

        /// <summary>
        /// How often the dashboard UI polls the /stats endpoint in milliseconds. Default is 10000 milliseconds.
        /// </summary>
        public int StatsPollingInterval { get; set; } = 10000;

        /// <summary>
        /// TBA
        /// </summary>
        public int? SqlServerCommandBatchMaxTimeoutSeconds { get; set; }

        /// <summary>
        /// TBA
        /// </summary>
        public int? SqlServerSlidingInvisibilityTimeoutSeconds { get; set; }

        /// <summary>
        /// How often Hangfire polls the job queue when using SQL Server. Odin default is 2 seconds.
        /// </summary>
        public int? SqlServerQueuePollIntervalSeconds { get; set; } = 2;

        /// <summary>
        /// TBA
        /// </summary>
        public bool? SqlServerUseRecommendedIsolationLevel { get; set; }

        /// <summary>
        /// TBA
        /// </summary>
        public bool? SqlServerDisableGlobalLocks { get; set; }
    }
}