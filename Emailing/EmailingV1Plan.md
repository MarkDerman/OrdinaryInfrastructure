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
1.1 - Tabulate and research popularity of all potential providers in the .NET ecosystem.
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


Below is a table of providers, their popularity in the .NET ecosystem, the types of clients they typically have, whether they support SMTP