using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using Odin.DesignContracts;
using Odin.Logging;
using Odin.System;

namespace Odin.Email;

/// <summary>
/// Sends email via Office365 Microsoft GraphClient
/// </summary>
public class Office365EmailSender : IEmailSender
{
    private readonly GraphServiceClient _graphClient;
    private readonly string _senderUserId;
    private readonly EmailSendingOptions _emailSettings;
    private readonly ILoggerWrapper<Office365EmailSender> _logger;

    /// <summary>
    /// Sends email via Office365 GraphClient
    /// </summary>
    /// <param name="office365Options"></param>
    /// <param name="emailSettings"></param>
    /// <param name="logger">Microsoft UserId</param>
    public Office365EmailSender(Office365Options office365Options, EmailSendingOptions emailSettings , ILoggerWrapper<Office365EmailSender> logger)
    {
        Precondition.RequiresNotNull(office365Options);
        Precondition.RequiresNotNull(emailSettings);
        Precondition.RequiresNotNull(logger);

        _emailSettings = emailSettings;
        _logger = logger;
        
        ClientSecretCredentialOptions credentialOptions = new ClientSecretCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };
        ClientSecretCredential clientSecretCredential = new ClientSecretCredential(
            office365Options.MicrosoftGraphClientSecretCredentials!.TenantId,
            office365Options.MicrosoftGraphClientSecretCredentials.ClientId,
            office365Options.MicrosoftGraphClientSecretCredentials.ClientSecret,
            credentialOptions);

        _graphClient = new GraphServiceClient(clientSecretCredential);

        _senderUserId = string.IsNullOrWhiteSpace(office365Options.SenderUserId)
            ? _emailSettings.DefaultFromAddress!
            : office365Options.SenderUserId;
    }


    const string MicrosoftGraphFileAttachmentOdataType = "#microsoft.graph.fileAttachment";
    
    /// <inheritdoc />
    public async Task<ResultValue<string?>> SendEmail(IEmailMessage email)
    {
        if (email.From is null)
        {
            Precondition.Requires(!string.IsNullOrWhiteSpace(_emailSettings.DefaultFromAddress), "Cannot fall back to the default from address, since it is missing.");
            email.From = new EmailAddress(_emailSettings.DefaultFromAddress!, _emailSettings.DefaultFromName);
        }
        email.Subject = string.Concat(_emailSettings.SubjectPrefix, email.Subject,
            _emailSettings.SubjectPostfix);
        
       // string userId = email.From?.Address ?? defaultSenderUserId;
        try
        {
            SendMailPostRequestBody requestBody = new SendMailPostRequestBody()
            {
                Message = new Message
                {
                    Subject = email.Subject,
                    Body = new ItemBody
                    {
                        ContentType = email.IsHtml ? BodyType.Html : BodyType.Text,
                        Content = email.Body,

                    },
                    ToRecipients = email.To.Select(a => new Recipient
                    {
                        EmailAddress = a.ToOffice365EmailAddress()
                    }).ToList(),
                    CcRecipients = email.CC.Select(a => new Recipient
                    {
                        EmailAddress = a.ToOffice365EmailAddress()
                    }).ToList(),
                    BccRecipients = email.BCC.Select(a => new Recipient
                    {
                        EmailAddress = a.ToOffice365EmailAddress()
                    }).ToList(),
                    From = email.From is null
                        ? null
                        : new Recipient
                        {
                            EmailAddress = email.From.ToOffice365EmailAddress()
                        },
                    ReplyTo = email.ReplyTo is null
                        ? []
                        :
                        [
                            new Recipient
                            {
                                EmailAddress = email.ReplyTo.ToOffice365EmailAddress()
                            }
                        ],
                    Attachments = email.Attachments.Select(a => new FileAttachment
                    {
                        OdataType = MicrosoftGraphFileAttachmentOdataType,
                        Name = a.FileName,
                        ContentType = a.ContentType,
                        ContentBytes = ToByteArray(a.Data),
                    } as Microsoft.Graph.Models.Attachment).ToList(),
                    Categories = _emailSettings.DefaultTags == null ? 
                        email.Tags : email.Tags.Concat(_emailSettings.DefaultTags)
                            .Distinct().ToList(),
                }
            };

            await _graphClient.Users[_senderUserId].SendMail.PostAsync(requestBody);
            LogSendEmailResult(email, true, LogLevel.Information, $"Sent with Office365 via user {_senderUserId}");
            return ResultValue<string?>.Success("Success");
        }
        catch (Exception ex)
        {
            LogSendEmailResult(email, false, LogLevel.Error, $"Failed to send with Office365 via user {_senderUserId}", ex);
            return ResultValue<string?>.Failure("Fail: " + ex.Message);
        }
    }


    static byte[] ToByteArray(Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        if (inputStream.CanSeek)
        {
            inputStream.Seek(0, SeekOrigin.Begin);
        }

        using MemoryStream memoryStream = new MemoryStream();
        inputStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
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
}