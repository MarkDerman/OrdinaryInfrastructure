using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class DependencyInjectionExtensionsTests
    {
        [Fact]
        public void Add_Null_provider()
        {
            WebApplicationBuilder Builder = WebApplication.CreateBuilder();
            Builder.Configuration.AddJsonStream(Stream(GetNullSenderConfigJson()));
            Builder.Services.AddOdinEmailSending(Builder.Configuration);
            WebApplication sut = Builder.Build();

            IEmailSender? mailSender = sut.Services.GetService<IEmailSender>();
            EmailSendingOptions? config = sut.Services.GetService<EmailSendingOptions>();

            Assert.NotNull(mailSender);
            Assert.IsType<NullEmailSender>(mailSender);
            Assert.NotNull(config);
        }

        [Fact]
        public void Add_Mailgun_provider()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonStream(Stream(MailgunConfigJson));
            builder.Services.AddOdinEmailSending(builder.Configuration);
            WebApplication sut = builder.Build();

            IEmailSender? provider = sut.Services.GetService<IEmailSender>();
            EmailSendingOptions? config = sut.Services.GetService<EmailSendingOptions>();
            MailgunOptions? mailgunConfig = sut.Services.GetService<MailgunOptions>();

            Assert.NotNull(provider);
            Assert.IsType<MailgunEmailSender>(provider);
            Assert.NotNull(config);
            Assert.NotNull(mailgunConfig);
        }
        
        [Fact]
        public void Add_Office365_provider()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonStream(Stream(Office365ConfigJson));
            builder.Services.AddOdinEmailSending(builder.Configuration);
            WebApplication sut = builder.Build();

            IEmailSender? provider = sut.Services.GetService<IEmailSender>();
            EmailSendingOptions? config = sut.Services.GetService<EmailSendingOptions>();
            Office365Options? providerConfig = sut.Services.GetService<Office365Options>();

            Assert.NotNull(provider);
            Assert.IsType<Office365EmailSender>(provider);
            Assert.NotNull(config);
            Assert.NotNull(providerConfig);
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
