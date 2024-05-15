﻿namespace Puppy.SequenceSourceGenerator.Sample;

public partial class Alice : IAlice
{
    public Task<HiAliceResponse> HiAlice(HiBobResponse greetingResult)
    {
        Console.WriteLine(nameof(HiAlice));
        return Task.FromResult(new HiAliceResponse());
    }
}
