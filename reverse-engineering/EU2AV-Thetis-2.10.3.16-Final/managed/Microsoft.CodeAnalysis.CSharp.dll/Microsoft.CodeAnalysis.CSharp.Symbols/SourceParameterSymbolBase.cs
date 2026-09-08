using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceParameterSymbolBase : ParameterSymbol
{
	private readonly Symbol _containingSymbol;

	private readonly ushort _ordinal;

	public sealed override int Ordinal => _ordinal;

	public sealed override Symbol ContainingSymbol => _containingSymbol;

	public sealed override AssemblySymbol ContainingAssembly => _containingSymbol.ContainingAssembly;

	public SourceParameterSymbolBase(Symbol containingSymbol, int ordinal)
	{
		_ordinal = (ushort)ordinal;
		_containingSymbol = containingSymbol;
	}

	public sealed override bool Equals(Symbol obj, TypeCompareKind compareKind)
	{
		if ((object)obj == this)
		{
			return true;
		}
		if (obj is NativeIntegerParameterSymbol nativeIntegerParameterSymbol)
		{
			return nativeIntegerParameterSymbol.Equals(this, compareKind);
		}
		if (obj is SourceParameterSymbolBase sourceParameterSymbolBase && sourceParameterSymbolBase.Ordinal == Ordinal)
		{
			return sourceParameterSymbolBase._containingSymbol.Equals(_containingSymbol, compareKind);
		}
		return false;
	}

	public sealed override int GetHashCode()
	{
		return Hash.Combine(_containingSymbol.GetHashCode(), Ordinal);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		AddSynthesizedAttributes(this, moduleBuilder, ref attributes);
	}

	internal static void AddSynthesizedAttributes(ParameterSymbol parameter, PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		CSharpCompilation declaringCompilation = parameter.DeclaringCompilation;
		if (parameter.IsParamsArray)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_ParamArrayAttribute__ctor));
		}
		else if (parameter.IsParamsCollection)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeParamCollectionAttribute(parameter));
		}
		ConstantValue explicitDefaultConstantValue = parameter.ExplicitDefaultConstantValue;
		if (explicitDefaultConstantValue != null && explicitDefaultConstantValue.SpecialType == SpecialType.System_Decimal && parameter is SourceParameterSymbolBase sourceParameterSymbolBase && sourceParameterSymbolBase.DefaultValueFromAttributes == null)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDecimalConstantAttribute(explicitDefaultConstantValue.DecimalValue));
		}
		TypeWithAnnotations typeWithAnnotations = parameter.TypeWithAnnotations;
		if (typeWithAnnotations.Type.ContainsDynamic())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers.Length + parameter.RefCustomModifiers.Length, parameter.RefKind));
		}
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(typeWithAnnotations.Type))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNativeIntegerAttribute(parameter, typeWithAnnotations.Type));
		}
		if (ParameterHelpers.RequiresScopedRefAttribute(parameter))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeScopedRefAttribute(parameter, parameter.EffectiveScope));
		}
		if (typeWithAnnotations.Type.ContainsTupleNames())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(typeWithAnnotations.Type));
		}
		switch (parameter.RefKind)
		{
		case RefKind.In:
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(parameter));
			break;
		case RefKind.RefReadOnlyParameter:
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeRequiresLocationAttribute(parameter));
			break;
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(parameter))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableAttributeIfNecessary(parameter, parameter.GetNullableContextValue(), typeWithAnnotations));
		}
	}

	internal abstract ParameterSymbol WithCustomModifiersAndParams(TypeSymbol newType, ImmutableArray<CustomModifier> newCustomModifiers, ImmutableArray<CustomModifier> newRefCustomModifiers, bool newIsParams);
}
