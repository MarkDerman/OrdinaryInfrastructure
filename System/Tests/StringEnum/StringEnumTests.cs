using System.Collections.Immutable;
using NUnit.Framework;
using Odin.System;

namespace Tests.Odin.System.StringEnum
{
    [TestFixture]
    public sealed class StringEnumTests
    {
        [Test]
        [TestCase("val1",true )]
        [TestCase("val1",true )]
        [TestCase("VAL1",false )]
        [TestCase("Elephant",false  )]
        public void HasValue(string? testValue, bool expectedResult)
        {
            Result sut = FourValsStringEnum.HasValue(testValue);

            Assert.That(sut.IsSuccess, Is.EqualTo(expectedResult), sut.MessagesToString());
        }
        
        [Test]
        public void Values_operation()
        {
            ImmutableHashSet<string> sut = FourValsStringEnum.Values;
            
            Assert.That(sut.Count, Is.EqualTo(4));
            Assert.That(sut.Contains("val1"), Is.True);
            Assert.That(sut.Contains("val2"), Is.True);
            Assert.That(sut.Contains("val3"), Is.True);
            Assert.That(sut.Contains("val4"), Is.True);
            Assert.That(sut.Contains("Rusty"), Is.False);
        }
        
        [Test]
        public void Duplicate_values_are_prohibited()
        {
            ImmutableHashSet<string> sut = FourValsStringEnum.Values;
            
            Assert.That(sut.Count, Is.EqualTo(4));
            Assert.That(sut.Contains("val1"), Is.True);
            Assert.That(sut.Contains("val2"), Is.True);
            Assert.That(sut.Contains("val3"), Is.True);
            Assert.That(sut.Contains("val4"), Is.True);
            Assert.That(sut.Contains("Rusty"), Is.False);
        }
        
        [Test]
        public void Values_with_duplicates_are_not_supported()
        {
            Assert.That(() => DuplicateValsStringEnum.Values, Throws.TypeOf<NotSupportedException>());
        }
        
        [Test]
        public void HasValue_with_duplicates_are_not_supported()
        {
            Assert.That(() => DuplicateValsStringEnum.HasValue("123"), Throws.TypeOf<NotSupportedException>());
        }
        
        [Test]
        public void Values_with_duplicates_by_case_only_are_supported()
        {
            Assert.DoesNotThrow(() => { ImmutableHashSet<string> _ = DuplicateValsByCaseOnlyStringEnum.Values; });
        }
        
        [Test]
        public void HasValue_with_duplicates_by_case_only_are_supported()
        {
            Assert.DoesNotThrow(() => DuplicateValsByCaseOnlyStringEnum.HasValue("123"));
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