using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Odin.Email;
using Odin.System;


namespace Tests.Odin.Email.Office365;

[TestFixture]
[Category("IntegrationTest")]
public class Office365EmailSenderTests : IntegrationTest
{
    private string _toTestEmail = null!;
    private string _fromTestEmail = null!;

    [SetUp]
    public void Setup()
    {
        IConfiguration config = AppFactory.GetConfiguration();
        IConfigurationSection office365Options = config.GetRequiredSection("Email-Office365");

        _toTestEmail = config["Email-TestToAddress"] ?? throw new Exception("Email-TestToAddress required in configuration");
        Office365Options options = new Office365Options();
        office365Options.Bind(options);
        _fromTestEmail = options.SenderUserId!;
        
    }

    private EmailSendingOptions GetEmailOptionsForOffice365()
    {
        return new EmailSendingOptions()
        {
            Provider = EmailSendingProviders.Office365
        };
    }

    [Test]
    [TestCase("1-Tag")]
    [TestCase("Null-Tags")]
    [TestCase("Empty-Tags")]
    [TestCase("Many-Tags")]
    [TestCase("1-Attachment")]
    [TestCase("2-Attachments")]
    [TestCase("Plain-Text-Body")]
    public async Task Send_various_emails(string testCase)
    {
        EmailMessage email = new EmailMessage()
        {
            Subject = "Office365 test case: ",
            Body = $"<p>{testCase}</p>",
            To = [new EmailAddress(_toTestEmail, "Test To Name")],
            From = new EmailAddress(_fromTestEmail, "Test From Name"),
            IsHtml = true
        };
        email.Subject += testCase;
        switch (testCase)
        {
            case "1-Tag":
                email.Tags = ["Dev"];
                break;
            case "Many-Tags":
                email.Tags = ["QA", "Dev", "Outgoing Payments"];
                break;
            case "Null-Tags":
                email.Tags = null!;
                break;
            case "Empty-Tags":
                email.Tags = new List<string>();
                break;
            case "1-Attachment":
                email.Attachments =
                    [new Attachment("file.txt", new MemoryStream("This is a text file"u8.ToArray()), "text/plain")];
                break;
            case "2-Attachments":
                email.Attachments =
                [
                    new Attachment("file1.txt", new MemoryStream("This is a text file 1"u8.ToArray()), "text/plain"),
                    new Attachment("file2.txt", new MemoryStream("This is a text file 2"u8.ToArray()), "text/plain")
                ];
                break;
            case "Plain-Text-Body":
                email.IsHtml = false;
                email.Body = testCase;
                break;
            default:
                Assert.Fail($"Case {testCase} not implemented");
                break;
        }
        IConfiguration config = AppFactory.GetConfiguration();

        Office365EmailSenderTestBuilder scenario = new Office365EmailSenderTestBuilder()
            .WithOffice365OptionsFromTestConfiguration(config)
            .WithEmailSendingOptionsFromTestConfiguration(config);
        Office365EmailSender sut = scenario.Build();
        
        ResultValue<string?> result = await sut.SendEmail(email);

        VerifySuccessfulSendAndLogging(scenario, email, result);
    }

    [Test]
    [TestCase("Subject-prefix")]
    [TestCase("Subject-postfix")]
    [TestCase("Default-from-is-used")]
    [TestCase("Default-from-and-name-are-used")]
    [TestCase("No-default-from-does-not-throw")]
    [TestCase("1-tag")]
    [TestCase("2-tags")]
    public async Task Send_correctly_implements_email_options(string testCase)
    {
        EmailSendingOptions emailSendingOptions = new EmailSendingOptions();
        IConfiguration config = AppFactory.GetConfiguration();
        Office365EmailSenderTestBuilder scenario = new Office365EmailSenderTestBuilder()
            .WithOffice365OptionsFromTestConfiguration(config);
        scenario.EmailSendingOptions = emailSendingOptions;

        EmailMessage email = new EmailMessage()
        {
            Subject = "Office365 EmailOptions test case: ",
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
                Assert.Fail($"Case {testCase} not implemented");
                break;
        }

        Office365EmailSender sut = scenario.Build();

        ResultValue<string?> result = await sut.SendEmail(email);

        VerifySuccessfulSendAndLogging(scenario, email, result);
    }

    private void VerifySuccessfulSendAndLogging(Office365EmailSenderTestBuilder scenario, EmailMessage message,
        ResultValue<string?> result)
    {
        // Result
        Assert.That(result.IsSuccess, Is.True, result.MessagesToString());
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(string.IsNullOrWhiteSpace(result.Value), Is.False, "Message Id expected from Office365");

        // Should be no warnings, errors
        scenario.LoggerMock!.Verify(
            c => c.Log(It.IsNotIn(LogLevel.Information), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        scenario.LoggerMock!.Verify(c => c.Log(It.IsNotIn(LogLevel.Information), It.IsAny<Exception>()), Times.Never);
        // Should log a message to Information
        string expectedLogMessage =
            $"SendEmail to {_toTestEmail} succeeded. Subject - '{message.Subject}'. Sent with Office365 via user {scenario.Office365Options.SenderUserId}";

        scenario.LoggerMock!.Verify(c => c.Log(LogLevel.Information, expectedLogMessage), Times.Once);
    }
}