using CaseExtensions;

namespace Puppy.SequenceSourceGenerator;

public record SequenceParticipant(string ParticipantName, string Alias, string Type)
{
    private List<string> _messages = [];
    public void AddMessage(string message)
    {
        _messages.Add(message.Trim().ToPascalCase());
    }

    public IReadOnlyList<string> GetMessages() => _messages;
}