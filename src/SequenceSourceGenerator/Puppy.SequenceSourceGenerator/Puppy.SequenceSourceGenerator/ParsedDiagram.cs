namespace Puppy.SequenceSourceGenerator;

public class ParsedDiagram(Dictionary<string, SequenceParticipant> participants, List<SequenceMessage> messages)
{
    public Dictionary<string, SequenceParticipant> Participants { get; } = participants;
    public List<SequenceMessage> Messages { get; } = messages;

    public void Deconstruct(out Dictionary<string, SequenceParticipant> participants, out List<SequenceMessage> messages)
    {
        participants = this.Participants;
        messages = this.Messages;
    }
}