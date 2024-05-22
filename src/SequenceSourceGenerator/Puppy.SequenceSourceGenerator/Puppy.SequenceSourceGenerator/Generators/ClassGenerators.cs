namespace Puppy.SequenceSourceGenerator.Generators;

public class ClassGenerators
{
    private readonly ParticipantInterfaceGenerator _participantInterfaceGenerator;
    private readonly OrchestratorClassGenerator _orchestratorClassGenerator;
    private readonly PayloadClassesGenerator _payloadClassesGenerator;

    public ClassGenerators(string nameSpace)
    {
        _participantInterfaceGenerator = new(nameSpace);
        _orchestratorClassGenerator = new(nameSpace);
        _payloadClassesGenerator = new(nameSpace);
    }

    public IEnumerable<(string ClassName, InterfaceToGenerate Contents)> GenerateCodeForInterfaces(
        ParsedDiagram diagram)
        => _participantInterfaceGenerator.GenerateCodeForInterfaces(diagram);

    public IEnumerable<(string InterfaceName, string Contents)> GenerateCodeForPayloads(ParsedDiagram diagram) 
        => _payloadClassesGenerator.GenerateCodeForPayloads(diagram);


    public (string ClassName, string Contents) GenerateCodeForOrchestrator(ParsedDiagram diagram, string flowName)
        => _orchestratorClassGenerator.GenerateCodeForOrchestrator(diagram, flowName);
}