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
1.2 - Tabulate and research the top 5-10 providers API and email message object models

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
6.1 - Mailgun
6.2 - Sendgrid
6.3 - SMTP
6.4 - Office365
6.5 - Amazon SES


# 1  New IEmailMessage model and revised email sending model

## 1.1 Table of Potential Providers
Below is a researched table of potential providers, ordered by directional popularity in the .NET ecosystem.
The primary ordering signal is representative NuGet package download count fetched on 2026-05-24, rounded to the nearest useful number.
These counts are package-download signals, not market-share or customer-count claims. Broad SDKs such as Microsoft Graph and Infobip can overstate email-specific usage, while SMTP-first providers can be understated because .NET teams often integrate them through MailKit, System.Net.Mail, or a generic HTTP client.

| Rank | Provider | API documentation | Representative .NET popularity signal | Typical client types | SMTP support | Mail focus areas |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Standard SMTP relay / protocol adapters | [MailKit SmtpClient API](https://mimekit.net/docs/html/T_MailKit_Net_Smtp_SmtpClient.htm) | [MailKit](https://www.nuget.org/packages/MailKit) ~231.5M downloads; [FluentEmail.Smtp](https://www.nuget.org/packages/FluentEmail.Smtp) ~8.7M downloads; System.Net.Mail is built in | Any .NET app, legacy LOB systems, devices, internal relays, teams wanting provider portability | Yes, this is the baseline protocol | Lowest-common-denominator sending abstraction, portability, relay replacement, device/app compatibility |
| 2 | Microsoft 365 / Exchange Online | [Graph sendMail API](https://learn.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0) | [Microsoft.Graph](https://www.nuget.org/packages/Microsoft.Graph/) ~218.5M downloads, though broad across Microsoft cloud | Enterprise tenants, internal business apps, mailbox-based workflows, Microsoft-first organizations | Yes via [Exchange Online SMTP AUTH](https://learn.microsoft.com/en-us/Exchange/clients-and-mobile-in-exchange-online/authenticated-client-smtp-submission); API sending via [Graph sendMail](https://learn.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0) | Tenant/mailbox send, compliance, delegated or application permissions, Office integration |
| 3 | Twilio SendGrid | [Mail Send API](https://www.twilio.com/docs/sendgrid/api-reference/mail-send/mail-send) | [SendGrid](https://www.nuget.org/packages/SendGrid) ~218.4M downloads; [SendGrid.Extensions.DependencyInjection](https://www.nuget.org/packages/SendGrid.Extensions.DependencyInjection) ~20.8M downloads | SaaS products, startups, Azure/.NET apps, high-volume transactional and marketing senders | Yes, [SMTP API](https://www.twilio.com/docs/sendgrid/for-developers/sending-email/integrating-with-the-smtp-api) | Developer email API, SMTP relay, dynamic templates, analytics, deliverability, marketing campaigns |
| 4 | Amazon SES | [SES v2 SendEmail API](https://docs.aws.amazon.com/ses/latest/APIReference-V2/API_SendEmail.html) | [AWSSDK.SimpleEmail](https://www.nuget.org/packages/AWSSDK.SimpleEmail) ~67.9M downloads plus [AWSSDK.SimpleEmailV2](https://www.nuget.org/packages/AWSSDK.SimpleEmailV2) ~20.6M downloads | AWS-hosted apps, cloud-native teams, cost-sensitive high-volume senders | Yes, [SES SMTP interface](https://docs.aws.amazon.com/ses/latest/dg/send-an-email-using-smtp.html) | Low-cost high-volume sending, transactional and marketing email, AWS IAM/infrastructure integration |
| 5 | Google Workspace / Gmail | [Gmail users.messages.send API](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/send) | [Google.Apis.Gmail.v1](https://www.nuget.org/packages/Google.Apis.Gmail.v1) ~19.7M downloads | Google Workspace tenants, mailbox-centric apps, admin-managed SMTP relay scenarios | Yes via [Workspace SMTP relay](https://support.google.com/a/answer/2956491?hl=en); API sending via [Gmail API](https://developers.google.com/gmail/api/guides/sending) | User/mailbox send, Workspace relay, OAuth-controlled integrations, small business and internal tooling |
| 6 | Azure Communication Services Email | [Email Send REST API](https://learn.microsoft.com/en-us/rest/api/communication/email/email/send?tabs=HTTP&view=rest-communication-email-2023-03-31) | [Azure.Communication.Email](https://www.nuget.org/packages/Azure.Communication.Email) ~11.1M downloads | Azure-first apps, enterprise LOB apps, teams replacing old authenticated SMTP | Yes, [ACS SMTP support](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-smtp-overview) | Azure-hosted outbound email, modern SMTP auth through Entra, SDK/API sending, centralized Azure control |
| 7 | Postmark | [Email API](https://postmarkapp.com/developer/api/email-api) | [Postmark](https://www.nuget.org/packages/Postmark) ~8.6M downloads | SaaS and product teams that prioritize transactional reliability and support | Yes, [SMTP sending](https://postmarkapp.com/developer/user-guide/send-email-with-smtp) | Transactional email, message streams, templates, inbound parsing, delivery visibility |
| 8 | Mailchimp Transactional (Mandrill) | [Messages API](https://mailchimp.com/developer/transactional/api/messages/) | [Mandrill.net](https://www.nuget.org/packages/Mandrill.net) ~8.5M downloads and [Mandrill](https://www.nuget.org/packages/Mandrill) ~8.5M downloads | Mailchimp users, ecommerce, legacy Mandrill users, event-driven app mail | Yes, [SMTP integration](https://mailchimp.com/developer/transactional/docs/smtp-integration/) | Transactional email add-on, templates, event messages, Mailchimp ecosystem fit |
| 9 | Mailjet | [Send API v3.1](https://dev.mailjet.com/email/guides/send-api-v31/) | [Mailjet.Api](https://www.nuget.org/packages/Mailjet.Api) ~6.3M downloads | SMB and mid-market teams, European/Sinch customers, combined marketing and transactional senders | Yes, [SMTP relay](https://documentation.mailjet.com/hc/en-us/articles/360043229473-How-can-I-configure-my-SMTP-parameters) | Transactional and marketing email, team email editor, templates, deliverability, SMTP/API |
| 10 | Brevo (formerly Sendinblue) | [sendTransacEmail API](https://developers.brevo.com/reference/sendtransacemail) | [sib_api_v3_sdk](https://www.nuget.org/packages/sib_api_v3_sdk) ~3.6M downloads | SMBs, ecommerce, CRM/marketing automation users, transactional app senders | Yes, [SMTP relay](https://developers.brevo.com/docs/smtp-integration) | Marketing automation, transactional email, contacts/CRM, SMS and multichannel campaigns |
| 11 | Mailgun | [Messages API](https://documentation.mailgun.com/docs/mailgun/api-reference/send/mailgun/messages) | [FluentEmail.Mailgun](https://www.nuget.org/packages/FluentEmail.Mailgun) ~1.7M downloads; direct .NET packages are small/unofficial, for example [Mailgun.Api](https://www.nuget.org/packages/Mailgun.Api/) ~44k | Developers, SaaS teams, platforms needing API-first send, inbound routing, validation | Yes, [SMTP or API](https://help.mailgun.com/hc/en-us/articles/202464990-How-can-I-start-sending-email) | Developer email API, SMTP relay, routing, inbound parse, validation, logs and analytics |
| 12 | SparkPost | [Transmissions API](https://developers.sparkpost.com/api/transmissions/) | [SparkPost](https://www.nuget.org/packages/SparkPost) ~1.1M downloads | High-volume senders, deliverability-focused teams, enterprise email programs | Yes, [SMTP API](https://developers.sparkpost.com/api/smtp/) | High-volume transactional and marketing email, analytics, webhooks, deliverability signals |
| 13 | Infobip Email | [Email over HTTP API](https://www.infobip.com/docs/email/email-over-api/send-email-over-http-api) | [Infobip.Api.Client](https://www.nuget.org/packages/Infobip.Api.Client) ~1.1M downloads, though broad across Infobip APIs | Enterprise and global brands using omnichannel CPaaS | Yes, [SMTP API](https://www.infobip.com/docs/email/smtp-specification) | Omnichannel messaging, transactional and marketing email, global delivery, SMS/WhatsApp adjacency |
| 14 | Resend | [Send Email API](https://resend.com/docs/api-reference/emails/send-email) | [Resend](https://www.nuget.org/packages/Resend) ~608k downloads | Modern SaaS/startup developers, product teams wanting simple API-first email | Yes, [SMTP sending](https://resend.com/docs/send-with-smtp) | Developer-first transactional email, API ergonomics, templates, domains, webhooks |
| 15 | Elastic Email | [REST API](https://elasticemail.com/developers/api-documentation/rest-api) | [ElasticEmail](https://www.nuget.org/packages/ElasticEmail) ~146k downloads | Cost-conscious SMBs, bulk marketers, apps needing combined marketing and transactional email | Yes, [SMTP settings](https://help.elasticemail.com/en/articles/4803409-smtp-settings) | Bulk and campaign email, REST API, contact/campaign management, transactional sending |
| 16 | Mailtrap Email API/SMTP | [Transactional API](https://docs.mailtrap.io/developers/email-sending) | [FluentEmail.Mailtrap](https://www.nuget.org/packages/FluentEmail.Mailtrap) ~136k downloads; official .NET sending package signal appears small/new | Developers and QA teams, apps needing sandbox testing plus production sending | Yes, [Email API/SMTP](https://docs.mailtrap.io/getting-started) | Email sandbox/testing, production transactional sending, logs, templates, deliverability diagnostics |
| 17 | Oracle Cloud Infrastructure Email Delivery | [Email Delivery HTTP API guide](https://docs.oracle.com/en/learn/send-email-with-ociemaildelivery-http/index.html) | [OCI.DotNetSDK.Email](https://www.nuget.org/packages/OCI.DotNetSDK.Email) ~121k downloads | OCI-hosted workloads, enterprise cloud apps, Oracle customers | Yes, [SMTP or HTTPS submission](https://docs.oracle.com/en-us/iaas/Content/Email/Reference/gettingstarted_topic-Begin_sending_email.htm) | Managed outbound relay for transactional and high-volume email in OCI |
| 18 | MailerSend | [Sending an Email API](https://developers.mailersend.com/api/v1/email.html) | [MailerSendNetCore](https://www.nuget.org/packages/MailerSendNetCore) ~32k downloads and [MailerSend.AspNetCore](https://www.nuget.org/packages/MailerSend.AspNetCore) ~20k downloads | SMB/SaaS apps, product teams needing transactional templates and webhooks | Yes, [SMTP relay](https://developers.mailersend.com/smtp-relay) | Transactional email API/SMTP, templates, inbound routing, webhooks, activity logs |
| 19 | SMTP2GO | [Email send API](https://developers.smtp2go.com/reference/send-standard-email) | No notable provider-specific .NET package found; typically integrated through SMTP or direct API | SMBs, MSPs, operational apps, devices, WordPress/CMS, systems needing reliable relay | Yes, [SMTP relay](https://developers.smtp2go.com/docs/smtp-relay) | SMTP-first relay, reporting, deliverability, regional relay infrastructure, simple migration |
| 20 | Zoho ZeptoMail | [API index](https://www.zoho.com/zeptomail/help/api-index.html) | No notable provider-specific .NET package found; typically integrated through SMTP or direct API | Zoho customers, SMB apps, transactional-only application senders | Yes, [SMTP configuration](https://help.zoho.com/portal/en/kb/zeptomail/faqs/sending-emails/articles/how-to-configure-smtp) | Transactional email only, OTPs, password resets, invoices, templates, separation from bulk marketing |
