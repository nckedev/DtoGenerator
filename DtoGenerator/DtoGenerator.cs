using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DtoGenerator;

[Generator]
public class DtoGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarationSyntax =
            context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: (node, _) => IsSyntaxTargetForGeneration(node),
                    transform: (syntaxContext, _) => GetSemanticTargetForGeneration(syntaxContext))
                .Where(type => type is not null).Collect();

        context.RegisterSourceOutput(classDeclarationSyntax, GenerateCode);
    }

    //check if is an intresting node
    private static bool IsSyntaxTargetForGeneration(SyntaxNode s)
    {
        if (s is not AttributeSyntax attr)
            return false;

        var name = ExtractAttributeName(attr.Name);

        return name is "GenerateDto" or "GenerateDtoAttribute";
    }

    //get the name of the attribute
    private static string? ExtractAttributeName(NameSyntax? name) =>
        name switch
        {
            SimpleNameSyntax sns => sns.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };


    static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        //todo 
        var attributeSyntax = (AttributeSyntax) context.Node;
        if (attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax classDeclaration)
            return null;

        return classDeclaration;
        // var type = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as ITypeSymbol;
        // return type;
    }

    private static void GenerateCode(SourceProductionContext context,
        ImmutableArray<ClassDeclarationSyntax?> enumerations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace GeneratedDtos");

        sb.AppendLine("{");
        sb.AppendLine("\tpublic partial class " + enumerations.First()?.Identifier.Text + "Dto");
        sb.AppendLine("\t{");
        foreach (var type in enumerations)
        {
            foreach (var prop in GetProperties(type))
            {
                sb.AppendLine("\t\t" + prop + Environment.NewLine);
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            context.AddSource(type.Identifier.Text + "Dto.g.cs", sb.ToString());
        }
    }

    private static IEnumerable<string> GetProperties(ClassDeclarationSyntax classDeclarationSyntax)
    {
        List<string> props = new List<string>();
        foreach (var child in classDeclarationSyntax.ChildNodes())
        {
            //todo 
            if (child is PropertyDeclarationSyntax prop)
            {
                if (!prop.ToString().Contains("ExcludeFromDto") && !prop.ToString().StartsWith("private"))
                {
                    props.Add(prop.ToString());
                }
            }
        }

        return props;
    }
}