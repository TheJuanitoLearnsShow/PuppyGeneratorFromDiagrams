using CaseExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
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
            var steps = orchestrator.GetMessagesSent()
                    .Aggregate(
                        new StepsCalledState(),
                        (state, message) => GenerateStepCode(message, state, flowName)
                    );
            var callingCode =
                string.Join("\n", steps.CallingCode) + (steps.CurrentOptBlock.IsEmpty ? string.Empty : "\n}\n");
            var stepMethods =
                string.Join("\n", steps.Methods.Select(m => m.ToOverridableCode()));
            var mainClass = $"""
                                 namespace {_nameSpace};
                                 using System.Collections.Generic;

                                 public partial class {participantInterfaceName}
                                 """
                                + "\n{\n" 
                                + string.Join("\n", fieldsToCalledParticipants)
                                + $"\npublic async Task Execute{flowName}()" 
                                + " {\n"
                                + callingCode
                                + "\n}\n"
                                + stepMethods
                                + "\n}\n";
            return (participantInterfaceName + '.' + flowName, mainClass);
        }

        private StepsCalledState GenerateStepCode(SynchronousMessage msg, 
            StepsCalledState state
            , string flowName)
        {
            var stepNum = state.StepIdx + 1;
            var methodParams =
                string.IsNullOrEmpty(msg.ParametersCode)
                    ? []
                    : msg.ParametersCode.Split(',').Select(
                        pName => MapToParamToGenerate(pName, state.ResponsesSoFar))
                        .ToList();
            var callingMethodCode = string.IsNullOrEmpty(msg.ParametersCode)
                ? $"return {msg.To}.{msg.MessageName}();"
                : $"return {msg.To}.{msg.MessageName}({msg.ParametersCode});";

            var methodForStep = new MethodToGenerate()
            {
                Name = $"{flowName}Step{stepNum}",
                ReturnType = msg.ResponseType,
                MethodParams = methodParams,
                MethodBody = callingMethodCode
            };
            var resultInfo = string.IsNullOrEmpty(msg.ResultAssignmentCode)
                ? new ParamToGenerate() { Name = $"step{stepNum}", Type = msg.ResponseType }
                : new ParamToGenerate() { Name = $"{msg.ResultAssignmentCode}", Type = msg.ResponseType };
                // : $"\nvar {msg.ResultAssignmentCode} = ";
            state.Methods.Add(methodForStep);
            state.ResponsesSoFar.Add(resultInfo);
            state.CallingCode.Add(GenerateStepCallingCode(msg, stepNum, methodForStep.Name, state));
            state.StepIdx++;
            return state;
        }
        
        
        private string GenerateStepCallingCode(SynchronousMessage msg, int stepNum, string stepMethodName,
            StepsCalledState state)
        {
            var optBlockCode = string.Empty;
            if (state.CurrentOptBlock.Condition != msg.OptBlock.Condition)
            {
                if (msg.OptBlock.IsElse) {
                    optBlockCode = $"\n}}\nelse if ({msg.OptBlock.Condition}) {{\n";
                }
                else {
                    optBlockCode = msg.OptBlock.IsEmpty ? "\n}\n" : 
                        $"\nif ({msg.OptBlock.Condition}) {{\n";
                }
                state.CurrentOptBlock = msg.OptBlock;
            }
            var resultStorageCode = string.IsNullOrEmpty(msg.ResultAssignmentCode) ?
                $"\n    var step{stepNum} = "
                : $"\n    var {msg.ResultAssignmentCode} = ";
            var callingMethodCode = string.IsNullOrEmpty(msg.ParametersCode)
                ? $"await {stepMethodName}();"
                : $"await {stepMethodName}({msg.ParametersCode});";
            return optBlockCode + resultStorageCode + callingMethodCode;
        }

        private ParamToGenerate MapToParamToGenerate(string pName, List<ParamToGenerate> stateResponsesSoFar)
        {
            var resultFromPrevStep = stateResponsesSoFar.FirstOrDefault(r => r.Name == pName);
            return resultFromPrevStep;
        }

        private IEnumerable<(string ClassName, string Contents)> GenerateCodeForParticipantOld(
            SequenceParticipant participant, IReadOnlyCollection<SequenceParticipant> participants)
        {
            var participantInterfaceName = participant.Type.ToPascalCase();
            var mainInterface = $"""
namespace {_nameSpace};
using System.Collections.Generic;

public partial interface {participantInterfaceName} 
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
