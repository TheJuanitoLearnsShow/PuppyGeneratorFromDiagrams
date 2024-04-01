using System.Text.RegularExpressions;
using CaseExtensions;

namespace Puppy.SequenceSourceGenerator;

public record ParsedDiagram(Dictionary<string, SequenceParticipant> Participants, List<SequenceMessage> Messages);
public partial class SequenceDiagramParser
    {
        private enum State
        {
            None,
            Participant,
            Message,
            MessageReply
        }

        

        public SequenceDiagramParser()
        {
        }

        public ParsedDiagram Parse(string input)
        {
            var participants = new Dictionary<string, SequenceParticipant>();
            var messages = new List<SequenceMessage>();

            var lines = input.Split('\n');
            var state = State.None;

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("participant"))
                {
                    state = State.Participant;
                }
                else if (line.Contains("-->>")) // more specific than ->>
                {
                    state = State.MessageReply;
                }
                else if (line.Contains("->>"))
                {
                    state = State.Message;
                }

                switch (state)
                {
                    case State.Participant:
                        var participantMatch = ParticipantRegex().Match(line);
                        if (participantMatch.Success)
                        {
                            var participantName = participantMatch.Groups[1].Value;
                            var participantParts = line.Split(" as ");
                            if (participantParts.Length == 1)
                            {
                                participants[participantName] = new SequenceParticipant(participantName, 
                                    participantName, participantName.ToPascalCase());
                            }
                            else
                            {
                                var aliasParts = participantParts.Last().Split(":");
                                
                                var alias = aliasParts.First().Trim();
                                var type = (aliasParts.LastOrDefault()?.Trim() ?? string.Empty).ToPascalCase();
                                participants[participantName] = new SequenceParticipant(
                                    alias, participantName, type);
                            }
                        }
                        break;
                    case State.Message:
                        var messageMatch = MessageRegex().Match(line);
                        if (messageMatch.Success)
                        {
                            var from = messageMatch.Groups[1].Value;
                            var to = messageMatch.Groups[2].Value;
                            var message = messageMatch.Groups[3].Value;
                            var msg = new SynchronousMessage(message, from, to);
                            participants[from].AddCallMade(msg);
                            participants[to].AddMessage(msg);
                            messages.Add(new SequenceMessage(from, to, message));
                        }
                        break;
                    case State.MessageReply:
                        var messageReplyMatch = ReplyMessageRegex().Match(line);
                        var toReply = messageReplyMatch.Groups[2].Value;
                        var messageReply = messageReplyMatch.Groups[3].Value;
                        participants[toReply].SetResponseToLastSyncMessageSent( messageReply);
                        break;
                    case State.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        return new ParsedDiagram(participants, messages);
        }

    [GeneratedRegex(@"(\w+)->>(\w+): (.*)")]
    private static partial Regex MessageRegex();
    
    [GeneratedRegex(@"(\w+)-->>(\w+): (.*)")]
    private static partial Regex ReplyMessageRegex();
    [GeneratedRegex(@"participant (\w+)(?: as (\w+))?")]
    private static partial Regex ParticipantRegex();
}
