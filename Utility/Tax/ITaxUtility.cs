
namespace Odin.Utility;

/// <summary>
/// Supports convenience tax operations and a date-aware knowledge of historical tax rate changes.
/// </summary>
public interface ITaxUtility
{
    /// <summary>
    /// Returns the tax rate as a percentage.
    /// Eg 15m
    /// </summary>
    /// <param name="asAtDate"></param>
    /// <returns></returns>
    decimal GetTaxRateAsPercentage(DateOnly asAtDate);

    /// <summary>
    /// Returns the tax rate as a fraction.
    /// Eg 0.15m
    /// </summary>
    /// <param name="asAtDate"></param>
    /// <returns></returns>
    decimal GetTaxRateAsFraction(DateOnly asAtDate);

    /// <summary>
    /// Calculates the Tax amount within a tax inclusive amount.
    /// </summary>
    /// <param name="amountIncludingTax"></param>
    /// <param name="asAtDate">The date on which the tax operation is effective. Used to retrieve the tax rate on that date.</param>
    /// <param name="roundToDecimalPlaces">Must be from 0 to 20. Default is 10.</param>
    /// <returns></returns>
    public decimal CalculateTaxPortionOfTaxInclusiveAmount(decimal amountIncludingTax, DateOnly asAtDate,
        int roundToDecimalPlaces = 10);

    /// <summary>
    ///  Calculates the Tax amount on a tax exclusive amount.
    /// </summary>
    /// <param name="amountExcludingTax"></param>
    /// <param name="asAtDate">The date on which the tax operation is effective. Used to retrieve the tax rate on that date.</param>
    /// <param name="roundToDecimalPlaces">Must be from 0 to 20. Default is 10.</param>
    /// <returns></returns>
    public decimal CalculateTaxOnTaxExclusiveAmount(decimal amountExcludingTax, DateOnly asAtDate,
        int roundToDecimalPlaces = 10);
}