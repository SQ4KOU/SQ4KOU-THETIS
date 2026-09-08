using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ConstructedNamedTypeSymbol : SubstitutedNamedTypeSymbol
{
	private readonly ImmutableArray<TypeWithAnnotations> _typeArgumentsWithAnnotations;

	private readonly NamedTypeSymbol _constructedFrom;

	public override NamedTypeSymbol ConstructedFrom => _constructedFrom;

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => _typeArgumentsWithAnnotations;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ConstructedNamedTypeSymbol.cs", 135);
		}
	}

	internal ConstructedNamedTypeSymbol(NamedTypeSymbol constructedFrom, ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotations, bool unbound = false, TupleExtraData tupleData = null)
		: base(constructedFrom.ContainingSymbol, new TypeMap(constructedFrom.ContainingType, constructedFrom.OriginalDefinition.TypeParameters, typeArgumentsWithAnnotations), constructedFrom.OriginalDefinition, constructedFrom, unbound, tupleData)
	{
		_typeArgumentsWithAnnotations = typeArgumentsWithAnnotations;
		_constructedFrom = constructedFrom;
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		return new ConstructedNamedTypeSymbol(_constructedFrom, _typeArgumentsWithAnnotations, IsUnboundGenericType, newData);
	}

	internal static bool TypeParametersMatchTypeArguments(ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeWithAnnotations> typeArguments)
	{
		int length = typeParameters.Length;
		for (int i = 0; i < length; i++)
		{
			if (!typeArguments[i].Is(typeParameters[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal sealed override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (ConstructedFrom.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, _typeArgumentsWithAnnotations, owner, ref checkedTypes))
		{
			return true;
		}
		foreach (TypeWithAnnotations typeArgumentsWithAnnotationsNoUseSiteDiagnostic in TypeArgumentsWithAnnotationsNoUseSiteDiagnostics)
		{
			if (Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, typeArgumentsWithAnnotationsNoUseSiteDiagnostic.CustomModifiers, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}
}
