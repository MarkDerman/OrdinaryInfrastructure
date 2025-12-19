using Microsoft.Extensions.Configuration;
using Odin.DesignContracts;

namespace Odin.Utility;

/// <summary>
/// Stores a list of Values over time to implement IVaryingValueProvider.
/// </summary>
/// <typeparam name="TRangeType"></typeparam>
/// <typeparam name="TValueType"></typeparam>
public class ValueChangesListProvider<TRangeType, TValueType> : IVaryingValueProvider<TRangeType, TValueType> 
    where TRangeType : IComparable
{
    internal List<ValueChange<TRangeType, TValueType>> _valueChangesInOrder = null!;
    
    /// <summary>
    /// Initialise from multiple values. Note that the values do not need to be in the correct ascending order of TRangeType.
    /// </summary>
    /// <param name="valuesAcrossRange"></param>
    public ValueChangesListProvider(IEnumerable<ValueChange<TRangeType, TValueType>> valuesAcrossRange)
    {
        Precondition.Requires(valuesAcrossRange!=null!);
        InitialiseFrom(valuesAcrossRange.ToList());
    }
    
    /// <summary>
    /// Only a single value
    /// </summary>
    /// <param name="singleValue"></param>
    public ValueChangesListProvider(ValueChange<TRangeType, TValueType> singleValue)
    {
        Precondition.Requires(singleValue!=null!);
        InitialiseFrom( new List<ValueChange<TRangeType, TValueType>> { singleValue } );
    }

    /// <summary>
    /// Load value changes from IConfiguration
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="sectionName">The name of the configuration section, eg 'TaxHistory'</param>
    public ValueChangesListProvider(IConfiguration configuration, string sectionName)
    {
        Precondition.Requires(configuration!=null!);
        List<ValueChange<TRangeType, TValueType>> valuesInConfig = new List<ValueChange<TRangeType, TValueType>>();
        configuration.Bind(sectionName, valuesInConfig);
        InitialiseFrom(valuesInConfig);
    }
    
    /// <summary>
    /// Load value changes from an IConfigurationSection
    /// </summary>
    /// <param name="valueChangesSection"></param>
    public ValueChangesListProvider(IConfigurationSection valueChangesSection)
    {
        Precondition.Requires(valueChangesSection!=null!);
        List<ValueChange<TRangeType, TValueType>> valuesInConfig = new List<ValueChange<TRangeType, TValueType>>();
        valueChangesSection.Bind(valuesInConfig);
        InitialiseFrom(valuesInConfig);
    }

    private void InitialiseFrom(List<ValueChange<TRangeType, TValueType>> valuesAcrossRange)
    {
        Precondition.Requires(valuesAcrossRange!=null!);
        _valueChangesInOrder = valuesAcrossRange.OrderBy(c => c.From).ToList();
    }

    /// <inheritdoc />
    public TValueType? GetValue(TRangeType atPointAlongDimension)
    {
        IEnumerable<ValueChange<TRangeType, TValueType>> candidates = _valueChangesInOrder.Where(c => c.From.CompareTo(atPointAlongDimension) <= 0);
        ValueChange<TRangeType, TValueType>? applicableValue = candidates.MaxBy(c => c.From);
        if (applicableValue == null)
        {
            return default;
        }
        return applicableValue.Value;
    }
}