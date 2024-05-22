using CaseExtensions;

namespace Puppy.SequenceSourceGenerator.Generators;

public class ParticipantInterfaceGenerator
{
    private readonly string _nameSpace;

    public ParticipantInterfaceGenerator(string nameSpace)
    {
        _nameSpace = nameSpace;
    }
    
    private (string ClassName, InterfaceToGenerate Contents) GenerateCodeForParticipant(
        SequenceParticipant participant, IReadOnlyCollection<SequenceParticipant> participants)
    {
        var participantInterfaceName = participant.Type.ToPascalCase();
        var mainInterface = new InterfaceToGenerate
        {
            Name = participantInterfaceName,
            Methods = participant.GetMessages()
                .Select<SynchronousMessage, MethodToGenerate>(m =>
                {
                    var caller = participants.FirstOrDefault(p => p.Alias == m.From);
                    return GenerateMethodDeclarationForMessage(m, caller);
                })
                .ToList()
        };

        return (participantInterfaceName, mainInterface);
    }

    private MethodToGenerate GenerateMethodDeclarationForMessage(SynchronousMessage msg, SequenceParticipant? caller)
    {
        if (caller == null || string.IsNullOrEmpty(msg.ParametersCode))
        {
            return new MethodToGenerate()
            {
                Name = msg.MessageName,
                ReturnType = msg.ResponseType,
                MethodParams = []
            };
        }

        var paramsForMethod = 
            msg.ParametersCode
                .Split(',')
                .Select(p => caller.GetVarDeclarationFor(p.Trim()))
                .ToList();
        return new MethodToGenerate()
        {
            Name = msg.MessageName,
            ReturnType = msg.ResponseType,
            MethodParams = paramsForMethod
        };
    }

    public IEnumerable<(string ClassName, InterfaceToGenerate Contents)> GenerateCodeForInterfaces(ParsedDiagram diagram)
    {
        var participants = diagram.Participants
            .Select(p => p.Value)
            .ToList();
        var classes = participants
            .Select(p =>
                GenerateCodeForParticipant(p, participants));
        return classes;
    }
}