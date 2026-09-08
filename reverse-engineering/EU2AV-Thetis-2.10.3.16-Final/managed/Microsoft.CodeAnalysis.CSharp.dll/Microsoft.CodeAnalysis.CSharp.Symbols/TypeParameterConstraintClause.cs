using System.Collections.Generic;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeParameterConstraintClause
{
	internal static readonly TypeParameterConstraintClause Empty = new TypeParameterConstraintClause(TypeParameterConstraintKind.None, ImmutableArray<TypeWithAnnotations>.Empty);

	internal static readonly TypeParameterConstraintClause ObliviousNullabilityIfReferenceType = new TypeParameterConstraintClause(TypeParameterConstraintKind.ObliviousNullabilityIfReferenceType, ImmutableArray<TypeWithAnnotations>.Empty);

	public readonly TypeParameterConstraintKind Constraints;

	public readonly ImmutableArray<TypeWithAnnotations> ConstraintTypes;

	internal static TypeParameterConstraintClause Create(TypeParameterConstraintKind constraints, ImmutableArray<TypeWithAnnotations> constraintTypes)
	{
		if (constraintTypes.IsEmpty)
		{
			switch (constraints)
			{
			case TypeParameterConstraintKind.None:
				return Empty;
			case TypeParameterConstraintKind.ObliviousNullabilityIfReferenceType:
				return ObliviousNullabilityIfReferenceType;
			}
		}
		return new TypeParameterConstraintClause(constraints, constraintTypes);
	}

	private TypeParameterConstraintClause(TypeParameterConstraintKind constraints, ImmutableArray<TypeWithAnnotations> constraintTypes)
	{
		Constraints = constraints;
		ConstraintTypes = constraintTypes;
	}

	internal static SmallDictionary<TypeParameterSymbol, bool> BuildIsValueTypeMap(ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeParameterConstraintClause> constraintClauses)
	{
		SmallDictionary<TypeParameterSymbol, bool> smallDictionary = new SmallDictionary<TypeParameterSymbol, bool>(ReferenceEqualityComparer.Instance);
		foreach (TypeParameterSymbol item in typeParameters)
		{
			isValueType(item, constraintClauses, smallDictionary, ConsList<TypeParameterSymbol>.Empty);
		}
		return smallDictionary;
		static bool isValueType(TypeParameterSymbol thisTypeParameter, ImmutableArray<TypeParameterConstraintClause> constraintClauses2, SmallDictionary<TypeParameterSymbol, bool> isValueTypeMap, ConsList<TypeParameterSymbol> inProgress)
		{
			if (inProgress.ContainsReference(thisTypeParameter))
			{
				return false;
			}
			if (isValueTypeMap.TryGetValue(thisTypeParameter, out var value))
			{
				return value;
			}
			TypeParameterConstraintClause typeParameterConstraintClause = constraintClauses2[thisTypeParameter.Ordinal];
			bool flag = false;
			if ((typeParameterConstraintClause.Constraints & TypeParameterConstraintKind.AllValueTypeKinds) != TypeParameterConstraintKind.None)
			{
				flag = true;
			}
			else
			{
				Symbol containingSymbol = thisTypeParameter.ContainingSymbol;
				inProgress = inProgress.Prepend(thisTypeParameter);
				foreach (TypeWithAnnotations constraintType in typeParameterConstraintClause.ConstraintTypes)
				{
					TypeSymbol typeSymbol = (constraintType.IsResolved ? constraintType.Type : constraintType.DefaultType);
					if (typeSymbol is TypeParameterSymbol typeParameterSymbol && (object)typeParameterSymbol.ContainingSymbol == containingSymbol)
					{
						if (isValueType(typeParameterSymbol, constraintClauses2, isValueTypeMap, inProgress))
						{
							flag = true;
							break;
						}
					}
					else if (typeSymbol.IsValueType)
					{
						flag = true;
						break;
					}
				}
			}
			isValueTypeMap.Add(thisTypeParameter, flag);
			return flag;
		}
	}

	internal static SmallDictionary<TypeParameterSymbol, bool> BuildIsReferenceTypeFromConstraintTypesMap(ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeParameterConstraintClause> constraintClauses)
	{
		SmallDictionary<TypeParameterSymbol, bool> smallDictionary = new SmallDictionary<TypeParameterSymbol, bool>(ReferenceEqualityComparer.Instance);
		foreach (TypeParameterSymbol item in typeParameters)
		{
			isReferenceTypeFromConstraintTypes(item, constraintClauses, smallDictionary, ConsList<TypeParameterSymbol>.Empty);
		}
		return smallDictionary;
		static bool isReferenceTypeFromConstraintTypes(TypeParameterSymbol thisTypeParameter, ImmutableArray<TypeParameterConstraintClause> constraintClauses2, SmallDictionary<TypeParameterSymbol, bool> isReferenceTypeFromConstraintTypesMap, ConsList<TypeParameterSymbol> inProgress)
		{
			if (inProgress.ContainsReference(thisTypeParameter))
			{
				return false;
			}
			if (isReferenceTypeFromConstraintTypesMap.TryGetValue(thisTypeParameter, out var value))
			{
				return value;
			}
			TypeParameterConstraintClause typeParameterConstraintClause = constraintClauses2[thisTypeParameter.Ordinal];
			bool flag = false;
			Symbol containingSymbol = thisTypeParameter.ContainingSymbol;
			inProgress = inProgress.Prepend(thisTypeParameter);
			foreach (TypeWithAnnotations constraintType in typeParameterConstraintClause.ConstraintTypes)
			{
				TypeSymbol typeSymbol = (constraintType.IsResolved ? constraintType.Type : constraintType.DefaultType);
				if (typeSymbol is TypeParameterSymbol typeParameterSymbol)
				{
					if ((object)typeParameterSymbol.ContainingSymbol == containingSymbol)
					{
						if (isReferenceTypeFromConstraintTypes(typeParameterSymbol, constraintClauses2, isReferenceTypeFromConstraintTypesMap, inProgress))
						{
							flag = true;
							break;
						}
					}
					else if (typeParameterSymbol.IsReferenceTypeFromConstraintTypes)
					{
						flag = true;
						break;
					}
				}
				else if (TypeParameterSymbol.NonTypeParameterConstraintImpliesReferenceType(typeSymbol))
				{
					flag = true;
					break;
				}
			}
			isReferenceTypeFromConstraintTypesMap.Add(thisTypeParameter, flag);
			return flag;
		}
	}
}
