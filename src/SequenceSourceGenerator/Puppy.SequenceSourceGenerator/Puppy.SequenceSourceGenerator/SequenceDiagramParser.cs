using System.Text.RegularExpressions;
using CaseExtensions;

namespace Puppy.SequenceSourceGenerator;

public partial class SequenceDiagramParser
    {
        private enum State
        {
            None,
            Participant,
            Message
        }

        public Dictionary<string, SequenceParticipant> Participants { get; private set; }
        public List<SequenceMessage> Messages { get; private set; }

        public SequenceDiagramParser()
        {
            Participants = new Dictionary<string, SequenceParticipant>();
            Messages = new List<SequenceMessage>();
        }

        public void Parse(string input)
        {
            var lines = input.Split('\n');
            var state = State.None;

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("participant"))
                {
                    state = State.Participant;
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
                                Participants[participantName] = new SequenceParticipant(participantName, participantName, participantName.ToPascalCase());
                            }
                            else
                            {
                                var aliasParts = participantParts.Last().Split(":");
                                
                                var alias = aliasParts.First().Trim();
                                var type = (aliasParts.LastOrDefault()?.Trim() ?? string.Empty).ToPascalCase();
                                Participants[participantName] = new SequenceParticipant(participantName, 
                                    alias, type);
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
                            Messages.Add(new SequenceMessage(from, to, message));
                        }
                        break;
                    case State.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

    [GeneratedRegex(@"(\w+)->>(\w+): (.*)")]
    private static partial Regex MessageRegex();
    [GeneratedRegex(@"participant (\w+)(?: as (\w+))?")]
    private static partial Regex ParticipantRegex();
}
