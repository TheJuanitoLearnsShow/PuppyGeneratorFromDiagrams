namespace Puppy.SequenceSourceGenerator.Tests;

public class SequenceParsingTests
{
    [Fact]
    public void TestParse()
    {
        var mdFile = File.ReadAllLines("sample-flow.md");
        var mermaidDiagram = mdFile.SkipWhile(l => !l.StartsWith("```mermaid"))
            .Skip(1)
            .TakeWhile(l => !l.StartsWith("```"));
        var parser = new SequenceDiagramParser();
        parser.Parse(string.Join(Environment.NewLine, mermaidDiagram));
        Assert.Equal(2, parser.Participants.Count);
        Assert.NotEmpty(parser.Messages);
    }
}