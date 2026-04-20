using Odin.Utility;

namespace Tests.Odin.Utility
{
    public sealed class ValueChangeTests
    {
        [Fact]
        public void Create_DateOnly_and_Decimal_value_change()
        {
            ValueChange<DateOnly, decimal> sut = new ValueChange<DateOnly, Decimal>(new DateOnly(2025, 5, 1), 15.5m);
                
            Assert.NotNull(sut);
            Assert.Equal(new DateOnly(2025, 5, 1), sut.From);
            Assert.Equal(15.5m, sut.Value);
        }
        
         // [Test]
        // public void Create_various_value_changes([ValueSource(nameof(GetVaryingDimensionTypeTestValues))] object varyingDimensionValue, 
        //     [ValueSource(nameof(GetValueTypeTestValues))] object testValue)
        // {
        //     Type varyingDimensionType = varyingDimensionValue.GetType();
        //     Type valueType = testValue.GetType();
        //     Type closedGenericTypeDefinition = typeof(ValueChange<,>).MakeGenericType(varyingDimensionType, valueType);
        //     var sutObject = Activator2.CreateInstance(closedGenericTypeDefinition, varyingDimensionValue, testValue);
        //     var sut = null;// Cast sutObject to ValueChange<,> ? How to instantiate?
        //         
        //     Assert.That(sut, Is.Not.Null);
        //     Assert.That(sut.From, Is.EqualTo(varyingDimensionValue));
        //     Assert.That(sut.Value, Is.EqualTo(testValue));
        // }

        private static List<object?> GetVaryingDimensionTypeTestValues()
        {
            return new List<object?>() { new DateOnly(2025, 5, 1), 2, 45.3m, "FooBar", DateTimeOffset.Now, DateTime.Now } ;
        }

        private static List<object?> GetValueTypeTestValues()  
        {
            return new List<object?>() { 15.0m, 5, null as Decimal?} ;
        }

    }
}
