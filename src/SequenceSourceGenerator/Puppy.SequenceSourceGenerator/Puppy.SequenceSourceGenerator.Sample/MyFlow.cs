using Puppy.SequenceSourceGenerator.Generators;

namespace Puppy.SequenceSourceGenerator.Sample;

[FlowDefinition(DefinitionFilePath = "sample-flow.md", FlowName = "Flow1") ]
public partial class MyFlow : FlowOrchestratorBase
{

    public MyFlow(IAlice a, IBOb b)
    {
        this.a = a;
        this.b = b;
    }

    public Task StartFlow1()
    {
        return ExecuteFlow1();
    }
}
