# Odin.Emailing V1 Development Plan

## A - Project Goals

The Odin.Emailing namespace will provide a common object model for creating and sending email through the world's most popular providers used by developers of .NET APIs and applications. 

The objective is to provide a single common object model for use with all the world's most popular transactional, 
marketing, developer-focussed, enterprise SaaS providers sending APIs and email object models.

Aspects that might emerge as needing to be incorporated (over and above simple email send) 
could include tracking, tagging, email merging and templates, and more... It depends what services the email sending providers provide that are popular.

Providers to support in rough order of priority (popularity in the .NET community) are suspected to be 
SendGrid, Mailgun, standard SMTP, Amazon SES, Office365, Azure Communication Services, Postmark, Resend, SparkPost, Brevo, Mailjet, SMTP2GO.

This will be an evolution of the work done previously in Odin.Email.

## B - Project Plan

1 - Isolate the most popular providers and document their API surface areas

1.1 - Tabulate and research popularity of all potential emailing providers in the .NET ecosystem.

1.2 - Research the top 10 providers API and email message object models.

2 - Create a new expanded provider abstraction API surface: IEmailMessage model, revised email sending models and maybe more.

2.1 - Create a new IEmailMessage model with expanded properties and methods

2.2 - Implement a new email sending interface, that will separate email send\dispath from a Provider sending adaptor. The overall surface area of this may need to expand to accommodate 'email merge' or other Provider functionality not known yet.  

2.3- Evaluate whether 'mail merge' needs a distinct API?

3 - IEmailSender configuration handling 

3.1 - Create an object model for IConfiguration injection of a dictionary of 
provider-specific options. Keep the settings for 'defaults' used in Odin.Email, but introduce allowing > 1 providers.

4 - IEmailDispatcher implementations

4.1 - Research and tabulate what dispatching features are or would be popular: Eg queueing, retries, concurrency, logging to database, etc.

4.2 - Choose dispatching features for this V1.

4.3 - TBA...

4.4 - Create extensive unit tests

5 - Email sending integration testing

5.1 - Research and tabulate options to actually test delivery of email through temporary concrete inboxes, eg  Mailinator, Mailosaur, Mailtrap

5.2 - Create email send and receive integration testing scaffolding.

6 - Provider implementations




# 1  New IEmailMessage model and revised email sending model

## 1.1 Table of Potential Providers
Below is a researched table of potential providers, ordered by directional popularity with developers and deployed applications, not by NuGet package downloads.

There is no single perfect source for "developer popularity." Media and comparison roundups are useful for surfacing mindshare and newer developer-first platforms, but they can be biased by sponsorship, SEO, affiliate incentives, and recency. A better ranking should combine several imperfect signals:

- Public web-technology detection, such as [BuiltWith Transactional Email usage](https://trends.builtwith.com/mx/transactional-email), which captures deployed footprint but can miss API-only usage.
- Company install-base datasets, such as [Enlyft Transactional Email products](https://enlyft.com/tech/transactional-email), which are useful for relative market presence but have opaque collection methods.
- Developer self-reporting and stack pages, such as [StackShare Email Services](https://stackshare.io/email-services), which better reflects developer familiarity but is sample-biased.
- Developer media, benchmarks, and community discussion, which help identify rising providers such as Resend and Mailtrap but should not be treated as hard market share.
- Platform gravity from AWS, Google Workspace, Microsoft 365, Azure, and OCI, especially where teams choose the email service because it is native to the cloud or productivity platform they already use.

The table therefore uses a broad, qualitative popularity evidence column. It is intentionally not a numeric market-share table.

| Rank | Provider | API documentation | Broad popularity evidence, excluding NuGet | Typical client types | SMTP support | Mail focus areas |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Standard SMTP relay / protocol adapters | [MailKit SmtpClient API](https://mimekit.net/docs/html/T_MailKit_Net_Smtp_SmtpClient.htm) | Provider-neutral baseline rather than a vendor. SMTP remains the common fallback across legacy apps, devices, business systems, and providers that expose both SMTP and HTTP APIs. | Any .NET app, legacy LOB systems, devices, internal relays, teams wanting provider portability | Yes, this is the baseline protocol | Lowest-common-denominator sending abstraction, portability, relay replacement, device/app compatibility |
| 2 | Twilio SendGrid | [Mail Send API](https://www.twilio.com/docs/sendgrid/api-reference/mail-send/mail-send) | Top-tier across broad sources: Enlyft lists SendGrid first among transactional email products, StackShare lists it as the top developer email API after Gmail, and BuiltWith places it near the top of detected transactional email usage. | SaaS products, startups, Azure/.NET apps, high-volume transactional and marketing senders | Yes, [SMTP API](https://www.twilio.com/docs/sendgrid/for-developers/sending-email/integrating-with-the-smtp-api) | Developer email API, SMTP relay, dynamic templates, analytics, deliverability, marketing campaigns |
| 3 | Amazon SES | [SES v2 SendEmail API](https://docs.aws.amazon.com/ses/latest/APIReference-V2/API_SendEmail.html) | Very strong deployed footprint: BuiltWith lists Amazon SES as the most detected transactional email technology, Enlyft places it among the top three, and StackShare shows high developer adoption. | AWS-hosted apps, cloud-native teams, cost-sensitive high-volume senders | Yes, [SES SMTP interface](https://docs.aws.amazon.com/ses/latest/dg/send-an-email-using-smtp.html) | Low-cost high-volume sending, transactional and marketing email, AWS IAM/infrastructure integration |
| 4 | Mailgun | [Messages API](https://documentation.mailgun.com/docs/mailgun/api-reference/send/mailgun/messages) | Consistently top-tier in web usage, company install-base data, StackShare adoption, and developer discussions about API-first transactional email. | Developers, SaaS teams, platforms needing API-first send, inbound routing, validation | Yes, [SMTP or API](https://help.mailgun.com/hc/en-us/articles/202464990-How-can-I-start-sending-email) | Developer email API, SMTP relay, routing, inbound parse, validation, logs and analytics |
| 5 | Mailchimp Transactional (Mandrill) | [Messages API](https://mailchimp.com/developer/transactional/api/messages/) | Strong legacy and deployed footprint: Mandrill is high in BuiltWith and Enlyft, and StackShare still shows meaningful developer use despite its Mailchimp-addon positioning. | Mailchimp users, ecommerce, legacy Mandrill users, event-driven app mail | Yes, [SMTP integration](https://mailchimp.com/developer/transactional/docs/smtp-integration/) | Transactional email add-on, templates, event messages, Mailchimp ecosystem fit |
| 6 | Google Workspace / Gmail | [Gmail users.messages.send API](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/send) | Huge developer familiarity and StackShare presence, but it is primarily mailbox/workspace email rather than a transactional email provider. Strong for apps sending as users or through Workspace relay. | Google Workspace tenants, mailbox-centric apps, admin-managed SMTP relay scenarios | Yes via [Workspace SMTP relay](https://support.google.com/a/answer/2956491?hl=en); API sending via [Gmail API](https://developers.google.com/gmail/api/guides/sending) | User/mailbox send, Workspace relay, OAuth-controlled integrations, small business and internal tooling |
| 7 | Microsoft 365 / Exchange Online | [Graph sendMail API](https://learn.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0) | Massive enterprise platform gravity and strong relevance for business apps, but usage is mailbox/tenant-oriented rather than specialist transactional-email market share. | Enterprise tenants, internal business apps, mailbox-based workflows, Microsoft-first organizations | Yes via [Exchange Online SMTP AUTH](https://learn.microsoft.com/en-us/Exchange/clients-and-mobile-in-exchange-online/authenticated-client-smtp-submission); API sending via [Graph sendMail](https://learn.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0) | Tenant/mailbox send, compliance, delegated or application permissions, Office integration |
| 8 | Mailjet | [Send API v3.1](https://dev.mailjet.com/email/guides/send-api-v31/) | Strong mid-market and European footprint: Enlyft and BuiltWith both place Mailjet high among detected transactional email providers. | SMB and mid-market teams, European/Sinch customers, combined marketing and transactional senders | Yes, [SMTP relay](https://documentation.mailjet.com/hc/en-us/articles/360043229473-How-can-I-configure-my-SMTP-parameters) | Transactional and marketing email, team email editor, templates, deliverability, SMTP/API |
| 9 | Brevo (formerly Sendinblue) | [sendTransacEmail API](https://developers.brevo.com/reference/sendtransacemail) | Strong SMB and marketing-automation presence: Enlyft lists SendinBlue/Brevo near Mailjet, and BuiltWith shows both Brevo and SendinBlue signals with meaningful detected usage. | SMBs, ecommerce, CRM/marketing automation users, transactional app senders | Yes, [SMTP relay](https://developers.brevo.com/docs/smtp-integration) | Marketing automation, transactional email, contacts/CRM, SMS and multichannel campaigns |
| 10 | Postmark | [Email API](https://postmarkapp.com/developer/api/email-api) | Smaller deployed footprint than SendGrid/SES/Mailgun, but disproportionately strong developer reputation for transactional deliverability and simple APIs. Frequently appears in developer recommendations and comparison media. | SaaS and product teams that prioritize transactional reliability and support | Yes, [SMTP sending](https://postmarkapp.com/developer/user-guide/send-email-with-smtp) | Transactional email, message streams, templates, inbound parsing, delivery visibility |
| 11 | SparkPost | [Transmissions API](https://developers.sparkpost.com/api/transmissions/) | Meaningful high-volume/enterprise presence. BuiltWith and Enlyft show a smaller but established footprint; developer mindshare is lower than SendGrid/Mailgun/Postmark. | High-volume senders, deliverability-focused teams, enterprise email programs | Yes, [SMTP API](https://developers.sparkpost.com/api/smtp/) | High-volume transactional and marketing email, analytics, webhooks, deliverability signals |
| 12 | Elastic Email | [REST API](https://elasticemail.com/developers/api-documentation/rest-api) | Visible in BuiltWith and Enlyft with a meaningful long-tail footprint, especially among cost-sensitive senders and mixed marketing/API use cases. | Cost-conscious SMBs, bulk marketers, apps needing combined marketing and transactional email | Yes, [SMTP settings](https://help.elasticemail.com/en/articles/4803409-smtp-settings) | Bulk and campaign email, REST API, contact/campaign management, transactional sending |
| 13 | Resend | [Send Email API](https://resend.com/docs/api-reference/emails/send-email) | Lower deployed-footprint signal than older providers, but strong modern developer mindshare in API-first email discussions and recent comparison media. Worth watching despite newer market position. | Modern SaaS/startup developers, product teams wanting simple API-first email | Yes, [SMTP sending](https://resend.com/docs/send-with-smtp) | Developer-first transactional email, API ergonomics, templates, domains, webhooks |
| 14 | MailerSend | [Sending an Email API](https://developers.mailersend.com/api/v1/email.html) | Detected in BuiltWith and Enlyft below the largest providers. Relevant as a modern transactional-focused service with API, SMTP, templates, and webhooks. | SMB/SaaS apps, product teams needing transactional templates and webhooks | Yes, [SMTP relay](https://developers.mailersend.com/smtp-relay) | Transactional email API/SMTP, templates, inbound routing, webhooks, activity logs |
| 15 | Mailtrap Email API/SMTP | [Transactional API](https://docs.mailtrap.io/developers/email-sending) | Strong developer awareness for email sandbox/testing; production sending is newer and has a smaller deployed-footprint signal than the long-standing providers. | Developers and QA teams, apps needing sandbox testing plus production sending | Yes, [Email API/SMTP](https://docs.mailtrap.io/getting-started) | Email sandbox/testing, production transactional sending, logs, templates, deliverability diagnostics |
| 16 | SMTP2GO | [Email send API](https://developers.smtp2go.com/reference/send-standard-email) | Visible in BuiltWith as an SMTP-first relay provider. Less developer-media mindshare than API-first providers but common in operational and migration scenarios. | SMBs, MSPs, operational apps, devices, WordPress/CMS, systems needing reliable relay | Yes, [SMTP relay](https://developers.smtp2go.com/docs/smtp-relay) | SMTP-first relay, reporting, deliverability, regional relay infrastructure, simple migration |
| 17 | Zoho ZeptoMail | [API index](https://www.zoho.com/zeptomail/help/api-index.html) | Visible in BuiltWith and backed by Zoho's broader SMB/productivity ecosystem. More common as a Zoho-adjacent transactional choice than a general developer default. | Zoho customers, SMB apps, transactional-only application senders | Yes, [SMTP configuration](https://help.zoho.com/portal/en/kb/zeptomail/faqs/sending-emails/articles/how-to-configure-smtp) | Transactional email only, OTPs, password resets, invoices, templates, separation from bulk marketing |
| 18 | Azure Communication Services Email | [Email Send REST API](https://learn.microsoft.com/en-us/rest/api/communication/email/email/send?tabs=HTTP&view=rest-communication-email-2023-03-31) | Important for Azure-native and Microsoft-aligned applications, but public broad-market popularity signals are weaker than SendGrid, SES, Mailgun, and Postmark. | Azure-first apps, enterprise LOB apps, teams replacing old authenticated SMTP | Yes, [ACS SMTP support](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-smtp-overview) | Azure-hosted outbound email, modern SMTP auth through Entra, SDK/API sending, centralized Azure control |
| 19 | Infobip Email | [Email over HTTP API](https://www.infobip.com/docs/email/email-over-api/send-email-over-http-api) | Strong CPaaS/enterprise provider, but general developer email-provider mindshare is lower than its SMS/omnichannel footprint. Relevant for teams already using Infobip. | Enterprise and global brands using omnichannel CPaaS | Yes, [SMTP API](https://www.infobip.com/docs/email/smtp-specification) | Omnichannel messaging, transactional and marketing email, global delivery, SMS/WhatsApp adjacency |
| 20 | Oracle Cloud Infrastructure Email Delivery | [Email Delivery HTTP API guide](https://docs.oracle.com/en/learn/send-email-with-ociemaildelivery-http/index.html) | Platform-native OCI option. Important inside Oracle Cloud environments, but smaller broad developer mindshare and public detected footprint than AWS SES or the specialist providers. | OCI-hosted workloads, enterprise cloud apps, Oracle customers | Yes, [SMTP or HTTPS submission](https://docs.oracle.com/en-us/iaas/Content/Email/Reference/gettingstarted_topic-Begin_sending_email.htm) | Managed outbound relay for transactional and high-volume email in OCI |

## 1.2 Provider API and Message Object Model Comparison

This table compares the top 10 provider/protocol entries from the 1.1 popularity research and maps them to a suggested `IEmailMessage` property model. The top 10 are interpreted as:

1. Standard SMTP / MimeKit
2. Twilio SendGrid
3. Amazon SES
4. Mailgun
5. Mailchimp Transactional
6. Google Workspace / Gmail
7. Microsoft 365 / Exchange Online
8. Mailjet
9. Brevo
10. Postmark

The table focuses on message construction and send-time API fields, not account configuration or post-send event/webhook models. Blank or `n/a` cells mean the provider does not expose a close first-class equivalent in the send API. Some providers can still support the concept through raw MIME, custom headers, or provider-specific options.

Primary references:

- [MimeKit `MimeMessage` properties](https://mimekit.net/docs/html/Properties_T_MimeKit_MimeMessage.htm)
- [Twilio SendGrid Mail Send API](https://www.twilio.com/docs/sendgrid/api-reference/mail-send) and [personalizations](https://www.twilio.com/docs/sendgrid/for-developers/sending-email/personalizations)
- [Amazon SES v2 `SendEmail`](https://docs.aws.amazon.com/ses/latest/APIReference-V2/API_SendEmail.html)
- [Mailgun Messages API](https://documentation.mailgun.com/docs/mailgun/api-reference/send/mailgun/messages)
- [Mailchimp Transactional Messages API](https://mailchimp.com/developer/transactional/api/messages/)
- [Gmail `users.messages` resource](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages)
- [Microsoft Graph `message` resource](https://learn.microsoft.com/en-us/graph/api/resources/message?view=graph-rest-1.0) and [`sendMail` action](https://learn.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0)
- [Mailjet Send API v3.1](https://dev.mailjet.com/email/guides/send-api-v31/)
- [Brevo `sendTransacEmail`](https://developers.brevo.com/reference/send-transac-email)
- [Postmark Email API](https://postmarkapp.com/developer/api/email-api)

| Suggested `IEmailMessage` property | Suggested Odin type | SMTP / MimeKit | SendGrid | Amazon SES v2 | Mailgun | Mailchimp Transactional | Gmail API | Microsoft Graph | Mailjet | Brevo | Postmark |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `From` | `EmailAddress?` | `MimeMessage.From: InternetAddressList` | `from: EmailAddress` | `FromEmailAddress: string` | `from: string` | `message.from_email: string`, `message.from_name: string` | `raw` MIME `From` header | `message.from: recipient` | `Messages[].From: object` | `sender: object` | `From: string` |
| `Sender` | `EmailAddress?` | `MimeMessage.Sender: MailboxAddress` | n/a | n/a | `h:Sender: string` header | n/a | `raw` MIME `Sender` header | `message.sender: recipient` | `Messages[].Sender: object` | n/a | custom `Headers[]` |
| `ReplyTo` | `IReadOnlyList<EmailAddress>` | `MimeMessage.ReplyTo: InternetAddressList` | `reply_to: EmailAddress`, `reply_to_list: EmailAddress[]` | `ReplyToAddresses: string[]` | `h:Reply-To: string` header | `message.headers["Reply-To"]: string` | `raw` MIME `Reply-To` header | `message.replyTo: recipient[]` | `Messages[].ReplyTo: object` | `replyTo: object` | `ReplyTo: string` |
| `To` | `IReadOnlyList<EmailAddress>` | `MimeMessage.To: InternetAddressList` | `personalizations[].to: EmailAddress[]` | `Destination.ToAddresses: string[]` | `to: string[]` | `message.to[].type = "to"` | `raw` MIME `To` header | `message.toRecipients: recipient[]` | `Messages[].To: object[]` | `to: object[]` | `To: string` |
| `Cc` | `IReadOnlyList<EmailAddress>` | `MimeMessage.Cc: InternetAddressList` | `personalizations[].cc: EmailAddress[]` | `Destination.CcAddresses: string[]` | `cc: string[]` | `message.to[].type = "cc"` | `raw` MIME `Cc` header | `message.ccRecipients: recipient[]` | `Messages[].Cc: object[]` | `cc: object[]` | `Cc: string` |
| `Bcc` | `IReadOnlyList<EmailAddress>` | `MimeMessage.Bcc: InternetAddressList` | `personalizations[].bcc: EmailAddress[]` | `Destination.BccAddresses: string[]` | `bcc: string[]` | `message.to[].type = "bcc"`, or `message.bcc_address: string` | `raw` MIME `Bcc` header | `message.bccRecipients: recipient[]` | `Messages[].Bcc: object[]` | `bcc: object[]` | `Bcc: string` |
| `Subject` | `string?` | `MimeMessage.Subject: string` | `subject: string`, or `personalizations[].subject` | `Content.Simple.Subject.Data: string` | `subject: string` | `message.subject: string` | `raw` MIME `Subject` header | `message.subject: string` | `Messages[].Subject: string` | `subject: string` | `Subject: string` |
| `TextBody` | `string?` | `BodyBuilder.TextBody: string` | `content[].type = "text/plain"`, `content[].value: string` | `Content.Simple.Body.Text.Data: string` | `text: string` | `message.text: string` | `raw` MIME `text/plain` part | `message.body.content` when `contentType = "text"` | `Messages[].TextPart: string` | `textContent: string` | `TextBody: string` |
| `HtmlBody` | `string?` | `BodyBuilder.HtmlBody: string` | `content[].type = "text/html"`, `content[].value: string` | `Content.Simple.Body.Html.Data: string` | `html: string` | `message.html: string` | `raw` MIME `text/html` part | `message.body.content` when `contentType = "html"` | `Messages[].HTMLPart: string` | `htmlContent: string` | `HtmlBody: string` |
| `AmpHtmlBody` | `string?` | MIME `text/x-amp-html` part | n/a | raw MIME only | `amp-html: string` | n/a | `raw` MIME `text/x-amp-html` part | raw MIME only | n/a | n/a | n/a |
| `RawMime` | `ReadOnlyMemory<byte>?` | `MimeMessage` serialized with `WriteTo` | n/a | `Content.Raw.Data: blob` | `/messages.mime` MIME upload | `/messages/send-raw raw_message: string` | `raw: base64url string` | MIME send supported separately from `message` JSON | n/a | n/a | n/a |
| `Attachments` | `IReadOnlyList<EmailAttachment>` | `BodyBuilder.Attachments: AttachmentCollection` | `attachments[]: object` | `Content.Simple.Attachments[]` or `Content.Template.Attachments[]` | `attachment: binary[]` | `message.attachments[]: object[]` | `raw` MIME parts, or `payload.parts[]` when reading | `message.attachments: attachment[]` | `Messages[].Attachments: object[]` | `attachment: object[]` | `Attachments: object[]` |
| `InlineAttachments` | `IReadOnlyList<EmailAttachment>` | `BodyBuilder.LinkedResources: AttachmentCollection` | `attachments[].disposition = "inline"`, `content_id: string` | attachment `ContentDisposition = "INLINE"`, `ContentId: string` | `inline: binary[]` | `message.images[]: object[]` | `raw` MIME parts with `Content-ID` | `fileAttachment.isInline: bool`, `contentId: string` | `Messages[].InlinedAttachments: object[]` | inline via attachment plus content IDs in HTML, no strong first-class split | `Attachments[].ContentID: string` |
| `Headers` | `IReadOnlyDictionary<string, string>` | `MimeMessage.Headers: HeaderList` | `headers: object`, or `personalizations[].headers` | `Content.Simple.Headers[]` or `Content.Template.Headers[]` | `h:*` form fields | `message.headers: object` | `raw` MIME headers, read via `payload.headers[]` | `message.internetMessageHeaders: internetMessageHeader[]` on create | `Messages[].Headers: object` | `headers: object` | `Headers: object[]` |
| `Tags` | `IReadOnlyList<string>` | custom headers, for example `X-Tag` | `categories: string[]` | `EmailTags: MessageTag[]` | `o:tag: string[]` | `message.tags: string[]` | custom headers or labels after send, not send API | `categories: string[]` | `Messages[].CustomCampaign: string` | `tags: string[]` | `Tag: string` |
| `Metadata` | `IReadOnlyDictionary<string, string>` | custom headers | `custom_args: object`, or `personalizations[].custom_args` | `EmailTags: MessageTag[]` for event tags | `v:*` custom variables | `message.metadata: object`, `recipient_metadata[]` | custom headers in raw MIME | `extensions`, `singleValueExtendedProperties`, or custom headers | `Messages[].Variables: object` | `params: object` | `Metadata: object` |
| `TemplateId` | `string?` | n/a | `template_id: string` | `Content.Template.TemplateName: string` or `TemplateArn: string` | `template: string` | `/messages/send-template template_name: string` | n/a | n/a | `Messages[].TemplateID: integer` | `templateId: integer` | template endpoint uses `TemplateId` or `TemplateAlias` |
| `TemplateModel` | `object?` | n/a | `personalizations[].dynamic_template_data: object` | `Content.Template.TemplateData: string` JSON | `t:variables: string` JSON, or `recipient-variables: string` JSON | `global_merge_vars[]`, `merge_vars[]` | n/a | n/a | `Messages[].Variables: object`, `TemplateLanguage: bool` | `params: object` | template endpoint `TemplateModel: object` |
| `Personalizations` | `IReadOnlyList<EmailPersonalization>` | n/a | `personalizations[]: object[]` | bulk send uses `BulkEmailEntry[]` with replacement content | `recipient-variables: string` JSON | `merge_vars[]`, `recipient_metadata[]` | n/a | n/a | multiple `Messages[]` entries or `Variables` per message | `messageVersions[]: object[]` | batch send uses one object per recipient/message |
| `SendAt` | `DateTimeOffset?` | no provider scheduling; MIME `Date` is only the message date | `send_at: integer` Unix time, or `personalizations[].send_at` | n/a | `o:deliverytime: string` | top-level `send_at: string` | n/a | n/a | n/a | `scheduledAt: string` | n/a |
| `Tracking` | `EmailTrackingOptions?` | n/a | `tracking_settings: object` | via configuration set events, not message body tracking flags | `o:tracking`, `o:tracking-clicks`, `o:tracking-opens` | `track_opens: bool`, `track_clicks: bool` | n/a | `isReadReceiptRequested`, `isDeliveryReceiptRequested` | account/campaign settings; limited message-level equivalents | provider/account settings, no simple message-level equivalent in basic send body | `TrackOpens: bool`, `TrackLinks: string` |
| `MessageStream` | `string?` | n/a | `asm.group_id` for unsubscribe grouping, `ip_pool_name` for IP pool | `ConfigurationSetName: string`, `ListManagementOptions: object` | `o:campaign: string`, route/domain determines stream | `subaccount: string`, `signing_domain`, `return_path_domain` | Gmail label/thread concepts are not send streams | mailbox/user route rather than message stream | `CustomCampaign: string` | `batchId: string`, tags for grouping | `MessageStream: string` |
| `Priority` | `EmailPriority?` | `MimeMessage.Priority`, `Importance`, `XPriority` | custom headers only | custom headers only | `h:Importance`, `h:X-Priority` headers | `message.important: bool`, or custom headers | raw MIME priority headers | `message.importance: importance` | custom headers only | `headers` only | custom `Headers[]` |
| `ProviderOptions` | `IReadOnlyDictionary<string, object>` | SMTP client delivery options outside message | `mail_settings`, `asm`, `batch_id`, `ip_pool_name` | `TenantName`, identity ARNs, feedback forwarding, endpoint ID | `o:*`, `t:*`, domain route options | `async`, `ip_pool`, domains, analytics options | `threadId`, `labelIds` for draft/import workflows | `saveToSentItems`, mailbox user route, extensions | `DeduplicateCampaign`, template error settings | `batchId`, `messageVersions`, provider constraints | server token/stream/template endpoint details |

### Suggested Shape

The common model should make the basic RFC email envelope and body first-class, because every provider supports that path either directly or through raw MIME. Provider-specific delivery controls should stay out of the core `IEmailMessage` until they are common across several providers.

Recommended V2 core surface:

```csharp
public interface IEmailMessage
{
    EmailAddress? From { get; }
    EmailAddress? Sender { get; }
    IReadOnlyList<EmailAddress> ReplyTo { get; }
    IReadOnlyList<EmailAddress> To { get; }
    IReadOnlyList<EmailAddress> Cc { get; }
    IReadOnlyList<EmailAddress> Bcc { get; }
    string? Subject { get; }
    string? TextBody { get; }
    string? HtmlBody { get; }
    string? AmpHtmlBody { get; }
    IReadOnlyList<EmailAttachment> Attachments { get; }
    IReadOnlyList<EmailAttachment> InlineAttachments { get; }
    IReadOnlyDictionary<string, string> Headers { get; }
    IReadOnlyList<string> Tags { get; }
    IReadOnlyDictionary<string, string> Metadata { get; }
    string? TemplateId { get; }
    object? TemplateModel { get; }
    IReadOnlyList<EmailPersonalization> Personalizations { get; }
    DateTimeOffset? SendAt { get; }
    EmailTrackingOptions? Tracking { get; }
    string? MessageStream { get; }
    EmailPriority? Priority { get; }
    ReadOnlyMemory<byte>? RawMime { get; }
    IReadOnlyDictionary<string, object> ProviderOptions { get; }
}
```

Likely supporting value objects:

```csharp
public sealed record EmailAddress(string Address, string? DisplayName = null);

public sealed record EmailAttachment(
    string FileName,
    string ContentType,
    ReadOnlyMemory<byte> Content,
    string? ContentId = null,
    bool IsInline = false);

public sealed record EmailTrackingOptions(
    bool? TrackOpens = null,
    bool? TrackClicks = null,
    bool? RequestReadReceipt = null,
    bool? RequestDeliveryReceipt = null);

public sealed record EmailPersonalization(
    IReadOnlyList<EmailAddress> To,
    IReadOnlyList<EmailAddress>? Cc = null,
    IReadOnlyList<EmailAddress>? Bcc = null,
    string? Subject = null,
    object? TemplateModel = null,
    IReadOnlyDictionary<string, string>? Metadata = null,
    DateTimeOffset? SendAt = null);
```
