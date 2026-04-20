using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Odin.Utility;

namespace Tests.Odin.Utility
{
    public class TaxUtilityTests
    {
        [Theory]
        [MemberData(nameof(IncludedTaxPortionCases))]
        public void Calculation_of_an_included_tax_portion_is_correct(decimal amount, decimal taxRatePercentage, int? roundToDecimalPlaces, string expectedAsString)
        {
            decimal expected = decimal.Parse(expectedAsString, CultureInfo.InvariantCulture); // NUnit conversion to decimal only goes up to about 14 decimal points for some reason...
            TaxUtility sut = new TaxUtility(taxRatePercentage);
            decimal result;
            if (roundToDecimalPlaces.HasValue)
            {
                result = sut.CalculateTaxPortionOfTaxInclusiveAmount(amount,DateOnly.MaxValue,roundToDecimalPlaces.Value );
            }
            else
            {
                result = sut.CalculateTaxPortionOfTaxInclusiveAmount(amount,DateOnly.MaxValue);
            }
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ExclusiveTaxCases))]
        public void Calculation_of_tax_on_an_exclusive_amount_is_correct(decimal amount, decimal taxRatePercentage, int? roundToDecimalPlaces, decimal expected)
        {
            TaxUtility sut = new TaxUtility(taxRatePercentage);
            decimal result;
            if (roundToDecimalPlaces.HasValue)
            {
                result = sut.CalculateTaxOnTaxExclusiveAmount(amount,DateOnly.MaxValue,roundToDecimalPlaces.Value );
            }
            else
            {
                result = sut.CalculateTaxOnTaxExclusiveAmount(amount,DateOnly.MaxValue);
            }
            Assert.Equal(expected, result);
        }


        [Theory]
        [MemberData(nameof(SingleTaxRateCases))]
        public void Using_a_single_tax_rate(decimal testRatePercentage)
        {
            TaxUtility sut = new TaxUtility(testRatePercentage);
            
            Assert.Equal(testRatePercentage * 0.01m, sut.GetTaxRateAsFraction(DateOnly.MinValue));
            Assert.Equal(testRatePercentage, sut.GetTaxRateAsPercentage(DateOnly.MinValue));
            Assert.Equal(testRatePercentage * 0.01m, sut.GetTaxRateAsFraction(DateOnly.MaxValue));
            Assert.Equal(testRatePercentage, sut.GetTaxRateAsPercentage(DateOnly.MaxValue));
            Assert.Equal(testRatePercentage * 0.01m, sut.GetTaxRateAsFraction(DateOnly.FromDateTime(DateTime.Now)));
            Assert.Equal(testRatePercentage, sut.GetTaxRateAsPercentage(DateOnly.FromDateTime(DateTime.Now)));
        }
        
        [Fact]
        public void Multiple_tax_rates_are_supported()
        {
            TaxUtility sut = new TaxUtility(CreateSouthAfricanVatHistory());

            AssertSouthAfricanVatHistoryRates(sut);
        }

        private void AssertSouthAfricanVatHistoryRates(TaxUtility sut)
        {
            Assert.Equal(default(decimal), sut.GetTaxRateAsPercentage(DateOnly.MinValue));
            Assert.Equal(default(decimal), sut.GetTaxRateAsPercentage(new DateOnly(1800, 1, 1)));
            Assert.Equal(15m, sut.GetTaxRateAsPercentage(new DateOnly(1900, 1, 1)));
            Assert.Equal(15m, sut.GetTaxRateAsPercentage(new DateOnly(1900, 1, 2)));
            Assert.Equal(15m, sut.GetTaxRateAsPercentage(new DateOnly(2025, 4, 30)));
            Assert.Equal(15.5m, sut.GetTaxRateAsPercentage(new DateOnly(2025, 5, 1)));
            Assert.Equal(15.5m, sut.GetTaxRateAsPercentage(new DateOnly(2026, 4, 30)));
            Assert.Equal(16m, sut.GetTaxRateAsPercentage(new DateOnly(2026, 5, 1)));
            Assert.Equal(16m, sut.GetTaxRateAsPercentage(DateOnly.MaxValue));
        }
        
        
        [Fact]
        public void Multiple_tax_rates_are_supported_via_IConfiguration()
        {
            TaxUtility sut = new TaxUtility(CreateSouthAfricanVatHistoryConfiguration(),"TaxHistory");

            AssertSouthAfricanVatHistoryRates(sut);
        }

        [Fact]
        public void Multiple_tax_rates_are_supported_via_IConfigurationSection()
        {
            TaxUtility sut = new TaxUtility(CreateSouthAfricanVatHistoryConfiguration().GetSection("TaxHistory"));

            AssertSouthAfricanVatHistoryRates(sut);
        }

        private List<ValueChange<DateOnly, decimal>> CreateSouthAfricanVatHistory()
        {
            return new List<ValueChange<DateOnly, decimal>>()
            {
                new ValueChange<DateOnly, decimal>(new DateOnly(1900, 1, 1), 15m),
                new ValueChange<DateOnly, decimal>(new DateOnly(2025, 5, 1), 15.5m),
                new ValueChange<DateOnly, decimal>(new DateOnly(2026, 5, 1), 16.0m)
            };
        }
        private IConfigurationRoot CreateSouthAfricanVatHistoryConfiguration()
        {
            string json = """
                          {
                            "TaxHistory": [
                              {
                                "From": "1900-01-01",
                                "Value": 15
                              },
                              {
                                "From": "2025-05-01",
                                "Value": 15.5
                              },
                              {
                                "From": "2026-05-01",
                                "Value": 16
                              }
                            ]
                          }
                          """;
            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
        }

        public static IEnumerable<object?[]> IncludedTaxPortionCases()
        {
            yield return [115m, 15m, 2, "15"];
            yield return [130m, 30m, 2, "30"];
            yield return [105m, 5m, 2, "5"];
            yield return [95m, -5m, 2, "-5"];
            yield return [-115m, 15m, 10, "-15"];
            yield return [123.78m, 15m, 4, "16.1452"];
            yield return [123.78m, 15m, 2, "16.14"];
            yield return [123.78m, 15m, 0, "16.00"];
            yield return [123.78m, 15m, -2, "16.00"];
            yield return [123.78m, 15m, 25, "16.14521739130434782608"];
            yield return [123.78m, 15m, 20, "16.14521739130434782608"];
            yield return [123.78m, 15m, 10, "16.1452173913"];
            yield return [123.78m, 15m, null, "16.1452173913"];
        }

        public static IEnumerable<object?[]> ExclusiveTaxCases()
        {
            yield return [100m, 15m, 2, 15m];
            yield return [100m, 30m, 2, 30m];
            yield return [100m, 5m, 2, 5m];
            yield return [100m, -5m, 2, -5m];
            yield return [-100m, 15m, 10, -15m];
            yield return [99.37m, 15.3m, 5, 15.20361m];
            yield return [99.37m, 15.3m, 25, 15.20361m];
            yield return [99.37m, 15.3m, 10, 15.2036100000m];
            yield return [99.37m, 15.3m, 4, 15.2036m];
            yield return [99.37m, 15.3m, 2, 15.20m];
            yield return [99.37m, 15.3m, 0, 15.0m];
            yield return [99.37m, 15.3m, null, 15.2036100000m];
        }

        public static IEnumerable<object?[]> SingleTaxRateCases()
        {
            yield return [15m];
            yield return [15.5m];
            yield return [0m];
        }

    }
}
