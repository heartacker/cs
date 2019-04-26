using System;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Runtime.Loader;

namespace CodeProviders
{
    class cs
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                LogError("Must specify an expression or code (variable `arg` contains standard input)");
                return;
            }

            var standardInput = new StringBuilder();
            if (Console.IsInputRedirected)
            {
                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    standardInput.AppendLine(line);
                }
            }
            else
            {
                if (args.Length > 1)
                {
                    standardInput.AppendLine(args[1]);
                }
            }

            Eval(args[0], standardInput.ToString());
        }

        private static void PrintHelp()
        {
            Log("csharp - An unix cli tool to execute arbitrary CSharp code");
            Log("Usage:");
            Log("csharp <csharp code>");
            Log("");
            Log("Example:");
            Log("cat cs.csproj |  csharp \"var x = 10; Echo(x + arg.Split('<')[1]);\"");
            Log("");
            Log("(Echo is an alias to Console.WriteLine)");
        }

        static void Log(string message)
        {
            Console.WriteLine(message);
        }

        static void LogError(string message)
        {
            Log($"[ERROR] {message}");
            Log("");
            Log("");
            PrintHelp();
            Environment.Exit(1);
        }

        public static void Eval(string code, string arg)
        {
            if(!code.Contains("Console.Write") && !code.Contains("Echo"))
            {
                code = $"Echo({code});";
            }
            string codeToCompile = @"
            using System;
            using System.IO;
            using System.Text;
            using System.Reflection;
            using System.Linq;
            using System.Collections.Generic;
            namespace cs
            {
                public class code
                {
                    public static void exec(string arg)
                    {
                        " + code + @"
                    }

                    static void Echo(string arg) => Console.WriteLine(arg);
                }
            }";

            var syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

            string assemblyName = Path.GetRandomFileName();
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(PathOf(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute))),
                MetadataReference.CreateFromFile(PathOf(typeof(object))),
                MetadataReference.CreateFromFile(PathOf(typeof(Path))),
                MetadataReference.CreateFromFile(PathOf(typeof(Console))),
                MetadataReference.CreateFromFile(PathOf(typeof(Enumerable))),
            };

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTrees: new[] { syntaxTree }, references: references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    var type = assembly.GetType("cs.code");
                    var instance = assembly.CreateInstance("code.exec");
                    var meth = type.GetMember("exec").First() as MethodInfo;
                    meth.Invoke(instance, new[] { arg });
                }
            }

            string PathOf(Type type) => type.Assembly.Location;
        }

    }
}
