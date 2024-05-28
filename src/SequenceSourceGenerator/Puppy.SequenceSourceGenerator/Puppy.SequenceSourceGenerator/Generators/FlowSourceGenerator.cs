using System.Collections.Immutable;
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
            var generatorResults = GetGeneratorResults(classDeclarations, additionalTexts).ToList();
            var resultsByNamespace = generatorResults
                .GroupBy(r => r.NameSpace)
                .SelectMany(grp =>
                {
                    return grp.Aggregate( null, (GeneratorResult? state, GeneratorResult generatorResult) =>
                        {
                            if (state == null) return generatorResult;
                            return state
                                .Merge(generatorResult);
                        })?
                        .ToFilesToGenerate(grp.Key);
                })
                .ToList();
            foreach (var fileToGenerate in resultsByNamespace)
            {
                spc.AddSource($"Flow_{fileToGenerate.ClassName}_{Guid.NewGuid()}.cs", 
                    SourceText.From(fileToGenerate.Contents, Encoding.UTF8));
            }
        });
    }

    private static IEnumerable<GeneratorResult> GetGeneratorResults(ImmutableArray<FlowClassInfo> classDeclarations, AdditionalText additionalTexts)
    {
        foreach (var classDeclaration in classDeclarations)
        {
            // Find the attribute and get the file name
            var additionalFileName = classDeclaration.FilePath;

            // Find the additional file that matches the file name
            if (additionalTexts.Path.EndsWith(additionalFileName))
            {
                var lines = additionalTexts.GetText()?.Lines.Select(l => l.ToString() ?? string.Empty).ToList() ?? [];
                // Generate the source code using the content of the additional file
                // string generatedCode = GenerateCodeBasedOnAdditionalFile(classDeclaration, additionalTexts.GetText().Lines.Select(l => SourceText.From(l.so)));

                var generatorResult = GenerateFromDiagram(lines, classDeclaration.Namespace, classDeclaration.FlowName);

                yield return generatorResult;
            }
        }
    }

    private static GeneratorResult GenerateFromDiagram(List<string> mdFile, string nameSpace, string flowName)
    {
        var mermaidDiagram = mdFile.SkipWhile(l => !l.StartsWith("```mermaid"))
            .Skip(1)
            .TakeWhile(l => !l.StartsWith("```"));
        var parser = new SequenceDiagramParser();
        var result = parser.Parse(string.Join("\n", mermaidDiagram));

        var generator = new ClassGenerators(nameSpace);
        var generatorResult = new GeneratorResult(result, generator, flowName)
        {
            NameSpace = nameSpace
        };
        return generatorResult;
    }
    
    private FlowClassInfo GetFlowClassInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var type = (INamedTypeSymbol)context.TargetSymbol;
        var flowFilePath = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name.ToString() == attributeName)
            ?.NamedArguments.FirstOrDefault(a => a.Key == "DefinitionFilePath")
            .Value.Value?.ToString() ?? "";
        var flowName = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name.ToString() == attributeName)
            ?.NamedArguments.FirstOrDefault(a => a.Key == "FlowName")
            .Value.Value?.ToString() ?? "Flow";
        var classInfo = new FlowClassInfo(type, flowFilePath, flowName);

        return classInfo;
    }
}