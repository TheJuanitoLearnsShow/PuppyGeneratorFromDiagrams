using Puppy.SequenceSourceGenerator.Generators;

namespace Puppy.SequenceSourceGenerator.Sample;

[FlowDefinition(DefinitionFilePath = "sample-flow.md", FlowName = "Flow1") ]
public partial class MyFlow : FlowOrchestratorBase
{

    public MyFlow(IAlice a, IBOb b) : base(b,a)
    {
    }

    public Task<FlowOrchestratorStateBase> StartFlow1()
    {
        return ExecuteFlow1(new FlowOrchestratorStateBase(), new HiBobGreatSeeingYouRequest());
    }
}
