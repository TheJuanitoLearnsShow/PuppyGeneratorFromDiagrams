using Microsoft.CodeAnalysis;

namespace Puppy.SequenceSourceGenerator.Generators;

internal readonly struct FlowClassInfo(INamedTypeSymbol type, string flowFilePath, string flowName)
{
    public readonly string Namespace = type.ContainingNamespace.IsGlobalNamespace ? string.Empty : type.ContainingNamespace.ToString();
    public readonly string FilePath = flowFilePath;
    public readonly string FlowName = flowName;
}