using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedParameterSymbol : WrappedParameterSymbol
{
	private object _mapOrType;

	private readonly Symbol _containingSymbol;

	public override ParameterSymbol OriginalDefinition => _underlyingParameter.OriginalDefinition;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override TypeWithAnnotations TypeWithAnnotations
	{
		get
		{
			object mapOrType = _mapOrType;
			if (mapOrType is TypeWithAnnotations)
			{
				return (TypeWithAnnotations)mapOrType;
			}
			TypeWithAnnotations typeWithAnnotations = ((TypeMap)mapOrType).SubstituteType(_underlyingParameter.TypeWithAnnotations);
			if (typeWithAnnotations.CustomModifiers.IsEmpty && _underlyingParameter.TypeWithAnnotations.CustomModifiers.IsEmpty && _underlyingParameter.RefCustomModifiers.IsEmpty)
			{
				_mapOrType = typeWithAnnotations;
			}
			return typeWithAnnotations;
		}
	}

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => _underlyingParameter.InterpolatedStringHandlerArgumentIndexes;

	internal override bool HasInterpolatedStringHandlerArgumentError => _underlyingParameter.HasInterpolatedStringHandlerArgumentError;

	internal override bool HasEnumeratorCancellationAttribute => _underlyingParameter.HasEnumeratorCancellationAttribute;

	public override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			if (!(_mapOrType is TypeMap typeMap))
			{
				return _underlyingParameter.RefCustomModifiers;
			}
			return typeMap.SubstituteCustomModifiers(_underlyingParameter.RefCustomModifiers);
		}
	}

	internal override bool IsCallerLineNumber => _underlyingParameter.IsCallerLineNumber;

	internal override bool IsCallerFilePath => _underlyingParameter.IsCallerFilePath;

	internal override bool IsCallerMemberName => _underlyingParameter.IsCallerMemberName;

	internal override int CallerArgumentExpressionParameterIndex => _underlyingParameter.CallerArgumentExpressionParameterIndex;

	internal SubstitutedParameterSymbol(MethodSymbol containingSymbol, TypeMap map, ParameterSymbol originalParameter)
		: this((Symbol)containingSymbol, map, originalParameter)
	{
	}

	internal SubstitutedParameterSymbol(PropertySymbol containingSymbol, TypeMap map, ParameterSymbol originalParameter)
		: this((Symbol)containingSymbol, map, originalParameter)
	{
	}

	internal SubstitutedParameterSymbol(NamedTypeSymbol containingSymbol, TypeMap map, ParameterSymbol originalParameter)
		: this((Symbol)containingSymbol, map, originalParameter)
	{
	}

	private SubstitutedParameterSymbol(Symbol containingSymbol, TypeMap map, ParameterSymbol originalParameter)
		: base(originalParameter)
	{
		_containingSymbol = containingSymbol;
		_mapOrType = map;
	}

	public sealed override bool Equals(Symbol obj, TypeCompareKind compareKind)
	{
		if ((object)this == obj)
		{
			return true;
		}
		if (obj is SubstitutedParameterSymbol substitutedParameterSymbol && Ordinal == substitutedParameterSymbol.Ordinal)
		{
			return ContainingSymbol.Equals(substitutedParameterSymbol.ContainingSymbol, compareKind);
		}
		return false;
	}

	public sealed override int GetHashCode()
	{
		return Hash.Combine(ContainingSymbol, _underlyingParameter.Ordinal);
	}

	internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedParameterSymbol.cs", 134);
	}
}
