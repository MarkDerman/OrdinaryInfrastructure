using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Odin.DesignContracts.Analyzers;

/// <summary>
/// Validates usage of postconditions expressed via <c>Odin.DesignContracts.Contract.Ensures</c>
/// and return-value placeholders expressed via <c>Odin.DesignContracts.Contract.Result&lt;T&gt;</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PostconditionsAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Postconditions must be declared in a contract block at the top of a method.
    /// </summary>
    public const string ContractBlockDiagnosticId = "ODIN101";

    /// <summary>
    /// <c>Contract.Result&lt;T&gt;</c> must only be used inside an <c>Ensures</c> condition.
    /// </summary>
    public const string ResultUsageDiagnosticId = "ODIN102";

    /// <summary>
    /// Postconditions are not supported for async/iterator methods (v1).
    /// </summary>
    public const string UnsupportedMethodDiagnosticId = "ODIN103";

    /// <summary>
    /// Postconditions require the build-time rewriter to be enabled.
    /// </summary>
    public const string RewriterNotEnabledDiagnosticId = "ODIN104";

    /// <summary>
    /// A contract block must be terminated by <c>Contract.EndContractBlock()</c> (v1).
    /// </summary>
    public const string MissingEndContractBlockDiagnosticId = "ODIN105";

    private static readonly DiagnosticDescriptor ContractBlockRule = new(
        ContractBlockDiagnosticId,
        title: "Postconditions must be declared in a contract block at the start of the method",
        messageFormat: "Postconditions must appear only at the start of the method (contract block) before any non-contract statements.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ResultUsageRule = new(
        ResultUsageDiagnosticId,
        title: "Contract.Result<T>() can only be used inside a postcondition",
        messageFormat: "Contract.Result<T>() must only be used inside the first argument of Contract.Ensures(...).",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedMethodRule = new(
        UnsupportedMethodDiagnosticId,
        title: "Postconditions are not supported for async/iterator methods (v1)",
        messageFormat: "Postconditions are not supported for this method kind (async/iterator).",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RewriterNotEnabledRule = new(
        RewriterNotEnabledDiagnosticId,
        title: "Postconditions require the build-time rewriter",
        messageFormat: "Postconditions are present but the Odin DesignContracts rewriter is not enabled for this project.",
        category: "Build",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingEndContractBlockRule = new(
        MissingEndContractBlockDiagnosticId,
        title: "Contract.EndContractBlock() is required (v1)",
        messageFormat: "Postconditions are present but the contract block is not terminated by Contract.EndContractBlock().",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(ContractBlockRule, ResultUsageRule, UnsupportedMethodRule, RewriterNotEnabledRule,
            MissingEndContractBlockRule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
            return;

        // Expression-bodied methods can't have a contract block.
        if (method.ExpressionBody is not null)
        {
            // Still validate Result<T>() usage if present.
            AnalyzeResultUsage(context, method);
            return;
        }

        if (method.Body is null)
            return;

        bool hasPostconditions = ContainsEnsuresInvocation(context, method);
        if (!hasPostconditions)
        {
            // Even without Ensures, ensure Result<T>() isn't misused.
            AnalyzeResultUsage(context, method);
            return;
        }

        // Enforce sync-only (v1).
        if (method.Modifiers.Any(SyntaxKind.AsyncKeyword) || ContainsYield(method.Body))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                UnsupportedMethodRule,
                method.Identifier.GetLocation()));
        }

        // Enforce contract block positioning at the top of the method.
        bool seenNonContractStatement = false;
        bool sawEndContractBlock = false;
        foreach (StatementSyntax statement in method.Body.Statements)
        {
            if (IsContractBlockStatement(context, statement))
            {
                if (seenNonContractStatement)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        ContractBlockRule,
                        statement.GetLocation()));
                }

                if (statement is ExpressionStatementSyntax es && es.Expression is InvocationExpressionSyntax inv &&
                    IsEndContractBlockInvocation(context, inv))
                {
                    sawEndContractBlock = true;
                }
            }
            else
            {
                seenNonContractStatement = true;
            }
        }

        if (!sawEndContractBlock)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MissingEndContractBlockRule,
                method.Identifier.GetLocation()));
        }

        // Ensure Result<T>() is used only inside Ensures condition.
        AnalyzeResultUsage(context, method);

        // Require rewriter enabled when postconditions are present.
        if (!IsRewriterEnabled(context))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RewriterNotEnabledRule,
                method.Identifier.GetLocation()));
        }
    }

    private static bool IsRewriterEnabled(SyntaxNodeAnalysisContext context)
    {
        if (context.Options.AnalyzerConfigOptionsProvider.GlobalOptions
            .TryGetValue("build_property.OdinDesignContractsRewriterEnabled", out string? enabledText))
        {
            return string.Equals(enabledText?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool ContainsYield(BlockSyntax body)
        => body.DescendantNodes().OfType<YieldStatementSyntax>().Any();

    private static bool ContainsEnsuresInvocation(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method)
        => method.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(i => IsEnsuresInvocation(context, i));

    private static bool IsContractBlockStatement(SyntaxNodeAnalysisContext context, StatementSyntax statement)
    {
        if (statement is not ExpressionStatementSyntax exprStmt)
            return false;

        if (exprStmt.Expression is not InvocationExpressionSyntax invocation)
            return false;

        return IsEnsuresInvocation(context, invocation) || IsEndContractBlockInvocation(context, invocation);
    }

    private static bool IsEnsuresInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        => IsContractInvocation(context, invocation, methodName: "Ensures");

    private static bool IsEndContractBlockInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        => IsContractInvocation(context, invocation, methodName: "EndContractBlock");

    private static bool IsResultInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        => IsContractInvocation(context, invocation, methodName: "Result");

    private static bool IsContractInvocation(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        string methodName)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        if (memberAccess.Name.Identifier.Text != methodName)
            return false;

        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return false;

        if (methodSymbol.Name != methodName)
            return false;

        if (methodSymbol.ContainingType is not { Name: "Contract", ContainingNamespace: { } ns })
            return false;

        return ns.ToDisplayString() == "Odin.DesignContracts";
    }

    private static void AnalyzeResultUsage(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method)
    {
        foreach (InvocationExpressionSyntax invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!IsResultInvocation(context, invocation))
                continue;

            // Must be inside: Contract.Ensures( /* condition */ ... ) - specifically inside argument #0.
            if (!IsWithinEnsuresConditionArgument(context, invocation))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ResultUsageRule,
                    invocation.GetLocation()));
            }
        }
    }

    private static bool IsWithinEnsuresConditionArgument(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax resultInvocation)
    {
        // Walk up: Result() -> ... -> Argument -> ArgumentList -> Invocation (Ensures)
        SyntaxNode? node = resultInvocation;
        while (node is not null)
        {
            if (node is ArgumentSyntax arg && arg.Parent is ArgumentListSyntax argList &&
                argList.Parent is InvocationExpressionSyntax parentInvocation &&
                IsEnsuresInvocation(context, parentInvocation))
            {
                // Ensure it's the first argument.
                int index = argList.Arguments.IndexOf(arg);
                return index == 0;
            }

            node = node.Parent;
        }

        return false;
    }
}
