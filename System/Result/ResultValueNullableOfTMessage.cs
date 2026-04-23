using System.Text.Json.Serialization;

namespace Odin.System;

/// <summary>
/// Represents the success or failure of an operation that returns a Value\Result on success,
/// and list of messages, of type TMessage.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TMessage"></typeparam>
[Obsolete("Prefer ResultValue<TValue, TMessage> for non-null success values. Keep ResultValueNullable only for legacy flows that intentionally treat null as a successful value.")]
public class ResultValueNullable<TValue, TMessage> : Result<TMessage> where TMessage : class
{
    /// <summary>
    /// Value may or may not be set when successful.
    /// Value is null when Success is false.
    /// </summary>
    [JsonPropertyOrder(-1)]
    public TValue? Value { get; init; }

    /// <summary>
    /// Parameterless constructor for serialisation, etc.
    /// </summary>
    public ResultValueNullable()
    {
        Value = default(TValue);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="success">true or false</param>
    /// <param name="value">Required if successful</param>
    /// <param name="messages">Optional, but good practice is to provide messages for failed results.</param>
    protected ResultValueNullable(bool success, TValue? value, IEnumerable<TMessage>? messages) : base(success, messages)
    {
        Value = value;
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="success">true or false</param>
    /// <param name="value">Required if successful</param>
    /// <param name="message">Optional, but good practice is to provide messages for failed results.</param>
    protected ResultValueNullable(bool success, TValue? value, TMessage? message = null) : base(success, message)
    {
        Value = value;
    }

    /// <summary>
    /// Success.
    /// </summary>
    /// <param name="value">Required.</param>
    /// <param name="messages">Not normally used for successful operations, but can be for informational purposes.</param>
    /// <returns></returns>
    public static ResultValueNullable<TValue, TMessage> Success(TValue value, IEnumerable<TMessage>? messages)
    {
        return new ResultValueNullable<TValue, TMessage>(true, value, messages);
    }

    /// <summary>
    /// Creates a successful Result with Value set.
    /// </summary>
    /// <param name="value">Required.</param>
    /// <returns></returns>
    public static ResultValueNullable<TValue, TMessage> Success(TValue value)
    {
        return new ResultValueNullable<TValue, TMessage>(true, value, null as TMessage);
    }

    /// <summary>
    /// Creates a successful Result with Value set, and 1 Message item.
    /// </summary>
    /// <param name="value">Required.</param>
    /// <param name="message">Not normally used for successful operations, but can be for informational purposes.</param>
    /// <returns></returns>
    public static ResultValueNullable<TValue, TMessage> Success(TValue value, TMessage? message)
    {
        return new ResultValueNullable<TValue, TMessage>(true, value, message);
    }

    /// <summary>
    /// Creates a successful Result with Value set, and several Messages.
    /// </summary>
    /// <param name="messages">At least 1 not null message is required.</param>
    /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
    /// <returns></returns>
    public static ResultValueNullable<TValue, TMessage> Failure(IEnumerable<TMessage> messages, TValue? value = default(TValue))
    {
        Precondition.RequiresNotNull(messages);
        List<TMessage> messagesList = messages.ToList();
        Precondition.Requires(messagesList.Any(m => m != null!), "At least 1 message is required.");
        return new ResultValueNullable<TValue, TMessage>(false, value, messagesList);
    }

    /// <summary>
    /// Success.
    /// </summary>
    /// <param name="message">Required for failed operations.</param>
    /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
    /// <returns></returns>
    public static ResultValueNullable<TValue, TMessage> Failure(TMessage message, TValue? value = default(TValue))
    {
        Precondition.RequiresNotNull(message);
        return new ResultValueNullable<TValue, TMessage>(false, value, new List<TMessage>() { message });
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Result ToResult()
    {
        return IsSuccess
            ? Result.Success()
            : Result.Failure(Messages.Select(m =>
                m.ToString()
                ?? $"No string representation of message of type {typeof(TMessage).FullName}"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOtherValue"></typeparam>
    /// <returns></returns>
    public virtual ResultValueNullable<TOtherValue, TMessage> ToFailedResult<TOtherValue>() where TOtherValue : notnull
    {
        if (IsSuccess)
        {
            throw new ArgumentException($"Cannot convert a successful result of type {GetType().FullName} " +
                                        $"to a failed result of type {typeof(ResultValueNullable<TOtherValue, TMessage>).FullName}.");
        }

        return ResultValueNullable<TOtherValue, TMessage>.Failure(Messages);
    }

    /// <summary>
    /// Projects the value when successful.
    /// Failures are propagated unchanged.
    /// </summary>
    /// <typeparam name="TOtherValue"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    public ResultValueNullable<TOtherValue, TMessage> Map<TOtherValue>(Func<TValue?, TOtherValue?> map)
    {
        Precondition.RequiresNotNull(map);

        return IsSuccess
            ? ResultValueNullable<TOtherValue, TMessage>.Success(map(Value))
            : ResultValueNullable<TOtherValue, TMessage>.Failure(Messages);
    }

    /// <summary>
    /// Chains the next result-producing operation when successful.
    /// Failures are propagated unchanged.
    /// </summary>
    /// <typeparam name="TOtherValue"></typeparam>
    /// <param name="bind"></param>
    /// <returns></returns>
    public ResultValueNullable<TOtherValue, TMessage> Bind<TOtherValue>(Func<TValue?, ResultValueNullable<TOtherValue, TMessage>> bind)
        where TOtherValue : notnull
    {
        Precondition.RequiresNotNull(bind);

        return IsSuccess
            ? bind(Value)
            : ResultValueNullable<TOtherValue, TMessage>.Failure(Messages);
    }

    /// <summary>
    /// Executes the action when successful and returns the same instance.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <returns></returns>
    public ResultValueNullable<TValue, TMessage> Tap(Action<TValue?> onSuccess)
    {
        Precondition.RequiresNotNull(onSuccess);

        if (IsSuccess)
        {
            onSuccess(Value);
        }

        return this;
    }

    /// <summary>
    /// Matches the success or failure branch and returns the projected value.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="onFailure"></param>
    /// <returns></returns>
    public TResult Match<TResult>(Func<TValue?, TResult> onSuccess, Func<IReadOnlyList<TMessage>, TResult> onFailure)
    {
        Precondition.RequiresNotNull(onSuccess);
        Precondition.RequiresNotNull(onFailure);

        return IsSuccess
            ? onSuccess(Value)
            : onFailure(Messages);
    }
}
