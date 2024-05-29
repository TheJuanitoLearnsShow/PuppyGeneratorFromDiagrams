using System.Collections.Immutable;

namespace Puppy.SequenceSourceGenerator.Generators;

public class GeneratorResult
{
    public string FlowName { get; private set; }
    public string NameSpace { get; set; }
    public ImmutableDictionary<string, InterfaceToGenerate> Participants { get; private set; }
    public ImmutableDictionary<string, string> PayloadClasses { get; private set; }
    public ImmutableList<(string, string, string)> Orchestrators { get; private set; }

    private GeneratorResult()
    {
            
    }
    public GeneratorResult(ParsedDiagram parsedDiagram, ClassGenerators generator, string flowName)
    {
        FlowName = flowName;
        Participants = generator.GenerateCodeForInterfaces(parsedDiagram).ToImmutableDictionary(
            v => v.ClassName, v => v.Contents);
        PayloadClasses = generator.GenerateCodeForPayloads(parsedDiagram).ToImmutableDictionary(
            v => v.InterfaceName, v => v.Contents);
        var orchestrator = generator.GenerateCodeForOrchestrator(parsedDiagram, flowName);
        Orchestrators = new [] {
            (flowName, orchestrator.ClassName, orchestrator.Contents)
        }.ToImmutableList();
    }

    public GeneratorResult Merge(GeneratorResult other)
    {
        var newParticipants = new Dictionary<string, InterfaceToGenerate>(Participants);

        foreach (var otherParticipant in other.Participants)
        {
            var key = otherParticipant.Key;
            if (newParticipants.TryGetValue(key, out var existingVal))
            {
                newParticipants[key] = existingVal.Merge(otherParticipant.Value);
            }
        }

        var newPayloadClasses = PayloadClasses.Concat(other.PayloadClasses).Distinct();
        return new GeneratorResult()
        {
            PayloadClasses = newPayloadClasses.ToImmutableDictionary(),
            Participants = newParticipants.ToImmutableDictionary(),
            Orchestrators = Orchestrators.Concat(other.Orchestrators).ToImmutableList()
        };
    }
    
    public ImmutableList<(string FlowName, string ClassName, string Contents)> ToFilesToGenerate(string nameSpace)
    {
        var fileToGenerate = Participants.Select(kv =>
            (FlowName, $"{nameSpace}.{kv.Key}" , GenerateCodeForParticipant(nameSpace, kv.Value))
        ).ToList();
        fileToGenerate.AddRange(PayloadClasses.Select(kv => (FlowName,$"{nameSpace}.{kv.Key}", kv.Value)));
        fileToGenerate.AddRange(Orchestrators);
        return fileToGenerate.ToImmutableList();
    }
    private string GenerateCodeForParticipant(string nameSpace,
        InterfaceToGenerate participant)
    {
        var participantInterfaceName = participant.Name;
        var mainInterface = $"""
                             namespace {nameSpace};
                             using System.Collections.Generic;

                             public partial interface {participantInterfaceName}
                             """
                            +
                            "\n{\n" +
                            string.Join("\n\n",  participant.Methods.Select(m =>
                                                     m.ToCode()
                                                     )
                                                 )
                            +
                            "\n}\n";
        return mainInterface;
    }
}