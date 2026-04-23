## About Odin.System.Result

[![NuGet](https://img.shields.io/nuget/v/Odin.System.Result.svg)](https://www.nuget.org/packages/Odin.System.Result)  ![Nuget](https://img.shields.io/nuget/dt/Odin.System.Result)

[Odin.System.Result](https://github.com/MarkDerman/OrdinaryInfrastructure/tree/master/System/Result), part of the [OrDinary INfrastructure](https://github.com/MarkDerman/OrdinaryInfrastructure) 
libraries, provides lightweight **Result** types that encapsulate the outcome of an operation together with a list of messages.

## Preferred Concepts

### `Result`

Represents success or failure with string messages.

### `ResultValue<TValue>`

Represents success or failure with a required non-null value on success.

### `Result<TMessage>`

Advanced form for callers that want a structured message payload instead of `string`.

### `ResultValue<TValue, TMessage>`

Advanced form for callers that want both a required non-null success value and a structured message payload.

## Legacy Concepts

The following types remain for backward compatibility but are no longer the preferred direction:

- `ResultValueNullable<TValue>`
- `ResultValueNullable<TValue, TMessage>`
- `ResultEx`
- `ResultValueEx<TValue>`
- `MessageEx`

Prefer using `ResultMessage` with the generic result forms when structured logging or exception data needs to travel with the result.

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
            var results = new List<MemSnippet>();
            ...
            ...
            // On success...
            return ResultValue<IReadonlyList<MemSnippet>>.Success(results);

            // On failure...
            return ResultValue<IReadonlyList<MemSnippet>>.Failure("Tampering detected!")
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

Uses a message type that is aligned with logging...

```csharp
    ResultEx result = ResultEx.Failure(LogLevel.Critical, 
        "Zaphod has broken the improbability drive", unhandledWarpExceptionCaught);
    ...
    MessageEx message =  result.Messages[0];
    _logger.Log(message.Severity, message.Error, message.Message);    

```

## Composition Helpers

The preferred result types now expose a small composition surface inspired by modern .NET result libraries:

- `Map`
- `Bind`
- `Match`
- `Tap`
- `CombineAll`

Example:

```csharp
ResultValue<int> parseResult = ResultValue<string>.Success("42")
    .Map(int.Parse)
    .Tap(value => _logger.LogInformation("Parsed {Value}", value));

string outcome = parseResult.Match(
    onSuccess: value => $"Value = {value}",
    onFailure: messages => string.Join(" | ", messages));
```
