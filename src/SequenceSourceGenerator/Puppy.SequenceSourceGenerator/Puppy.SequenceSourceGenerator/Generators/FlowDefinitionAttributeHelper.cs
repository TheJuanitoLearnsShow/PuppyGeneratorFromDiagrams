namespace Puppy.SequenceSourceGenerator.Generators;

public class FlowDefinitionAttributeHelper
{
    public const string Attribute = @"
namespace Puppy.SequenceSourceGenerator.Generators
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FlowDefinitionAttribute : Attribute
    {
        public string FlowName { get; set; } = string.Empty;
        public string DefinitionFilePath { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }
}";
}