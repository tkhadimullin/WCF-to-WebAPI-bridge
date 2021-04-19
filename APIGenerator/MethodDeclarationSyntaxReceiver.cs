using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APIGenerator
{
    public class MethodDeclarationSyntaxReceiver : ISyntaxReceiver
    {
        public ConcurrentDictionary<ClassDeclarationSyntax, List<MethodDeclarationSyntax>> methodsToGenerate { get; set; } = new ConcurrentDictionary<ClassDeclarationSyntax, List<MethodDeclarationSyntax>>();


        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            var methodDeclaration = syntaxNode as MethodDeclarationSyntax;
            if (methodDeclaration == null) return;
            var classDeclaration = methodDeclaration.Parent as ClassDeclarationSyntax;
            if (classDeclaration == null) return;

            var isDecorated = methodDeclaration.AttributeLists.SelectMany(al => al.Attributes).Any(a => (a.Name as IdentifierNameSyntax).Identifier.ValueText == "GenerateApiEndpoint");
            
            if (!isDecorated) return;

            var methodList = methodsToGenerate.GetOrAdd(classDeclaration, new List<MethodDeclarationSyntax>());

            methodList.Add(methodDeclaration);

        }
    }
}
