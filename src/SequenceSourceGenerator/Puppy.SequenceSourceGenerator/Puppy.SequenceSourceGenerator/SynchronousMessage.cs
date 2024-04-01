using CaseExtensions;

namespace Puppy.SequenceSourceGenerator
{
    public class SynchronousMessage
    {
        public string MessageName { get; }
        public string ResponseName { set; get; }

        public SynchronousMessage(string messageName)
        {
            MessageName = messageName.Trim().ToPascalCase();
            ResponseName = MessageName;
        }

        internal void SetResponseName(string responseName)
        {
            ResponseName = responseName.Trim().ToPascalCase();
        }
    }
}