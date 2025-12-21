<div align="center">

![Odin logo](Assets/icon320.png)

# OrDinary INfrastructure

</div>

<div align="center">

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

</div>

## The Odin components

... are a collection born after years of building many line-of-business applications on .NET...  
The result of componentising various recurring ordinary use-cases that we kept repeating in client systems at [Soulv Software](https://soulv.co.za/).

As at Dec 2025, the library is a hodge-podge of miscellaneous bits and bobs.

With .Net Core almost 10 years old now, I have never stopped missing using invariants on my domain entities with 
the old Code Contracts from .NET Framework days, so I am now putting my attention to creating some form of runtime 
support for preconditions, postconditions and class invariants for .NET 8 and up.

<br/><br/>

## Design Contracts :pencil2:

Coming soon... :construction:

<p>&nbsp;</p>

## Result Pattern: Result and ResultValue

[![NuGet](https://img.shields.io/nuget/v/Odin.System.Result.svg)](https://www.nuget.org/packages/Odin.System.Result)            ![Nuget](https://img.shields.io/nuget/dt/Odin.System.Result)

[Odin.System.Result](https://www.nuget.org/packages/Odin.System.Result) provides Result and ResultValue<TValue> concepts, that encapsulate the outcome of an operation (success or failure), together with a list of Messages.

Flexibility in the type of the Messages is included, with implementations for Result of TMessage and ResultValue of TValue, TMessage.
<p>&nbsp;</p>

## Email Sending :email:

[Odin.Email](https://www.nuget.org/packages/Odin.Email) provides an IEmailSender with email sending support currently for Mailgun and Office365.

1 - Add configuration

```json
{
  "EmailSending": {
    "Provider": "Mailgun",
    "DefaultFromAddress": "team@domain.com",
    "DefaultFromName": "MyTeam",
    "DefaultTags": [ "QA", "MyApp" ],
    "SubjectPrefix": "QA: ",
    "Mailgun": {
      "ApiKey": "XXX",
      "Domain": "mailgun.domain.com",
      "Region": "EU"
    }
  }
}
```

2 - Add package references to Odin.Email, and in this case Odin.Email.Mailgun

3 - Add IEmailSender to DI in your startup code...

```csharp
    builder.Services.AddOdinEmailSending();
```
4 - Use IEmailSender from DI 

```csharp
    MyService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
```
5 - Send email

```csharp
    IEmailMessage email = new EmailMessage(to, from, subject, htmlBody);
    ResultValue<string?> sendResult = await _emailSender.SendEmail(email);
```

| Package                                                                     | Description                                              |                                                                                      Latest Version                                                                                      |
|:----------------------------------------------------------------------------|:---------------------------------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
| [Odin.Email](https://www.nuget.org/packages/Odin.Email)                     | IEmailSender and IEmailMessage concepts                  |           [![NuGet](https://img.shields.io/nuget/v/Odin.Email.svg)](https://www.nuget.org/packages/Odin.Email)            ![Nuget](https://img.shields.io/nuget/dt/Odin.Email)           |
| [Odin.Email.Mailgun](https://www.nuget.org/packages/Odin.Email.Mailgun)     | Mailgun V3 API support                     |   [![NuGet](https://img.shields.io/nuget/v/Odin.Email.Mailgun.svg)](https://www.nuget.org/packages/Odin.Email.Mailgun)   ![Nuget](https://img.shields.io/nuget/dt/Odin.Email.Mailgun)    |
| [Odin.Email.Office365](https://www.nuget.org/packages/Odin.Email.Office365) | Microsoft Office365 support (via MS Graph) | [![NuGet](https://img.shields.io/nuget/v/Odin.Email.Office365.svg)](https://www.nuget.org/packages/Odin.Email.Office365)  ![Nuget](https://img.shields.io/nuget/dt/Odin.Email.Office365) |

<p>&nbsp;</p>

## Odin.Logging :clipboard:

[![NuGet](https://img.shields.io/nuget/v/Odin.Logging.svg)](https://www.nuget.org/packages/Odin.Logging)            ![Nuget](https://img.shields.io/nuget/dt/Odin.Logging)   

Provides an **ILoggerWrapper of T** that extends .NET's ILogger of T with all the LogXXX(...) calls as provided by the .NET LoggerExtensions extension methods (and a few more), for simpler logging assertion verifications. 

[Read more...](Logging/)

```csharp
    // Log as you always do in your app...
   _logger.LogWarning("Ford Prefect is missing!");

    // Assert logging calls more simply in your tests...    
    _loggerWrapperMock.Verify(x => x.LogWarning(It.Is<string>(c => 
        c.Contains("Ford Prefect"))), Times.Once);
    
    // as opposed to this with ILogger
    _iLoggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) =>
                state.ToString() == "Ford Prefect is missing!"),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
```

<p>&nbsp;</p>

## Razor Templating

Provides an IRazorTemplateRenderer for rendering .cshtml Razor files outside of the context of ASP.Net. 

```csharp
    // 1 - Add to DI in startup... 
    services.AddOdinRazorTemplating(typeof(AppBuilder).Assembly, "App.EmailViews.");
    
    // 2 - Render cshtml views by passing in a model
    ResultValue<string> result = await _razorTemplateRenderer
          .RenderAsync("AlertsEmail", alertingEmailModel);
    myEmail.Body = result.Value;
```

| Package                                                                                     |                                                                                                        Latest Version                                                                                                          |
|:--------------------------------------------------------------------------------------------|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
| [Odin.Templating.Razor.Abstractions](https://www.nuget.org/packages/Odin.Templating.Razor.Abstractions) | [![NuGet](https://img.shields.io/nuget/v/Odin.Templating.Razor.Abstractions.svg)](https://www.nuget.org/packages/Odin.Templating.Razor.Abstractions)            ![Nuget](https://img.shields.io/nuget/dt/Odin.Templating.Razor.Abstractions) |
| [Odin.Templating.Razor](https://www.nuget.org/packages/Odin.Templating.Razor)               |              [![NuGet](https://img.shields.io/nuget/v/Odin.Templating.Razor.svg)](https://www.nuget.org/packages/Odin.Templating.Razor)            ![Nuget](https://img.shields.io/nuget/dt/Odin.Templating.Razor)               |

<p>&nbsp;</p>

## StringEnum

[![NuGet](https://img.shields.io/nuget/v/Odin.System.StringEnum.svg)](https://www.nuget.org/packages/Odin.System.StringEnum)            ![Nuget](https://img.shields.io/nuget/dt/Odin.System.StringEnum)

[Odin.System.StringEnum](System/StringEnum/) provides enum-like behaviour for a set of string values via StringEnum, 
as well as a useful StringEnumMemberAttribute. [Read more...](System/StringEnum)

1 - Define your string 'enum' with public string constants

```csharp
    public class LoaderTypes : StringEnum<LoaderTypes>
    {
        public const string File = "FILE";
        public const string DynamicSql = "DYNAMIC-SQL";
    }
```
2 - Use like an enum

```csharp
    if (loaderOptions.LoaderType == LoaderTypes.DynamicSql)
```
3 - HasValue

```csharp
    bool memberExists = LoaderTypes.HasValue("CUSTOM"); // returns false
```

4 - Values property

```csharp
    string message = $"Valid members are: {string.Join(" | ", LoaderTypes.Values)}"
```

5 - Validation attribute

```csharp
    public record LoaderEditModel : IValidatableObject
    {
        [Required(AllowEmptyStrings = false)]
        [StringEnumMember<LoaderTypes>]
        public required string Loader { get; set; }
        ...
    }
```

## Other Libraries

In various states of incubation, deprecation or neglect...

| Area                                                                               | Description                                                                                                                    | Status          |                                                               Version                                                                |
|:-----------------------------------------------------------------------------------|:-------------------------------------------------------------------------------------------------------------------------------|-----------------|:------------------------------------------------------------------------------------------------------------------------------------:|
| [Remote files \ SFTP \ FTPS](https://www.nuget.org/packages?q=Odin.RemoteFiles)    | An abstraction of SFTP and FTPS file operations.                                                                               | Needs attention |           [![NuGet](https://img.shields.io/nuget/v/Odin.RemoteFiles.svg)](https://www.nuget.org/packages/Odin.RemoteFiles)           |
| [SQL scripts runner](https://www.nuget.org/packages?q=Odin.Data)                   | Useful for running database migration scripts at application deployment time.                                                  |                 | [![NuGet](https://img.shields.io/nuget/v/Odin.Data.SQLScriptsRunner.svg)](https://www.nuget.org/packages/Odin.Data.SQLScriptsRunner) |
| [Utility - Tax](https://www.nuget.org/packages?q=Odin.Utility.Tax)                 | Simple support for storing tax rate changes over time in application configuration, and then getting tax rates as at any date. | No docs         |           [![NuGet](https://img.shields.io/nuget/v/Odin.Utility.Tax.svg)](https://www.nuget.org/packages/Odin.Utility.Tax)           |
| [BackgroundProcessing](https://www.nuget.org/packages?q=Odin.BackgroundProcessing) | Wrapper around Hangfire                                                                                                        | Deprecated      |  [![NuGet](https://img.shields.io/nuget/v/Odin.BackgroundProcessing.svg)](https://www.nuget.org/packages/Odin.BackgroundProcessing)  |
| [Notifications](https://www.nuget.org/packages?q=Odin.Notifications)               | Messaging                                                                                                                      | Incubator       |         [![NuGet](https://img.shields.io/nuget/v/Odin.Notifications.svg)](https://www.nuget.org/packages/Odin.Notifications)         |
| [Cryptography](https://www.nuget.org/packages?q=Odin.Cryptography)                 | Wrapper around IDataProtector                                                                                                  | Deprecated      |          [![NuGet](https://img.shields.io/nuget/v/Odin.Cryptography.svg)](https://www.nuget.org/packages/Odin.Cryptography)           |




