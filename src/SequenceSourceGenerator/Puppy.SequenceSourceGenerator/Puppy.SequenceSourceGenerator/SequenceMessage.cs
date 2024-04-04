namespace Puppy.SequenceSourceGenerator;

public class SequenceMessage(string from, string to, string message)
{
    public string From { get; } = from;
    public string To { get; } = to;
    public string Message { get; } = message;

    public void Deconstruct(out string from, out string to, out string message)
    {
        from = this.From;
        to = this.To;
        message = this.Message;
    }
}