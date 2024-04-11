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

    public ParsedDiagram Merge(ParsedDiagram other)
    {
        Dictionary<string, SequenceParticipant> newParticipants = new(participants.Count + other.Participants.Count);
        List<SequenceMessage> newMessages = new(Messages.Count + other.Messages.Count);
        
        var uniqueKeys = new HashSet<string>(participants.Keys);
        uniqueKeys.UnionWith(other.Participants.Keys);

// Populate mergedDictionary with actual values from dict1 and dict2
        foreach (var key in uniqueKeys)
        {
            var foundInThisDiagram = participants.TryGetValue(key, out var value1);
            if (foundInThisDiagram)
                newParticipants[key] = value1;

            if (other.Participants.TryGetValue(key, out var value2))
            {
                if (foundInThisDiagram)
                {
                    newParticipants[key].Merge();
                }
            }
            newParticipants[key] = value2;
        }
    }
}