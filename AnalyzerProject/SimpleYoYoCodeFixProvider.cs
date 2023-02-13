using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OrlovAnalyzer.SimpleYOYOAnalyzer
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

            if (!(node is InvocationExpressionSyntax addInvocation))
            {
                throw new Exception("Expected node to be of type InvocationExpressionSyntax");
            }

            context.RegisterCodeFix(CodeAction.Create("Move method declaration", _ =>
            {
                var (method, correctMethod) = ClassSyntax.GetMethodInClass(addInvocation);

                var newRoot = root.InsertNodesAfter(method, new[] { correctMethod });
                newRoot = newRoot.RemoveNode(newRoot.FindNode(correctMethod.Span), SyntaxRemoveOptions.KeepEndOfLine);

                return Task.FromResult(doc.WithSyntaxRoot(newRoot));
            }), diagnostic);
        }
    }
}