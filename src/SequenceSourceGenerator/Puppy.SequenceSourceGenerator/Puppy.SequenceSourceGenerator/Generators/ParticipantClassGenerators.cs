using CaseExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Puppy.SequenceSourceGenerator.Generators
{
    public class ParticipantClassGenerators
    {
        readonly string _nameSpace;

        public ParticipantClassGenerators(string nameSpace)
        {
            _nameSpace = nameSpace;
        }

        private IEnumerable<(string ClassName, string Contents)> GenerateCodeForParticipant(SequenceParticipant participant)
        {
            var participantInterfaceName = participant.Type.ToPascalCase();
            var mainInterface = $"""
namespace {_nameSpace};
using System.Collections.Generic;

public interface {participantInterfaceName} 
"""
+
"\n{\n" +
    string.Join("\n\n",  participant.GetMessages().Select(GenerateMethodDeclarationForMessage))
+
"\n}\n";
            var payloadClasses = participant.GetMessages().SelectMany(GenerateClassesForMessage);
            return payloadClasses.Append((participantInterfaceName, mainInterface));
        }

        private string GenerateMethodDeclarationForMessage(string messageName)
        {
            return $"{messageName}Response {messageName}({messageName}Request request);";
        }

        public IEnumerable<(string InterfaceName, string Contents)> GenerateCode(ParsedDiagram diagram)
        {
            var classes = diagram.Participants.SelectMany(kv => GenerateCodeForParticipant(kv.Value));
            return classes;
        }
        IEnumerable<(string ClassName, string Contents)> GenerateClassesForMessage(string messageName)
        {
            yield return ($"{messageName}Response", GenerateMessagePayloadClass($"{messageName}Response"));
            yield return ($"{messageName}Request", GenerateMessagePayloadClass($"{messageName}Request"));
        }

        private string GenerateMessagePayloadClass(string className)
        {
            return $"""
namespace {_nameSpace};
using System.Collections.Generic;

public partial class {className} 
"""
+
@"
{
}
";
        }
    }
}
