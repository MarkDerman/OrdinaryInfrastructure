using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Odin.DesignContracts.Analyzers
{
    /// <summary>
    /// Analyzes calls to <c>Odin.DesignContracts.Contract.Requires</c> where the condition
    /// is a constant boolean literal (true or false).
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RequiresPreconditionLiteralAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The diagnostic identifier for redundant or doomed preconditions.
        /// </summary>
        public const string DiagnosticId = "ODIN001";

        private static readonly LocalizableString Title =
            "Contract.Requires uses a constant boolean condition";

        private static readonly LocalizableString MessageFormat =
            "Contract.Requires uses a constant condition '{0}'. This is either redundant or will always fail.";

        private static readonly LocalizableString Description =
            "Contract preconditions should depend on runtime state. Using a constant boolean literal is usually a mistake.";

        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            // Basic sanity defaults.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(
                AnalyzeInvocation,
                SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not InvocationExpressionSyntax invocation)
                return;

            // Must be Contract.Requires(...)
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "Requires")
                return;

            // Resolve the symbol to ensure it's Odin.DesignContracts.Contract.Requires
            SymbolInfo symbolInfo  = context.SemanticModel.GetSymbolInfo(memberAccess);
            IMethodSymbol? methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            if (methodSymbol is null)
                return;

            if (methodSymbol.Name != "Requires")
                return;

            if (methodSymbol.ContainingType is not { Name: "Contract", ContainingNamespace: { } ns })
                return;

            var nsName = ns.ToDisplayString();
            if (nsName != "Odin.DesignContracts")
                return;

            // We now know this is Odin.DesignContracts.Contract.Requires(...)

            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count == 0)
                return;

            ArgumentSyntax conditionArg = arguments[0];

            if (conditionArg.Expression is LiteralExpressionSyntax literal &&
                (literal.IsKind(SyntaxKind.TrueLiteralExpression) ||
                 literal.IsKind(SyntaxKind.FalseLiteralExpression)))
            {
                var constantText = literal.Token.Text;

                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    literal.GetLocation(),
                    constantText);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
