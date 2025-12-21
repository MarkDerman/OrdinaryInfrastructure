## About Odin.Logging

[![NuGet](https://img.shields.io/nuget/v/Odin.Logging.svg)](https://www.nuget.org/packages/Odin.Logging)  ![Nuget](https://img.shields.io/nuget/dt/Odin.Logging)

Odin.Logging, part of the [OrDinary INfrastructure](https://github.com/MarkDerman/OrdinaryInfrastructure) libraries,  provides an **ILoggerWrapper of T** that extends .NET's ILogger of T with all the LogXXX(...) calls as provided by the .NET LoggerExtensions extension methods (and a few more), for simpler logging assertion verifications.

## Getting Started

### 1 - Add package

Add the Odin.Logging package from NuGet to your project using the command...

```shell
   dotnet add package Odin.Logging
```    
### 2 - Add ILoggerWrapper<T> to DI in your startup code

```csharp
    var builder = WebApplication.CreateBuilder(args);
    ...
    builder.Services.AddOdinLoggerWrapper();
```    

### 3 - Configure .NET Logging and ILogger 

As you normally would in startup code and configuration. Eg...

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MyApp": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting": "Information",
      "System": "Warning"
    }
  }
}
```    

### 4 - Log using ILoggerWrapper<T> instead of ILogger<T>

```csharp
    
    public class HitchHikerService(ILoggerWrapper<HitchHikerService> logger) : IHitchHikerService
    {
        public async Task VisitRestaurantAtEndOfUniverse()
        {
            ...
            _logger.LogError("Ford Prefect is missing!");
            ...
        }
    }
```

### 5 - Assert logging calls more simply in tests

```csharp
    _loggerWrapperMock.Verify(x => x.LogError(It.Is<string>(c => 
        c.Contains("Ford Prefect"))), Times.Once);
    
    // as opposed to this with ILogger
    _iLoggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) =>
                state.ToString() == "Ford Prefect is missing!"),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
```

## On Robustness and asserting logging behaviour...

Some musings on why I consider validation by unit testing of appropriate logging (of 1000s of out-of-scope to be handled application situations) 
to be an important contributor towards excellence with respect to **Robustness**. 

**Correctness**, the prime quality of excellent software, reflects the ability of software to perform its intended exact behaviour. 
While 'exact behaviour' can range anywhere from a grey area of implicitly agreed-in-conversation-with-stakeholders rough functionality, to well-defined 
clearly written (and usually emerging) specifications (if you are lucky and have excellent product ownership).

**Robustness**, although a rather fuzzy notion, is a reflection of how appropriately software reacts and behaves outside of it's 
intended specification \ use cases, what the industry terms an 'abnormal case'. It is important to note that 'normal' in this sense 
means 'within specification', not 'desirable' or 'ideal' which are subjective notions. When an abnormal case emerges, where 
there is no specification or agreement on what the software must do, does the software communicate the matter timeously, 
fail gracefully if necessary, and not cause any damage? This is **Robustness**.

Timely communication of abnormal cases is generally handled at least through logging and telemetry.

A best practice to achieve and continuously maintain a high level of robustness in large applications it to assert all logging 
scenarios in automated tests, which is the only reason for the creation of ILoggerWrapper, namely less onerous verification of logging calls.
