using System.Reflection;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Odin.DesignContracts;
using Odin.System;

namespace Odin.Data
{
    /// <summary>
    /// Represents 
    /// </summary>
    public class SqlScriptsRunner
    {
        // private readonly ILogger2<T> _logger;

        private readonly Assembly _assemblyWithEmbeddedScripts;
        private SqlConnectionStringBuilder _connectionStringBuilder = null!;

        public bool EnsureDatabaseCreated { get; set; } = false;
        public string ConnectionString { get; protected init; } = null!;
        public JournalModeEnum JournalMode { get; set; } = JournalModeEnum.RunOnlyScriptsNotRunBefore;
        public TransactionMode TransactionHandling { get; set; } = TransactionMode.TransactionPerScript;
        public int ExecutionTimeoutSeconds { get; set; } = 300;
        public string JournalToTableName { get; set; } = "DeploymentMigrations";
        public string JournalToSchemaName { get; set; } = "dbo";
        public string DatabaseName => _connectionStringBuilder.InitialCatalog;
        public string HostName => _connectionStringBuilder.DataSource;
        
        /// <summary>
        /// Dot (.) separated list of partial paths in the assembly to find embedded *.sql files.
        /// </summary>
        public string? ScriptsLocation { get; set; }

        public ScriptsLocationTypeEnum ScriptsLocationType { get; set; } =
            ScriptsLocationTypeEnum.EmbeddedResourcePath;

        private SqlScriptsRunner(Assembly assemblyWithEmbeddedScripts)
        {
            // Contract.Requires(logger);
            // _logger = logger;
            Contract.Requires(assemblyWithEmbeddedScripts!=null!);
            _assemblyWithEmbeddedScripts = assemblyWithEmbeddedScripts!;
        }

        public static ResultValue<SqlScriptsRunner> CreateFromConnectionStringName(string connectionStringName,
            Assembly assemblyWithEmbeddedScripts, IConfiguration configuration)
        {
            Contract.Requires(configuration!=null!);
            Contract.Requires(assemblyWithEmbeddedScripts!=null!);
            SqlScriptsRunner runner = new SqlScriptsRunner(assemblyWithEmbeddedScripts)
            {
                ConnectionString = configuration.GetConnectionString(connectionStringName)!
            };
            if (string.IsNullOrWhiteSpace(runner.ConnectionString))
            {
                return ResultValue<SqlScriptsRunner>.Failure(
                    $"No ConnectionString named {connectionStringName} exists in application configuration.");
            }

            try
            {
                runner._connectionStringBuilder = new SqlConnectionStringBuilder(runner.ConnectionString);
            }
            catch (Exception err)
            {
                return ResultValue<SqlScriptsRunner>.Failure($"Unable to build connectionString. {err.Message}");
            }

            return ResultValue<SqlScriptsRunner>.Succeed(runner);
        }

        public static ResultValue<SqlScriptsRunner> CreateFromConnectionString(string connectionString, Assembly assemblyWithEmbeddedScripts)
        {
            Contract.Requires(assemblyWithEmbeddedScripts!=null!);
            Contract.Requires(!string.IsNullOrWhiteSpace(connectionString));
            SqlScriptsRunner runner = new SqlScriptsRunner(assemblyWithEmbeddedScripts)
            {
                ConnectionString = connectionString
            };
            try
            {
                runner._connectionStringBuilder = new SqlConnectionStringBuilder(runner.ConnectionString);
            }
            catch (Exception err)
            {
                return ResultValue<SqlScriptsRunner>.Failure($"Unable to build connectionString. {err.Message}");
            }

            return ResultValue<SqlScriptsRunner>.Succeed(runner);
        }


        /// <summary>
        /// Ensure each database exists, creating a blank database if necessary.
        /// </summary>
        /// <returns></returns>
        private Result EnsureDatabaseExists()
        {
            try
            {
                OutputWriteInformation(
                    $"Ensuring database {DatabaseName} on {HostName} is created");

                EnsureDatabase.For.SqlDatabase(ConnectionString);
                return Result.Success();
            }
            catch (Exception err)
            {
                string message = $"Ensuring that database {DatabaseName} exists on {HostName} failed - {err.Message}";
                OutputWriteError(err);
                OutputWriteError(message);
                return Result.Failure(message);
            }
        }

        /// <summary>
        /// Runs the SQL scripts...
        /// </summary>
        /// <returns></returns>
        public Result Run()
        {
            // Validation...
            // if (!EmbeddedScriptPaths.Any()) OK if this is not specified we use all *.sql embedded resources in the assembly.
            // {
            //     return ResultValue.Fail($"No (. separated) paths specified in {nameof(EmbeddedScriptPaths)} ");
            // }
            
            if (EnsureDatabaseCreated)
            {
                Result databaseExists = EnsureDatabaseExists();
                if (!databaseExists.IsSuccess)
                {
                    return Result.Failure($"Run failed. {databaseExists.MessagesToString()}");
                }
            }

            //////////////////////////////////////////////////////
            // Setup migration engine
            //////////////////////////////////////////////////////
            UpgradeEngineBuilder upgradeBuilder = DeployChanges.To
                .SqlDatabase(ConnectionString);

            //////////////////////////////////////////////////////
            // Journalling mode...
            //////////////////////////////////////////////////////
            if (JournalMode == JournalModeEnum.RunOnlyScriptsNotRunBefore)
            {
                upgradeBuilder = upgradeBuilder.JournalToSqlTable(JournalToSchemaName, JournalToTableName);
            }
            else if (JournalMode == JournalModeEnum.AlwaysRunAllScripts)
            {
                upgradeBuilder = upgradeBuilder.JournalTo(new NullJournal());
            }
            else
            {
                throw new Exception($"Unknown {nameof(JournalMode)}: {JournalMode.ToString()}");
            }
            
            //////////////////////////////////////////////////////
            // Script locations
            //////////////////////////////////////////////////////
            switch (ScriptsLocationType)
            {
                case ScriptsLocationTypeEnum.EmbeddedResourcePath:
                    if (!string.IsNullOrWhiteSpace(ScriptsLocation))
                    {
                        upgradeBuilder = upgradeBuilder.WithScriptsEmbeddedInAssembly(_assemblyWithEmbeddedScripts,
                            s => s.Contains(ScriptsLocation));
                    }
                    else
                    {
                        upgradeBuilder = upgradeBuilder.WithScriptsEmbeddedInAssembly(_assemblyWithEmbeddedScripts);
                    }

                    break;
                case ScriptsLocationTypeEnum.FileSystemPath:
                    if (string.IsNullOrWhiteSpace(ScriptsLocation))
                    {
                        throw new Exception($"A value for {nameof(ScriptsLocation)} is required when {nameof(ScriptsLocationType)} is {ScriptsLocationTypeEnum.FileSystemPath.ToString()}");
                    }
                    string path = ScriptsLocation;
                    if (Path.DirectorySeparatorChar != '\\')
                    {
                        path = ScriptsLocation.Replace('\\',Path.DirectorySeparatorChar );
                    }
                    if (Path.DirectorySeparatorChar != '/')
                    {
                        path = ScriptsLocation.Replace('/',Path.DirectorySeparatorChar );
                    }
                    upgradeBuilder = upgradeBuilder.WithScriptsFromFileSystem(path);
                    break;
                default:
                    throw new ApplicationException(
                        $"Unknown {nameof(ScriptsLocationType)} value {ScriptsLocationType.ToString()}");
            }

            //////////////////////////////////////////////////////
            // Transaction handling
            //////////////////////////////////////////////////////
            switch (TransactionHandling)
            {
                case TransactionMode.SingleTransactionAlwaysRollback:
                    upgradeBuilder = upgradeBuilder.WithTransactionAlwaysRollback();
                    break;
                case TransactionMode.TransactionPerScript:
                    upgradeBuilder = upgradeBuilder.WithTransactionPerScript();
                    break;
                case TransactionMode.NoTransaction:
                    upgradeBuilder = upgradeBuilder.WithoutTransaction();
                    break;
                case TransactionMode.SingleTransaction:
                    upgradeBuilder = upgradeBuilder.WithTransaction();
                    break;
                default:
                    throw new ApplicationException(
                        $"Unknown {nameof(TransactionHandling)} value {TransactionHandling.ToString()}");
            }

            UpgradeEngine scriptsRun = upgradeBuilder.LogScriptOutput().LogToConsole().WithExecutionTimeout(TimeSpan.FromSeconds(ExecutionTimeoutSeconds)).Build();
            List<SqlScript> scriptsToRun;
            try
            {
                scriptsToRun = scriptsRun.GetScriptsToExecute();
            }
            catch (Exception err)
            {
                return Result.Failure($"Unable to get scripts to run: {err}");
            }
            if (!scriptsToRun.Any())
            {
                OutputWriteInformation($"{DatabaseName} - No new migration scripts found...");
            }
            else
            {
                OutputWriteInformation(
                    $"{DatabaseName} - {scriptsToRun.Count} new migration scripts found...");
            }

            if (scriptsToRun.Any())
            {
                OutputWriteInformation(
                    $"{DatabaseName} - Running {scriptsToRun.Count} new migration scripts against {DatabaseName} on {HostName}");
                DatabaseUpgradeResult runnerResult = scriptsRun.PerformUpgrade();
                if (!runnerResult.Successful)
                {
                    string err =
                        $"{DatabaseName} - Run migration scripts completed with errors. {runnerResult.Error}";
                    OutputWriteError(err);
                    return Result.Failure(err);
                }
                string message =
                    $"{DatabaseName} - {scriptsToRun.Count} scripts executed successfully.";
                OutputWriteInformation(message);
                return Result.Success(message);
            }
            return Result.Success("No scripts needed to be run.");
        }

        private void OutputWriteError(Exception error)
        {
            if (error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error.Message);
                // _logger.LogError(error);
                Console.ResetColor();
                // _logger.Log(LogLevel.Error,error.Message, error);
            }
        }

        private void OutputWriteInformation(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine(message);
               //_logger.LogInformation(message);
            }
        }

        private void OutputWriteError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
               // _logger.LogError(error);
                Console.ResetColor();
            }
        }
    }

    public enum ScriptsLocationTypeEnum
    {
        EmbeddedResourcePath = 0,
        FileSystemPath = 1
    }
    
    public enum JournalModeEnum
    {
        RunOnlyScriptsNotRunBefore = 0,
        AlwaysRunAllScripts = 1
    }
}