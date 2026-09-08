using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeMap : AbstractTypeParameterMap
{
	public static readonly Func<TypeWithAnnotations, TypeSymbol> AsTypeSymbol = (TypeWithAnnotations t) => t.Type;

	private static readonly SmallDictionary<TypeParameterSymbol, TypeWithAnnotations> s_emptyDictionary = new SmallDictionary<TypeParameterSymbol, TypeWithAnnotations>(ReferenceEqualityComparer.Instance);

	private static readonly TypeMap s_emptyTypeMap = new TypeMap();

	public static TypeMap Empty => s_emptyTypeMap;

	internal static ImmutableArray<TypeWithAnnotations> TypeParametersAsTypeSymbolsWithAnnotations(ImmutableArray<TypeParameterSymbol> typeParameters)
	{
		return typeParameters.SelectAsArray((TypeParameterSymbol tp) => TypeWithAnnotations.Create(tp));
	}

	internal static ImmutableArray<TypeWithAnnotations> TypeParametersAsTypeSymbolsWithIgnoredAnnotations(ImmutableArray<TypeParameterSymbol> typeParameters)
	{
		return typeParameters.SelectAsArray((TypeParameterSymbol tp) => TypeWithAnnotations.Create(tp, NullableAnnotation.Ignored));
	}

	internal static ImmutableArray<TypeSymbol> AsTypeSymbols(ImmutableArray<TypeWithAnnotations> typesOpt)
	{
		if (!typesOpt.IsDefault)
		{
			return typesOpt.SelectAsArray(AsTypeSymbol);
		}
		return default(ImmutableArray<TypeSymbol>);
	}

	internal TypeMap(ImmutableArray<TypeParameterSymbol> from, ImmutableArray<TypeWithAnnotations> to, bool allowAlpha = false)
		: base(ConstructMapping(from, to))
	{
	}

	internal TypeMap(ImmutableArray<TypeParameterSymbol> from, ImmutableArray<TypeParameterSymbol> to, bool allowAlpha = false)
		: this(from, TypeParametersAsTypeSymbolsWithAnnotations(to), allowAlpha)
	{
	}

	private TypeMap(SmallDictionary<TypeParameterSymbol, TypeWithAnnotations> mapping)
		: base(new SmallDictionary<TypeParameterSymbol, TypeWithAnnotations>(mapping, ReferenceEqualityComparer.Instance))
	{
	}

	private static SmallDictionary<TypeParameterSymbol, TypeWithAnnotations> ForType(NamedTypeSymbol containingType)
	{
		if (!(containingType is SubstitutedNamedTypeSymbol substitutedNamedTypeSymbol))
		{
			return new SmallDictionary<TypeParameterSymbol, TypeWithAnnotations>(ReferenceEqualityComparer.Instance);
		}
		return new SmallDictionary<TypeParameterSymbol, TypeWithAnnotations>(substitutedNamedTypeSymbol.TypeSubstitution.Mapping, ReferenceEqualityComparer.Instance);
	}

	internal TypeMap(NamedTypeSymbol containingType, ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeWithAnnotations> typeArguments)
		: base(ForType(containingType))
	{
		for (int i = 0; i < typeParameters.Length; i++)
		{
			TypeParameterSymbol typeParameterSymbol = typeParameters[i];
			TypeWithAnnotations value = typeArguments[i];
			if (!value.Is(typeParameterSymbol))
			{
				Mapping.Add(typeParameterSymbol, value);
			}
		}
	}

	private TypeMap()
		: base(s_emptyDictionary)
	{
	}

	internal TypeMap WithAlphaRename(ImmutableArray<TypeParameterSymbol> oldTypeParameters, Symbol newOwner, bool propagateAttributes, out ImmutableArray<TypeParameterSymbol> newTypeParameters)
	{
		if (oldTypeParameters.Length == 0)
		{
			newTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
			return this;
		}
		TypeMap typeMap = new TypeMap(Mapping);
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		bool flag = (object)oldTypeParameters[0].ContainingSymbol.OriginalDefinition != newOwner.OriginalDefinition;
		int num = 0;
		foreach (TypeParameterSymbol item in oldTypeParameters)
		{
			TypeParameterSymbol typeParameterSymbol = (flag ? ((SubstitutedTypeParameterSymbolBase)new SynthesizedTypeParameterSymbol(newOwner, typeMap, item, num, propagateAttributes)) : ((SubstitutedTypeParameterSymbolBase)new SubstitutedTypeParameterSymbol(newOwner, typeMap, item, num)));
			typeMap.Mapping.Add(item, TypeWithAnnotations.Create(typeParameterSymbol));
			instance.Add(typeParameterSymbol);
			num++;
		}
		newTypeParameters = instance.ToImmutableAndFree();
		return typeMap;
	}

	internal TypeMap WithAlphaRename(NamedTypeSymbol oldOwner, NamedTypeSymbol newOwner, out ImmutableArray<TypeParameterSymbol> newTypeParameters)
	{
		return WithAlphaRename(oldOwner.OriginalDefinition.TypeParameters, newOwner, propagateAttributes: false, out newTypeParameters);
	}

	internal TypeMap WithAlphaRename(MethodSymbol oldOwner, Symbol newOwner, bool propagateAttributes, out ImmutableArray<TypeParameterSymbol> newTypeParameters)
	{
		return WithAlphaRename(oldOwner.OriginalDefinition.TypeParameters, newOwner, propagateAttributes, out newTypeParameters);
	}

	internal static ImmutableArray<TypeParameterSymbol> ConcatMethodTypeParameters(MethodSymbol oldOwner, MethodSymbol stopAt)
	{
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		while (oldOwner != null && oldOwner != stopAt)
		{
			ImmutableArray<TypeParameterSymbol> typeParameters = oldOwner.OriginalDefinition.TypeParameters;
			for (int num = typeParameters.Length - 1; num >= 0; num--)
			{
				instance.Add(typeParameters[num]);
			}
			oldOwner = oldOwner.ContainingSymbol.OriginalDefinition as MethodSymbol;
		}
		instance.ReverseContents();
		return instance.ToImmutableAndFree();
	}

	private static SmallDictionary<TypeParameterSymbol, TypeWithAnnotations> ConstructMapping(ImmutableArray<TypeParameterSymbol> from, ImmutableArray<TypeWithAnnotations> to)
	{
		SmallDictionary<TypeParameterSymbol, TypeWithAnnotations> smallDictionary = new SmallDictionary<TypeParameterSymbol, TypeWithAnnotations>(ReferenceEqualityComparer.Instance);
		for (int i = 0; i < from.Length; i++)
		{
			TypeParameterSymbol typeParameterSymbol = from[i];
			TypeWithAnnotations value = to[i];
			if (!value.Is(typeParameterSymbol))
			{
				smallDictionary.Add(typeParameterSymbol, value);
			}
		}
		return smallDictionary;
	}
}
