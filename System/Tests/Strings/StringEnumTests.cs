using Odin.System;
using System.Collections.Immutable;

namespace Tests.Odin.System.Strings
{
    public sealed class StringEnumTests
    {
        [TestCase("val1", true)]
        [TestCase("VAL1", false)]
        [TestCase("Elephant", false)]
        public void HasValue(string? testValue, bool expectedResult)
        {
            bool sut = FourValsStringEnum.HasValue(testValue);

            Assert.That(sut, Is.EqualTo(expectedResult));
        }
        [TestCase("val1", true)]
        [TestCase("VAL1", false)]
        [TestCase("Elephant", false)]
        public void ValidateValue(string? testValue, bool expectedResult)
        {
            Result sut = FourValsStringEnum.ValidateValue(testValue);

            Assert.That(sut.IsSuccess == expectedResult, Is.True, sut.MessagesToString());
        }

        [Test]
        public void Values_operation()
        {
            ImmutableHashSet<string> sut = FourValsStringEnum.Values;

            Assert.That(sut.Count, Is.EqualTo(4));
            Assert.That(sut, Does.Contain("val1"));
            Assert.That(sut, Does.Contain("val2"));
            Assert.That(sut, Does.Contain("val3"));
            Assert.That(sut, Does.Contain("val4"));
            Assert.That(sut, Does.Not.Contain("Rusty"));
        }

        [Test]
        public void Duplicate_values_are_prohibited()
        {
            ImmutableHashSet<string> sut = FourValsStringEnum.Values;

            Assert.That(sut.Count, Is.EqualTo(4));
            Assert.That(sut, Does.Contain("val1"));
            Assert.That(sut, Does.Contain("val2"));
            Assert.That(sut, Does.Contain("val3"));
            Assert.That(sut, Does.Contain("val4"));
            Assert.That(sut, Does.Not.Contain("Rusty"));
        }

        [Test]
        public void Values_with_duplicates_are_not_supported()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                ImmutableHashSet<string> _ = DuplicateValsStringEnum.Values;
            });
        }

        [Test]
        public void HasValue_with_duplicates_are_not_supported()
        {
            Assert.Throws<NotSupportedException>(() => DuplicateValsStringEnum.HasValue("123"));
        }

        [Test]
        public void Values_with_duplicates_by_case_only_are_supported()
        {
            Assert.DoesNotThrow(() =>
            {
                ImmutableHashSet<string> _ = DuplicateValsByCaseOnlyStringEnum.Values;
            });
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
