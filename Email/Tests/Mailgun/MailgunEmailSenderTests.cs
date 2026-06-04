using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Odin.Email;
using Odin.System;
using System.Text;

namespace Tests.Odin.Email.Mailgun
{
    [Category("IntegrationTest")]
    public sealed class MailgunEmailSenderTests : IntegrationTest
    {
        private string _toTestEmail = null!;
        private string _fromTestEmail = null!;

        public MailgunEmailSenderTests()
        {
            IConfiguration config = AppFactory.GetConfiguration();
            _toTestEmail = config["Email-TestToAddress"] ?? throw new Exception("Email-TestToAddress required in configuration");
            _fromTestEmail = config["Email-TestFromAddress"] ?? throw new Exception("Email-TestFromAddress required in configuration");
        }
        [TestCase("Subject-prefix")]
        [TestCase("Subject-postfix")]
        [TestCase("Default-from-is-used")]
        [TestCase("Default-from-and-name-are-used")]
        [TestCase("No-default-from-does-not-throw")]
        [TestCase("1-tag")]
        [TestCase("2-tags")]
        [Category("IntegrationTest")]
        public async Task Sending_using_various_email_options(string testCase)
        {
            EmailSendingOptions emailSendingOptions = new EmailSendingOptions();
            IConfiguration config = AppFactory.GetConfiguration();
            MailgunEmailSenderTestBuilder scenario = new MailgunEmailSenderTestBuilder()
                .WithMailgunOptionsFromTestConfiguration(config);
            scenario.EmailSendingOptions = emailSendingOptions;

            EmailMessage email = new EmailMessage()
            {
                Subject = "MailgunSender EmailOptions test case: ",
                Body = $"<p>{testCase}</p>",
                To = [new EmailAddress(_toTestEmail)],
                From = new EmailAddress(_fromTestEmail),
                IsHtml = true
            };
            email.Subject += testCase;

            switch (testCase)
            {
                case "Subject-prefix":
                    emailSendingOptions.SubjectPrefix = "WithPrefix: ";
                    break;
                case "Subject-postfix":
                    emailSendingOptions.SubjectPostfix = " :WithPostfix";
                    break;
                case "Default-from-is-used":
                    email.From = null;
                    emailSendingOptions.DefaultFromAddress = _fromTestEmail;
                    break;
                case "Default-from-and-name-are-used":
                    email.From = null;
                    emailSendingOptions.DefaultFromAddress = _fromTestEmail;
                    emailSendingOptions.DefaultFromName = "Test From Name";
                    break;
                case "No-default-from-does-not-throw":
                    emailSendingOptions.DefaultFromAddress = null;
                    emailSendingOptions.DefaultFromName = null;
                    break;
                case "1-tag":
                    emailSendingOptions.DefaultTags = new List<string> { "Dev" };
                    break;
                case "2-tags":
                    emailSendingOptions.DefaultTags = new List<string> { "Dev", "QA" };
                    break;
                default:
                    throw new InvalidOperationException($"Case {testCase} not implemented");
            }
            MailgunEmailSender sut = scenario.Build();

            ResultValue<string> result = await sut.SendEmail(email);

            VerifySuccessfulSendAndLogging(scenario, email, result);
        }

        [Test]
        [Category("IntegrationTest")]
        public async Task Send_with_attachment()
        {
            IConfiguration config = AppFactory.GetConfiguration();
            MailgunEmailSenderTestBuilder scenario = new MailgunEmailSenderTestBuilder()
                .WithMailgunOptionsFromTestConfiguration(config)
                .WithEmailSendingOptionsFromTestConfiguration(config);
            EmailMessage message = new EmailMessage();
            message.From = new EmailAddress(_fromTestEmail);
            message.ReplyTo = new EmailAddress(_fromTestEmail);
            message.To.Add(new EmailAddress(_toTestEmail));
            message.Subject = "MailgunEmailSenderTests.Send_email_with_attachment";
            message.IsHtml = true;
            message.Body = "<p>Body text</p>";
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes("Log file contents..."));
            stream.Position = 0;
            Attachment txtAttachment = new Attachment("MyFile.log", stream, "text/plain");
            message.Attachments.Add(txtAttachment);
            MailgunEmailSender mailgunSender = scenario.Build();

            ResultValue<string> result = await mailgunSender.SendEmail(message);

            VerifySuccessfulSendAndLogging(scenario, message, result);
        }

        private void VerifySuccessfulSendAndLogging(MailgunEmailSenderTestBuilder scenario, EmailMessage message, ResultValue<string> result)
        {
            // Result
            Assert.That(result.IsSuccess, Is.True, result.MessagesToString());
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(string.IsNullOrWhiteSpace(result.Value), Is.False, "Message Id expected from Mailgun");

            // Should be no warnings, errors
            scenario.LoggerMock!.Verify(c => c.Log(It.IsNotIn(LogLevel.Information), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
            scenario.LoggerMock!.Verify(c => c.Log(It.IsNotIn(LogLevel.Information), It.IsAny<Exception>()), Times.Never);
            // Should log a message to Information
            string expectedLogMessage =
                $"SendEmail to {_toTestEmail} succeeded. Subject - '{message.Subject}'. Sent with Mailgun reference {result.Value}.";

            scenario.LoggerMock!.Verify(c => c.Log(LogLevel.Information, expectedLogMessage, null as Exception), Times.Once);

        }

        /// <summary>
        /// Ensure send does not succeed, and that appropriate logging is called.
        /// </summary>
        [Test]
        [Category("IntegrationTest")]
        public async Task Send_handles_and_logs_for_bad_Mailgun_api_key()
        {
            IConfiguration config = AppFactory.GetConfiguration();
            MailgunEmailSenderTestBuilder scenario = new MailgunEmailSenderTestBuilder()
                .WithEmailSendingOptionsFromTestConfiguration(config)
                .WithMailgunOptionsFromTestConfiguration(config);
            scenario.MailgunOptions.ApiKey = "testing_incorrect_api_key";
            MailgunEmailSender mailgunSender = scenario.Build();
            EmailMessage message = new EmailMessage();
            message.From = new EmailAddress(_fromTestEmail);
            message.To.Add(new EmailAddress(_toTestEmail));
            message.Subject = "Bad api key test";
            message.IsHtml = false;
            message.Body = "Bad api key test";
            string expectedLogMessage =
                $"SendEmail to {_toTestEmail} failed. Subject - '{message.Subject}'. Error - Failed to send email with Mailgun. Status code: 401 Unauthorized. Response content: Forbidden";

            ResultValue<string> result = await mailgunSender.SendEmail(message);

            // 3 retries
            scenario.LoggerMock!.Verify(c => c.Log(LogLevel.Error, expectedLogMessage, It.IsAny<Exception>()), Times.Exactly(4));
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Messages, Is.Not.Empty);
            Assert.That(result.Messages[0], Does.Contain("401"));
            Assert.That(result.Messages[0], Does.Contain("Unauthorized"));
        }
    }
}