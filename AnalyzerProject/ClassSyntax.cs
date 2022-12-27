using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OrlovAnalyzer
{
    public static class ClassSyntax
    {
        public static (MethodDeclarationSyntax method, MethodDeclarationSyntax correctMethod) GetMethodInClass(
            InvocationExpressionSyntax invocationMethod)
        {
            MethodDeclarationSyntax method = null;
            switch (invocationMethod.Parent)
            {
                case null:
                    break;
                case ExpressionStatementSyntax expressionStatementSyntax:
                    if (expressionStatementSyntax.Parent is BlockSyntax bs)
                    {
                        method = (MethodDeclarationSyntax)bs.Parent;
                    }

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
                return (null, null);
            }

            var allClassMethods = ((ClassDeclarationSyntax)method.Parent).Members.OfType<MethodDeclarationSyntax>();

            var correctMethod = allClassMethods.FirstOrDefault(x =>
                x.Identifier.Text ==
                (invocationMethod.Expression is IdentifierNameSyntax ins ? ins.Identifier.Text : ""));
            return (method, correctMethod);
        }
    }
}