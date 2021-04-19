using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;

namespace APIGenerator
{
    public static class ParameterGenerationHandler
    {
        public static string GenerateWcfCallParameters(this SeparatedSyntaxList<ParameterSyntax> parameters) 
        {
            var parameterList = new StringBuilder();

            foreach (var parameter in parameters)
            {
                if (parameter.IsOut()) 
                {
                    parameterList.Append($"out var {parameter.Identifier}, ");
                } else {
                    parameterList.Append($"request.{parameter.Identifier}, ");
                }
            }

            return parameterList.TrimTrailingComma().ToString();
        }

        public static string GenerateOutParametersResultObject(this SeparatedSyntaxList<ParameterSyntax> parameters) 
        {
            var parameterList = new StringBuilder();

            foreach (var parameter in parameters.Where(p => p.IsOut()))
            { 
                parameterList.AppendLine($"{{ \"{parameter.Identifier}\", {parameter.Identifier} }},");
            }

            return parameterList.ToString();
        }

        public static bool IsOut(this ParameterSyntax parameter)
        {
            return parameter.Modifiers.Any(p => p.Text == "out");
        }

        public static StringBuilder TrimTrailingComma(this StringBuilder stringBuilder) 
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
            }
            return stringBuilder;
        }

    }
}
