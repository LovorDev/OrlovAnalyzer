using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace OrlovAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SimpleYoYoAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "YoYoProblem";
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);
        private const string Title = "Simple Yo-Yo problem analyzer";
        private const string Description = "Method declaration above method invocation";
        private const string Category = "Antipattern";

        private const DiagnosticSeverity Severity = DiagnosticSeverity.Info;

        private DiagnosticDescriptor Diagnostic =>
            _mDiagnostic = _mDiagnostic ?? new DiagnosticDescriptor(Id, Title, Description, Category, Severity, true);

        private DiagnosticDescriptor _mDiagnostic;


        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
        }

        private void Analyze(SyntaxNodeAnalysisContext ctx)
        {
            var invocationMethod = (InvocationExpressionSyntax)ctx.Node;

            MethodDeclarationSyntax method = null;
            switch (invocationMethod.Parent)
            {
                case null:
                    break;
                case ExpressionStatementSyntax expressionStatementSyntax:
                    method = (MethodDeclarationSyntax)((BlockSyntax)expressionStatementSyntax.Parent).Parent;
                    break;
                case BlockSyntax blockSyntax:
                    method = (MethodDeclarationSyntax)blockSyntax.Parent;
                    break;
                case MethodDeclarationSyntax methodDeclarationSyntax:
                    method = methodDeclarationSyntax;
                    break;
            }

            if (method is null)
            {
                return;
            }

            var allClassMethods = ((ClassDeclarationSyntax)method.Parent).Members.OfType<MethodDeclarationSyntax>();

            var correctMethod = allClassMethods.FirstOrDefault(x =>
                x.Identifier.Text ==
                (invocationMethod.Expression is IdentifierNameSyntax ins ? ins.Identifier.Text : ""));

            if (correctMethod != null && correctMethod.SpanStart < method.SpanStart)
            {
                ctx.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, ctx.Node.GetLocation()));
            }
        }
    }
}