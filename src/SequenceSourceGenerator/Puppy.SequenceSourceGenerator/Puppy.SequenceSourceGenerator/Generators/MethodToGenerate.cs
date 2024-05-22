using CaseExtensions;

namespace Puppy.SequenceSourceGenerator.Generators;

public class MethodToGenerate : IEquatable<MethodToGenerate>
{
    private string _name = string.Empty;
    public string ReturnType { get; set; } = "object";

    public string Name
    {
        get => _name;
        set => _name = value.ToPascalCase();
    }

    public List<ParamToGenerate> MethodParams { get; set; } = [];

    public string MethodBody { get; set; } = string.Empty;

    public bool Equals(MethodToGenerate? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ReturnType == other.ReturnType 
               && Name == other.Name 
               && AreParamTypesEqual(MethodParams, other.MethodParams);
    }

    private bool AreParamTypesEqual(List<ParamToGenerate> methodParams, List<ParamToGenerate> otherMethodParams)
    {
        return methodParams.Select((p, idx) => otherMethodParams[idx].Type == p.Type)
            .All(r => true);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MethodToGenerate)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = ReturnType.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ MethodParams.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(MethodToGenerate? left, MethodToGenerate? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(MethodToGenerate? left, MethodToGenerate? right)
    {
        return !Equals(left, right);
    }

    public string ToCode()
    {
        var paramCode = GetParametersCode();
        return $"Task<{ReturnType}> {Name}({paramCode});";
    }
    public string ToOverridableCode()
    {
        var paramCode = GetParametersCode();
        return $"public virtual Task<{ReturnType}> {Name}({paramCode}) {{\n    " +
               MethodBody
               + "\n}";
    }

    public string GetParametersCode()
    {
        return string.Join(", ", MethodParams.Select(p => p.ToCode())).Trim();
    }
}