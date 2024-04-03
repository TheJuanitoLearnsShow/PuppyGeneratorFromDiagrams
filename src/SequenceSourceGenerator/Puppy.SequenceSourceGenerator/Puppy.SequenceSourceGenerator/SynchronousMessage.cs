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
        public string ParametersCode { private set; get; }
        public string ResultAssignmentCode { private set; get; }
        public string RequestType => ResponseName + "Request";
        public string ResponseType => ResponseName + "Response";

        public SynchronousMessage(string messageName, string from, string to)
        {
            _from = from.Trim();
            _to = to.Trim();
            var lastParenthesis = messageName.LastIndexOf('(');
            if (lastParenthesis >= 0)
            {
                var paramsPart = messageName[(lastParenthesis + 1)..].Trim().TrimEnd(')');
                ParametersCode = paramsPart;
                MessageName = messageName[..lastParenthesis].Trim().ToPascalCase();
            }
            else
            {
                ParametersCode = string.Empty;
                MessageName = messageName.Trim().ToPascalCase();
            }
            ResponseName = MessageName;
            ResultAssignmentCode = string.Empty;
        }

        internal void SetResponseName(string responseName)
        {
            var lastParenthesis = responseName.LastIndexOf('(');
            if (lastParenthesis >= 0)
            {
                var paramsPart = responseName[(lastParenthesis + 1)..].Trim().TrimEnd(')');
                ResultAssignmentCode = paramsPart;
            }
            else
            {
                ResultAssignmentCode = string.Empty;
                ResponseName = responseName.Trim().ToPascalCase();
            }

        }
    }
}