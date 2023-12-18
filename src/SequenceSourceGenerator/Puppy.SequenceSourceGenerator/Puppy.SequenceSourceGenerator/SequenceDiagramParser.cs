﻿using System.Text.RegularExpressions;

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
                            Participants[participantName] = new SequenceParticipant( participantName);
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
    [GeneratedRegex(@"participant (\w+)")]
    private static partial Regex ParticipantRegex();
}