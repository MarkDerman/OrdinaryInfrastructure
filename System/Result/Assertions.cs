namespace Odin.System;

internal static class Assertions
{
    internal static void RequiresArgumentPrecondition(bool requirement, string argumentRequirementMessage)
    {
        if (!requirement) throw new ArgumentException(argumentRequirementMessage);
    }

    internal static void RequiresArgumentNotNull(object? argument, string? argumentIsRequiredMessage = null)
    {
        if (argument != null) return;
        if (string.IsNullOrWhiteSpace(argumentIsRequiredMessage))
        {
            argumentIsRequiredMessage = $"{nameof(argument)} is required";
        }
        ArgumentNullException ex = new ArgumentNullException(argumentIsRequiredMessage);
        throw ex;
    }
}