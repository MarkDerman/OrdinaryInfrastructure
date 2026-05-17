namespace Odin.System;

internal static class Precondition
{
    internal static void Requires(bool requirement, string argumentRequirementMessage)
    {
        if (!requirement) throw new ArgumentException(argumentRequirementMessage);
    }
}