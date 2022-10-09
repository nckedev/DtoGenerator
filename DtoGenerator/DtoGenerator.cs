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


    static ITypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        //todo 
        var attributeSyntax = (AttributeSyntax) context.Node;
        if (attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax classDeclaration)
            return null;

        var type = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as ITypeSymbol;
        return type;
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol?> enumerations)
    {
        string code = "public partial class " + enumerations.First()?.Name +"Dto  { ";
        foreach (var type in enumerations)
        {
            code += type.Name + Environment.NewLine; //class name
            foreach (var prop in GetProperties(type))
            {
                code += prop + Environment.NewLine;
            }
        }

        code += "}";

        context.AddSource("generatedDto.g.cs", code);
    }

    private static IEnumerable<string> GetProperties(ITypeSymbol type)
    {
        var properties = type.GetMembers().Select(s =>
        {
            if (s.DeclaredAccessibility != Accessibility.Public
                || s.DeclaredAccessibility != Accessibility.Protected
                || s is not IPropertySymbol prop)
            {
                return null;
            }

            return SymbolEqualityComparer.Default.Equals(prop.Type, type) ? prop.Name : null;
        });
        return properties.Where(prop => prop is not null) as IEnumerable<string>;
    }
}

internal static class SourceGeneratorExtensions
{
    internal static string BuildDtoProperty(
        this PropertyDeclarationSyntax pds, Compilation compilation)
    {
        // get the symbol for this property from the semantic model
        var symbol = compilation
            .GetSemanticModel(pds.SyntaxTree)
            .GetDeclaredSymbol(pds);

        var property = (symbol as IPropertySymbol);
        // use the same type and name for the DTO properties as on the entity
        return $"public {property?.Type.Name} {property?.Name} {{get; set;}}";
    }
}