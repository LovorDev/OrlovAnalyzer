using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AsyncAnalyzerReady
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SimpleYoYoAnalyzer : DiagnosticAnalyzer
    {
        string id = "TEST1337";
        string title = "Test empty analzyer";
        string description = "yo-yo problem";
        string category = "EmptyTest";
        DiagnosticSeverity severity = DiagnosticSeverity.Error;

        DiagnosticDescriptor m_diagnostic;

        protected DiagnosticDescriptor diagnostic =>
            m_diagnostic = m_diagnostic ?? new DiagnosticDescriptor(id, title, description, category, severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(diagnostic);


        public override void Initialize(AnalysisContext context)
        {
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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var allClassMethods = ((ClassDeclarationSyntax)method.Parent).Members.OfType<MethodDeclarationSyntax>();

            var correctMethod = allClassMethods.FirstOrDefault(x =>
                x.Identifier.Text == (invocationMethod.Expression is IdentifierNameSyntax ins ? ins.Identifier.Text : ""));
            
            if(correctMethod != null && correctMethod.SpanStart < method.SpanStart)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(diagnostic, ctx.Node.GetLocation()));
            }

        }
    }
}

