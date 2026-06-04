using Odin.Email;
using Odin.System;


namespace Tests.Odin.Email
{
    public sealed class EmailSendingOptionsTests
    {
        [TestCase("Mailgun", true)]
        [TestCase("Null", true)]
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