using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailMessageTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Construction_sets_properties(bool isHtml)
        {
            EmailMessage sut = new EmailMessage("to@d.com", "from@d.com", "subj", "bod", isHtml);
         
            Assert.Equal("to@d.com", sut.To[0].Address);
            Assert.Equal("from@d.com", sut.From!.Address);
            Assert.Equal("subj", sut.Subject);
            Assert.Equal("bod", sut.Body);
            Assert.Equal(isHtml, sut.IsHtml);
            Assert.Equal(Priority.Normal, sut.Priority);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Construction_defaults_for_empty_body(string? body)
        {
            EmailMessage sut = new EmailMessage("to@d.com", "from@d.com", "subj", body, false);
            EmailMessage sut2 = new EmailMessage();
            
            Assert.Equal("", sut.Body);
            Assert.Equal("", sut2.Body);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Construction_defaults_for_empty_subject(string? subject)
        {
            EmailMessage sut = new EmailMessage("to@d.com", "from@d.com", subject, "body", false);
            EmailMessage sut2 = new EmailMessage();
            
            Assert.Equal("", sut.Subject);
            Assert.Equal("", sut2.Subject);
        }
        
    }
}
