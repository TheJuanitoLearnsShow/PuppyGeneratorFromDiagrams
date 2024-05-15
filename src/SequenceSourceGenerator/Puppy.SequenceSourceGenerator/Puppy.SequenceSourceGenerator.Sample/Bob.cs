namespace Puppy.SequenceSourceGenerator.Sample;

public partial class Bob : IBOb
{
    public Task<HiAgainResponse> HiAgain()
    {
        Console.WriteLine(nameof(HiAgain));
        return Task.FromResult(new HiAgainResponse());
    }

    public Task<HiBobResponse> HiBob()
    {
        Console.WriteLine(nameof(HiBob));
        return Task.FromResult(new HiBobResponse());
    }

    public Task<HiOneMoreTimeResponse> HiOneMoreTime()
    {
        Console.WriteLine(nameof(HiOneMoreTime));
        return Task.FromResult(new HiOneMoreTimeResponse());
    }

    public Task<OkThatIsFineResponse> OkThatIsFine()
    {
        Console.WriteLine(nameof(OkThatIsFine));
        return Task.FromResult(new OkThatIsFineResponse());
    }
}
