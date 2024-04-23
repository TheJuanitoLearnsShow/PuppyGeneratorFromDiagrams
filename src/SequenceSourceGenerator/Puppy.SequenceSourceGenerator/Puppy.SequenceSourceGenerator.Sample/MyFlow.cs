using Puppy.SequenceSourceGenerator.Generators;

namespace Puppy.SequenceSourceGenerator.Sample;

[FlowDefinition(DefinitionFilePath = "sample-flow.md", FlowName = "Flow1")]
public class MyFlow
{
    
}

public partial interface IBOb
{
    Task<HiBobResponse> HiBob();
}