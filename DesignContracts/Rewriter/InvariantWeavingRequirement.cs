namespace Odin.DesignContracts.Rewriter;

internal record InvariantWeavingRequirement
{
    public required bool OnEntry { get; init; }
    public required bool OnExit { get; init; }
}