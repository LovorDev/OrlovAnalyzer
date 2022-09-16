using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using AsyncAnalyzerReady;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestWithAnotherClass()
        {
            const string code = @"
    class Program
    {
        AnotherClass anotherClass = new AnotherClass();
        static void  Main(string[] args)
        {
        }
        public void FirstMethod(){}
        public void SimpleMethod()
        {
            FirstMethod();
            DownMethod();
            anotherClass.SimpleMethod();
        }

        public void DownMethod(){}

    }

    class AnotherClass
    {
        public void SimpleMethod(){}
    }
";
        }
        
        [TestMethod]
        public async Task TestWithNestedClass(){}
        [TestMethod]
        public async Task TestWithInheritedClass(){}
        
        
        [TestMethod]
        public async Task SimpleTest()
        {
            const string code = @"
    class Program
    {
        static void  Main(string[] args)
        {
        }
        public void FirstMethod()
        {
        }
        public void SimpleMethod()
        {
            FirstMethod();
            DownMethod();
        }

        public void DownMethod()
        {
        }
    }";
            ImmutableArray<Diagnostic> diagnostics = await GetDiagnostics(code);

            Assert.AreEqual(1, diagnostics.Length);
        }

        private static async Task<ImmutableArray<Diagnostic>> GetDiagnostics(string code)
        {
            AdhocWorkspace workspace = new AdhocWorkspace();

            var solution = workspace.CurrentSolution;

            var projectId = ProjectId.CreateNewId();

            solution = solution
                .AddProject(
                    projectId,
                    "MyTestProject",
                    "MyTestProject",
                    LanguageNames.CSharp);

            solution = solution
                .AddDocument(DocumentId.CreateNewId(projectId),
                "File.cs",
                code);

            var project = solution.GetProject(projectId);

            project = project.AddMetadataReference(
                MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location))
                .AddMetadataReferences(GetAllReferencesNeededForType(typeof(ImmutableArray)));

            var compilation = await project.GetCompilationAsync();

            var compilationWithAnalyzer = compilation.WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(new SimpleYoYoAnalyzer()));

            var diagnostics = await compilationWithAnalyzer.GetAllDiagnosticsAsync();
            return diagnostics;
        }

        private static MetadataReference[] GetAllReferencesNeededForType(Type type)
        {
            var files = GetAllAssemblyFilesNeededForType(type);

            return files.Select(x => MetadataReference.CreateFromFile(x)).Cast<MetadataReference>().ToArray();
        }

        private static ImmutableArray<string> GetAllAssemblyFilesNeededForType(Type type)
        {
            return type.Assembly.GetReferencedAssemblies()
                .Select(x => Assembly.Load(x.FullName))
                .Append(type.Assembly)
                .Select(x => x.Location)
                .ToImmutableArray();
        }
    }
}
