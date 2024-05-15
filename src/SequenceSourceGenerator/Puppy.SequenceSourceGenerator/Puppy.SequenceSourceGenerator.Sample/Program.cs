// See https://aka.ms/new-console-template for more information

using Puppy.SequenceSourceGenerator.Sample;

var orchestrator = new MyFlow(new Alice(), new Bob());
await orchestrator.StartFlow1();
Console.WriteLine("Hello, World!");