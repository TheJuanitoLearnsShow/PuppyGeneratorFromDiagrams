namespace Puppy.SequenceSourceGenerator.Generators;

public class StepsCalledState()
{
    public int StepIdx { get; set; } = 0;
    public List<ParamToGenerate> ResponsesSoFar { get; } = new();
    public List<MethodToGenerate> Methods { get; } = new();
    public List<string> CallingCode { get; } = new();
    public OptBlock CurrentOptBlock { get; set; } = new();

    public void Deconstruct(out int stepIdx, out List<ParamToGenerate> responsesSoFar, out List<MethodToGenerate> methods)
    {
        stepIdx = this.StepIdx;
        responsesSoFar = this.ResponsesSoFar;
        methods = this.Methods;
    }
}