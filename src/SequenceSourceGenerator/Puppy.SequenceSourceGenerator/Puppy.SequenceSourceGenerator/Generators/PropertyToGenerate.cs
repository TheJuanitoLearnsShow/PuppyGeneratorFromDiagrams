namespace Puppy.SequenceSourceGenerator.Generators;

public struct PropertyToGenerate
{
    public string Type;
    public string Name;

    public string ToCode()
    {
        return $"public virtual {Type} {Name} {{ get; set;}}";
    }
}
