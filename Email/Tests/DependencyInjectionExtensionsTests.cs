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
        [TestCaseSource(nameof(GetFakeSenderConfigs))]
        public void AddEmailSending_adds_FakeEmailSender_to_application_from_configuration(string json)
        {
            WebApplicationBuilder Builder = WebApplication.CreateBuilder();
            Builder.Configuration.AddJsonStream(Stream(json));
            Builder.Services.AddOdinEmailSending(Builder.Configuration);
            WebApplication sut = Builder.Build();
            
            IEmailSender? mailSender = sut.Services.GetService<IEmailSender>();
            EmailSendingOptions? config = sut.Services.GetService<EmailSendingOptions>();
            
            Assert.That(mailSender, Is.Not.Null);
            Assert.That(mailSender, Is.InstanceOf<NullEmailSender>());
            Assert.That(config, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetMailgunSenderConfigs))]
        public void AddEmailSending_adds_MailgunEmailSender_to_application_from_configuration(string json)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonStream(Stream(json));
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

        public static string[] GetFakeSenderConfigs()
        {
            return new[] { GetNullSenderConfigJson() };
        }
        
        public static string[] GetMailgunSenderConfigs()
        {
            return new[] { GetMailgunConfigJson() };
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
        
        public static string GetMailgunConfigJson()
        {
            return @"{
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
        }
        
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