﻿using System.Text.RegularExpressions;
using CaseExtensions;

namespace Puppy.SequenceSourceGenerator;

public class SequenceDiagramParser
{
    private static Regex MessageRegex => new(@"(\w+)->>(\w+): (.*)");

    private static Regex ReplyMessageRegex => new(@"(\w+)-->>(\w+): (.*)");

    private static Regex ParticipantRegex => new(@"participant (\w+)(?: as (\w+))?");

    private static Regex OptRegex => new(@"opt ");
    private static Regex AltRegex => new(@"alt ");

    public ParsedDiagram Parse(string input)
    {
        var participants = new Dictionary<string, SequenceParticipant>();
        var messages = new List<SequenceMessage>();

        var lines = input.Split('\n');
        var state = State.None;
        var currentOptBlock = OptBlock.Empty;
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("participant"))
            {
                state = State.Participant;
            }
            else if (trimmedLine.Contains("-->>")) // more specific than ->>
            {
                state = State.MessageReply;
            }
            else if (trimmedLine.Contains("->>"))
            {
                state = State.Message;
            }
            else if (trimmedLine.StartsWith("opt "))
            {
                state = State.Opt;
            }
            else if (trimmedLine.StartsWith("alt "))
            {
                state = State.Alt;
            }
            else if (trimmedLine.StartsWith("else "))
            {
                state = State.Else;
            }
            else if (trimmedLine.StartsWith("end"))
            {
                state = State.EndOpt;
            }
            else
            {
                state = State.None;
            }

            switch (state)
            {
                case State.Participant:
                    var participantMatch = ParticipantRegex.Match(line);
                    if (participantMatch.Success)
                    {
                        var participantName = participantMatch.Groups[1].Value;
                        var participantParts = line.Split([" as "], StringSplitOptions.None );
                        if (participantParts.Length == 1)
                        {
                            participants[participantName] = new SequenceParticipant(participantName,
                                participantName, participantName.ToPascalCase());
                        }
                        else
                        {
                            var aliasParts = participantParts.Last().Split(':');

                            var alias = aliasParts.First().Trim();
                            var type = (aliasParts.LastOrDefault()?.Trim() ?? string.Empty).ToPascalCase();
                            participants[participantName] = new SequenceParticipant(
                                alias, participantName, type);
                        }
                    }

                    break;
                case State.Message:
                    var messageMatch = MessageRegex.Match(line);
                    if (messageMatch.Success)
                    {
                        var from = messageMatch.Groups[1].Value;
                        var to = messageMatch.Groups[2].Value;
                        var message = messageMatch.Groups[3].Value;
                        var msg = new SynchronousMessage(message, from, to);
                        msg.OptBlock = currentOptBlock;
                        participants[from].AddCallMade(msg);
                        participants[to].AddMessage(msg);
                        messages.Add(new SequenceMessage(from, to, message));
                    }

                    break;
                case State.MessageReply:
                    var messageReplyMatch = ReplyMessageRegex.Match(line);
                    var toReply = messageReplyMatch.Groups[2].Value;
                    var messageReply = messageReplyMatch.Groups[3].Value;
                    participants[toReply].SetResponseToLastSyncMessageSent(messageReply);
                    break;
                case State.Opt:
                    var condition = line.Trim().Substring(4).Trim();
                    currentOptBlock = new OptBlock() { Condition = condition };
                    break;
                case State.Alt:
                    var conditionAlt = line.Trim().Substring(4).Trim();
                    currentOptBlock = new OptBlock() { Condition = conditionAlt };
                    break;
                case State.Else:
                    var conditionElse = line.Trim().Substring(5).Trim();
                    currentOptBlock = new OptBlock() { Condition = conditionElse, IsElse = true };
                    break;
                case State.EndOpt:
                    currentOptBlock = OptBlock.Empty;
                    break;
                case State.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new ParsedDiagram(participants, messages);
    }

    private enum State
    {
        None,
        Participant,
        Message,
        MessageReply,
        Opt,
        EndOpt,
        Alt,
        Else
    }
}

public class OptBlock
{
    public string Condition { get; set; } = string.Empty;
    public bool IsEmpty => string.IsNullOrEmpty(Condition);

    public bool IsElse { get; internal set; }

    public static OptBlock Empty = new();
}