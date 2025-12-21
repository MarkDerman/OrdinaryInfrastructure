## About Odin.System.Result

[![NuGet](https://img.shields.io/nuget/v/Odin.System.Result.svg)](https://www.nuget.org/packages/Odin.System.Result)  ![Nuget](https://img.shields.io/nuget/dt/Odin.System.Result)

[Odin.System.Result](https://github.com/MarkDerman/OrdinaryInfrastructure/tree/master/System/Result), part of the [OrDinary INfrastructure](https://github.com/MarkDerman/OrdinaryInfrastructure) 
libraries, provides several **'Result'** classes, which all encapsulate the outcome of an operation , together with a list of messages.

**Result** is the simplest concept.

**ResultValue<TValue>** adds a generic **Value** property.

**Result<TMessage>** and **ResultValue<TValue, TMessage>** add support for the **Messages** list to be of any type.

**ResultEx** and **ResultValueEx<TValue>** come with a TMessage type that is aligned with logging failure issues.

## Getting Started

### 1 - Result: Success() and Failure()

```csharp
    public class HeartOfGoldService
    {
        public Result WarpSpeedToMilliways()
        {
            if (_eddie.IsOK()) return Result.Success();
            return Result.Failure(["Zaphod, that is not possible...", "Error 42"])
        }
    }
```

### 2 - Result: IsSuccess and Messages properties

```csharp
    Result outcome = _heartOfGold.WarpSpeedToMilliways()
    if (!outcome.IsSuccess)
    {
        outcome.Messages.ForEach(m => _logger.LogWarning(m));
    }    
    
```
### 3 - ResultValue:

Adds a generic Value property to Result.

```csharp
    public class ZaphodMemoryFetcherService
    {
        public ResultValue<IReadonlyList<MemSnippet>> 
            FetchMemoriesFor(Source source, DateOnly day)
        {
            List<MemSnippet> results; 
            ...
            // On success...
            return ResultValue<IReadonlyList<Transaction>>.Success(results);

            // On failure...
            return ResultValue<IReadonlyList<Transaction>>.Failure("Tampering detected!")
        }
    }
```
### 4 - ResultValue: IsSuccess, Value and Messages properties

```csharp
    ResultValue<IReadonlyList<MemSnippet>> outcome = 
        _zaphod.FetchMemoriesFor(_milliwaysDinner, DateOnly.MaxValue)
    if (outcome.IsSuccess)
    {
        var memories = outcome.Value;
    }
    else
    {
        outcome.Messages.ForEach(m => _logger.LogWarning(m));
    }    
    
```
### 5 - Result & ResultValue Messages can also be of type TMessage, as opposed to just string.

```csharp
    public record MyMessage(int EventId, string Message)
    Result<MyMessage> result = Result<MyMessage>.Failure(
        new MyMessage(134, "Some message"));
    ...
    result.Messages.ForEach(m => _logger.LogWarning(m.EventId, m.Message));
```

### 6 - ResultEx & ResultValueEx 

Uses a message type that is aligned with logging failure issues...

```csharp
    ResultEx result = ResultEx.Failure(LogLevel.Critical, 
        "Zaphod has broken the improbability drive", new UnhandledWarpException());
    ...
    MessageEx message =  result.Messages[0];
    _logger.Log(message.Severity, message.Error, message.Message);    

```
