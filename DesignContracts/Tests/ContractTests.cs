using Odin.DesignContracts;

namespace Tests.Odin.DesignContracts
{
    public sealed class ContractTests
    {
        [Theory]
        [InlineData("not fred.", "(arg != fred)", "Precondition failed: not fred. [Condition: (arg != fred)]")]
        [InlineData("not fred.", "  ", "Precondition failed: not fred.")]
        [InlineData("not fred.", null, "Precondition failed: not fred.")]
        [InlineData("not fred.", "", "Precondition failed: not fred.")]
        [InlineData("", "", "Precondition failed.")]
        [InlineData("", null, "Precondition failed.")]
        [InlineData(" ", null, "Precondition failed.")]
        [InlineData(null, null, "Precondition failed.")]
        [InlineData(null, "", "Precondition failed.")]
        [InlineData(null, " ", "Precondition failed.")]
        [InlineData(null, "(arg==0)", "Precondition failed: (arg==0)")]
        public void Requires_throws_exception_with_correct_message_on_precondition_failure(string? conditionDescription, string? conditionText, string expectedExceptionMessage)
        {
            ContractException ex = Assert.Throws<ContractException>(() => Precondition.Requires(false, conditionDescription,conditionText));
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void Requires_does_not_throw_exception_on_precondition_success()
        {
            Exception? ex = Record.Exception(() => Precondition.Requires(true, "Message"));
            Assert.Null(ex);
        }
    }

    public abstract class ContractRequiresGenericTests<TException> where TException : Exception
    {
        [Fact]
        public void Requires_throws_specific_exception_on_precondition_failure()
        {
            TException ex = Assert.Throws<TException>(() => Precondition.Requires<TException>(false, "msg"));
            Assert.IsType<TException>(ex);
        }
    }

    public sealed class ContractRequiresArgumentNullExceptionTests
        : ContractRequiresGenericTests<ArgumentNullException>;

    public sealed class ContractRequiresArgumentExceptionTests
        : ContractRequiresGenericTests<ArgumentException>;

    public sealed class ContractRequiresDivideByZeroExceptionTests
        : ContractRequiresGenericTests<DivideByZeroException>;
}
