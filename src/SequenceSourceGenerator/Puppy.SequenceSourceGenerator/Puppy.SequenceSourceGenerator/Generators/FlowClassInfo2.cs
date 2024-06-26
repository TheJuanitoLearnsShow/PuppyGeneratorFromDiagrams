﻿using Microsoft.CodeAnalysis;

namespace Puppy.SequenceSourceGenerator.Generators;

internal class FlowClassInfo2 : IEquatable<FlowClassInfo2>
{
    public string? Namespace { get; }
    public string Name { get; }
    public bool HasNameProperty { get; }
    public IReadOnlyList<string> ItemNames { get; }

    public FlowClassInfo2(ITypeSymbol type)
    {
        Namespace = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToString();
        Name = type.Name;
        HasNameProperty = type.GetMembers().Any(m => m.Name == "Name"
                                                     && m is IPropertySymbol property
                                                     && property.Type.SpecialType == SpecialType.System_String);

        ItemNames = GetItemNames(type);
    }

    private static IReadOnlyList<string> GetItemNames(ITypeSymbol type)
    {
        return type.GetMembers()
            .Select(m =>
            {
                if (!m.IsStatic || m.DeclaredAccessibility != Accessibility.Public || m is not IFieldSymbol field)
                    return null;

                return SymbolEqualityComparer.Default.Equals(field.Type, type)
                    ? field.Name
                    : null;
            })
            .Where(name => name is not null)
            .ToList()!;
    }

    public override bool Equals(object? obj)
    {
        return obj is FlowClassInfo2 other && Equals(other);
    }

    public bool Equals(FlowClassInfo2? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Namespace == other.Namespace
               && Name == other.Name
               && HasNameProperty == other.HasNameProperty;
               // && ItemNames.EqualsTo(other.ItemNames);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ HasNameProperty.GetHashCode();
            // hashCode = (hashCode * 397) ^ ItemNames.ComputeHashCode();

            return hashCode;
        }
    }
}