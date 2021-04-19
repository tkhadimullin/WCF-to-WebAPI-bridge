using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;

namespace APIGenerator
{
    public static class DtoGenerationHelper
    {
        public static string GenerateDtoCode(this MethodDeclarationSyntax method) 
        {
            var methodName = method.Identifier.ValueText;

            var methodDtoCode = new StringBuilder($"public class {methodName}Dto {{")
                                                .AppendLine("");

            foreach (var parameter in method.ParameterList.Parameters)
            {
                var isOut = parameter.IsOut();

                if (!isOut)
                {
                    methodDtoCode.AppendLine($"public {parameter.Type} {parameter.Identifier} {{ get; set; }}");
                }
            }

            methodDtoCode.AppendLine("}");
            return methodDtoCode.ToString();
        }
    }
}
