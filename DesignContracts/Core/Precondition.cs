namespace Odin.DesignContracts;

/// <summary>
/// Provides methods for runtime validation and enforcement of preconditions.
/// ensuring that the calling consumer has met the agreed\advertised requirements.
/// </summary>
public static class Precondition
{
    /// <summary>
    /// Specifies a precondition that must hold true when the enclosing method is called.
    /// </summary>
    /// <param name="precondition">The precondition that is required to be <c>true</c>.</param>
    /// <param name="userMessage">Optional English description of what the precondition is.</param>
    /// <param name="conditionText">Optional pseudo-code representation of the condition expression.</param>
    /// <exception cref="ContractException">
    /// Thrown when <paramref name="precondition"/> is <c>false</c>.
    /// </exception>
    public static void Requires(bool precondition, string? userMessage = null, string? conditionText = null)
    {
        if (!precondition) Contract.ReportFailure(ContractFailureKind.Precondition, userMessage, conditionText);
    }

    /// <summary>
    /// Requires that argument be not null. If it is, raises an ArgumentNullException.
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="userMessage">Defaults to 'Argument must not be null'</param>
    /// <param name="conditionText">Optional pseudo-code representation of the not null expression.</param>
    public static void RequiresNotNull(object? argument, string? userMessage = "Argument must not be null"
        , string? conditionText = null)
    {
        Requires(argument != null, userMessage, conditionText);
    }

    /// <summary>
    /// Specifies a precondition that must hold true when the enclosing method is called
    /// and throws a specific exception type when the precondition fails.
    /// </summary>
    /// <typeparam name="TException">
    /// The type of exception to throw when the precondition fails.
    /// The type must have a public constructor that accepts a single <see cref="string"/> parameter.
    /// </typeparam>
    /// <param name="precondition">The condition that must be <c>true</c>.</param>
    /// <param name="userMessage">Optional user readable message describing the precondition.</param>
    /// <param name="conditionText">Optional user readable message describing the precondition.</param>
    /// <exception cref="ContractException">
    /// Thrown when the specified exception type cannot be constructed.
    /// </exception>
    /// <exception cref="Exception">
    /// An instance of <typeparamref name="TException"/> when <paramref name="precondition"/> is <c>false</c>.
    /// </exception>
    public static void Requires<TException>(bool precondition, string? userMessage = null,
        string? conditionText = null)
        where TException : Exception
    {
        if (precondition) return;

        // Try to honor the requested exception type first.
        string message = Contract.BuildFailureMessage(ContractFailureKind.Precondition, userMessage, conditionText);

        Exception? exception = null;
        try
        {
            exception = (Exception?)Activator.CreateInstance(typeof(TException), message);
        }
        catch
        {
            // Swallow and fall back to ContractException.
        }

        if (exception is not null)
        {
            throw exception;
        }

        // Fall back to standard handling if we cannot construct TException.
        Contract.ReportFailure(ContractFailureKind.Precondition, userMessage, conditionText: null);
    }
}