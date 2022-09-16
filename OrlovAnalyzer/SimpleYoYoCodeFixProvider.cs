using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OrlovAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimpleYoYoCodeFixProvider))]
    [Shared]
    public class SimpleYoYoCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SimpleYoYoAnalyzer.Id);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var doc = context.Document;

            var root = await doc.GetSyntaxRootAsync();

            var node = root.FindNode(diagnostic.Location.SourceSpan);

            if (!(node is InvocationExpressionSyntax invocationExpressionSyntax))
            {
                throw new Exception("Expected node of Method Invocation");
            }

            context.RegisterCodeFix(CodeAction.Create("Move method declaration", async c =>
            {
                var newRoot = root.RemoveNode(invocationExpressionSyntax, SyntaxRemoveOptions.AddElasticMarker);
                return doc.WithSyntaxRoot(newRoot);
            }), diagnostic);
        }
    }
}