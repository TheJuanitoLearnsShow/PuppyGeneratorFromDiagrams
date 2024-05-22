using System.Collections.Immutable;
using CaseExtensions;
using Puppy.SequenceSourceGenerator.Generators;

namespace Puppy.SequenceSourceGenerator;

public class SequenceParticipant(string participantName, string alias, string type)
{
    private List<string> _participantsCalled = [];
    private List<SynchronousMessage> _messagesReceived = [];
    private List<SynchronousMessage> _messagesSent = [];
    public string ParticipantName { get; } = participantName;
    public string Alias { get; } = alias;
    public string Type { get; } = type;

    public void AddMessage(SynchronousMessage message)
    {
        _messagesReceived.Add(message);
    }
    public void SetResponseToLastSyncMessageSent(string responseName)
    {
        _messagesSent.LastOrDefault()?.SetResponseName(responseName);
    }

    public void AddCallMade(SynchronousMessage message)
    {
        _messagesSent.Add(message);
    }
    
    public IReadOnlyList<SynchronousMessage> GetMessages() => _messagesReceived;
    
    public IReadOnlyList<SynchronousMessage> GetMessagesSent() => _messagesSent;
    
    public IReadOnlyList<string> GetParticipantsCalled() => 
        _messagesSent.Select(m => m.To).Distinct().ToImmutableList();

    internal ParamToGenerate GetVarDeclarationFor(string varName)
    {
        var varType = 
            _messagesSent
                .FirstOrDefault(m => m.ResultAssignmentCode == varName)
                ?.ResponseType;
        if (varType != null) return new ParamToGenerate() { Name = varName, Type = varType };
        var typeOfFirstUseAsParam = 
            _messagesSent
                .FirstOrDefault(m => m.ParameterNames.Contains(varName))
                ?.RequestType ?? "object";
        return new ParamToGenerate() { Name = varName, Type = typeOfFirstUseAsParam };
    }
    
    public void Deconstruct(out string ParticipantName, out string Alias, out string Type)
    {
        ParticipantName = this.ParticipantName;
        Alias = this.Alias;
        Type = this.Type;
    }
}