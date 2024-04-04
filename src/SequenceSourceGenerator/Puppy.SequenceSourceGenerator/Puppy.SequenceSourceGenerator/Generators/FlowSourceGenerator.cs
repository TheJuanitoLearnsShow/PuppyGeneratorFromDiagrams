namespace Puppy.SequenceSourceGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class FlowSourceGenerator: ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
    // Retrieve the compilation that represents the user's project
    var compilation = context.Compilation;

    // Find all class declarations in the user's project
    var classes = compilation.SyntaxTrees
        .SelectMany(syntaxTree => syntaxTree.GetRoot().DescendantNodes())
        .OfType<ClassDeclarationSyntax>();

    foreach (var classDeclaration in classes)
    {
        // Get the semantic model for the syntax tree that contains the class declaration
        var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

        // Get the symbol that represents the class
        var classSymbol = model.GetDeclaredSymbol(classDeclaration);

        // Iterate through all attributes of the class
        foreach (var attribute in classSymbol.GetAttributes())
        {
            // Check if the attribute is the one you're interested in
            if (attribute.AttributeClass.Name == "FlowDefinition")
            {
                // Read the attribute's properties
                foreach (var arg in attribute.NamedArguments)
                {
                    // Do something with the attribute's properties
                    // For example, print the name and value of the property
                    Console.WriteLine($"{arg.Key}: {arg.Value}");
                }
            }
        }
    }
}
}