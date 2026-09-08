using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BestTypeInferrer
{
	public static NullableAnnotation GetNullableAnnotation(ArrayBuilder<TypeWithAnnotations> types)
	{
		NullableAnnotation nullableAnnotation = NullableAnnotation.NotAnnotated;
		foreach (TypeWithAnnotations type in types)
		{
			nullableAnnotation = nullableAnnotation.Join(type.NullableAnnotation);
		}
		return nullableAnnotation;
	}

	public static NullableFlowState GetNullableState(ArrayBuilder<TypeWithState> types)
	{
		NullableFlowState nullableFlowState = NullableFlowState.NotNull;
		foreach (TypeWithState type in types)
		{
			nullableFlowState = nullableFlowState.Join(type.State);
		}
		return nullableFlowState;
	}

	public static TypeSymbol? InferBestType(ImmutableArray<BoundExpression> exprs, ConversionsBase conversions, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool inferredFromFunctionType)
	{
		HashSet<TypeSymbol> hashSet = new HashSet<TypeSymbol>(conversions.IncludeNullability ? Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything : Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.IgnoringNullable);
		foreach (BoundExpression item in exprs)
		{
			TypeSymbol typeOrFunctionType = item.GetTypeOrFunctionType();
			if ((object)typeOrFunctionType == null)
			{
				continue;
			}
			if (typeOrFunctionType.ContainsErrorType())
			{
				if (typeOrFunctionType is FunctionTypeSymbol functionTypeSymbol)
				{
					inferredFromFunctionType = true;
					return functionTypeSymbol.GetInternalDelegateType();
				}
				inferredFromFunctionType = false;
				return typeOrFunctionType;
			}
			hashSet.Add(typeOrFunctionType);
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(hashSet.Count);
		instance.AddRange(hashSet);
		TypeSymbol bestType = GetBestType(instance, conversions, ref useSiteInfo);
		instance.Free();
		if (bestType is FunctionTypeSymbol functionTypeSymbol2)
		{
			bestType = functionTypeSymbol2.GetInternalDelegateType();
			inferredFromFunctionType = (object)bestType != null;
			return bestType;
		}
		inferredFromFunctionType = false;
		return bestType;
	}

	public static TypeSymbol? InferBestTypeForConditionalOperator(BoundExpression expr1, BoundExpression expr2, Conversions conversions, out bool hadMultipleCandidates, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		try
		{
			Conversions conversions2 = conversions.WithNullability(includeNullability: false);
			TypeSymbol type = expr1.Type;
			if ((object)type != null)
			{
				if (type.IsErrorType())
				{
					hadMultipleCandidates = false;
					return type;
				}
				if (conversions2.ClassifyImplicitConversionFromExpression(expr2, type, ref useSiteInfo).Exists)
				{
					instance.Add(type);
				}
			}
			TypeSymbol type2 = expr2.Type;
			if ((object)type2 != null)
			{
				if (type2.IsErrorType())
				{
					hadMultipleCandidates = false;
					return type2;
				}
				if (conversions2.ClassifyImplicitConversionFromExpression(expr1, type2, ref useSiteInfo).Exists)
				{
					instance.Add(type2);
				}
			}
			hadMultipleCandidates = instance.Count > 1;
			return GetBestType(instance, conversions, ref useSiteInfo);
		}
		finally
		{
			instance.Free();
		}
	}

	internal static TypeSymbol? GetBestType(ArrayBuilder<TypeSymbol> types, ConversionsBase conversions, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		switch (types.Count)
		{
		case 0:
			return null;
		case 1:
			return checkType(types[0]);
		default:
		{
			TypeSymbol typeSymbol = null;
			int num = -1;
			for (int i = 0; i < types.Count; i++)
			{
				TypeSymbol typeSymbol2 = checkType(types[i]);
				if ((object)typeSymbol2 == null)
				{
					continue;
				}
				if ((object)typeSymbol == null)
				{
					typeSymbol = typeSymbol2;
					num = i;
					continue;
				}
				TypeSymbol typeSymbol3 = Better(typeSymbol, typeSymbol2, conversions, ref useSiteInfo);
				if ((object)typeSymbol3 == null)
				{
					typeSymbol = null;
					continue;
				}
				typeSymbol = typeSymbol3;
				num = i;
			}
			if ((object)typeSymbol == null)
			{
				return null;
			}
			for (int j = 0; j < num; j++)
			{
				TypeSymbol typeSymbol4 = checkType(types[j]);
				if ((object)typeSymbol4 != null)
				{
					TypeSymbol t = Better(typeSymbol, typeSymbol4, conversions, ref useSiteInfo);
					if (!typeSymbol.Equals(t, TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
					{
						return null;
					}
				}
			}
			return typeSymbol;
		}
		}
		static TypeSymbol? checkType(TypeSymbol type)
		{
			if (!(type is FunctionTypeSymbol functionTypeSymbol) || (object)functionTypeSymbol.GetInternalDelegateType() != null)
			{
				return type;
			}
			return null;
		}
	}

	private static TypeSymbol? Better(TypeSymbol type1, TypeSymbol? type2, ConversionsBase conversions, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (type1.IsErrorType())
		{
			return type2;
		}
		if ((object)type2 == null || type2.IsErrorType())
		{
			return type1;
		}
		if (type1 is FunctionTypeSymbol)
		{
			if (!(type2 is FunctionTypeSymbol))
			{
				return type2;
			}
		}
		else if (type2 is FunctionTypeSymbol)
		{
			return type1;
		}
		ConversionsBase conversionsBase = conversions.WithNullability(includeNullability: false);
		bool exists = conversionsBase.ClassifyImplicitConversionFromTypeWhenNeitherOrBothFunctionTypes(type1, type2, ref useSiteInfo).Exists;
		bool exists2 = conversionsBase.ClassifyImplicitConversionFromTypeWhenNeitherOrBothFunctionTypes(type2, type1, ref useSiteInfo).Exists;
		if (exists & exists2)
		{
			if (type1.Equals(type2, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
			{
				return type1.MergeEquivalentTypes(type2, VarianceKind.Out);
			}
			return null;
		}
		if (exists)
		{
			return type2;
		}
		if (exists2)
		{
			return type1;
		}
		return null;
	}
}
