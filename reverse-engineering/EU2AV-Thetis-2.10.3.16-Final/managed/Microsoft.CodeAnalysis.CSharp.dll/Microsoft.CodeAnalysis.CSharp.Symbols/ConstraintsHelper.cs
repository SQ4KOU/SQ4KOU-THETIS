using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ConstraintsHelper
{
	internal readonly struct CheckConstraintsArgs
	{
		public readonly CSharpCompilation? CurrentCompilation;

		public readonly ConversionsBase Conversions;

		public readonly bool IncludeNullability;

		public readonly Location Location;

		public readonly BindingDiagnosticBag Diagnostics;

		public readonly CompoundUseSiteInfo<AssemblySymbol> Template;

		public CheckConstraintsArgs(CSharpCompilation currentCompilation, ConversionsBase conversions, Location location, BindingDiagnosticBag diagnostics)
			: this(currentCompilation, conversions, currentCompilation.IsFeatureEnabled(MessageID.IDS_FeatureNullableReferenceTypes), location, diagnostics)
		{
		}

		public CheckConstraintsArgs(CSharpCompilation currentCompilation, ConversionsBase conversions, bool includeNullability, Location location, BindingDiagnosticBag diagnostics)
			: this(currentCompilation, conversions, includeNullability, location, diagnostics, new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, currentCompilation.Assembly))
		{
		}

		public CheckConstraintsArgs(CSharpCompilation currentCompilation, ConversionsBase conversions, bool includeNullability, Location location, BindingDiagnosticBag diagnostics, CompoundUseSiteInfo<AssemblySymbol> template)
		{
			CurrentCompilation = currentCompilation;
			Conversions = conversions;
			IncludeNullability = includeNullability;
			Location = location;
			Diagnostics = diagnostics;
			Template = template;
		}
	}

	internal sealed class CheckConstraintsArgsBoxed
	{
		public CheckConstraintsArgs Args;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static CheckConstraintsArgsBoxed Allocate(CSharpCompilation currentCompilation, ConversionsBase conversions, Location location, BindingDiagnosticBag diagnostics)
		{
			CheckConstraintsArgsBoxed checkConstraintsArgsBoxed = s_checkConstraintsArgsBoxedPool.Allocate();
			checkConstraintsArgsBoxed.Args = new CheckConstraintsArgs(currentCompilation, conversions, location, diagnostics);
			return checkConstraintsArgsBoxed;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static CheckConstraintsArgsBoxed Allocate(CSharpCompilation currentCompilation, ConversionsBase conversions, bool includeNullability, Location location, BindingDiagnosticBag diagnostics)
		{
			CheckConstraintsArgsBoxed checkConstraintsArgsBoxed = s_checkConstraintsArgsBoxedPool.Allocate();
			checkConstraintsArgsBoxed.Args = new CheckConstraintsArgs(currentCompilation, conversions, includeNullability, location, diagnostics);
			return checkConstraintsArgsBoxed;
		}

		public void Free()
		{
			Args = default(CheckConstraintsArgs);
			s_checkConstraintsArgsBoxedPool.Free(this);
		}
	}

	private enum ConstructorConstraintError
	{
		None,
		NoPublicParameterlessConstructorOrAbstractType,
		HasRequiredMembers
	}

	private static readonly ObjectPool<CheckConstraintsArgsBoxed> s_checkConstraintsArgsBoxedPool = new ObjectPool<CheckConstraintsArgsBoxed>(() => new CheckConstraintsArgsBoxed());

	private static readonly Func<TypeSymbol, CheckConstraintsArgsBoxed, bool, bool> s_checkConstraintsSingleTypeFunc = (TypeSymbol type, CheckConstraintsArgsBoxed arg, bool unused) => CheckConstraintsSingleType(type, in arg.Args);

	public static TypeParameterBounds ResolveBounds(this SourceTypeParameterSymbol typeParameter, AssemblySymbol corLibrary, ConsList<TypeParameterSymbol> inProgress, ImmutableArray<TypeWithAnnotations> constraintTypes, bool inherited, CSharpCompilation currentCompilation, BindingDiagnosticBag diagnostics)
	{
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		TypeParameterBounds typeParameterBounds = typeParameter.ResolveBounds(corLibrary, inProgress, constraintTypes, inherited, currentCompilation, instance, ref useSiteDiagnosticsBuilder, new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, currentCompilation.Assembly));
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			diagnostics.Add(item.UseSiteInfo, item.TypeParameter.GetFirstLocation());
		}
		instance.Free();
		if (typeParameter.AllowsRefLikeType)
		{
			if (inherited)
			{
				Location firstLocation = typeParameter.GetFirstLocation();
				Binder.CheckFeatureAvailability(firstLocation.SourceTree, MessageID.IDS_FeatureAllowsRefStructConstraint, diagnostics, firstLocation);
				if (!typeParameter.DeclaringCompilation.Assembly.RuntimeSupportsByRefLikeGenerics)
				{
					diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, firstLocation);
				}
			}
			else
			{
				SpecialType specialType = ((!typeParameter.HasReferenceTypeConstraint) ? (typeParameterBounds?.EffectiveBaseClass.SpecialType ?? SpecialType.System_Object) : SpecialType.None);
				if ((uint)(specialType - 1) > 1u && specialType != SpecialType.System_ValueType)
				{
					diagnostics.Add(ErrorCode.ERR_ClassIsCombinedWithRefStruct, typeParameter.GetFirstLocation());
				}
			}
		}
		return typeParameterBounds;
	}

	public static TypeParameterBounds ResolveBounds(this TypeParameterSymbol typeParameter, AssemblySymbol corLibrary, ConsList<TypeParameterSymbol> inProgress, ImmutableArray<TypeWithAnnotations> constraintTypes, bool inherited, CSharpCompilation currentCompilation, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder, CompoundUseSiteInfo<AssemblySymbol> template)
	{
		NamedTypeSymbol namedTypeSymbol = corLibrary.GetSpecialType((!typeParameter.HasValueTypeConstraint) ? SpecialType.System_Object : SpecialType.System_ValueType);
		TypeSymbol typeSymbol = namedTypeSymbol;
		ImmutableArray<NamedTypeSymbol> interfaces;
		if (constraintTypes.Length == 0)
		{
			interfaces = ImmutableArray<NamedTypeSymbol>.Empty;
		}
		else
		{
			ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
			ArrayBuilder<NamedTypeSymbol> instance2 = ArrayBuilder<NamedTypeSymbol>.GetInstance();
			TypeConversions typeConversions = corLibrary.TypeConversions;
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(template);
			foreach (TypeWithAnnotations item in constraintTypes)
			{
				NamedTypeSymbol namedTypeSymbol2;
				TypeSymbol typeSymbol2;
				switch (item.TypeKind)
				{
				case TypeKind.TypeParameter:
				{
					TypeParameterSymbol typeParameterSymbol2 = (TypeParameterSymbol)item.Type;
					ConsList<TypeParameterSymbol> inProgress2;
					if (typeParameterSymbol2.ContainingSymbol == typeParameter.ContainingSymbol)
					{
						if (inProgress.ContainsReference(typeParameterSymbol2))
						{
							diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameterSymbol2, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_CircularConstraint, typeParameterSymbol2, typeParameter))));
							continue;
						}
						inProgress2 = inProgress;
					}
					else
					{
						inProgress2 = ConsList<TypeParameterSymbol>.Empty;
					}
					namedTypeSymbol2 = typeParameterSymbol2.GetEffectiveBaseClass(inProgress2);
					typeSymbol2 = typeParameterSymbol2.GetDeducedBaseType(inProgress2);
					AddInterfaces(instance2, typeParameterSymbol2.GetInterfaces(inProgress2));
					if (inherited || currentCompilation == null || !typeParameterSymbol2.IsFromCompilation(currentCompilation))
					{
						break;
					}
					ErrorCode code;
					if (typeParameterSymbol2.HasUnmanagedTypeConstraint)
					{
						code = ErrorCode.ERR_ConWithUnmanagedCon;
					}
					else
					{
						if (!typeParameterSymbol2.HasValueTypeConstraint)
						{
							break;
						}
						code = ErrorCode.ERR_ConWithValCon;
					}
					diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(code, typeParameter, typeParameterSymbol2))));
					continue;
				}
				case TypeKind.Class:
				case TypeKind.Delegate:
				case TypeKind.Interface:
					if (item.Type.IsInterfaceType())
					{
						AddInterface(instance2, (NamedTypeSymbol)item.Type);
						instance.Add(item);
						continue;
					}
					namedTypeSymbol2 = (NamedTypeSymbol)item.Type;
					typeSymbol2 = item.Type;
					break;
				case TypeKind.Struct:
					if (item.IsNullableType())
					{
						TypeSymbol nullableUnderlyingType = item.Type.GetNullableUnderlyingType();
						if (nullableUnderlyingType.TypeKind == TypeKind.TypeParameter)
						{
							TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)nullableUnderlyingType;
							if (typeParameterSymbol.ContainingSymbol == typeParameter.ContainingSymbol && inProgress.ContainsReference(typeParameterSymbol))
							{
								diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameterSymbol, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_CircularConstraint, typeParameterSymbol, typeParameter))));
								continue;
							}
						}
					}
					namedTypeSymbol2 = corLibrary.GetSpecialType(SpecialType.System_ValueType);
					typeSymbol2 = item.Type;
					break;
				case TypeKind.Enum:
					namedTypeSymbol2 = corLibrary.GetSpecialType(SpecialType.System_Enum);
					typeSymbol2 = item.Type;
					break;
				case TypeKind.Array:
					namedTypeSymbol2 = corLibrary.GetSpecialType(SpecialType.System_Array);
					typeSymbol2 = item.Type;
					break;
				case TypeKind.Error:
					namedTypeSymbol2 = (NamedTypeSymbol)item.Type;
					typeSymbol2 = item.Type;
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(item.TypeKind);
				case TypeKind.Pointer:
				case TypeKind.FunctionPointer:
					continue;
				}
				instance.Add(item);
				if (!typeSymbol.IsErrorType() && !typeSymbol2.IsErrorType() && !IsEncompassedBy(typeConversions, typeSymbol, typeSymbol2, ref useSiteInfo))
				{
					if (!IsEncompassedBy(typeConversions, typeSymbol2, typeSymbol, ref useSiteInfo))
					{
						diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BaseConstraintConflict, typeParameter, typeSymbol2, typeSymbol))));
					}
					else
					{
						typeSymbol = typeSymbol2;
						namedTypeSymbol = namedTypeSymbol2;
					}
				}
			}
			AppendUseSiteDiagnostics(useSiteInfo, typeParameter, ref useSiteDiagnosticsBuilder);
			constraintTypes = instance.ToImmutableAndFree();
			interfaces = instance2.ToImmutableAndFree();
		}
		if (constraintTypes.Length == 0 && typeSymbol.SpecialType == SpecialType.System_Object)
		{
			return null;
		}
		TypeParameterBounds typeParameterBounds = new TypeParameterBounds(constraintTypes, interfaces, namedTypeSymbol, typeSymbol);
		if (inherited)
		{
			CheckOverrideConstraints(typeParameter, typeParameterBounds, diagnosticsBuilder);
		}
		return typeParameterBounds;
	}

	internal static ImmutableArray<ImmutableArray<TypeWithAnnotations>> MakeTypeParameterConstraintTypes(this MethodSymbol containingSymbol, Binder withTypeParametersBinder, ImmutableArray<TypeParameterSymbol> typeParameters, TypeParameterListSyntax typeParameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BindingDiagnosticBag diagnostics)
	{
		if (typeParameters.Length == 0 || constraintClauses.Count == 0)
		{
			return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
		}
		withTypeParametersBinder = withTypeParametersBinder.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.GenericConstraintsClause);
		ImmutableArray<TypeParameterConstraintClause> immutableArray = withTypeParametersBinder.BindTypeParameterConstraintClauses(containingSymbol, typeParameters, typeParameterList, constraintClauses, diagnostics, performOnlyCycleSafeValidation: false);
		if (immutableArray.All((TypeParameterConstraintClause clause) => clause.ConstraintTypes.IsEmpty))
		{
			return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
		}
		return immutableArray.SelectAsArray((TypeParameterConstraintClause clause) => clause.ConstraintTypes);
	}

	internal static ImmutableArray<TypeParameterConstraintKind> MakeTypeParameterConstraintKinds(this MethodSymbol containingSymbol, Binder withTypeParametersBinder, ImmutableArray<TypeParameterSymbol> typeParameters, TypeParameterListSyntax typeParameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		if (typeParameters.Length == 0)
		{
			return ImmutableArray<TypeParameterConstraintKind>.Empty;
		}
		ImmutableArray<TypeParameterConstraintClause> immutableArray;
		if (constraintClauses.Count == 0)
		{
			immutableArray = withTypeParametersBinder.GetDefaultTypeParameterConstraintClauses(typeParameterList);
		}
		else
		{
			withTypeParametersBinder = withTypeParametersBinder.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.GenericConstraintsClause | BinderFlags.SuppressTypeArgumentBinding);
			immutableArray = withTypeParametersBinder.BindTypeParameterConstraintClauses(containingSymbol, typeParameters, typeParameterList, constraintClauses, BindingDiagnosticBag.Discarded, performOnlyCycleSafeValidation: true);
			immutableArray = AdjustConstraintKindsBasedOnConstraintTypes(typeParameters, immutableArray);
		}
		if (immutableArray.All((TypeParameterConstraintClause clause) => clause.Constraints == TypeParameterConstraintKind.None))
		{
			return ImmutableArray<TypeParameterConstraintKind>.Empty;
		}
		return immutableArray.SelectAsArray((TypeParameterConstraintClause clause) => clause.Constraints);
	}

	internal static ImmutableArray<TypeParameterConstraintClause> AdjustConstraintKindsBasedOnConstraintTypes(ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeParameterConstraintClause> constraintClauses)
	{
		int length = typeParameters.Length;
		SmallDictionary<TypeParameterSymbol, bool> smallDictionary = TypeParameterConstraintClause.BuildIsValueTypeMap(typeParameters, constraintClauses);
		SmallDictionary<TypeParameterSymbol, bool> smallDictionary2 = TypeParameterConstraintClause.BuildIsReferenceTypeFromConstraintTypesMap(typeParameters, constraintClauses);
		ArrayBuilder<TypeParameterConstraintClause> arrayBuilder = null;
		for (int i = 0; i < length; i++)
		{
			TypeParameterConstraintClause typeParameterConstraintClause = constraintClauses[i];
			TypeParameterSymbol key = typeParameters[i];
			TypeParameterConstraintKind typeParameterConstraintKind = typeParameterConstraintClause.Constraints;
			if ((typeParameterConstraintKind & TypeParameterConstraintKind.AllValueTypeKinds) == 0 && smallDictionary[key])
			{
				typeParameterConstraintKind |= TypeParameterConstraintKind.ValueTypeFromConstraintTypes;
			}
			if (smallDictionary2[key])
			{
				typeParameterConstraintKind |= TypeParameterConstraintKind.ReferenceTypeFromConstraintTypes;
			}
			if (typeParameterConstraintClause.Constraints != typeParameterConstraintKind)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<TypeParameterConstraintClause>.GetInstance(constraintClauses.Length);
					arrayBuilder.AddRange(constraintClauses);
				}
				arrayBuilder[i] = TypeParameterConstraintClause.Create(typeParameterConstraintKind, typeParameterConstraintClause.ConstraintTypes);
			}
		}
		if (arrayBuilder != null)
		{
			constraintClauses = arrayBuilder.ToImmutableAndFree();
		}
		return constraintClauses;
	}

	private static void CheckOverrideConstraints(TypeParameterSymbol typeParameter, TypeParameterBounds bounds, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder)
	{
		TypeSymbol deducedBaseType = bounds.DeducedBaseType;
		ImmutableArray<TypeWithAnnotations> constraintTypes = bounds.ConstraintTypes;
		if (IsValueType(typeParameter, constraintTypes) && IsReferenceType(typeParameter, constraintTypes))
		{
			diagnosticsBuilder.Add(GenerateConflictingConstraintsError(typeParameter, deducedBaseType, deducedBaseType.IsValueType));
		}
		else if (deducedBaseType.IsNullableType() && (typeParameter.HasValueTypeConstraint || typeParameter.HasReferenceTypeConstraint))
		{
			diagnosticsBuilder.Add(GenerateConflictingConstraintsError(typeParameter, deducedBaseType, typeParameter.HasReferenceTypeConstraint));
		}
	}

	public static void CheckAllConstraints(this TypeSymbol type, CSharpCompilation compilation, ConversionsBase conversions, Location location, BindingDiagnosticBag diagnostics)
	{
		bool includeNullability = compilation.IsFeatureEnabled(MessageID.IDS_FeatureNullableReferenceTypes);
		CheckConstraintsArgsBoxed checkConstraintsArgsBoxed = CheckConstraintsArgsBoxed.Allocate(compilation, conversions, includeNullability, location, diagnostics);
		type.CheckAllConstraints(checkConstraintsArgsBoxed);
		checkConstraintsArgsBoxed.Free();
	}

	public static bool CheckAllConstraints(this TypeSymbol type, CSharpCompilation compilation, ConversionsBase conversions)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
		CheckConstraintsArgsBoxed checkConstraintsArgsBoxed = CheckConstraintsArgsBoxed.Allocate(compilation, conversions, includeNullability: false, NoLocation.Singleton, instance);
		type.CheckAllConstraints(checkConstraintsArgsBoxed);
		bool result = !instance.HasAnyErrors();
		checkConstraintsArgsBoxed.Free();
		instance.Free();
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CheckAllConstraints(this TypeSymbol type, CheckConstraintsArgsBoxed args)
	{
		type.VisitType(s_checkConstraintsSingleTypeFunc, args);
	}

	private static bool CheckConstraintsSingleType(TypeSymbol type, in CheckConstraintsArgs args)
	{
		if (type.Kind == SymbolKind.NamedType)
		{
			((NamedTypeSymbol)type).CheckConstraints(in args);
		}
		else if (type.Kind == SymbolKind.PointerType && args.CurrentCompilation != null)
		{
			Binder.CheckManagedAddr(args.CurrentCompilation, ((PointerTypeSymbol)type).PointedAtType, args.Location, args.Diagnostics);
		}
		return false;
	}

	public static void CheckConstraints(this NamedTypeSymbol tuple, in CheckConstraintsArgs args, SyntaxNode typeSyntax, ImmutableArray<Location> elementLocations, BindingDiagnosticBag nullabilityDiagnosticsOpt)
	{
		if (!RequiresChecking(tuple) || typeSyntax.HasErrors)
		{
			return;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> instance2 = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<NamedTypeSymbol> instance3 = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		NamedTypeSymbol.GetUnderlyingTypeChain(tuple, instance3);
		int offset = 0;
		foreach (NamedTypeSymbol item in instance3)
		{
			ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
			CheckTypeConstraints(item, in args, instance, (nullabilityDiagnosticsOpt == null) ? null : instance2, ref useSiteDiagnosticsBuilder);
			if (useSiteDiagnosticsBuilder != null)
			{
				instance.AddRange(useSiteDiagnosticsBuilder);
			}
			populateDiagnosticsAndClear(instance, args.Diagnostics);
			populateDiagnosticsAndClear(instance2, nullabilityDiagnosticsOpt);
			offset += 7;
		}
		instance3.Free();
		instance.Free();
		instance2.Free();
		void populateDiagnosticsAndClear(ArrayBuilder<TypeParameterDiagnosticInfo> builder, BindingDiagnosticBag bag)
		{
			if (bag == null)
			{
				builder.Clear();
			}
			else
			{
				foreach (TypeParameterDiagnosticInfo item2 in builder)
				{
					int ordinal = item2.TypeParameter.Ordinal;
					Location location = ((ordinal == 7) ? typeSyntax.Location : elementLocations[ordinal + offset]);
					bag.Add(item2.UseSiteInfo, location);
				}
				builder.Clear();
			}
		}
	}

	public static bool CheckConstraintsForNamedType(this NamedTypeSymbol type, in CheckConstraintsArgs args, SyntaxNode typeSyntax, SeparatedSyntaxList<TypeSyntax> typeArgumentsSyntax, ConsList<TypeSymbol> basesBeingResolved)
	{
		if (!RequiresChecking(type))
		{
			return true;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool result = !typeSyntax.HasErrors && CheckTypeConstraints(type, in args, instance, args.IncludeNullability ? instance : null, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			int ordinal = item.TypeParameter.Ordinal;
			Location location = ((ordinal < typeArgumentsSyntax.Count) ? typeArgumentsSyntax[ordinal].Location : args.Location);
			args.Diagnostics.Add(item.UseSiteInfo, location);
		}
		instance.Free();
		if (HasDuplicateInterfaces(type, basesBeingResolved))
		{
			result = false;
			args.Diagnostics.Add(ErrorCode.ERR_BogusType, args.Location, type);
		}
		return result;
	}

	public static bool CheckConstraints(this NamedTypeSymbol type, in CheckConstraintsArgs args)
	{
		if (!RequiresChecking(type))
		{
			return true;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool result = CheckTypeConstraints(type, in args, instance, args.IncludeNullability ? instance : null, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			args.Diagnostics.Add(item.UseSiteInfo, args.Location);
		}
		instance.Free();
		if ((args.CurrentCompilation == null || !type.IsFromCompilation(args.CurrentCompilation)) && HasDuplicateInterfaces(type, null))
		{
			result = false;
			args.Diagnostics.Add(ErrorCode.ERR_BogusType, args.Location, type);
		}
		return result;
	}

	private static bool HasDuplicateInterfaces(NamedTypeSymbol type, ConsList<TypeSymbol> basesBeingResolved)
	{
		if (!(type.OriginalDefinition is PENamedTypeSymbol))
		{
			return false;
		}
		ImmutableArray<NamedTypeSymbol> immutableArray = type.OriginalDefinition.InterfacesNoUseSiteDiagnostics(basesBeingResolved);
		switch (immutableArray.Length)
		{
		case 0:
		case 1:
			return false;
		case 2:
			if ((object)immutableArray[0].OriginalDefinition != immutableArray[1].OriginalDefinition)
			{
				return false;
			}
			break;
		default:
		{
			PooledHashSet<object> instance = PooledHashSet<object>.GetInstance();
			ImmutableArray<NamedTypeSymbol>.Enumerator enumerator = immutableArray.GetEnumerator();
			NamedTypeSymbol current;
			do
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					continue;
				}
				instance.Free();
				return false;
			}
			while (instance.Add(current.OriginalDefinition));
			instance.Free();
			break;
		}
		}
		return ImmutableArrayExtensions.HasDuplicates(type.InterfacesNoUseSiteDiagnostics(basesBeingResolved), SymbolEqualityComparer.IgnoringDynamicTupleNamesAndNullability);
	}

	public static bool CheckConstraints(this MethodSymbol method, in CheckConstraintsArgs args)
	{
		if (!RequiresChecking(method))
		{
			return true;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool result = CheckMethodConstraints(method, in args, instance, args.IncludeNullability ? instance : null, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			args.Diagnostics.Add(item.UseSiteInfo, args.Location);
		}
		instance.Free();
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CheckTypeConstraints(NamedTypeSymbol type, in CheckConstraintsArgs args, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ArrayBuilder<TypeParameterDiagnosticInfo> nullabilityDiagnosticsBuilderOpt, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder)
	{
		return type.CheckConstraints(in args, type.TypeSubstitution, type.OriginalDefinition.TypeParameters, type.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics, diagnosticsBuilder, nullabilityDiagnosticsBuilderOpt, ref useSiteDiagnosticsBuilder);
	}

	public static bool CheckMethodConstraints(MethodSymbol method, in CheckConstraintsArgs args, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ArrayBuilder<TypeParameterDiagnosticInfo> nullabilityDiagnosticsBuilderOpt, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder, BitVector skipParameters = default(BitVector))
	{
		return method.CheckConstraints(in args, method.TypeSubstitution, method.OriginalDefinition.TypeParameters, method.TypeArgumentsWithAnnotations, diagnosticsBuilder, nullabilityDiagnosticsBuilderOpt, ref useSiteDiagnosticsBuilder, skipParameters);
	}

	public static bool CheckConstraints(this Symbol constructedContainingSymbol, in CheckConstraintsArgs args, TypeMap substitution, ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeWithAnnotations> typeArguments, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ArrayBuilder<TypeParameterDiagnosticInfo> nullabilityDiagnosticsBuilderOpt, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder, BitVector skipParameters = default(BitVector), HashSet<TypeParameterSymbol> ignoreTypeConstraintsDependentOnTypeParametersOpt = null)
	{
		bool flag = true;
		if (typeParameters.Length > 0 && substitution != null)
		{
			int length = typeParameters.Length;
			for (int i = 0; i < length; i++)
			{
				if (!skipParameters[i] && !CheckConstraints(constructedContainingSymbol, in args, substitution, typeParameters[i], typeArguments[i], diagnosticsBuilder, nullabilityDiagnosticsBuilderOpt, ref useSiteDiagnosticsBuilder, ignoreTypeConstraintsDependentOnTypeParametersOpt))
				{
					flag = false;
				}
			}
		}
		if (constructedContainingSymbol.IsExtensionBlockMember())
		{
			NamedTypeSymbol containingType = constructedContainingSymbol.ContainingType;
			if ((object)containingType != null && containingType.Arity > 0 && containingType.TypeSubstitution != null)
			{
				flag &= containingType.CheckConstraints(in args, containingType.TypeSubstitution, containingType.TypeParameters, containingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics, diagnosticsBuilder, nullabilityDiagnosticsBuilderOpt, ref useSiteDiagnosticsBuilder);
			}
		}
		return flag;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool CheckBasicConstraints(Symbol containingSymbol, in CheckConstraintsArgs args, TypeParameterSymbol typeParameter, TypeWithAnnotations typeArgument, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ArrayBuilder<TypeParameterDiagnosticInfo> nullabilityDiagnosticsBuilderOpt, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder)
	{
		if (typeArgument.Type.IsPointerOrFunctionPointer() || typeArgument.IsRestrictedType(ignoreSpanLikeTypes: true) || typeArgument.IsVoidType())
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BadTypeArgument, typeArgument.Type))));
			return false;
		}
		if (typeArgument.Type.IsRefLikeOrAllowsRefLikeType())
		{
			if (!typeParameter.AllowsRefLikeType)
			{
				diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument.Type))));
				return false;
			}
			if (args.CurrentCompilation != null && args.CurrentCompilation.SourceModule != typeParameter.ContainingModule)
			{
				CSDiagnosticInfo featureAvailabilityDiagnosticInfo = MessageID.IDS_FeatureAllowsRefStructConstraint.GetFeatureAvailabilityDiagnosticInfo(args.CurrentCompilation);
				if (featureAvailabilityDiagnosticInfo != null)
				{
					diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(featureAvailabilityDiagnosticInfo)));
				}
				if (!args.CurrentCompilation.Assembly.RuntimeSupportsByRefLikeGenerics)
				{
					diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics))));
				}
			}
		}
		if (typeArgument.IsStatic)
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_GenericArgIsStaticClass, typeArgument.Type))));
			return false;
		}
		if (typeParameter.HasReferenceTypeConstraint && !typeArgument.Type.IsReferenceType)
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_RefConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument.Type))));
			return false;
		}
		CheckNullability(containingSymbol, typeParameter, typeArgument, nullabilityDiagnosticsBuilderOpt);
		if (typeParameter.HasUnmanagedTypeConstraint)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(args.Template);
			ManagedKind managedKind = typeArgument.Type.GetManagedKind(ref useSiteInfo);
			AppendUseSiteDiagnostics(useSiteInfo, typeParameter, ref useSiteDiagnosticsBuilder);
			if (managedKind == ManagedKind.Managed || !typeArgument.Type.IsNonNullableValueType())
			{
				diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_UnmanagedConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument.Type))));
				return false;
			}
			if (managedKind == ManagedKind.UnmanagedWithGenerics && args.CurrentCompilation != null)
			{
				CSDiagnosticInfo featureAvailabilityDiagnosticInfo2 = MessageID.IDS_FeatureUnmanagedConstructedTypes.GetFeatureAvailabilityDiagnosticInfo(args.CurrentCompilation);
				if (featureAvailabilityDiagnosticInfo2 != null)
				{
					diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(featureAvailabilityDiagnosticInfo2)));
					return false;
				}
			}
		}
		if (typeParameter.HasValueTypeConstraint && !typeArgument.Type.IsNonNullableValueType())
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_ValConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument.Type))));
			return false;
		}
		return true;
	}

	private static bool CheckConstraints(Symbol constructedContainingSymbol, in CheckConstraintsArgs args, TypeMap substitution, TypeParameterSymbol typeParameter, TypeWithAnnotations typeArgument, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ArrayBuilder<TypeParameterDiagnosticInfo> nullabilityDiagnosticsBuilderOpt, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder, HashSet<TypeParameterSymbol> ignoreTypeConstraintsDependentOnTypeParametersOpt)
	{
		if (typeArgument.Type.IsErrorType())
		{
			return true;
		}
		if (!CheckBasicConstraints(constructedContainingSymbol, in args, typeParameter, typeArgument, diagnosticsBuilder, nullabilityDiagnosticsBuilderOpt, ref useSiteDiagnosticsBuilder))
		{
			return false;
		}
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(args.Template);
		ImmutableArray<TypeWithAnnotations> original = typeParameter.ConstraintTypesWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		substitution.SubstituteConstraintTypesDistinctWithoutModifiers(typeParameter, original, instance, ignoreTypeConstraintsDependentOnTypeParametersOpt);
		bool hasError = false;
		if (typeArgument.Type is NamedTypeSymbol { IsInterface: not false } namedTypeSymbol && SelfOrBaseHasStaticAbstractMember(namedTypeSymbol, ref useSiteInfo, out var memberWithoutImplementation))
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_GenericConstraintNotSatisfiedInterfaceWithStaticAbstractMembers, namedTypeSymbol, memberWithoutImplementation))));
			hasError = true;
		}
		foreach (TypeWithAnnotations item in instance)
		{
			CheckConstraintType(constructedContainingSymbol, in args, typeParameter, typeArgument, diagnosticsBuilder, nullabilityDiagnosticsBuilderOpt, ref useSiteInfo, item, ref hasError);
		}
		instance.Free();
		if (AppendUseSiteDiagnostics(useSiteInfo, typeParameter, ref useSiteDiagnosticsBuilder))
		{
			hasError = true;
		}
		if (typeParameter.HasConstructorConstraint && errorIfNotSatisfiesConstructorConstraint(constructedContainingSymbol, typeParameter, typeArgument, diagnosticsBuilder))
		{
			return false;
		}
		return !hasError;
		[MethodImpl(MethodImplOptions.NoInlining)]
		static bool errorIfNotSatisfiesConstructorConstraint(Symbol containingSymbol, TypeParameterSymbol typeParameterSymbol, TypeWithAnnotations typeWithAnnotations, ArrayBuilder<TypeParameterDiagnosticInfo> arrayBuilder)
		{
			ConstructorConstraintError constructorConstraintError = SatisfiesConstructorConstraint(typeWithAnnotations.Type);
			switch (constructorConstraintError)
			{
			case ConstructorConstraintError.None:
				return false;
			case ConstructorConstraintError.NoPublicParameterlessConstructorOrAbstractType:
				arrayBuilder.Add(new TypeParameterDiagnosticInfo(typeParameterSymbol, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_NewConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameterSymbol, typeWithAnnotations.Type))));
				return true;
			case ConstructorConstraintError.HasRequiredMembers:
				arrayBuilder.Add(new TypeParameterDiagnosticInfo(typeParameterSymbol, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_NewConstraintCannotHaveRequiredMembers, containingSymbol.ConstructedFrom(), typeParameterSymbol, typeWithAnnotations.Type))));
				return true;
			default:
				throw ExceptionUtilities.UnexpectedValue(constructorConstraintError);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CheckNullability(Symbol containingSymbol, TypeParameterSymbol typeParameter, TypeWithAnnotations typeArgument, ArrayBuilder<TypeParameterDiagnosticInfo> nullabilityDiagnosticsBuilderOpt)
	{
		if (nullabilityDiagnosticsBuilderOpt != null)
		{
			if (typeParameter.HasNotNullConstraint && typeArgument.GetValueNullableAnnotation().IsAnnotated() && !typeArgument.Type.IsNonNullableValueType())
			{
				nullabilityDiagnosticsBuilderOpt.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.WRN_NullabilityMismatchInTypeParameterNotNullConstraint, containingSymbol.ConstructedFrom(), typeParameter, typeArgument))));
			}
			if (typeParameter.HasReferenceTypeConstraint && typeParameter.ReferenceTypeConstraintIsNullable == false && typeArgument.GetValueNullableAnnotation().IsAnnotated())
			{
				nullabilityDiagnosticsBuilderOpt.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.WRN_NullabilityMismatchInTypeParameterReferenceTypeConstraint, containingSymbol.ConstructedFrom(), typeParameter, typeArgument))));
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CheckConstraintType(Symbol containingSymbol, in CheckConstraintsArgs args, TypeParameterSymbol typeParameter, TypeWithAnnotations typeArgument, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ArrayBuilder<TypeParameterDiagnosticInfo> nullabilityDiagnosticsBuilderOpt, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, TypeWithAnnotations constraintType, ref bool hasError)
	{
		if (SatisfiesConstraintType(args.Conversions.WithNullability(includeNullability: false), typeArgument, constraintType, ref useSiteInfo))
		{
			if (nullabilityDiagnosticsBuilderOpt != null && (!SatisfiesConstraintType(args.Conversions.WithNullability(includeNullability: true), typeArgument, constraintType, ref useSiteInfo) || !constraintTypeAllows(in constraintType, getTypeArgumentState(in typeArgument))))
			{
				nullabilityDiagnosticsBuilderOpt.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, containingSymbol.ConstructedFrom(), constraintType, typeParameter, typeArgument))));
			}
			return;
		}
		ErrorCode code = (typeArgument.Type.IsReferenceType ? ErrorCode.ERR_GenericConstraintNotSatisfiedRefType : (typeArgument.IsNullableType() ? (constraintType.Type.IsInterfaceType() ? ErrorCode.ERR_GenericConstraintNotSatisfiedNullableInterface : ErrorCode.ERR_GenericConstraintNotSatisfiedNullableEnum) : ((typeArgument.TypeKind != TypeKind.TypeParameter) ? ErrorCode.ERR_GenericConstraintNotSatisfiedValType : ErrorCode.ERR_GenericConstraintNotSatisfiedTyVar)));
		object obj;
		object obj2;
		if (constraintType.Type.Equals(typeArgument.Type, TypeCompareKind.AllIgnoreOptions))
		{
			obj = constraintType.Type;
			obj2 = typeArgument.Type;
		}
		else
		{
			SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(args.CurrentCompilation, constraintType.Type, typeArgument.Type);
			obj = symbolDistinguisher.First;
			obj2 = symbolDistinguisher.Second;
		}
		diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(code, containingSymbol.ConstructedFrom(), obj, typeParameter, obj2))));
		hasError = true;
		static bool constraintTypeAllows(in TypeWithAnnotations typeWithAnnotations, NullableFlowState state)
		{
			if (state == NullableFlowState.NotNull)
			{
				return true;
			}
			TypeSymbol type = typeWithAnnotations.Type;
			if ((object)type == null || type.IsValueType)
			{
				return true;
			}
			NullableAnnotation nullableAnnotation = typeWithAnnotations.NullableAnnotation;
			if (nullableAnnotation - 1 <= NullableAnnotation.Oblivious)
			{
				return true;
			}
			if (!(type is TypeParameterSymbol { IsNotNullable: var isNotNullable } typeParameterSymbol) || isNotNullable == true)
			{
				return false;
			}
			foreach (TypeWithAnnotations constraintTypesNoUseSiteDiagnostic in typeParameterSymbol.ConstraintTypesNoUseSiteDiagnostics)
			{
				if (!constraintTypeAllows(constraintTypesNoUseSiteDiagnostic, state))
				{
					return false;
				}
			}
			return state == NullableFlowState.MaybeNull;
		}
		static NullableFlowState getTypeArgumentState(in TypeWithAnnotations typeWithAnnotations)
		{
			TypeSymbol type = typeWithAnnotations.Type;
			if ((object)type == null)
			{
				return NullableFlowState.NotNull;
			}
			if (type.IsValueType)
			{
				if (!type.IsNullableTypeOrTypeParameter())
				{
					return NullableFlowState.NotNull;
				}
				return NullableFlowState.MaybeNull;
			}
			switch (typeWithAnnotations.NullableAnnotation)
			{
			case NullableAnnotation.Annotated:
				if (!type.IsTypeParameterDisallowingAnnotationInCSharp8())
				{
					return NullableFlowState.MaybeNull;
				}
				return NullableFlowState.MaybeDefault;
			case NullableAnnotation.Oblivious:
				return NullableFlowState.NotNull;
			default:
			{
				if (!(type is TypeParameterSymbol { IsNotNullable: var isNotNullable } typeParameterSymbol) || isNotNullable == true)
				{
					return NullableFlowState.NotNull;
				}
				NullableFlowState? nullableFlowState = null;
				foreach (TypeWithAnnotations constraintTypesNoUseSiteDiagnostic2 in typeParameterSymbol.ConstraintTypesNoUseSiteDiagnostics)
				{
					NullableFlowState nullableFlowState2 = getTypeArgumentState(constraintTypesNoUseSiteDiagnostic2);
					nullableFlowState = (nullableFlowState.HasValue ? new NullableFlowState?(nullableFlowState.Value.Meet(nullableFlowState2)) : new NullableFlowState?(nullableFlowState2));
				}
				return nullableFlowState ?? NullableFlowState.MaybeNull;
			}
			}
		}
	}

	private static bool AppendUseSiteDiagnostics(CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, TypeParameterSymbol typeParameter, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder)
	{
		if ((!useSiteInfo.AccumulatesDiagnostics || !useSiteInfo.HasErrors) && useSiteInfo.AccumulatesDependencies && !useSiteInfo.Dependencies.IsNullOrEmpty())
		{
			ensureUseSiteDiagnosticsBuilder(ref useSiteDiagnosticsBuilder).Add(new TypeParameterDiagnosticInfo(typeParameter, (useSiteInfo.Dependencies.Count == 1) ? new UseSiteInfo<AssemblySymbol>(useSiteInfo.Dependencies.Single()) : new UseSiteInfo<AssemblySymbol>(useSiteInfo.Dependencies.ToImmutableHashSet())));
		}
		if (!useSiteInfo.AccumulatesDiagnostics)
		{
			return false;
		}
		IReadOnlyCollection<DiagnosticInfo> diagnostics = useSiteInfo.Diagnostics;
		if (diagnostics.IsNullOrEmpty())
		{
			return false;
		}
		ensureUseSiteDiagnosticsBuilder(ref useSiteDiagnosticsBuilder);
		bool result = false;
		foreach (DiagnosticInfo item in diagnostics)
		{
			if (item.Severity == DiagnosticSeverity.Error)
			{
				result = true;
			}
			useSiteDiagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(item)));
		}
		return result;
		static ArrayBuilder<TypeParameterDiagnosticInfo> ensureUseSiteDiagnosticsBuilder(ref ArrayBuilder<TypeParameterDiagnosticInfo> reference)
		{
			return reference ?? (reference = new ArrayBuilder<TypeParameterDiagnosticInfo>());
		}
	}

	private static bool SatisfiesConstraintType(ConversionsBase conversions, TypeWithAnnotations typeArgument, TypeWithAnnotations constraintType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (constraintType.Type.IsErrorType())
		{
			return false;
		}
		if (conversions.HasIdentityOrImplicitReferenceConversion(typeArgument.Type, constraintType.Type, ref useSiteInfo))
		{
			return true;
		}
		if (typeArgument.Type.IsValueType)
		{
			if (conversions.HasBoxingConversion(typeArgument.Type.IsNullableType() ? ((NamedTypeSymbol)typeArgument.Type).ConstructedFrom : typeArgument.Type, constraintType.Type, ref useSiteInfo))
			{
				return true;
			}
			TypeSymbol type = typeArgument.Type;
			if (type is NamedTypeSymbol derivedType && type.IsRefLikeType && conversions.ImplementsVarianceCompatibleInterface(derivedType, constraintType.Type, ref useSiteInfo))
			{
				return true;
			}
		}
		if (typeArgument.TypeKind == TypeKind.TypeParameter)
		{
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)typeArgument.Type;
			if (conversions.HasImplicitTypeParameterConversion(typeParameterSymbol, constraintType.Type, ref useSiteInfo))
			{
				return true;
			}
			foreach (TypeWithAnnotations item in typeParameterSymbol.ConstraintTypesWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
			{
				if (SatisfiesConstraintType(conversions, item, constraintType, ref useSiteInfo))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool SelfOrBaseHasStaticAbstractMember(NamedTypeSymbol iface, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out Symbol memberWithoutImplementation)
	{
		foreach (Symbol member in iface.GetMembers())
		{
			if (member.IsStatic && member.IsImplementableInterfaceMember() && (object)iface.FindImplementationForInterfaceMember(member) == null)
			{
				memberWithoutImplementation = member;
				return true;
			}
		}
		foreach (NamedTypeSymbol key in iface.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Keys)
		{
			foreach (Symbol member2 in key.GetMembers())
			{
				if (member2.IsStatic && member2.IsImplementableInterfaceMember() && (object)iface.FindImplementationForInterfaceMember(member2) == null)
				{
					memberWithoutImplementation = member2;
					return true;
				}
			}
			key.OriginalDefinition.AddUseSiteInfo(ref useSiteInfo);
		}
		memberWithoutImplementation = null;
		return false;
	}

	private static bool IsReferenceType(TypeParameterSymbol typeParameter, ImmutableArray<TypeWithAnnotations> constraintTypes)
	{
		if (!typeParameter.HasReferenceTypeConstraint)
		{
			return TypeParameterSymbol.CalculateIsReferenceTypeFromConstraintTypes(constraintTypes);
		}
		return true;
	}

	private static bool IsValueType(TypeParameterSymbol typeParameter, ImmutableArray<TypeWithAnnotations> constraintTypes)
	{
		if (!typeParameter.HasValueTypeConstraint)
		{
			return TypeParameterSymbol.CalculateIsValueTypeFromConstraintTypes(constraintTypes);
		}
		return true;
	}

	private static TypeParameterDiagnosticInfo GenerateConflictingConstraintsError(TypeParameterSymbol typeParameter, TypeSymbol deducedBase, bool classConflict)
	{
		return new TypeParameterDiagnosticInfo(typeParameter, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BaseConstraintConflict, typeParameter, deducedBase, classConflict ? "class" : "struct")));
	}

	private static void AddInterfaces(ArrayBuilder<NamedTypeSymbol> builder, ImmutableArray<NamedTypeSymbol> interfaces)
	{
		foreach (NamedTypeSymbol item in interfaces)
		{
			AddInterface(builder, item);
		}
	}

	private static void AddInterface(ArrayBuilder<NamedTypeSymbol> builder, NamedTypeSymbol @interface)
	{
		if (!builder.Contains(@interface))
		{
			builder.Add(@interface);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static ConstructorConstraintError SatisfiesConstructorConstraint(TypeSymbol typeArgument)
	{
		switch (typeArgument.TypeKind)
		{
		case TypeKind.Struct:
			return SatisfiesPublicParameterlessConstructor((NamedTypeSymbol)typeArgument, synthesizedIfMissing: true);
		case TypeKind.Dynamic:
		case TypeKind.Enum:
			return ConstructorConstraintError.None;
		case TypeKind.Class:
			if (typeArgument.IsAbstract)
			{
				return ConstructorConstraintError.NoPublicParameterlessConstructorOrAbstractType;
			}
			return SatisfiesPublicParameterlessConstructor((NamedTypeSymbol)typeArgument, synthesizedIfMissing: false);
		case TypeKind.TypeParameter:
		{
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)typeArgument;
			if (!typeParameterSymbol.HasConstructorConstraint && !typeParameterSymbol.IsValueType)
			{
				return ConstructorConstraintError.NoPublicParameterlessConstructorOrAbstractType;
			}
			return ConstructorConstraintError.None;
		}
		case TypeKind.Submission:
			throw ExceptionUtilities.UnexpectedValue(typeArgument.TypeKind);
		default:
			return ConstructorConstraintError.NoPublicParameterlessConstructorOrAbstractType;
		}
	}

	private static ConstructorConstraintError SatisfiesPublicParameterlessConstructor(NamedTypeSymbol type, bool synthesizedIfMissing)
	{
		bool hasAnyRequiredMembers = type.HasAnyRequiredMembers;
		foreach (MethodSymbol instanceConstructor in type.InstanceConstructors)
		{
			if (instanceConstructor.ParameterCount == 0)
			{
				if (instanceConstructor.DeclaredAccessibility != Accessibility.Public)
				{
					return ConstructorConstraintError.NoPublicParameterlessConstructorOrAbstractType;
				}
				if (hasAnyRequiredMembers && instanceConstructor.ShouldCheckRequiredMembers())
				{
					return ConstructorConstraintError.HasRequiredMembers;
				}
				return ConstructorConstraintError.None;
			}
		}
		if (synthesizedIfMissing)
		{
			if (hasAnyRequiredMembers)
			{
				return ConstructorConstraintError.HasRequiredMembers;
			}
			return ConstructorConstraintError.None;
		}
		return ConstructorConstraintError.NoPublicParameterlessConstructorOrAbstractType;
	}

	private static bool IsEncompassedBy(ConversionsBase conversions, TypeSymbol a, TypeSymbol b, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!conversions.HasIdentityOrImplicitReferenceConversion(a, b, ref useSiteInfo))
		{
			return conversions.HasBoxingConversion(a, b, ref useSiteInfo);
		}
		return true;
	}

	private static bool IsValidEncompassedByArgument(TypeSymbol type)
	{
		TypeKind typeKind = type.TypeKind;
		if (typeKind - 1 <= TypeKind.Class || typeKind == TypeKind.Enum || typeKind == TypeKind.Struct)
		{
			return true;
		}
		return false;
	}

	public static bool RequiresChecking(NamedTypeSymbol type)
	{
		if (type.Arity == 0)
		{
			return false;
		}
		if ((object)type.OriginalDefinition == type)
		{
			return false;
		}
		return true;
	}

	public static bool RequiresChecking(MethodSymbol method)
	{
		if (method.GetMemberArityIncludingExtension() == 0)
		{
			return false;
		}
		if ((object)method.OriginalDefinition == method)
		{
			return false;
		}
		return true;
	}

	[Conditional("DEBUG")]
	private static void CheckEffectiveAndDeducedBaseTypes(ConversionsBase conversions, TypeSymbol effectiveBase, TypeSymbol deducedBase)
	{
		_ = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
	}

	internal static TypeWithAnnotations ConstraintWithMostSignificantNullability(TypeWithAnnotations type1, TypeWithAnnotations type2)
	{
		switch (type2.NullableAnnotation)
		{
		case NullableAnnotation.Annotated:
			return type1;
		case NullableAnnotation.NotAnnotated:
			return type2;
		case NullableAnnotation.Oblivious:
			if (type1.NullableAnnotation.IsNotAnnotated())
			{
				return type1;
			}
			return type2;
		default:
			throw ExceptionUtilities.UnexpectedValue(type2.NullableAnnotation);
		}
	}

	internal static bool IsObjectConstraint(TypeWithAnnotations type, ref TypeWithAnnotations bestObjectConstraint)
	{
		if (type.SpecialType == SpecialType.System_Object)
		{
			if (type.NullableAnnotation != NullableAnnotation.Annotated)
			{
				if (!bestObjectConstraint.HasType)
				{
					bestObjectConstraint = type;
				}
				else
				{
					bestObjectConstraint = ConstraintWithMostSignificantNullability(bestObjectConstraint, type);
				}
			}
			return true;
		}
		return false;
	}

	internal static bool IsObjectConstraintSignificant(bool? isNotNullable, TypeWithAnnotations objectConstraint)
	{
		if (isNotNullable.HasValue)
		{
			if (isNotNullable == true)
			{
				return false;
			}
		}
		else if (objectConstraint.NullableAnnotation.IsOblivious())
		{
			return false;
		}
		return true;
	}
}
