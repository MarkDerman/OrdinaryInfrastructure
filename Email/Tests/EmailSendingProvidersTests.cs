using NUnit.Framework;
using Odin.Email;

namespace Tests.Odin.Email
{
    [TestFixture]
    public sealed class EmailSendingProvidersTests
    {
        [Test]
        [TestCase("Mailgun", true)]
        [TestCase("Fake", false)]
        [TestCase("Null", true)]
        [TestCase(EmailSendingProviders.Mailgun, true)]
        [TestCase(EmailSendingProviders.Null, true)]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Nonsense", false)]
        public void IsProviderSupported(string provider, bool isSupported)
        {
            Assert.That(EmailSendingProviders.HasValue(provider), Is.EqualTo(isSupported));
        }
    }
}