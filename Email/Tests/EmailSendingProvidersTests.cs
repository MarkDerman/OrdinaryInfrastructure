using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailSendingProvidersTests
    {
        [Theory]
        [InlineData("Mailgun", true)]
        [InlineData("Fake", false)]
        [InlineData("Null", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Nonsense", false)]
        public void IsProviderSupported(string? provider, bool isSupported)
        {
            Assert.Equal(isSupported, EmailSendingProviders.HasValue(provider));
        }
    }
}
