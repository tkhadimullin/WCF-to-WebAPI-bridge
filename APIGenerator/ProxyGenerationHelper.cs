using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace APIGenerator
{
    /// <summary>
    /// Generates proxy class based on given name and interface
    /// </summary>
    public static class ProxyGenerationHelper
    {
        public static StringBuilder GenerateWcfProxies(this ClassDeclarationSyntax classDeclaration, HashSet<INamedTypeSymbol> proxiesToGenerate, SemanticModel model)
        {
            var proxyCode = new StringBuilder();
            foreach (var contract in proxiesToGenerate)
            {
                proxyCode.Append(GenerateWcfProxy(classDeclaration, contract, model));
            }

            return proxyCode;
        }

        private static StringBuilder GenerateWcfProxy(this ClassDeclarationSyntax classDeclaration, INamedTypeSymbol proxyToGenerate, SemanticModel model)
        {
            var proxyCode = new StringBuilder("using System;").AppendLine()
                                .AppendLine("using System.ServiceModel;")
                                .AppendLine("namespace WcfService.BridgeControllers {");


            proxyCode.AppendLine($"public class {proxyToGenerate.Name}Proxy: ClientBase<{proxyToGenerate}>, {proxyToGenerate} {{");

            foreach (var method in proxyToGenerate.GetMembers().OfType<IMethodSymbol>()) 
            {
                var parameters = method.Parameters.Aggregate(new StringBuilder(), (sb, p) => sb.Append($"{p} {p.Name}").Append(", ")).TrimTrailingComma();
                var parametersToCall = method.Parameters.Aggregate(new StringBuilder(), (sb, p) => sb.Append($"{(p.RefKind == RefKind.Out? "out ": "")}{p.Name}").Append(", ")).TrimTrailingComma();

                proxyCode.AppendLine($"public {method.ReturnType} {method.Name}({parameters}) {{")
                    .AppendLine($"return Channel.{method.Name}({parametersToCall});")
                    .AppendLine("}")
                    .AppendLine();
            }

            proxyCode.AppendLine("}}");

            return proxyCode;
        }
    }
}



/*

    public class ServiceClient : ClientBase<IService>, IService
    {
        public string GetData(int value)
        {
            return Channel.GetData(value);
        }

        public string GetDataOut(int value, out DateTime creationDate)
        {
            return Channel.GetDataOut(value, out creationDate);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            return Channel.GetDataUsingDataContract(composite);
        }

 */