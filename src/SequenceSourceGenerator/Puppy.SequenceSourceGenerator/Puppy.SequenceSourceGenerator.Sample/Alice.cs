namespace Puppy.SequenceSourceGenerator.Sample;

public partial class Alice : IAlice
{
    public Task<GreetingResponse> HiAlice(HiBobResponse greetingResult)
    {
        throw new NotImplementedException();
    }

    public Task<GoodByeResponse> Bye()
    {
        throw new NotImplementedException();
    }

    public Task<HiAliceResponse> HiAlice(HiBobGreatSeeingYouRequest initiatorPayload, HiBobGreatSeeingYouResponse greetingResult)
    {
        Console.WriteLine(nameof(HiAlice));
        return Task.FromResult(new HiAliceResponse());
    }
}
