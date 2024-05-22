namespace Puppy.SequenceSourceGenerator.Sample;

public partial class Bob : IBOb
{
    public Task<HiBobGreatSeeingYouResponse> HiBobGreatSeeingYou(HiBobGreatSeeingYouRequest initiatorPayload)
    {
        Console.WriteLine(nameof(HiBobGreatSeeingYou));
        return Task.FromResult(new HiBobGreatSeeingYouResponse());
    }

    public Task<HiAgainResponse> HiAgain()
    {
        Console.WriteLine(nameof(HiAgain));
        return Task.FromResult(new HiAgainResponse());
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
