using Microsoft.Extensions.Configuration;
using Odin.DesignContracts;

namespace Odin.Utility;

/// <inheritdoc />
public class TaxUtility : ITaxUtility
{
    private ValueChangesListProvider<DateOnly, decimal> _taxValues;

    /// <summary>
    /// Single tax rate
    /// </summary>
    /// <param name="singleTaxRate"></param>
    public TaxUtility(decimal singleTaxRate)
    {
        _taxValues =
            new ValueChangesListProvider<DateOnly, decimal>(
                new ValueChange<DateOnly, decimal>(DateOnly.MinValue, singleTaxRate));
    }

    /// <summary>
    /// Many tax rates over time 
    /// </summary>
    /// <param name="taxRatesAsPercentageHistory">Note that tax rates must be expressed as a percentage number, not a fraction.</param>
    public TaxUtility(IEnumerable<ValueChange<DateOnly, decimal>> taxRatesAsPercentageHistory)
    {
        Precondition.Requires(taxRatesAsPercentageHistory!=null!);
        _taxValues = new ValueChangesListProvider<DateOnly, decimal>(taxRatesAsPercentageHistory);
    }

    /// <summary>
    /// Load tax rates from IConfiguration
    /// </summary>
    /// <param name="taxValuesConfig"></param>
    /// <param name="taxHistorySectionName"></param>
    public TaxUtility(IConfiguration taxValuesConfig, string taxHistorySectionName)
    {
        _taxValues = new ValueChangesListProvider<DateOnly, decimal>(taxValuesConfig.GetSection(taxHistorySectionName));
    }
    
    /// <summary>
    /// Load tax rates from IConfigurationSection
    /// </summary>
    /// <param name="taxValuesConfigSection"></param>
    public TaxUtility(IConfigurationSection taxValuesConfigSection)
    {
        _taxValues = new ValueChangesListProvider<DateOnly, decimal>(taxValuesConfigSection);
    }

    /// <summary>
    /// Returns the tax rate as a percentage. Eg 15.5
    /// </summary>
    /// <param name="asAtDate"></param>
    /// <returns></returns>
    public decimal GetTaxRateAsPercentage(DateOnly asAtDate)
    {
        return _taxValues.GetValue(asAtDate);
    }

    /// <summary>
    /// Returns the tax rate as a fraction. Eg 0.155
    /// </summary>
    /// <param name="asAtDate"></param>
    /// <returns></returns>
    public decimal GetTaxRateAsFraction(DateOnly asAtDate)
    {
        return _taxValues.GetValue(asAtDate) * 0.01m;
    }
   
    /// <summary>
    /// Calculates the tax part of a tax inclusive amount as at a certain date.
    /// </summary>
    /// <param name="amountIncludingTax"></param>
    /// <param name="asAtDate"></param>
    /// <param name="roundToDecimalPlaces"></param>
    /// <returns></returns>
    public decimal CalculateTaxPortionOfTaxInclusiveAmount(decimal amountIncludingTax,
        DateOnly asAtDate, int roundToDecimalPlaces = 10)
    {
        //Calculates Tax From Tax Inclusive Amount
        //Tax is e.g. 15% = 15m
        decimal taxAmount = amountIncludingTax - amountIncludingTax / (1 + GetTaxRateAsFraction(asAtDate));
        return RoundTowardsZeroToDecimalPlaces(taxAmount, roundToDecimalPlaces);
    }

    /// <summary>
    ///  Calculates the Tax amount on a tax exclusive amount.
    /// </summary>
    /// <param name="amountExcludingTax"></param>
    /// <param name="asAtDate">The date on which the tax operation is effective. Used to retrieve the tax rate on that date.</param>
    /// <param name="roundToDecimalPlaces">Must be from 0 to 20. Default is 10.</param>
    /// <returns></returns>
    public decimal CalculateTaxOnTaxExclusiveAmount(decimal amountExcludingTax, DateOnly asAtDate, int roundToDecimalPlaces = 10)
    {
        decimal taxAmount =  amountExcludingTax * GetTaxRateAsFraction(asAtDate);
        return RoundTowardsZeroToDecimalPlaces(taxAmount, roundToDecimalPlaces);
    }

    internal decimal RoundTowardsZeroToDecimalPlaces(decimal n, int places)
    {
        if (places < 0)
        {
            places = 0;
        }

        if (places > 20)
        {
            places = 20;
        }

        decimal magnifyingFactor = 1m;
        decimal minifyingFactor = 1m;
        for (long i = 0; i < places; i++)
        {
            magnifyingFactor *= 10m;
            minifyingFactor *= 0.1m;
        }

        return n >= 0
            ? minifyingFactor * decimal.Floor(n * magnifyingFactor)
            : minifyingFactor * decimal.Ceiling(n * magnifyingFactor);
    }
    
}