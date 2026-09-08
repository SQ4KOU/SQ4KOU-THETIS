using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedParameterSymbol : SynthesizedParameterSymbolBase
{
	internal sealed override bool IsMetadataIn
	{
		get
		{
			RefKind refKind = RefKind;
			if (refKind - 3 <= RefKind.Ref)
			{
				return true;
			}
			return false;
		}
	}

	internal sealed override bool IsMetadataOut => RefKind == RefKind.Out;

	internal override ConstantValue? DefaultValueFromAttributes => null;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool HasEnumeratorCancellationAttribute => false;

	internal override MarshalPseudoCustomAttributeData? MarshallingInformation => null;

	internal override bool HasUnscopedRefAttribute => false;

	private SynthesizedParameterSymbol(Symbol? container, TypeWithAnnotations type, int ordinal, RefKind refKind, ScopedKind scope, string name)
		: base(container, type, ordinal, refKind, scope, name)
	{
	}

	public static ParameterSymbol Create(Symbol? container, TypeWithAnnotations type, int ordinal, RefKind refKind, string name = "", ScopedKind scope = ScopedKind.None, ConstantValue? defaultValue = null, ImmutableArray<CustomModifier> refCustomModifiers = default(ImmutableArray<CustomModifier>), ParameterSymbol? baseParameterForAttributes = null, bool isParams = false, bool hasUnscopedRefAttribute = false)
	{
		if (!isParams && refCustomModifiers.IsDefaultOrEmpty && (object)baseParameterForAttributes == null && (object)defaultValue == null && !hasUnscopedRefAttribute)
		{
			return new SynthesizedParameterSymbol(container, type, ordinal, refKind, scope, name);
		}
		return new SynthesizedComplexParameterSymbol(container, type, ordinal, refKind, scope, defaultValue, name, refCustomModifiers.NullToEmpty(), baseParameterForAttributes, isParams, hasUnscopedRefAttribute);
	}

	internal static ImmutableArray<ParameterSymbol> DeriveParameters(MethodSymbol sourceMethod, MethodSymbol destinationMethod)
	{
		return sourceMethod.Parameters.SelectAsArray((ParameterSymbol oldParam, MethodSymbol destination) => DeriveParameter(destination, oldParam), destinationMethod);
	}

	internal static ParameterSymbol DeriveParameter(Symbol destination, ParameterSymbol oldParam)
	{
		return Create(destination, oldParam.TypeWithAnnotations, oldParam.Ordinal, oldParam.RefKind, oldParam.Name, oldParam.EffectiveScope, oldParam.ExplicitDefaultConstantValue, oldParam.RefCustomModifiers);
	}
}
