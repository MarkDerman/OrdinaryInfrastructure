using Odin.DesignContracts;
using Odin.System;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// BackgroundProcessing for loading from configuration 
    /// </summary>
    public sealed class BackgroundProcessingOptions
    {
        private string _provider = BackgroundProcessingProviders.Null;

        /// <summary>
        /// Fake or Hangfire
        /// </summary>
        public string Provider
        {
            get => _provider;
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _provider = value.Replace("BackgroundProcessor", "", StringComparison.OrdinalIgnoreCase);   
            }
        }

        /// <summary>
        /// Validates the settings instance
        /// </summary>
        /// <returns></returns>
        public Result Validate()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Provider))
            {
                errors.Add($"{nameof(Provider)} has not been specified. Must be 1 of {string.Join(" | ",BackgroundProcessingProviders.Values)}");
            }
            else if (!BackgroundProcessingProviders.HasValue(Provider))
            {
                errors.Add($"The {Constants.ModuleNoun} provider specified ({Provider}) is not one of the supported providers: {string.Join(" | ",BackgroundProcessingProviders.Values)}");
            }
            return new Result(!errors.Any(), errors);
        }
    }
}
