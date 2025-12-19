using NUnit.Framework;
using Odin.Email;
using Odin.System;


namespace Tests.Odin.Email
{
    [TestFixture]
    public sealed class EmailSendingOptionsTests
    {
        [Test]
        [TestCase("Mailgun", true)]
        [TestCase("Null", true)]
        [TestCase(EmailSendingProviders.Mailgun, true)]
        [TestCase(EmailSendingProviders.Null, true)]
        public void IsConfigurationValid_requires_valid_provider(string provider, bool isValidConfig)
        {
            EmailSendingOptions sut = new EmailSendingOptions()
            {
                DefaultFromAddress = "123",
                DefaultFromName = "Tiger",
                Provider = provider
            };

            Result result = sut.Validate();
            
            Assert.That(result.IsSuccess, Is.EqualTo(isValidConfig));
        }
    }
}