using Odin.Email;
using Odin.System;


namespace Tests.Odin.Email
{
    public sealed class EmailSendingOptionsTests
    {
        [Theory]
        [InlineData("Mailgun", true)]
        [InlineData("Null", true)]
        public void IsConfigurationValid_requires_valid_provider(string provider, bool isValidConfig)
        {
            EmailSendingOptions sut = new EmailSendingOptions()
            {
                DefaultFromAddress = "123",
                DefaultFromName = "Tiger",
                Provider = provider
            };

            Result result = sut.Validate();
            
            Assert.Equal(isValidConfig, result.IsSuccess);
        }
    }
}
