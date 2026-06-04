using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailMessageTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void Construction_sets_properties(bool isHtml)
        {
            EmailMessage sut = new EmailMessage("to@d.com", "from@d.com", "subj", "bod", isHtml);

            Assert.That(sut.To[0].Address, Is.EqualTo("to@d.com"));
            Assert.That(sut.From!.Address, Is.EqualTo("from@d.com"));
            Assert.That(sut.Subject, Is.EqualTo("subj"));
            Assert.That(sut.Body, Is.EqualTo("bod"));
            Assert.That(sut.IsHtml, Is.EqualTo(isHtml));
            Assert.That(sut.Priority, Is.EqualTo(Priority.Normal));
        }
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Construction_defaults_for_empty_body(string? body)
        {
            EmailMessage sut = new EmailMessage("to@d.com", "from@d.com", "subj", body, false);
            EmailMessage sut2 = new EmailMessage();

            Assert.That(sut.Body, Is.EqualTo(""));
            Assert.That(sut2.Body, Is.EqualTo(""));
        }
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Construction_defaults_for_empty_subject(string? subject)
        {
            EmailMessage sut = new EmailMessage("to@d.com", "from@d.com", subject, "body", false);
            EmailMessage sut2 = new EmailMessage();

            Assert.That(sut.Subject, Is.EqualTo(""));
            Assert.That(sut2.Subject, Is.EqualTo(""));
        }

    }
}