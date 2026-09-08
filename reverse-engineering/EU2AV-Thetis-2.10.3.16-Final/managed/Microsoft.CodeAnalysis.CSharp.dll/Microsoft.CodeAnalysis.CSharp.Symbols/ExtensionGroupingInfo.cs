using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ExtensionGroupingInfo
{
	private abstract class ExtensionGroupingOrMarkerType : INestedTypeDefinition, INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, ITypeDefinitionMember, ITypeMemberReference, INestedTypeReference
	{
		ushort ITypeDefinition.Alignment => 0;

		IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters => GenericParameters;

		protected abstract IEnumerable<IGenericTypeParameter> GenericParameters { get; }

		ushort ITypeDefinition.GenericParameterCount => GenericParameterCount;

		ushort INamedTypeReference.GenericParameterCount => GenericParameterCount;

		protected abstract ushort GenericParameterCount { get; }

		bool ITypeDefinition.HasDeclarativeSecurity => false;

		bool ITypeDefinition.IsAbstract => IsAbstract;

		protected abstract bool IsAbstract { get; }

		bool ITypeDefinition.IsBeforeFieldInit => false;

		bool ITypeDefinition.IsComObject => false;

		bool ITypeDefinition.IsGeneric => GenericParameterCount != 0;

		bool ITypeDefinition.IsInterface => false;

		bool ITypeDefinition.IsDelegate => false;

		bool ITypeDefinition.IsRuntimeSpecial => false;

		bool ITypeDefinition.IsSerializable => false;

		bool ITypeDefinition.IsSpecialName => true;

		bool ITypeDefinition.IsWindowsRuntimeImport => false;

		bool ITypeDefinition.IsSealed => IsSealed;

		protected abstract bool IsSealed { get; }

		LayoutKind ITypeDefinition.Layout => LayoutKind.Auto;

		IEnumerable<SecurityAttribute> ITypeDefinition.SecurityAttributes => SpecializedCollections.EmptyEnumerable<SecurityAttribute>();

		uint ITypeDefinition.SizeOf => 0u;

		CharSet ITypeDefinition.StringFormat => CharSet.Ansi;

		ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingTypeDefinition;

		protected abstract ITypeDefinition ContainingTypeDefinition { get; }

		TypeMemberVisibility ITypeDefinitionMember.Visibility => TypeMemberVisibility.Public;

		bool IDefinition.IsEncDeleted => false;

		bool INamedTypeReference.MangleName => MangleName;

		protected abstract bool MangleName { get; }

		string? INamedTypeReference.AssociatedFileIdentifier => null;

		bool ITypeReference.IsEnum => false;

		bool ITypeReference.IsValueType => false;

		Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

		TypeDefinitionHandle ITypeReference.TypeDef => default(TypeDefinitionHandle);

		IGenericMethodParameterReference? ITypeReference.AsGenericMethodParameterReference => null;

		IGenericTypeInstanceReference? ITypeReference.AsGenericTypeInstanceReference => null;

		IGenericTypeParameterReference? ITypeReference.AsGenericTypeParameterReference => null;

		INamespaceTypeReference? ITypeReference.AsNamespaceTypeReference => null;

		INestedTypeReference? ITypeReference.AsNestedTypeReference => this;

		ISpecializedNestedTypeReference? ITypeReference.AsSpecializedNestedTypeReference => null;

		string? INamedEntity.Name => Name;

		public abstract string Name { get; }

		bool INestedTypeReference.InheritsEnclosingTypeTypeParameters => false;

		protected abstract ITypeReference? ObjectType { get; }

		protected abstract IEnumerable<INestedTypeDefinition> NestedTypes { get; }

		IDefinition? IReference.AsDefinition(EmitContext context)
		{
			return this;
		}

		INamespaceTypeDefinition? ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
		{
			return null;
		}

		INestedTypeDefinition? ITypeReference.AsNestedTypeDefinition(EmitContext context)
		{
			return this;
		}

		ITypeDefinition? ITypeReference.AsTypeDefinition(EmitContext context)
		{
			return this;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit((ITypeDefinition)this);
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			return GetAttributes(context);
		}

		protected abstract IEnumerable<ICustomAttribute> GetAttributes(EmitContext context);

		ITypeReference? ITypeDefinition.GetBaseClass(EmitContext context)
		{
			return ObjectType;
		}

		ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
		{
			return ContainingTypeDefinition;
		}

		IEnumerable<IEventDefinition> ITypeDefinition.GetEvents(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<IEventDefinition>();
		}

		IEnumerable<Microsoft.Cci.MethodImplementation> ITypeDefinition.GetExplicitImplementationOverrides(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<Microsoft.Cci.MethodImplementation>();
		}

		IEnumerable<IFieldDefinition> ITypeDefinition.GetFields(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<IFieldDefinition>();
		}

		ISymbolInternal? IReference.GetInternalSymbol()
		{
			return null;
		}

		IEnumerable<IMethodDefinition> ITypeDefinition.GetMethods(EmitContext context)
		{
			return GetMethods(context);
		}

		protected abstract IEnumerable<IMethodDefinition> GetMethods(EmitContext context);

		IEnumerable<INestedTypeDefinition> ITypeDefinition.GetNestedTypes(EmitContext context)
		{
			return NestedTypes;
		}

		IEnumerable<IPropertyDefinition> ITypeDefinition.GetProperties(EmitContext context)
		{
			return GetProperties(context);
		}

		protected abstract IEnumerable<IPropertyDefinition> GetProperties(EmitContext context);

		ITypeDefinition? ITypeReference.GetResolvedType(EmitContext context)
		{
			return this;
		}

		IEnumerable<TypeReferenceWithAttributes> ITypeDefinition.Interfaces(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<TypeReferenceWithAttributes>();
		}

		public sealed override bool Equals(object? obj)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/ExtensionGroupingInfo.cs", 672);
		}

		public sealed override int GetHashCode()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/ExtensionGroupingInfo.cs", 678);
		}
	}

	private sealed class ExtensionGroupingType : ExtensionGroupingOrMarkerType, IComparable<ExtensionGroupingType>
	{
		private readonly string _name;

		public readonly ImmutableArray<ExtensionMarkerType> ExtensionMarkerTypes;

		private ImmutableArray<ExtensionGroupingTypeTypeParameter> _lazyTypeParameters;

		protected override IEnumerable<IGenericTypeParameter> GenericParameters
		{
			get
			{
				if (_lazyTypeParameters.IsDefault)
				{
					ImmutableArray<ExtensionGroupingTypeTypeParameter> value = ((ExtensionMarkerTypes[0].UnderlyingExtensions[0].Arity != 0) ? ((ITypeDefinition)ExtensionMarkerTypes[0].UnderlyingExtensions[0].GetCciAdapter()).GenericParameters.SelectAsArray((IGenericTypeParameter p, ExtensionGroupingType @this) => new ExtensionGroupingTypeTypeParameter(@this, p), this) : ImmutableArray<ExtensionGroupingTypeTypeParameter>.Empty);
					ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameters, value);
				}
				return _lazyTypeParameters;
			}
		}

		protected override ushort GenericParameterCount => (ushort)ExtensionMarkerTypes[0].UnderlyingExtensions[0].Arity;

		protected override bool IsAbstract => false;

		protected override bool IsSealed => true;

		protected override ITypeDefinition ContainingTypeDefinition => ExtensionMarkerTypes[0].UnderlyingExtensions[0].ContainingType.GetCciAdapter();

		public override string Name => _name;

		protected override ITypeReference? ObjectType => ExtensionMarkerTypes[0].UnderlyingExtensions[0].ContainingAssembly.GetSpecialType(SpecialType.System_Object).GetCciAdapter();

		protected override IEnumerable<INestedTypeDefinition> NestedTypes => ExtensionMarkerTypes;

		protected override bool MangleName => GenericParameterCount != 0;

		public ExtensionGroupingType(string name, MultiDictionary<string, SourceNamedTypeSymbol> extensionMarkerTypes)
		{
			_name = name;
			ArrayBuilder<ExtensionMarkerType> instance = ArrayBuilder<ExtensionMarkerType>.GetInstance(extensionMarkerTypes.Count);
			foreach (KeyValuePair<string, MultiDictionary<string, SourceNamedTypeSymbol>.ValueSet> extensionMarkerType in extensionMarkerTypes)
			{
				instance.Add(new ExtensionMarkerType(this, extensionMarkerType.Key, extensionMarkerType.Value));
			}
			instance.Sort();
			ExtensionMarkerTypes = instance.ToImmutableAndFree();
		}

		int IComparable<ExtensionGroupingType>.CompareTo(ExtensionGroupingType? other)
		{
			return ExtensionMarkerTypes[0].CompareTo(other.ExtensionMarkerTypes[0]);
		}

		protected override IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
		{
			foreach (ExtensionMarkerType extensionMarkerType in ExtensionMarkerTypes)
			{
				foreach (SourceNamedTypeSymbol underlyingExtension in extensionMarkerType.UnderlyingExtensions)
				{
					foreach (MethodSymbol item in underlyingExtension.GetMethodsToEmit())
					{
						if (item.GetCciAdapter().ShouldInclude(context))
						{
							yield return item.GetCciAdapter();
						}
					}
				}
			}
		}

		protected override IEnumerable<IPropertyDefinition> GetProperties(EmitContext context)
		{
			foreach (ExtensionMarkerType extensionMarkerType in ExtensionMarkerTypes)
			{
				foreach (SourceNamedTypeSymbol underlyingExtension in extensionMarkerType.UnderlyingExtensions)
				{
					foreach (PropertySymbol item in underlyingExtension.GetPropertiesToEmit())
					{
						IPropertyDefinition cciAdapter = item.GetCciAdapter();
						if (cciAdapter.ShouldInclude(context) || !cciAdapter.GetAccessors(context).IsEmpty())
						{
							yield return cciAdapter;
						}
					}
				}
			}
		}

		protected override IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
		{
			SynthesizedAttributeData synthesizedAttributeData = ExtensionMarkerTypes[0].UnderlyingExtensions[0].DeclaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor);
			if (synthesizedAttributeData != null)
			{
				yield return synthesizedAttributeData;
			}
		}
	}

	private sealed class ExtensionGroupingTypeTypeParameter : InheritedTypeParameter
	{
		public override string? Name => "$T" + base.Index;

		internal ExtensionGroupingTypeTypeParameter(ExtensionGroupingType inheritingType, IGenericTypeParameter parentParameter)
			: base(parentParameter.Index, inheritingType, parentParameter)
		{
		}

		public override IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context)
		{
			foreach (TypeReferenceWithAttributes constraint in base.GetConstraints(context))
			{
				yield return new TypeReferenceWithAttributes(constraint.TypeRef);
			}
		}

		public override IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
		{
			if (base.MustBeValueType)
			{
				Symbol symbol = ((PEModuleBuilder)context.Module).TryGetSynthesizedIsUnmanagedAttribute()?.Constructors[0] ?? ((ExtensionGroupingType)base.DefiningType).ExtensionMarkerTypes[0].UnderlyingExtensions[0].DeclaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_IsUnmanagedAttribute__ctor);
				if ((object)symbol != null)
				{
					foreach (ICustomAttribute attribute in base.GetAttributes(context))
					{
						if (attribute is SynthesizedAttributeData synthesizedAttributeData && synthesizedAttributeData.AttributeConstructor == symbol)
						{
							return new _003C_003Ez__ReadOnlySingleElementList<ICustomAttribute>(synthesizedAttributeData);
						}
					}
				}
			}
			return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
		}
	}

	private sealed class ExtensionMarkerType : ExtensionGroupingOrMarkerType, IComparable<ExtensionMarkerType>
	{
		public readonly ExtensionGroupingType GroupingType;

		private readonly string _name;

		public readonly ImmutableArray<SourceNamedTypeSymbol> UnderlyingExtensions;

		private ImmutableArray<InheritedTypeParameter> _lazyTypeParameters;

		protected override IEnumerable<IGenericTypeParameter> GenericParameters
		{
			get
			{
				if (_lazyTypeParameters.IsDefault)
				{
					ImmutableArray<InheritedTypeParameter> value = ((UnderlyingExtensions[0].Arity != 0) ? ((ITypeDefinition)UnderlyingExtensions[0].GetCciAdapter()).GenericParameters.SelectAsArray((IGenericTypeParameter p, ExtensionMarkerType @this) => new InheritedTypeParameter(p.Index, @this, p), this) : ImmutableArray<InheritedTypeParameter>.Empty);
					ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameters, value);
				}
				return _lazyTypeParameters;
			}
		}

		protected override ushort GenericParameterCount => (ushort)UnderlyingExtensions[0].Arity;

		protected override bool IsAbstract => true;

		protected override bool IsSealed => true;

		protected override ITypeDefinition ContainingTypeDefinition => GroupingType;

		public override string Name => _name;

		protected override ITypeReference? ObjectType => UnderlyingExtensions[0].ContainingAssembly.GetSpecialType(SpecialType.System_Object).GetCciAdapter();

		protected override IEnumerable<INestedTypeDefinition> NestedTypes => SpecializedCollections.EmptyEnumerable<INestedTypeDefinition>();

		protected override bool MangleName => false;

		public ExtensionMarkerType(ExtensionGroupingType groupingType, string name, MultiDictionary<string, SourceNamedTypeSymbol>.ValueSet extensions)
		{
			GroupingType = groupingType;
			_name = name;
			ArrayBuilder<SourceNamedTypeSymbol> instance = ArrayBuilder<SourceNamedTypeSymbol>.GetInstance(extensions.Count);
			instance.AddRange(extensions);
			instance.Sort(LexicalOrderSymbolComparer.Instance);
			UnderlyingExtensions = instance.ToImmutableAndFree();
		}

		public int CompareTo(ExtensionMarkerType? other)
		{
			return LexicalOrderSymbolComparer.Instance.Compare(UnderlyingExtensions[0], other.UnderlyingExtensions[0]);
		}

		protected override IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
		{
			MethodSymbol methodSymbol = UnderlyingExtensions[0].TryGetOrCreateExtensionMarker();
			if ((object)methodSymbol != null)
			{
				yield return methodSymbol.GetCciAdapter();
			}
		}

		protected override IEnumerable<IPropertyDefinition> GetProperties(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<IPropertyDefinition>();
		}

		protected override IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
		}
	}

	private readonly ImmutableArray<ExtensionGroupingType> _groupingTypes;

	public ExtensionGroupingInfo(SourceMemberContainerTypeSymbol container)
	{
		Dictionary<string, MultiDictionary<string, SourceNamedTypeSymbol>> dictionary = new Dictionary<string, MultiDictionary<string, SourceNamedTypeSymbol>>(EqualityComparer<string>.Default);
		foreach (NamedTypeSymbol typeMember in container.GetTypeMembers(""))
		{
			if (typeMember.IsExtension)
			{
				SourceNamedTypeSymbol sourceNamedTypeSymbol = (SourceNamedTypeSymbol)typeMember;
				string extensionGroupingName = sourceNamedTypeSymbol.ExtensionGroupingName;
				if (!dictionary.TryGetValue(extensionGroupingName, out var value))
				{
					value = new MultiDictionary<string, SourceNamedTypeSymbol>(EqualityComparer<string>.Default, ReferenceEqualityComparer.Instance);
					dictionary.Add(extensionGroupingName, value);
				}
				value.Add(sourceNamedTypeSymbol.ExtensionMarkerName, sourceNamedTypeSymbol);
			}
		}
		ArrayBuilder<ExtensionGroupingType> instance = ArrayBuilder<ExtensionGroupingType>.GetInstance(dictionary.Count);
		foreach (KeyValuePair<string, MultiDictionary<string, SourceNamedTypeSymbol>> item in dictionary)
		{
			instance.Add(new ExtensionGroupingType(item.Key, item.Value));
		}
		instance.Sort();
		_groupingTypes = instance.ToImmutableAndFree();
	}

	[Conditional("DEBUG")]
	private void AssertInvariants(SourceMemberContainerTypeSymbol container)
	{
		ImmutableArray<NamedTypeSymbol> typeMembers = container.GetTypeMembers("");
		for (int i = 0; i < typeMembers.Length; i++)
		{
			SourceNamedTypeSymbol sourceNamedTypeSymbol = (SourceNamedTypeSymbol)typeMembers[i];
			if (!sourceNamedTypeSymbol.IsExtension)
			{
				continue;
			}
			for (int j = i + 1; j < typeMembers.Length; j++)
			{
				SourceNamedTypeSymbol sourceNamedTypeSymbol2 = (SourceNamedTypeSymbol)typeMembers[j];
				if (sourceNamedTypeSymbol2.IsExtension)
				{
					_ = sourceNamedTypeSymbol.ComputeExtensionGroupingRawName() == sourceNamedTypeSymbol2.ComputeExtensionGroupingRawName();
					_ = sourceNamedTypeSymbol.ComputeExtensionMarkerRawName() == sourceNamedTypeSymbol2.ComputeExtensionMarkerRawName();
				}
			}
		}
	}

	public ImmutableArray<INestedTypeDefinition> GetGroupingTypes()
	{
		return ImmutableArray<INestedTypeDefinition>.CastUp(_groupingTypes);
	}

	public ITypeDefinition GetCorrespondingMarkerType(SynthesizedExtensionMarker markerMethod)
	{
		return GetCorrespondingMarkerType((SourceNamedTypeSymbol)markerMethod.ContainingType);
	}

	private ExtensionMarkerType GetCorrespondingMarkerType(SourceNamedTypeSymbol extension)
	{
		string extensionGroupingName = extension.ExtensionGroupingName;
		string extensionMarkerName = extension.ExtensionMarkerName;
		foreach (ExtensionGroupingType groupingType in _groupingTypes)
		{
			if (groupingType.Name != extensionGroupingName)
			{
				continue;
			}
			foreach (ExtensionMarkerType extensionMarkerType in groupingType.ExtensionMarkerTypes)
			{
				if (extensionMarkerType.Name == extensionMarkerName)
				{
					return extensionMarkerType;
				}
			}
			break;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/ExtensionGroupingInfo.cs", 139);
	}

	public TypeMemberVisibility GetCorrespondingMarkerMethodVisibility(SynthesizedExtensionMarker marker)
	{
		TypeMemberVisibility typeMemberVisibility = TypeMemberVisibility.Private;
		foreach (SourceNamedTypeSymbol underlyingExtension in GetCorrespondingMarkerType((SourceNamedTypeSymbol)marker.ContainingType).UnderlyingExtensions)
		{
			foreach (Symbol member in underlyingExtension.GetMembers())
			{
				TypeMemberVisibility metadataVisibility = member.MetadataVisibility;
				if (metadataVisibility == TypeMemberVisibility.Public)
				{
					return TypeMemberVisibility.Public;
				}
				if (typeMemberVisibility < metadataVisibility)
				{
					typeMemberVisibility = metadataVisibility;
				}
			}
		}
		return typeMemberVisibility;
	}

	public ITypeDefinition GetCorrespondingGroupingType(SourceNamedTypeSymbol extension)
	{
		string extensionGroupingName = extension.ExtensionGroupingName;
		foreach (ExtensionGroupingType groupingType in _groupingTypes)
		{
			if (groupingType.Name == extensionGroupingName)
			{
				return groupingType;
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/ExtensionGroupingInfo.cs", 188);
	}

	internal ImmutableArray<SourceNamedTypeSymbol> GetMergedExtensions(SourceNamedTypeSymbol extension)
	{
		return GetCorrespondingMarkerType(extension).UnderlyingExtensions;
	}

	internal IEnumerable<ImmutableArray<SourceNamedTypeSymbol>> EnumerateMergedExtensionBlocks()
	{
		foreach (ExtensionGroupingType groupingType in _groupingTypes)
		{
			foreach (ExtensionMarkerType extensionMarkerType in groupingType.ExtensionMarkerTypes)
			{
				yield return extensionMarkerType.UnderlyingExtensions;
			}
		}
	}

	internal static bool HaveSameILSignature(SourceNamedTypeSymbol extension1, SourceNamedTypeSymbol extension2)
	{
		if (extension1.Arity != extension2.Arity)
		{
			return false;
		}
		TypeMap typeMap = MemberSignatureComparer.GetTypeMap(extension1);
		TypeMap typeMap2 = MemberSignatureComparer.GetTypeMap(extension2);
		if (extension1.Arity > 0 && !MemberSignatureComparer.HaveSameConstraints(extension1.TypeParameters, typeMap, extension2.TypeParameters, typeMap2, TypeCompareKind.CLRSignatureCompareOptions))
		{
			return false;
		}
		ParameterSymbol extensionParameter = extension1.ExtensionParameter;
		ParameterSymbol extensionParameter2 = extension2.ExtensionParameter;
		if ((object)extensionParameter == null || (object)extensionParameter2 == null)
		{
			if ((object)extensionParameter == null)
			{
				return (object)extensionParameter2 == null;
			}
			return false;
		}
		if (!MemberSignatureComparer.HaveSameParameterType(extensionParameter, typeMap, extensionParameter2, typeMap2, MemberSignatureComparer.RefKindCompareMode.IgnoreRefKind, considerDefaultValues: false, TypeCompareKind.CLRSignatureCompareOptions))
		{
			return false;
		}
		return true;
	}

	internal static bool HaveSameCSharpSignature(SourceNamedTypeSymbol extension1, SourceNamedTypeSymbol extension2)
	{
		int arity = extension1.Arity;
		if (arity != extension2.Arity)
		{
			return false;
		}
		TypeMap typeMap = MemberSignatureComparer.GetTypeMap(extension1);
		TypeMap typeMap2 = MemberSignatureComparer.GetTypeMap(extension2);
		if (arity > 0)
		{
			ImmutableArray<TypeParameterSymbol> typeParameters = extension1.TypeParameters;
			ImmutableArray<TypeParameterSymbol> typeParameters2 = extension2.TypeParameters;
			if (!typeParameters.SequenceEqual(typeParameters2, (TypeParameterSymbol p1, TypeParameterSymbol p2) => p1.Name == p2.Name))
			{
				return false;
			}
			if (!typeParameters.SequenceEqual(typeParameters2, (TypeParameterSymbol p1, TypeParameterSymbol p2) => hasSameAttributes(p1.GetAttributes(), p2.GetAttributes())))
			{
				return false;
			}
			for (int num = 0; num < arity; num++)
			{
				if (!haveSameConstraints(typeParameters[num], typeMap, typeParameters2[num], typeMap2))
				{
					return false;
				}
			}
		}
		ParameterSymbol extensionParameter = extension1.ExtensionParameter;
		ParameterSymbol extensionParameter2 = extension2.ExtensionParameter;
		if ((object)extensionParameter == null)
		{
			return (object)extensionParameter2 == null;
		}
		if ((object)extensionParameter2 == null)
		{
			return (object)extensionParameter == null;
		}
		if (extensionParameter.DeclaredScope != extensionParameter2.DeclaredScope)
		{
			return false;
		}
		if (extensionParameter.Name != extensionParameter2.Name)
		{
			return false;
		}
		if (!MemberSignatureComparer.HaveSameParameterType(extensionParameter, typeMap, extensionParameter2, typeMap2, MemberSignatureComparer.RefKindCompareMode.ConsiderDifferences, considerDefaultValues: false, TypeCompareKind.ConsiderEverything))
		{
			return false;
		}
		if (!hasSameAttributes(extensionParameter.GetAttributes(), extensionParameter2.GetAttributes()))
		{
			return false;
		}
		return true;
		static bool areConstraintTypesSubset(HashSet<TypeWithAnnotations> constraintTypes1, HashSet<TypeWithAnnotations> constraintTypes2, TypeParameterSymbol typeParameter2)
		{
			foreach (TypeWithAnnotations item in constraintTypes1)
			{
				if (!constraintTypes2.Contains(item))
				{
					return false;
				}
			}
			return true;
		}
		static bool hasSameAttributes(ImmutableArray<CSharpAttributeData> attributes1, ImmutableArray<CSharpAttributeData> attributes2)
		{
			if (attributes1.IsEmpty && attributes2.IsEmpty)
			{
				return true;
			}
			Dictionary<CSharpAttributeData, int> dictionary = new Dictionary<CSharpAttributeData, int>(CommonAttributeDataComparer.InstanceIgnoringNamedArgumentOrder);
			foreach (CSharpAttributeData item2 in attributes1)
			{
				if (!item2.IsConditionallyOmitted)
				{
					dictionary[item2] = ((!dictionary.TryGetValue(item2, out var value)) ? 1 : (value + 1));
				}
			}
			foreach (CSharpAttributeData item3 in attributes2)
			{
				if (!item3.IsConditionallyOmitted)
				{
					if (!dictionary.TryGetValue(item3, out var value2) || value2 == 0)
					{
						return false;
					}
					dictionary[item3] = value2 - 1;
				}
			}
			return dictionary.Values.All((int c) => c == 0);
		}
		static bool haveSameConstraints(TypeParameterSymbol typeParameter1, TypeMap? typeMap3, TypeParameterSymbol typeParameter2, TypeMap? typeMap4)
		{
			if (typeParameter1.HasConstructorConstraint != typeParameter2.HasConstructorConstraint || typeParameter1.HasReferenceTypeConstraint != typeParameter2.HasReferenceTypeConstraint || typeParameter1.HasValueTypeConstraint != typeParameter2.HasValueTypeConstraint || typeParameter1.AllowsRefLikeType != typeParameter2.AllowsRefLikeType || typeParameter1.HasUnmanagedTypeConstraint != typeParameter2.HasUnmanagedTypeConstraint || typeParameter1.Variance != typeParameter2.Variance || typeParameter1.HasNotNullConstraint != typeParameter2.HasNotNullConstraint)
			{
				return false;
			}
			return haveSameTypeConstraints(typeParameter1, typeMap3, typeParameter2, typeMap4);
		}
		static bool haveSameTypeConstraints(TypeParameterSymbol typeParameter1, TypeMap? typeMap3, TypeParameterSymbol typeParameter2, TypeMap? typeMap4)
		{
			ImmutableArray<TypeWithAnnotations> constraintTypesNoUseSiteDiagnostics = typeParameter1.ConstraintTypesNoUseSiteDiagnostics;
			ImmutableArray<TypeWithAnnotations> constraintTypesNoUseSiteDiagnostics2 = typeParameter2.ConstraintTypesNoUseSiteDiagnostics;
			if (constraintTypesNoUseSiteDiagnostics.IsEmpty && constraintTypesNoUseSiteDiagnostics2.IsEmpty)
			{
				return true;
			}
			TypeWithAnnotations.EqualsComparer considerEverythingComparer = TypeWithAnnotations.EqualsComparer.ConsiderEverythingComparer;
			HashSet<TypeWithAnnotations> hashSet = new HashSet<TypeWithAnnotations>(considerEverythingComparer);
			HashSet<TypeWithAnnotations> hashSet2 = new HashSet<TypeWithAnnotations>(considerEverythingComparer);
			substituteConstraintTypes(constraintTypesNoUseSiteDiagnostics, typeMap3, hashSet);
			substituteConstraintTypes(constraintTypesNoUseSiteDiagnostics2, typeMap4, hashSet2);
			if (areConstraintTypesSubset(hashSet, hashSet2, typeParameter2))
			{
				return areConstraintTypesSubset(hashSet2, hashSet, typeParameter1);
			}
			return false;
		}
		static void substituteConstraintTypes(ImmutableArray<TypeWithAnnotations> types, TypeMap? typeMap3, HashSet<TypeWithAnnotations> result)
		{
			foreach (TypeWithAnnotations item4 in types)
			{
				result.Add(MemberSignatureComparer.SubstituteType(typeMap3, item4));
			}
		}
	}

	internal void CheckSignatureCollisions(BindingDiagnosticBag diagnostics)
	{
		PooledHashSet<SourceNamedTypeSymbol> alreadyReportedExtensions = null;
		foreach (ExtensionGroupingType groupingType in _groupingTypes)
		{
			checkCollisions(enumerateExtensionsInGrouping(groupingType), HaveSameILSignature, ref alreadyReportedExtensions, diagnostics);
		}
		foreach (ImmutableArray<SourceNamedTypeSymbol> item in EnumerateMergedExtensionBlocks())
		{
			checkCollisions(item, HaveSameCSharpSignature, ref alreadyReportedExtensions, diagnostics);
		}
		alreadyReportedExtensions?.Free();
		static void checkCollisions(IEnumerable<SourceNamedTypeSymbol> extensions, Func<SourceNamedTypeSymbol, SourceNamedTypeSymbol, bool> compare, ref PooledHashSet<SourceNamedTypeSymbol>? reference, BindingDiagnosticBag bindingDiagnosticBag)
		{
			SourceNamedTypeSymbol sourceNamedTypeSymbol = null;
			foreach (SourceNamedTypeSymbol extension in extensions)
			{
				if ((object)sourceNamedTypeSymbol == null)
				{
					sourceNamedTypeSymbol = extension;
				}
				else if (!compare(sourceNamedTypeSymbol, extension))
				{
					if (reference == null)
					{
						reference = PooledHashSet<SourceNamedTypeSymbol>.GetInstance();
					}
					if (reference.Add(extension))
					{
						bindingDiagnosticBag.Add(ErrorCode.ERR_ExtensionBlockCollision, extension.GetFirstLocation());
					}
				}
			}
		}
		static IEnumerable<SourceNamedTypeSymbol> enumerateExtensionsInGrouping(ExtensionGroupingType groupingType)
		{
			foreach (ExtensionMarkerType extensionMarkerType in groupingType.ExtensionMarkerTypes)
			{
				foreach (SourceNamedTypeSymbol underlyingExtension in extensionMarkerType.UnderlyingExtensions)
				{
					yield return underlyingExtension;
				}
			}
		}
	}
}
