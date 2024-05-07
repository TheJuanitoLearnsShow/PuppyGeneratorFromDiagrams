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
        const string nameSpace = "testGen";
        var generatorResult = GenerateFromDiagram("sample-flow.md", nameSpace, "flow1");
        var generatorResult2 = GenerateFromDiagram("sample-flow-2.md", nameSpace, "flow2");
        var filesGenerated = generatorResult
            .Merge(generatorResult2)
            .ToFilesToGenerate(nameSpace);

        const string folderName = "generated";
        if (!Directory.Exists(folderName))
        {
            Directory.CreateDirectory(folderName);
        }
        foreach(var f in filesGenerated)
        {
            File.WriteAllText( Path.Combine(folderName, f.ClassName + ".cs"), f.Contents);
        }
        _testOutputHelper.WriteLine(filesGenerated.ToString());
        Assert.Equal(19, filesGenerated.Count());

        Assert.Contains(filesGenerated, r => r.ClassName == "FlowOrchestratorBase.flow1");
        var aliceFile = filesGenerated.First(f => f.ClassName == "IAlice");
        var uniqueMethod = "GreetingResponse HiAlice(HiBobResponse greetingResult);";
        Assert.Equal(aliceFile.Contents.IndexOf(uniqueMethod, StringComparison.Ordinal),
            aliceFile.Contents.LastIndexOf(uniqueMethod, StringComparison.Ordinal));
    }

    private static GeneratorResult GenerateFromDiagram(string mdFilePath, string nameSpace, string flowName)
    {
        var mdFile = File.ReadAllLines(mdFilePath);
        var mermaidDiagram = mdFile.SkipWhile(l => !l.StartsWith("```mermaid"))
            .Skip(1)
            .TakeWhile(l => !l.StartsWith("```"));
        var parser = new SequenceDiagramParser();
        var result = parser.Parse(string.Join(Environment.NewLine, mermaidDiagram));

        var generator = new ParticipantClassGenerators(nameSpace);
        var generatorResult = new GeneratorResult(result, generator, flowName);
        return generatorResult;
    }
    //
}