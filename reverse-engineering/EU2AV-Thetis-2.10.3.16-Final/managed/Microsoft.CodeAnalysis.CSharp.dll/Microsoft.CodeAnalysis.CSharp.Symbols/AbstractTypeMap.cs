using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class AbstractTypeMap
{
	internal virtual NamedTypeSymbol SubstituteTypeDeclaration(NamedTypeSymbol previous)
	{
		NamedTypeSymbol namedTypeSymbol = SubstituteNamedType(previous.ContainingType);
		if ((object)namedTypeSymbol == null)
		{
			return previous;
		}
		return previous.OriginalDefinition.AsMember(namedTypeSymbol);
	}

	internal NamedTypeSymbol SubstituteNamedType(NamedTypeSymbol previous)
	{
		if ((object)previous == null)
		{
			return null;
		}
		if (previous.IsUnboundGenericType)
		{
			return previous;
		}
		if (previous.IsAnonymousType)
		{
			return ((AnonymousTypeManager.AnonymousTypeOrDelegatePublicSymbol)previous).SubstituteTypes(this);
		}
		NamedTypeSymbol constructedFrom = previous.ConstructedFrom;
		NamedTypeSymbol namedTypeSymbol = SubstituteTypeDeclaration(constructedFrom);
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = previous.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
		bool flag = (object)constructedFrom != namedTypeSymbol;
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length);
		for (int i = 0; i < typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length; i++)
		{
			TypeWithAnnotations typeWithAnnotations = typeArgumentsWithAnnotationsNoUseSiteDiagnostics[i];
			TypeWithAnnotations typeWithAnnotations2 = typeWithAnnotations.SubstituteType(this);
			if (!flag && !typeWithAnnotations.IsSameAs(typeWithAnnotations2))
			{
				flag = true;
			}
			instance.Add(typeWithAnnotations2);
		}
		if (!flag)
		{
			instance.Free();
			return previous;
		}
		return namedTypeSymbol.ConstructIfGeneric(instance.ToImmutableAndFree()).WithTupleDataFrom(previous);
	}

	internal TypeWithAnnotations SubstituteType(TypeSymbol previous)
	{
		if ((object)previous == null)
		{
			return default(TypeWithAnnotations);
		}
		TypeSymbol typeSymbol;
		switch (previous.Kind)
		{
		case SymbolKind.NamedType:
			typeSymbol = SubstituteNamedType((NamedTypeSymbol)previous);
			break;
		case SymbolKind.TypeParameter:
			return SubstituteTypeParameter((TypeParameterSymbol)previous);
		case SymbolKind.ArrayType:
			typeSymbol = SubstituteArrayType((ArrayTypeSymbol)previous);
			break;
		case SymbolKind.PointerType:
			typeSymbol = SubstitutePointerType((PointerTypeSymbol)previous);
			break;
		case SymbolKind.FunctionPointerType:
			typeSymbol = SubstituteFunctionPointerType((FunctionPointerTypeSymbol)previous);
			break;
		case SymbolKind.DynamicType:
			typeSymbol = SubstituteDynamicType();
			break;
		case SymbolKind.ErrorType:
			return ((ErrorTypeSymbol)previous).Substitute(this);
		default:
			typeSymbol = previous;
			break;
		}
		return TypeWithAnnotations.Create(typeSymbol);
	}

	internal TypeWithAnnotations SubstituteType(TypeWithAnnotations previous)
	{
		return previous.SubstituteType(this);
	}

	internal virtual ImmutableArray<CustomModifier> SubstituteCustomModifiers(ImmutableArray<CustomModifier> customModifiers)
	{
		if (customModifiers.IsDefaultOrEmpty)
		{
			return customModifiers;
		}
		for (int i = 0; i < customModifiers.Length; i++)
		{
			NamedTypeSymbol modifierSymbol = ((CSharpCustomModifier)customModifiers[i]).ModifierSymbol;
			NamedTypeSymbol namedTypeSymbol = SubstituteNamedType(modifierSymbol);
			if (TypeSymbol.Equals(modifierSymbol, namedTypeSymbol, TypeCompareKind.ConsiderEverything))
			{
				continue;
			}
			ArrayBuilder<CustomModifier> instance = ArrayBuilder<CustomModifier>.GetInstance(customModifiers.Length);
			instance.AddRange(customModifiers, i);
			instance.Add(customModifiers[i].IsOptional ? CSharpCustomModifier.CreateOptional(namedTypeSymbol) : CSharpCustomModifier.CreateRequired(namedTypeSymbol));
			for (i++; i < customModifiers.Length; i++)
			{
				modifierSymbol = ((CSharpCustomModifier)customModifiers[i]).ModifierSymbol;
				namedTypeSymbol = SubstituteNamedType(modifierSymbol);
				if (!TypeSymbol.Equals(modifierSymbol, namedTypeSymbol, TypeCompareKind.ConsiderEverything))
				{
					instance.Add(customModifiers[i].IsOptional ? CSharpCustomModifier.CreateOptional(namedTypeSymbol) : CSharpCustomModifier.CreateRequired(namedTypeSymbol));
				}
				else
				{
					instance.Add(customModifiers[i]);
				}
			}
			return instance.ToImmutableAndFree();
		}
		return customModifiers;
	}

	protected virtual TypeSymbol SubstituteDynamicType()
	{
		return DynamicTypeSymbol.Instance;
	}

	protected virtual TypeWithAnnotations SubstituteTypeParameter(TypeParameterSymbol typeParameter)
	{
		return TypeWithAnnotations.Create(typeParameter);
	}

	private ArrayTypeSymbol SubstituteArrayType(ArrayTypeSymbol t)
	{
		TypeWithAnnotations elementTypeWithAnnotations = t.ElementTypeWithAnnotations;
		TypeWithAnnotations elementTypeWithAnnotations2 = elementTypeWithAnnotations.SubstituteType(this);
		if (elementTypeWithAnnotations2.IsSameAs(elementTypeWithAnnotations))
		{
			return t;
		}
		if (t.IsSZArray)
		{
			ImmutableArray<NamedTypeSymbol> constructedInterfaces = t.InterfacesNoUseSiteDiagnostics();
			if (constructedInterfaces.Length == 1)
			{
				constructedInterfaces = ImmutableArray.Create(SubstituteNamedType(constructedInterfaces[0]));
			}
			else if (constructedInterfaces.Length == 2)
			{
				constructedInterfaces = ImmutableArray.Create(SubstituteNamedType(constructedInterfaces[0]), SubstituteNamedType(constructedInterfaces[1]));
			}
			else if (constructedInterfaces.Length != 0)
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AbstractTypeMap.cs", 210);
			}
			return ArrayTypeSymbol.CreateSZArray(elementTypeWithAnnotations2, t.BaseTypeNoUseSiteDiagnostics, constructedInterfaces);
		}
		return ArrayTypeSymbol.CreateMDArray(elementTypeWithAnnotations2, t.Rank, t.Sizes, t.LowerBounds, t.BaseTypeNoUseSiteDiagnostics);
	}

	private PointerTypeSymbol SubstitutePointerType(PointerTypeSymbol t)
	{
		TypeWithAnnotations pointedAtTypeWithAnnotations = t.PointedAtTypeWithAnnotations;
		TypeWithAnnotations pointedAtType = pointedAtTypeWithAnnotations.SubstituteType(this);
		if (pointedAtType.IsSameAs(pointedAtTypeWithAnnotations))
		{
			return t;
		}
		return new PointerTypeSymbol(pointedAtType);
	}

	private FunctionPointerTypeSymbol SubstituteFunctionPointerType(FunctionPointerTypeSymbol f)
	{
		TypeWithAnnotations typeWithAnnotations = f.Signature.ReturnTypeWithAnnotations.SubstituteType(this);
		ImmutableArray<CustomModifier> refCustomModifiers = f.Signature.RefCustomModifiers;
		ImmutableArray<CustomModifier> immutableArray = SubstituteCustomModifiers(refCustomModifiers);
		ImmutableArray<TypeWithAnnotations> parameterTypesWithAnnotations = f.Signature.ParameterTypesWithAnnotations;
		ImmutableArray<TypeWithAnnotations> immutableArray2 = SubstituteTypes(parameterTypesWithAnnotations);
		ImmutableArray<ImmutableArray<CustomModifier>> paramRefCustomModifiers = default(ImmutableArray<ImmutableArray<CustomModifier>>);
		int length = f.Signature.Parameters.Length;
		if (length > 0)
		{
			ArrayBuilder<ImmutableArray<CustomModifier>> instance = ArrayBuilder<ImmutableArray<CustomModifier>>.GetInstance(length);
			bool flag = false;
			foreach (ParameterSymbol parameter in f.Signature.Parameters)
			{
				ImmutableArray<CustomModifier> immutableArray3 = SubstituteCustomModifiers(parameter.RefCustomModifiers);
				instance.Add(immutableArray3);
				if (immutableArray3 != parameter.RefCustomModifiers)
				{
					flag = true;
				}
			}
			if (flag)
			{
				paramRefCustomModifiers = instance.ToImmutableAndFree();
			}
			else
			{
				instance.Free();
			}
		}
		if (immutableArray2 != parameterTypesWithAnnotations || !paramRefCustomModifiers.IsDefault || !f.Signature.ReturnTypeWithAnnotations.IsSameAs(typeWithAnnotations) || immutableArray != refCustomModifiers)
		{
			f = f.SubstituteTypeSymbol(typeWithAnnotations, immutableArray2, refCustomModifiers, paramRefCustomModifiers);
		}
		return f;
	}

	internal ImmutableArray<TypeSymbol> SubstituteTypesWithoutModifiers(ImmutableArray<TypeSymbol> original)
	{
		if (original.IsDefault)
		{
			return original;
		}
		TypeSymbol[] array = null;
		for (int i = 0; i < original.Length; i++)
		{
			TypeSymbol typeSymbol = original[i];
			TypeSymbol type = SubstituteType(typeSymbol).Type;
			if ((object)type != typeSymbol && array == null)
			{
				array = new TypeSymbol[original.Length];
				for (int j = 0; j < i; j++)
				{
					array[j] = original[j];
				}
			}
			if (array != null)
			{
				array[i] = type;
			}
		}
		if (array == null)
		{
			return original;
		}
		return ImmutableCollectionsMarshal.AsImmutableArray(array);
	}

	internal ImmutableArray<TypeWithAnnotations> SubstituteTypes(ImmutableArray<TypeWithAnnotations> original)
	{
		if (original.IsDefault)
		{
			return default(ImmutableArray<TypeWithAnnotations>);
		}
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(original.Length);
		foreach (TypeWithAnnotations item in original)
		{
			instance.Add(SubstituteType(item));
		}
		return instance.ToImmutableAndFree();
	}

	internal void SubstituteConstraintTypesDistinctWithoutModifiers(TypeParameterSymbol owner, ImmutableArray<TypeWithAnnotations> original, ArrayBuilder<TypeWithAnnotations> result, HashSet<TypeParameterSymbol> ignoreTypesDependentOnTypeParametersOpt)
	{
		DynamicTypeEraser dynamicEraser = null;
		if (original.Length == 0)
		{
			return;
		}
		if (original.Length == 1)
		{
			TypeWithAnnotations type = original[0];
			if (ignoreTypesDependentOnTypeParametersOpt == null || !type.Type.ContainsTypeParameters(ignoreTypesDependentOnTypeParametersOpt))
			{
				result.Add(substituteConstraintType(type));
			}
			return;
		}
		PooledDictionary<TypeSymbol, int> instance = PooledDictionary<TypeSymbol, int>.GetInstance();
		foreach (TypeWithAnnotations item in original)
		{
			if (ignoreTypesDependentOnTypeParametersOpt == null || !item.Type.ContainsTypeParameters(ignoreTypesDependentOnTypeParametersOpt))
			{
				TypeWithAnnotations typeWithAnnotations = substituteConstraintType(item);
				if (!instance.TryGetValue(typeWithAnnotations.Type, out var value))
				{
					instance.Add(typeWithAnnotations.Type, result.Count);
					result.Add(typeWithAnnotations);
				}
				else
				{
					result[value] = ConstraintsHelper.ConstraintWithMostSignificantNullability(result[value], typeWithAnnotations);
				}
			}
		}
		instance.Free();
		TypeWithAnnotations substituteConstraintType(TypeWithAnnotations previous)
		{
			if (dynamicEraser == null)
			{
				dynamicEraser = new DynamicTypeEraser(owner.ContainingAssembly.CorLibrary.GetSpecialType(SpecialType.System_Object));
			}
			TypeWithAnnotations typeWithAnnotations2 = SubstituteType(previous);
			return typeWithAnnotations2.WithTypeAndModifiers(dynamicEraser.EraseDynamic(typeWithAnnotations2.Type), typeWithAnnotations2.CustomModifiers);
		}
	}

	internal ImmutableArray<TypeParameterSymbol> SubstituteTypeParameters(ImmutableArray<TypeParameterSymbol> original)
	{
		return original.SelectAsArray((TypeParameterSymbol tp, AbstractTypeMap m) => (TypeParameterSymbol)m.SubstituteTypeParameter(tp).AsTypeSymbolOnly(), this);
	}

	internal ImmutableArray<NamedTypeSymbol> SubstituteNamedTypes(ImmutableArray<NamedTypeSymbol> original)
	{
		NamedTypeSymbol[] array = null;
		for (int i = 0; i < original.Length; i++)
		{
			NamedTypeSymbol namedTypeSymbol = original[i];
			NamedTypeSymbol namedTypeSymbol2 = SubstituteNamedType(namedTypeSymbol);
			if ((object)namedTypeSymbol2 != namedTypeSymbol && array == null)
			{
				array = new NamedTypeSymbol[original.Length];
				for (int j = 0; j < i; j++)
				{
					array[j] = original[j];
				}
			}
			if (array != null)
			{
				array[i] = namedTypeSymbol2;
			}
		}
		if (array == null)
		{
			return original;
		}
		return ImmutableCollectionsMarshal.AsImmutableArray(array);
	}
}
