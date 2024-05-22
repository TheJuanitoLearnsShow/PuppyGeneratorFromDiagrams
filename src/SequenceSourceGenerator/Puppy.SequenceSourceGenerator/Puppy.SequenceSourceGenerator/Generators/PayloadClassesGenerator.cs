namespace Puppy.SequenceSourceGenerator.Generators;

public class PayloadClassesGenerator
{
    private readonly string _nameSpace;

    public PayloadClassesGenerator(string nameSpace)
    {
        _nameSpace = nameSpace;
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