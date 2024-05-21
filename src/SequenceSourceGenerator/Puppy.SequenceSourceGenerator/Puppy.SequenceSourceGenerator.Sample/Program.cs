// See https://aka.ms/new-console-template for more information

using Puppy.SequenceSourceGenerator.Sample;

var orchestrator = new MyFlow(new Alice(), new Bob());
var finalFlowState = await orchestrator.StartFlow1();
Console.WriteLine("Hello " + finalFlowState.Flow1Step3HiAliceResult);