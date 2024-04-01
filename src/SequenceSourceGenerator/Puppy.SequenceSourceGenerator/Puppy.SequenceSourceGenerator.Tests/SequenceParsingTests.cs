using Puppy.SequenceSourceGenerator.Generators;
using Xunit.Abstractions;

namespace Puppy.SequenceSourceGenerator.Tests;

public class SequenceParsingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SequenceParsingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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
        var filesGenerated = generator.GenerateCode(result).ToList();

        var folderName = "generated";
        if (!Directory.Exists(folderName))
        {
            Directory.CreateDirectory(folderName);
        }
        foreach(var f in filesGenerated)
        {
            File.WriteAllText( Path.Combine(folderName, f.InterfaceName + ".cs"), f.Contents);
        }
        _testOutputHelper.WriteLine(filesGenerated.ToString());
        Assert.Equal(13, filesGenerated.Count());

    }
    //
}