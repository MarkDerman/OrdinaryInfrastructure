using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailSendingProvidersTests
    {
        [TestCase("Mailgun", true)]
        [TestCase("Fake", false)]
        [TestCase("Null", true)]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Nonsense", false)]
        public void IsProviderSupported(string? provider, bool isSupported)
        {
            Assert.That(EmailSendingProviders.HasValue(provider), Is.EqualTo(isSupported));
        }
    }
}