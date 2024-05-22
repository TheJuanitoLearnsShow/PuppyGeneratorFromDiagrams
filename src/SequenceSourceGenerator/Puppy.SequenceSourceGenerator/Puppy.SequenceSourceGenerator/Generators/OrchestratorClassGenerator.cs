using CaseExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Puppy.SequenceSourceGenerator.Generators;

public class OrchestratorClassGenerator
{
    readonly string _nameSpace;

    public OrchestratorClassGenerator(string nameSpace)
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
        var participantNamePascal = orchestrator.ParticipantName.ToPascalCase();
        var participantInterfaceName = participantNamePascal + "Base";
        var fieldsToCalledParticipants = orchestrator.GetParticipantsCalled().Select(pn =>
            participants.FirstOrDefault(p => p.Alias == pn)
        ).Where(p => p != null).ToList();
        var fieldsDeclarationCode = string.Join("\n",
            fieldsToCalledParticipants
                .Select(p => $"\nprotected readonly {p.Type} {p.Alias};")
                .ToList()
        );
        var fieldsAsConstructorParams = string.Join(",",
            fieldsToCalledParticipants
                .Select(p => $"{p.Type} {p.Alias}")
                .ToList()
        );
        var fieldsAsConstructorAssignments = string.Join("\n",
            fieldsToCalledParticipants
                .Select(p => $"this.{p.Alias} = {p.Alias};")
                .ToList()
        );
        //FlowStateGenerator
        var flowStateClassName = participantNamePascal + "StateBase";
        var steps = orchestrator.GetMessagesSent()
            .Aggregate(
                new StepsCalledState(),
                (state, message) => GenerateStepCode(message, state, flowName)
            );
        var callingCode =
            string.Join("\n", steps.CallingCode) + (steps.CurrentOptBlock.IsEmpty ? string.Empty : "\n}\n");
        var stepMethods =
            string.Join("\n", steps.Methods.Select(m => m.ToOverridableCode()));
        var flowStateClassGenerator = new FlowStateGenerator(flowStateClassName, steps.Methods.Select(m => 
            new PropertyToGenerate() { 
                Name = $"{m.Name}Result",
                Type = $"{m.ReturnType}"
            }
        ).ToList());
        var flowParametersCode = BuildFlowParametersCode(steps, flowStateClassName);
        var mainClass = $"""
                         namespace {_nameSpace};
                         using System.Collections.Generic;

                         {flowStateClassGenerator.ToCode()}

                         public partial class {participantInterfaceName}
                         """
                        + "\n{\n" 
                        + $"public {participantInterfaceName}({fieldsAsConstructorParams})"
                        + "\n{\n" 
                        + fieldsAsConstructorAssignments
                        + "\n}\n"
                        + fieldsDeclarationCode
                        + $"\npublic async Task<{flowStateClassName}> Execute{flowName}({flowParametersCode})" 
                        + " {\n"
                        + callingCode
                        + "\nreturn state;"
                        + "\n}\n"
                        + stepMethods
                        + "\n}\n";
        return (participantInterfaceName + '.' + flowName, mainClass);
    }

    /// <summary>
    /// If the first method called by the flow requires a parameter, then the flow itself needs to take
    /// that parameter as a parameter in addition to the state object.
    /// </summary>
    /// <param name="steps"></param>
    /// <param name="flowStateClassName"></param>
    /// <returns></returns>
    private static string BuildFlowParametersCode(StepsCalledState steps, string flowStateClassName)
    {
        var firstMessageParametersCode = steps.Methods.FirstOrDefault()?.GetParametersCode() ;
        var flowParametersCode = string.IsNullOrEmpty(firstMessageParametersCode)
            ? $"{flowStateClassName} state"
            : $"{flowStateClassName} state, {firstMessageParametersCode}";
        return flowParametersCode;
    }

    private StepsCalledState GenerateStepCode(SynchronousMessage msg, 
        StepsCalledState state
        , string flowName)
    {
        var stepNum = state.StepIdx + 1;
        var methodParams = msg.ParameterNames.Select(
                        pName => MapToParamToGenerate(pName, state, msg))
                    .ToList();
        var callingMethodCode = string.IsNullOrEmpty(msg.ParametersCode)
            ? $"return {msg.To}.{msg.MessageName}();"
            : $"return {msg.To}.{msg.MessageName}({msg.ParametersCode});";

        var methodForStep = new MethodToGenerate()
        {
            Name = $"{flowName}Step{stepNum}{msg.MessageName}".ToPascalCase(),
            ReturnType = msg.ResponseType,
            MethodParams = methodParams,
            MethodBody = callingMethodCode
        };
        var resultInfo = string.IsNullOrEmpty(msg.ResultAssignmentCode)
            ? new ParamToGenerate() { Name = $"step{stepNum}", Type = msg.ResponseType }
            : new ParamToGenerate() { Name = $"{msg.ResultAssignmentCode}", Type = msg.ResponseType };
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
        var callingMethodCode = string.IsNullOrEmpty(msg.ParametersCode)
            ? $"await {stepMethodName}();"
            : $"await {stepMethodName}({msg.ParametersCode});";
        var resultStorageCode = string.IsNullOrEmpty(msg.ResultAssignmentCode) ?
            $"\n    var step{stepNum} = {callingMethodCode}\n    state.{stepMethodName}Result = step{stepNum};"
            : $"\n    var {msg.ResultAssignmentCode} = {callingMethodCode}\n    state.{stepMethodName}Result = {msg.ResultAssignmentCode};";
        return optBlockCode + resultStorageCode;
    }

    private ParamToGenerate MapToParamToGenerate(string pName, StepsCalledState state,
        SynchronousMessage msg)
    {
        var resultFromPrevStep = state.ResponsesSoFar.FirstOrDefault(r => r.Name == pName);
        if (resultFromPrevStep.Name != null)
        {
            return resultFromPrevStep;
        }

        var paramFromPrevStep = state.Methods
            .Select(m => m.MethodParams.FirstOrDefault(p => p.Name == pName))
            .FirstOrDefault(r => r.Name == pName);
            
        return paramFromPrevStep.Name == null ? 
            new ParamToGenerate() { Name = pName, Type = msg.RequestType} 
            : paramFromPrevStep;
    }

    
    public (string ClassName, string Contents) GenerateCodeForOrchestrator(ParsedDiagram diagram, string flowName)
    {
        var participants = diagram.Participants
            .Select(p => p.Value)
            .ToList();
        return GenerateCodeForOrchestrator(participants, flowName);
    }
}