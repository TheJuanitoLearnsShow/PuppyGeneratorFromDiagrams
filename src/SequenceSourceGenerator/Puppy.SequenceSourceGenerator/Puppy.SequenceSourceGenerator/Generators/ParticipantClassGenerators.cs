using CaseExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Puppy.SequenceSourceGenerator.Generators
{
    public class ParticipantClassGenerators
    {
        readonly string _nameSpace;

        public ParticipantClassGenerators(string nameSpace)
        {
            _nameSpace = nameSpace;
        }

        private (string ClassName, string Contents) GenerateCodeForOrchestrator(IReadOnlyCollection<SequenceParticipant> participants)
        {
            var orchestrator = participants
                .FirstOrDefault(p =>
                    p.Type.Equals("IOrchestrator", StringComparison.InvariantCultureIgnoreCase));
            if (orchestrator == null) return (string.Empty, string.Empty);
            var participantInterfaceName = orchestrator.ParticipantName.ToPascalCase() + "Base";
            var fieldsToCalledParticipants = orchestrator.GetParticipantsCalled().Select(pn =>
                {
                    var pcalled = participants.FirstOrDefault(p => p.Alias == pn);
                    return pcalled;
                }
            ).Where(p => p != null)
            .Select(p => $"\nprivate readonly {p.Type} {p.Alias};")
            .ToList();
            var flowFunction = orchestrator.GetMessagesSent()
                .Select(GenerateStepCode)
                .ToImmutableList();
            var mainClass = $"""
                                 namespace {_nameSpace};
                                 using System.Collections.Generic;

                                 public partial class {participantInterfaceName}
                                 """
                                + "\n{\n" 
                                + string.Join("\n", fieldsToCalledParticipants)
                                + "\npublic async Task ExecuteFlow() {\n"
                                + string.Join("\n", flowFunction)
                                + "\n}"
                                + "\n}\n";
            var payloadClasses = orchestrator.GetMessages().SelectMany(GenerateClassesForMessage);
            return (participantInterfaceName, mainClass);
        }

        private string GenerateStepCode(SynchronousMessage msg, int stepIdx)
        {
            var resultStorageCode = string.IsNullOrEmpty(msg.ResultAssignmentCode) ?
                $"\nvar step{stepIdx + 1} = "
                : $"\nvar {msg.ResultAssignmentCode} = ";
            var callingMethodCode = string.IsNullOrEmpty(msg.ParametersCode)
                ? $"{msg.To}.{msg.MessageName}();"
                : $"{msg.To}.{msg.MessageName}({msg.ParametersCode});";
            return resultStorageCode + callingMethodCode;
        }

        private IEnumerable<(string ClassName, string Contents)> GenerateCodeForParticipant(
            SequenceParticipant participant, IReadOnlyCollection<SequenceParticipant> participants)
        {
            var participantInterfaceName = participant.Type.ToPascalCase();
            var mainInterface = $"""
namespace {_nameSpace};
using System.Collections.Generic;

public interface {participantInterfaceName} 
"""
+
"\n{\n" +
    string.Join("\n\n",  participant.GetMessages().Select(m =>
    {
        var caller = participants.FirstOrDefault(p => p.Alias == m.From);
        return GenerateMethodDeclarationForMessage(m,caller);
    }))
+
"\n}\n";
            var payloadClasses = participant.GetMessages().SelectMany(GenerateClassesForMessage);
            return payloadClasses.Append((participantInterfaceName, mainInterface));
        }

        private string GenerateMethodDeclarationForMessage(SynchronousMessage msg, SequenceParticipant? caller)
        {
            if (caller == null || string.IsNullOrEmpty(msg.ParametersCode))
            {
                return $"{msg.ResponseType} {msg.MessageName}({msg.MessageName}Request request);";
            }

            var paramsForMethod = 
                msg.ParametersCode
                    .Split(',')
                    .Select(p => caller.GetVarDeclarationFor(p.Trim()))
                    .ToArray();
            return $"{msg.ResponseType} {msg.MessageName}({string.Join(",", paramsForMethod) });";
        }

        public IEnumerable<(string InterfaceName, string Contents)> GenerateCode(ParsedDiagram diagram)
        {
            var participants = diagram.Participants.Select(p => p.Value).ToList();
            var classes = participants.SelectMany(p => GenerateCodeForParticipant(p, participants));
            return classes.Append(GenerateCodeForOrchestrator(participants));
        }
        
        IEnumerable<(string ClassName, string Contents)> GenerateClassesForMessage(SynchronousMessage msg)
        {
            yield return ($"{msg.ResponseType}", GenerateMessagePayloadClass($"{msg.ResponseType}"));
            yield return ($"{msg.RequestType}", GenerateMessagePayloadClass($"{msg.RequestType}"));
        }

        private string GenerateMessagePayloadClass(string className)
        {
            return $"""
namespace {_nameSpace};
using System.Collections.Generic;

public partial class {className} 
"""
+
@"
{
}
";
        }
    }
}
