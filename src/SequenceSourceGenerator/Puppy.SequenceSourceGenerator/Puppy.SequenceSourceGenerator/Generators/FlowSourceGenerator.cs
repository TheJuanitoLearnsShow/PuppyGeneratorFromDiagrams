using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Puppy.SequenceSourceGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class FlowSourceGenerator: IIncrementalGenerator
{
    private const string attributeName = "FlowDefinitionAttribute";
    private const string attributeFullName = "Puppy.SequenceSourceGenerator.Generators.FlowDefinitionAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "FlowExtensionsAttribute.g.cs", 
            SourceText.From(FlowDefinitionAttributeHelper.Attribute, Encoding.UTF8)));
        
        // Define a provider that finds all classes with the 'MyAttribute'
        
        var flowClassesInfo = context.SyntaxProvider
            .ForAttributeWithMetadataName(attributeFullName,
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                GetFlowClassInfo)
            .Collect()
            // .SelectMany((enumInfos, _) => enumInfos.Distinct())
            ;
        
        var onlyMarkDownsProvider = context.AdditionalTextsProvider.Where(f =>
            f.Path.EndsWith(".md", StringComparison.OrdinalIgnoreCase));
        // Combine the class declarations with the additional text files
        var combinedProvider = onlyMarkDownsProvider.Combine(
            flowClassesInfo
            );

        // Generate the source for each combination of class declaration and additional text
        context.RegisterSourceOutput(combinedProvider, (spc, source) => {
            var (additionalTexts , classDeclarations) = source;

            if (additionalTexts == null) return;
            foreach (var classDeclaration in classDeclarations)
            {
                // Find the attribute and get the file name
                var additionalFileName = classDeclaration.FilePath;

                // Find the additional file that matches the file name
                if (additionalTexts.Path.EndsWith(additionalFileName))
                {
                    // Generate the source code using the content of the additional file
                    string generatedCode = GenerateCodeBasedOnAdditionalFile(classDeclaration, additionalTexts.GetText().ToString());

                    // Add the generated source code to the compilation
                    spc.AddSource($"Flow_Generated.cs", SourceText.From(generatedCode, Encoding.UTF8));
                }
            }
        });
    }

    private string GenerateCodeBasedOnAdditionalFile(FlowClassInfo classDeclaration, string fileContent)
    {
        // Implement your logic to generate code based on the content of the additional file
        return $"// Generated code based on the additional file content {classDeclaration.Namespace} {classDeclaration.FilePath}";
    }
    private FlowClassInfo GetFlowClassInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var type = (INamedTypeSymbol)context.TargetSymbol;
        var flowFilePath = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name.ToString() == attributeName)
            ?.NamedArguments.FirstOrDefault(a => a.Key == "DefinitionFilePath")
            .Value.Value?.ToString() ?? "";
        var enumInfo = new FlowClassInfo(type, flowFilePath);
        //
        // if (_logger.IsEnabled(LogLevel.Debug))
        //     _logger.Log(LogLevel.Debug, $"Smart Enum found: {enumInfo.Namespace}.{enumInfo.Name}");

        return enumInfo;
    }
}