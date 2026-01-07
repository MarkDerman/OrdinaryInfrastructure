# Odin.DesignContracts (Design by Contract)

This repository includes a lightweight Design-by-Contract runtime in [`DesignContracts/Core/Contract.cs`](Core/Contract.cs:1) plus tooling to support **postconditions** (Ensures) by rewriting compiled IL.

## Why IL rewriting (and not source generators)

Roslyn **source generators** can only *add* new source; they cannot rewrite an existing method body to:

- capture a return value, and
- run postconditions on **every** exit path.

To support Code-Contracts-style authoring such as `Contract.Ensures(Contract.Result<T>() != null)` we use a build-time IL rewriter.

## Postconditions (v1)

Postconditions are declared with [`Contract.Ensures()`](Core/Contract-New.cs:41) and may refer to the enclosing method return value via [`Contract.Result<T>()`](Core/Contract-New.cs:13).

### Authoring rules (v1)

- Postconditions must appear in a **contract block** at the start of a method.
- The contract block must be terminated with [`Contract.EndContractBlock()`](Core/Contract-New.cs:31).
- Sync methods only (no `async`, no iterators) in v1.

Example:

```csharp
using Odin.DesignContracts;

public static string GetName(User user)
{
    Contract.Requires(user is not null);

    Contract.Ensures(Contract.Result<string>() is not null);
    Contract.EndContractBlock();

    return user.Name;
}
```

At build time the rewriter injects the `Ensures(...)` checks immediately before method exit, after capturing the return value.

### Runtime enable/disable

`Ensures` checks are gated by `ContractRuntime.PostconditionsEnabled` (see [`ContractRuntime`](Core/ContractRuntime.cs:1)). When disabled, postconditions are a no-op.

## Object invariants (v1)

Object invariants are authored by declaring a private, parameterless, `void` method marked with
[`ContractInvariantMethodAttribute`](Core/ContractAttributes.cs:1), and calling
[`Contract.Invariant()`](Core/Contract-New.cs:68) inside that method.

At build time the IL rewriter wires up invariant execution as follows:

- If a type declares exactly one invariant method (either `Odin.DesignContracts.ContractInvariantMethodAttribute`
  or `System.Diagnostics.Contracts.ContractInvariantMethodAttribute`), the rewriter injects calls to that invariant method
  at **entry and exit** of all **public instance** methods and **public instance** property accessors.
- Public instance constructors (`.ctor`) get invariant calls at **exit only**.
- Any public method/property accessor marked `[System.Diagnostics.Contracts.Pure]` is **excluded** from invariant weaving.

Invariant checks are gated by `ContractRuntime.InvariantsEnabled`.

## Tooling package

The meta-package [`Odin.DesignContracts.Tooling`](Tooling/Odin.DesignContracts.Tooling.csproj:1) is responsible for:

- shipping analyzers (to validate authoring), and
- running the IL rewriter after compilation via `buildTransitive` targets.

The IL rewriter implementation lives in [`DesignContracts/Rewriter/Program.cs`](Rewriter/Program.cs:1).
