using Puppy.SequenceSourceGenerator.Generators;

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
        var result = parser.Parse(string.Join(Environment.NewLine, mermaidDiagram));
        Assert.Equal(2, result.Participants.Count);
        Assert.Equal("IAlice", result.Participants.First().Value.Type);
        Assert.NotEmpty(result.Messages);
    }

    [Fact]
    public void TestGenerator()
    {
        var mdFile = File.ReadAllLines("sample-flow.md");
        var mermaidDiagram = mdFile.SkipWhile(l => !l.StartsWith("```mermaid"))
            .Skip(1)
            .TakeWhile(l => !l.StartsWith("```"));
        var parser = new SequenceDiagramParser();
        var result = parser.Parse(string.Join(Environment.NewLine, mermaidDiagram));

        var generator = new ParticipantClassGenerators("testGen");
        var filesGenerated = generator.GenerateCode(result);

        foreach(var f in filesGenerated)
        {
            File.WriteAllText(f.InterfaceName + ".cs", f.Contents);
        }
        Console.WriteLine(filesGenerated);
        Assert.Equal(6, filesGenerated.Count());

    }
    //
}