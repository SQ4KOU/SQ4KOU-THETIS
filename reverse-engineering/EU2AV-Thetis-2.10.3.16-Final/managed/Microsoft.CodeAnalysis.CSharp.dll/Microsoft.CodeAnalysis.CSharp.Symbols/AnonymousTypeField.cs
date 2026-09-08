using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal readonly struct AnonymousTypeField
{
	public readonly string Name;

	public readonly Location Location;

	public readonly TypeWithAnnotations TypeWithAnnotations;

	public readonly RefKind RefKind;

	public readonly ScopedKind Scope;

	public readonly ConstantValue? DefaultValue;

	public readonly bool IsParams;

	public readonly bool HasUnscopedRefAttribute;

	public TypeSymbol Type => TypeWithAnnotations.Type;

	public AnonymousTypeField(string name, Location location, TypeWithAnnotations typeWithAnnotations, RefKind refKind, ScopedKind scope, ConstantValue? defaultValue = null, bool isParams = false, bool hasUnscopedRefAttribute = false)
	{
		Name = name;
		Location = location;
		TypeWithAnnotations = typeWithAnnotations;
		RefKind = refKind;
		Scope = scope;
		DefaultValue = defaultValue;
		IsParams = isParams;
		HasUnscopedRefAttribute = hasUnscopedRefAttribute;
	}

	public AnonymousTypeField WithType(TypeWithAnnotations type)
	{
		return new AnonymousTypeField(Name, Location, type, RefKind, Scope, DefaultValue, IsParams, HasUnscopedRefAttribute);
	}

	internal static bool Equals(in AnonymousTypeField x, in AnonymousTypeField y, TypeCompareKind comparison)
	{
		if (x.TypeWithAnnotations.Equals(y.TypeWithAnnotations, comparison) && x.RefKind == y.RefKind && x.Scope == y.Scope && x.DefaultValue == y.DefaultValue && x.IsParams == y.IsParams)
		{
			return x.HasUnscopedRefAttribute == y.HasUnscopedRefAttribute;
		}
		return false;
	}

	[Conditional("DEBUG")]
	internal void AssertIsGood()
	{
	}
}
