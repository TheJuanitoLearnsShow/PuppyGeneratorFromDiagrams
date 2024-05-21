namespace Puppy.SequenceSourceGenerator.Sample;

public partial class HiAliceResponse
{
    public bool IsGood => true;
    public override string ToString()
    {
        return $"Ok, {IsGood}!";
    }
}