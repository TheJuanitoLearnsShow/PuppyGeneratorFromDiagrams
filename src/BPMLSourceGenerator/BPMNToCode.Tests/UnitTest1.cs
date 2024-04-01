namespace BPMNToCode.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var parser = new BpmnParser();
        parser.ParseBpmnFile(@"C:\Users\jptar\Downloads\complex.bpmn");
    }
}