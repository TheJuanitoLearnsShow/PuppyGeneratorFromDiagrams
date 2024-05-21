namespace Puppy.SequenceSourceGenerator.Generators;

public class FlowStateGenerator
{
    private readonly string _flowStateClassName;

    public FlowStateGenerator(string flowStateClassName, List<PropertyToGenerate> props)
    {
        _flowStateClassName = flowStateClassName;
        this.props = props;
    }

    private IList<PropertyToGenerate> props;

    public string ToCode() {
        var propsCode =
                string.Join("\n", props.Select(m => m.ToCode()));
        return $"""

        public partial class {_flowStateClassName}
        """
    + "\n{\n" 
    + propsCode
    + "\n}\n"
        ;
    }
}
