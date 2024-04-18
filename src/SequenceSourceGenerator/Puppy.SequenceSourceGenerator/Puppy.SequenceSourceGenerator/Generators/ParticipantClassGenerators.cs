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

        private (string ClassName, string Contents) GenerateCodeForOrchestrator(
            IReadOnlyCollection<SequenceParticipant> participants, string flowName)
        {
            var orchestrator = participants
                .FirstOrDefault(p =>
                    p.Type.Equals("IOrchestrator", StringComparison.InvariantCultureIgnoreCase));
            if (orchestrator == null) return (string.Empty, string.Empty);
            var participantInterfaceName = orchestrator.ParticipantName.ToPascalCase() + "Base";
            var fieldsToCalledParticipants = orchestrator.GetParticipantsCalled().Select(pn =>
                participants.FirstOrDefault(p => p.Alias == pn)
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
                                + $"\npublic async Task Execute{flowName}()" 
                                + " {\n"
                                + string.Join("\n", flowFunction)
                                + "\n}"
                                + "\n}\n";
            return (participantInterfaceName + '.' + flowName, mainClass);
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

        private IEnumerable<(string ClassName, string Contents)> GenerateCodeForParticipantOld(
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
        
        private (string ClassName, InterfaceToGenerate Contents) GenerateCodeForParticipant(
            SequenceParticipant participant, IReadOnlyCollection<SequenceParticipant> participants)
        {
            var participantInterfaceName = participant.Type.ToPascalCase();
            var mainInterface = new InterfaceToGenerate
            {
                Name = participantInterfaceName,
                Methods = participant.GetMessages()
                    .Select(m =>
                    {
                        var caller = participants.FirstOrDefault(p => p.Alias == m.From);
                        return GenerateMethodDeclarationForMessage(m, caller);
                    })
                    .ToList()
            };

            return (participantInterfaceName, mainInterface);
        }

        private MethodToGenerate GenerateMethodDeclarationForMessage(SynchronousMessage msg, SequenceParticipant? caller)
        {
            if (caller == null || string.IsNullOrEmpty(msg.ParametersCode))
            {
                return new MethodToGenerate()
                {
                    Name = msg.MessageName,
                    ReturnType = msg.ResponseType,
                    MethodParams =
                    [
                        new ParamToGenerate()
                        {
                            Type = $"{msg.MessageName}Request", Name = "request"
                        }
                    ]
                };
            }

            var paramsForMethod = 
                msg.ParametersCode
                    .Split(',')
                    .Select(p => caller.GetVarDeclarationFor(p.Trim()))
                    .ToList();
            return new MethodToGenerate()
            {
                Name = msg.MessageName,
                ReturnType = msg.ResponseType,
                MethodParams = paramsForMethod
            };
        }

        public IEnumerable<(string ClassName, InterfaceToGenerate Contents)> GenerateCodeForInterfaces(ParsedDiagram diagram)
        {
            var participants = diagram.Participants
                .Select(p => p.Value)
                .ToList();
            var classes = participants
                .Select(p =>
                    GenerateCodeForParticipant(p, participants));
            return classes;
        }
        public IEnumerable<(string InterfaceName, string Contents)> GenerateCodeForPayloads(ParsedDiagram diagram)
        {
            var participants = diagram.Participants
                .Select(p => p.Value)
                .ToList();
            
            var payloadClasses = participants
                .SelectMany(p => p.GetMessages()
                    .SelectMany(GenerateClassesForMessage)
                );
            return payloadClasses;
        }
        public (string ClassName, string Contents) GenerateCodeForOrchestrator(ParsedDiagram diagram, string flowName)
        {
            var participants = diagram.Participants
                .Select(p => p.Value)
                .ToList();
            return GenerateCodeForOrchestrator(participants, flowName);
        }
        
        private IEnumerable<(string ClassName, string Contents)> GenerateClassesForMessage(SynchronousMessage msg)
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
