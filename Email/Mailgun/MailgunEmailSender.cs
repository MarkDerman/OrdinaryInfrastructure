using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Odin.DesignContracts;
using Odin.Logging;
using Odin.System;
using Polly;
using Polly.Retry;

namespace Odin.Email
{
    /// <summary>
    /// Sends email via Mailgun.
    /// </summary>
    /// <see href="https://mailgun.com"/>
    public sealed class MailgunEmailSender : IEmailSender
    {
        private readonly MailgunOptions _mailgunSettings;
        private readonly EmailSendingOptions _emailSettings;
        private readonly ILoggerWrapper<MailgunEmailSender> _logger;
        private HttpClient _httpClient;
        
        private static ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(20))
            .AddRetry(new RetryStrategyOptions()
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(0.5),
                UseJitter = true,
            })
            .Build();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mailgunSettings"></param>
        /// <param name="emailSettings"></param>
        /// <param name="logger"></param>
        public MailgunEmailSender(MailgunOptions mailgunSettings,
            EmailSendingOptions emailSettings, ILoggerWrapper<MailgunEmailSender> logger)
        {
            Precondition.RequiresNotNull(mailgunSettings);
            Precondition.RequiresNotNull(emailSettings);
            Precondition.RequiresNotNull(logger);
            _mailgunSettings = mailgunSettings;
            _emailSettings = emailSettings;
            _logger = logger;
            _httpClient = new HttpClient();
            string endPoint = _mailgunSettings.Region.Equals(MailgunOptions.RegionEU, StringComparison.OrdinalIgnoreCase)
                    ? "https://api.eu.mailgun.net/v3"
                    : "https://api.mailgun.net/v3";
            
            // Trailing slash required so that the /v3 isn't replaced by /{domain}
            if (endPoint.Last() != '/')
            {
                endPoint += "/";
            }

            Precondition.Requires(!string.IsNullOrWhiteSpace(_mailgunSettings.Domain), "Domain missing in MailgunOptions");
            string subPath = $"{_mailgunSettings.Domain}/messages";
            // Leading slash will replace the /v3
            if (subPath[0] == '/')
            {
                subPath = subPath.Substring(1);
            }
            _httpClient.BaseAddress = new Uri(new Uri(endPoint), subPath);
            
            Precondition.Requires(!string.IsNullOrWhiteSpace(_mailgunSettings.ApiKey), "ApiKey missing in MailgunOptions");
            
            byte[] byteArray = Encoding.ASCII.GetBytes($"api:{_mailgunSettings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", 
                Convert.ToBase64String(byteArray));
        }

        private static string EncodeAsHtml(string input)
        {
            string encoded = WebUtility.HtmlEncode(input);
            encoded = encoded.Replace("\n", "<br/>");
            return encoded;
        }

        private static ByteArrayContent ToByteArrayContent(Stream stream)
        {
            Precondition.RequiresNotNull(stream);
            Precondition.Requires(stream.CanRead, "Stream.CanRead must be true");
            Precondition.Requires(stream.CanSeek, "Stream.CanSeek must be true");

            try
            {
                stream.Position = 0;
                using MemoryStream memoryStream = new MemoryStream((int)stream.Length);
                stream.CopyTo(memoryStream);
                byte[] byteArray = memoryStream.ToArray();
                return new ByteArrayContent(byteArray);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not convert stream to ByteArrayContent.", e);
            }
            
        }

        /// <summary>
        /// Sends the email using Mailgun's API at /{domain}/messages. Does three retries with exponential backoff. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>An Outcome containing the Mailgun messageId.</returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<ResultValue<string>> SendEmail(IEmailMessage email)
        {
            Precondition.RequiresNotNull(email);
            Precondition.Requires(email.To.Any(), "Mailgun requires one or more to addresses.");
            Precondition.Requires(!string.IsNullOrWhiteSpace(email.Subject), "Mailgun requires an email subject");
            
            try
            {
                MultipartFormDataContent content = new MultipartFormDataContent();

                if (email.From is null)
                {
                    if (string.IsNullOrWhiteSpace(_emailSettings.DefaultFromAddress)) 
                        throw new Exception("Cannot fall back to using the DefaultFromAddress. It is not set in configuration.");
                    email.From = new EmailAddress(_emailSettings.DefaultFromAddress!, _emailSettings.DefaultFromName);
                }
                email.Subject = string.Concat(_emailSettings.SubjectPrefix, email.Subject,
                    _emailSettings.SubjectPostfix);
                
                content.Add(new StringContent(email.From.ToString()), "from");
                content.Add(new StringContent(string.Join(",", email.To.Select(a => a.ToString()))), "to");
                content.Add(new StringContent(email.Subject), "subject");

                if (email.IsHtml)
                {
                    content.Add(new StringContent(email.Body ?? string.Empty), "html");
                }
                else
                {
                    content.Add(new StringContent(email.Body ?? string.Empty), "text");
                    // Mailgun API requires this field.
                    content.Add(new StringContent(EncodeAsHtml(email.Body ?? string.Empty)), "html");
                }

                if (email.ReplyTo is not null)
                {
                    content.Add(new StringContent(email.ReplyTo.ToString()), "h:Reply-To");
                }

                if (email.CC.Any())
                {
                    content.Add(new StringContent(string.Join(",", email.CC.Select(a => a.ToString()))), "cc");
                }

                if (email.BCC.Any())
                {
                    content.Add(new StringContent(string.Join(",", email.BCC.Select(a => a.ToString()))), "bcc");
                }

                foreach (Attachment attachment in email.Attachments)
                {
                    ByteArrayContent fileContent = ToByteArrayContent(attachment.Data);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(attachment.ContentType);
                    content.Add(fileContent, "attachment", attachment.FileName);
                }

                HttpResponseMessage responseMessage = await _resiliencePipeline.ExecuteAsync(async token =>
                {
                    HttpResponseMessage message = await _httpClient.PostAsync("", content, token);
                    if (!message.IsSuccessStatusCode)
                    {
                        string responseBody = await message.Content.ReadAsStringAsync(token);
                        string errorMessage = $"Failed to send email with Mailgun. Status code: {(int)message.StatusCode} {message.StatusCode}. Response content: " + responseBody;
                        LogSendEmailResult(email, false, LogLevel.Error, errorMessage);
                        throw new HttpRequestException(errorMessage, null, message.StatusCode);
                    }
                    return message;
                });


                MailgunSendResponse? response = await responseMessage.Content.ReadFromJsonAsync<MailgunSendResponse>();
                LogSendEmailResult(email, true, LogLevel.Information, $"Sent with Mailgun reference {response?.Id}.");
                return ResultValue<string>.Success(response?.Id ?? "");
            }
            catch (Exception e)
            {
                return ResultValue<string>.Failure(e.ToString());
            }

        }
        
        private void LogSendEmailResult(IEmailMessage email, bool isSuccess, LogLevel level, string message, Exception? exception = null)
        {
            string to = "";
            try
            {
                if (email.To != null!)
                {
                    to = string.Join(',', email.To.Select(c => c.Address).ToList());
                }
            }
            catch 
            {
            }

            if (isSuccess)
            {
                _logger.Log(level, $"{nameof(SendEmail)} to {to} succeeded. Subject - '{email.Subject}'. {message}",
                    exception);
            }
            else
            {
                _logger.Log(level, $"{nameof(SendEmail)} to {to} failed. Subject - '{email.Subject}'. Error - {message}",
                    exception);
            }
        }
        
        private static string GetEmailAsString(string email, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return email;
            return $"{name} <{email}>";
        }
    }
}