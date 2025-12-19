using System.Text;
using NUnit.Framework;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Odin.Email;

namespace Tests.Odin.Email
{
    [TestFixture]
    public sealed class DependencyInjectionExtensionsTests
    {
        [Test]
        public void Add_Null_provider()
        {
            WebApplicationBuilder Builder = WebApplication.CreateBuilder();
            Builder.Configuration.AddJsonStream(Stream(GetNullSenderConfigJson()));
            Builder.Services.AddOdinEmailSending(Builder.Configuration);
            WebApplication sut = Builder.Build();

            IEmailSender? mailSender = sut.Services.GetService<IEmailSender>();
            EmailSendingOptions? config = sut.Services.GetService<EmailSendingOptions>();

            Assert.That(mailSender, Is.Not.Null);
            Assert.That(mailSender, Is.InstanceOf<NullEmailSender>());
            Assert.That(config, Is.Not.Null);
        }

        [Test]
        public void Add_Mailgun_provider()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonStream(Stream(MailgunConfigJson));
            builder.Services.AddOdinEmailSending(builder.Configuration);
            WebApplication sut = builder.Build();

            IEmailSender? provider = sut.Services.GetService<IEmailSender>();
            EmailSendingOptions? config = sut.Services.GetService<EmailSendingOptions>();
            MailgunOptions? mailgunConfig = sut.Services.GetService<MailgunOptions>();

            Assert.That(provider, Is.Not.Null);
            Assert.That(provider, Is.InstanceOf<MailgunEmailSender>());
            Assert.That(config, Is.Not.Null);
            Assert.That(mailgunConfig, Is.Not.Null);
        }
        
        [Test]
        public void Add_Office365_provider()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonStream(Stream(Office365ConfigJson));
            builder.Services.AddOdinEmailSending(builder.Configuration);
            WebApplication sut = builder.Build();

            IEmailSender? provider = sut.Services.GetService<IEmailSender>();
            EmailSendingOptions? config = sut.Services.GetService<EmailSendingOptions>();
            Office365Options? providerConfig = sut.Services.GetService<Office365Options>();

            Assert.That(provider, Is.Not.Null);
            Assert.That(provider, Is.InstanceOf<Office365EmailSender>());
            Assert.That(config, Is.Not.Null);
            Assert.That(providerConfig, Is.Not.Null);
        }


        public static string GetNullSenderConfigJson()
        {
            return @"{
  ""EmailSending"": {
    ""DefaultFromAddress"": ""rubbish@domain.co.za"",
    ""DefaultFromName"": ""LocalDevelopment"",
    ""Provider"": ""Null"",
  }
}";
        }

        public const string MailgunConfigJson = @"{
  ""EmailSending"": {
    ""DefaultFromAddress"": ""noreply@splendid.bom"",
    ""DefaultFromName"": ""LocalDevelopment"",
    ""Provider"": ""Mailgun"",
    ""Mailgun"": {
      ""ApiKey"": ""AAAAAAAAAABBBBBBBBBBAAAAAAAAAABBBBBBBBBB"",
      ""Domain"": ""mailgun.domain.com""
    }
  }
}";

        public const string Office365ConfigJson = @"{
  ""EmailSending"": {
    ""DefaultFromAddress"": ""noreply@splendid.bom"",
    ""DefaultFromName"": ""LocalDevelopment"",
    ""Provider"": ""Office365"",
    ""Office365"": {
        ""SenderUserId"": ""Set-in-user-secrets"",
        ""MicrosoftGraphClientSecretCredentials"": {
          ""ClientId"": ""Set-in-user-secrets"",
          ""TenantId"": ""Set-in-user-secrets"",
          ""ClientSecret"": ""Set-in-user-secrets""
        }
    }
  }
}";
   

        public static Stream Stream(string input)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);

            writer.Write(input);
            writer.Flush();

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}