namespace Odin.System;

internal static class Precondition
{
    internal static void Requires(bool requirement, string argumentRequirementMessage)
    {
        if (!requirement) throw new ArgumentException(argumentRequirementMessage);
    }

    internal static void RequiresNotNull(object? argument, string? argumentIsRequiredMessage = null)
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