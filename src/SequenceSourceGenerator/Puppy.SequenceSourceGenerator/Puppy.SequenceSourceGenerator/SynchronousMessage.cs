using CaseExtensions;

namespace Puppy.SequenceSourceGenerator
{
    public class SynchronousMessage
    {
        public string From => _from;

        public string To => _to;

        private readonly string _from;
        private readonly string _to;
        public string MessageName { get; }
        public string ResponseName { set; get; }

        public SynchronousMessage(string messageName, string from, string to)
        {
            _from = from.Trim();
            _to = to.Trim();
            MessageName = messageName.Trim().ToPascalCase();
            ResponseName = MessageName;
        }

        internal void SetResponseName(string responseName)
        {
            ResponseName = responseName.Trim().ToPascalCase();
        }
    }
}