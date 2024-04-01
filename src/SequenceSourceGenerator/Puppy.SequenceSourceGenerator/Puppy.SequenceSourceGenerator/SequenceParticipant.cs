using CaseExtensions;

namespace Puppy.SequenceSourceGenerator;

public record SequenceParticipant(string ParticipantName, string Alias, string Type)
{
    private List<SynchronousMessage> _messagesReceived = [];
    private List<SynchronousMessage> _messagesSent = [];
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
}