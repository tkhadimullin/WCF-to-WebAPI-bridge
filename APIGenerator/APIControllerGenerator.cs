using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace APIGenerator
{
    /// <summary>
    /// https://www.codemag.com/article/0809101/WCF-the-Manual-Way%E2%80%A6-the-Right-Way        
    /// </summary>
    [Generator]
    public class APIControllerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            context.RegisterForSyntaxNotifications(() => new MethodDeclarationSyntaxReceiver());            
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            var ctx = (MethodDeclarationSyntaxReceiver)context.SyntaxReceiver;

            foreach (var classDeclaration in ctx.methodsToGenerate.Keys) {
                var namespaceDeclaration = classDeclaration.Parent as NamespaceDeclarationSyntax;
                var fileDeclaration = namespaceDeclaration.Parent as CompilationUnitSyntax;
                var usings = namespaceDeclaration.Usings.Union(fileDeclaration.Usings).Select(u => u.ToString()); // collect all usings. dont bother filtering for now
                var className = classDeclaration.Identifier.ValueText;
                var proxiesToGenerate = new HashSet<INamedTypeSymbol>();

                // start usings
                var sourceCode = usings.Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine(s));
                sourceCode.AppendLine("")
                           .AppendLine("using System.Web.Http;")
                           .AppendLine("using System.Collections.Generic;")
                           .AppendLine("using WcfService.Clients;")
                           .AppendLine("");

                sourceCode.AppendLine()
                            .AppendLine("namespace WcfService.BridgeControllers {")
                            .AppendLine()
                            .AppendLine($"[RoutePrefix(\"api/{className}\")]public class {className}Controller: ApiController {{");

                var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                INamedTypeSymbol classModel = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                var interfaces = (classModel as INamedTypeSymbol)
                                                .AllInterfaces
                                                .SelectMany(i => i.GetMembers().OfType<IMethodSymbol>())
                                                .Select(m => new { Interface = m.ContainingType, 
                                                                   Implementation = classModel.FindImplementationForInterfaceMember(m)
                                                }) // TODO: convert to Dictionary<IMethodSymbol, object>
                                                .ToList();


                foreach (var method in ctx.methodsToGenerate[classDeclaration]) {
                    var methodSymbol = model.GetDeclaredSymbol(method);
                    var clientProxy = interfaces.First(i => i.Implementation.Equals(methodSymbol)).Interface;
                    proxiesToGenerate.Add(clientProxy);

                    var dtoCode = method.GenerateDtoCode();
                    sourceCode.AppendLine().AppendLine(dtoCode).AppendLine(); // add DTO definition before consuming method

                    var methodName = method.Identifier.ValueText;
                    var parameters = method.ParameterList.Parameters;

                    var wcfCallParameterList = parameters.GenerateWcfCallParameters();
                    var outParameterResultList = parameters.GenerateOutParametersResultObject();

                    var methodCode = new StringBuilder($"[HttpPost][Route(\"{methodName}\")]").AppendLine()
                                        .AppendLine($"public Dictionary<string, object> {methodName}([FromBody] {methodName}Dto request) {{")
                                        .AppendLine($"var proxy = new {clientProxy.Name}Proxy();")
                                        .AppendLine($"var response = proxy.{methodName}({wcfCallParameterList});")
                                        .AppendLine("return new Dictionary<string, object> {")
                                        .AppendLine(" {\"response\", response },") // TODO: check what happens when return is void
                                        .AppendLine(outParameterResultList)
                                        .AppendLine("};")
                                        .AppendLine("}");

                    sourceCode.AppendLine(methodCode.ToString());
                    ;
                    
                    methodCode.AppendLine("}").AppendLine();
                }

                sourceCode.AppendLine("}}"); // close namespace and class

                var proxyCode = classDeclaration.GenerateWcfProxies(proxiesToGenerate, context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree));

                context.AddSource($"{className}Controller", sourceCode.ToString());
                context.AddSource($"{className}Proxy", proxyCode.ToString());
            }
        }
    }

    public class InterfaceMethodClass { 
        public INamedTypeSymbol Interface { get;set; }
        public List<IMethodSymbol> Methods { get; set; }
    }
}
