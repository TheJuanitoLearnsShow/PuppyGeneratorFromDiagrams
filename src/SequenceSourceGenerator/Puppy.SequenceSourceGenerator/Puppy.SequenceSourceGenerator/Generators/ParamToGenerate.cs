namespace Puppy.SequenceSourceGenerator.Generators;

public struct ParamToGenerate
{
    public string Type;
    public string Name;

    public string ToCode()
    {
        return $"{Type} {Name}";
    }
}