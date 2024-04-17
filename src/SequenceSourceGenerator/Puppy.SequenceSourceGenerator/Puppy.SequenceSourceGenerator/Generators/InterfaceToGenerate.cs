namespace Puppy.SequenceSourceGenerator.Generators;

public class InterfaceToGenerate
{
    public string Name { get; set; } = string.Empty;
    public List<MethodToGenerate> Methods { get; set; } = new ();

    public InterfaceToGenerate Merge(InterfaceToGenerate otherParticipant)
    {
        var newMethods = otherParticipant.Methods.Where(otherM =>
            Methods.Exists(mym => mym.Equals(otherM)));
        return new InterfaceToGenerate()
        {
            Name = Name,
            Methods = Methods.Concat(otherParticipant.Methods).Distinct().ToList()
        };
    }

    public object ToCode()
    {
        throw new NotImplementedException();
    }
}