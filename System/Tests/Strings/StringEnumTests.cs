using System.Collections.Immutable;
using Odin.System;

namespace Tests.Odin.System.StringEnum
{
    public sealed class StringEnumTests
    {
        [Theory]
        [InlineData("val1", true)]
        [InlineData("VAL1", false)]
        [InlineData("Elephant", false)]
        public void HasValue(string? testValue, bool expectedResult)
        {
            bool sut = FourValsStringEnum.HasValue(testValue);

            Assert.Equal(expectedResult, sut);
        }
        
        [Theory]
        [InlineData("val1", true)]
        [InlineData("VAL1", false)]
        [InlineData("Elephant", false)]
        public void ValidateValue(string? testValue, bool expectedResult)
        {
            Result sut = FourValsStringEnum.ValidateValue(testValue);

            Assert.True(sut.IsSuccess == expectedResult, sut.MessagesToString());
        }
        
        [Fact]
        public void Values_operation()
        {
            ImmutableHashSet<string> sut = FourValsStringEnum.Values;
            
            Assert.Equal(4, sut.Count);
            Assert.True(sut.Contains("val1"));
            Assert.True(sut.Contains("val2"));
            Assert.True(sut.Contains("val3"));
            Assert.True(sut.Contains("val4"));
            Assert.False(sut.Contains("Rusty"));
        }
        
        [Fact]
        public void Duplicate_values_are_prohibited()
        {
            ImmutableHashSet<string> sut = FourValsStringEnum.Values;
            
            Assert.Equal(4, sut.Count);
            Assert.True(sut.Contains("val1"));
            Assert.True(sut.Contains("val2"));
            Assert.True(sut.Contains("val3"));
            Assert.True(sut.Contains("val4"));
            Assert.False(sut.Contains("Rusty"));
        }
        
        [Fact]
        public void Values_with_duplicates_are_not_supported()
        {
            Assert.Throws<NotSupportedException>(() => DuplicateValsStringEnum.Values);
        }
        
        [Fact]
        public void HasValue_with_duplicates_are_not_supported()
        {
            Assert.Throws<NotSupportedException>(() => DuplicateValsStringEnum.HasValue("123"));
        }
        
        [Fact]
        public void Values_with_duplicates_by_case_only_are_supported()
        {
            Exception? ex = Record.Exception(() =>
            {
                ImmutableHashSet<string> _ = DuplicateValsByCaseOnlyStringEnum.Values;
            });
            Assert.Null(ex);
        }
        
        [Fact]
        public void HasValue_with_duplicates_by_case_only_are_supported()
        {
            Exception? ex = Record.Exception(() => DuplicateValsByCaseOnlyStringEnum.HasValue("123"));
            Assert.Null(ex);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class FourValsStringEnum : StringEnum<FourValsStringEnum>
    {
        public const string Val1 = "val1";
        public const string Val2 = "val2";  
        public const string Val3 = "val3"; 
        public const string Val4 = "val4";
    }
    
    public sealed class DuplicateValsStringEnum : StringEnum<DuplicateValsStringEnum>
    {
        public const string Val1 = "val";
        public const string Val2 = "val";  
        public const string Val3 = "val3"; 
    }
    
    public sealed class DuplicateValsByCaseOnlyStringEnum : StringEnum<DuplicateValsByCaseOnlyStringEnum>
    {
        public const string Val1 = "val";
        public const string Val2 = "VAL";  
        public const string Val3 = "val3"; 
    }
    
}
