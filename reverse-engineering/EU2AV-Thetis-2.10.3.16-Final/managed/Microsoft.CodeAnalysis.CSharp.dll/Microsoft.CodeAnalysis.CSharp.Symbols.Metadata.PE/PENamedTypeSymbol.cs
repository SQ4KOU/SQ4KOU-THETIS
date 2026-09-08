using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.DocumentationComments;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal abstract class PENamedTypeSymbol : NamedTypeSymbol
{
	private sealed class UncommonProperties
	{
		internal ImmutableArray<PEFieldSymbol> lazyInstanceEnumFields;

		internal NamedTypeSymbol lazyEnumUnderlyingType;

		internal ImmutableArray<CSharpAttributeData> lazyCustomAttributes;

		internal ImmutableArray<string> lazyConditionalAttributeSymbols;

		internal ObsoleteAttributeData lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

		internal AttributeUsageInfo lazyAttributeUsageInfo = AttributeUsageInfo.Null;

		internal ThreeState lazyContainsExtensionMethods;

		internal ThreeState lazyIsByRefLike;

		internal ThreeState lazyIsReadOnly;

		internal string lazyDefaultMemberName;

		internal NamedTypeSymbol lazyComImportCoClassType = ErrorTypeSymbol.UnknownResultType;

		internal CollectionBuilderAttributeData lazyCollectionBuilderAttributeData = CollectionBuilderAttributeData.Uninitialized;

		internal ThreeState lazyHasEmbeddedAttribute;

		internal ThreeState lazyHasCompilerLoweringPreserveAttribute;

		internal ThreeState lazyHasInterpolatedStringHandlerAttribute;

		internal ThreeState lazyHasRequiredMembers;

		internal ImmutableArray<byte> lazyFilePathChecksum;

		internal string lazyDisplayFileName;

		internal ExtensionInfo extensionInfo;
	}

	private class ExtensionInfo(PENamedTypeSymbol markerType, MethodDefinitionHandle markerMethod)
	{
		public readonly PENamedTypeSymbol MarkerTypeSymbol = markerType;

		public readonly MethodDefinitionHandle MarkerMethodHandle = markerMethod;

		public PEMethodSymbol? LazyMarkerMethodSymbol;

		public StrongBox<ParameterSymbol?>? LazyExtensionParameter;

		public ConcurrentDictionary<MethodSymbol, MethodSymbol?>? LazyImplementationMap;

		public PENamedTypeSymbol GroupingTypeSymbol => (PENamedTypeSymbol)MarkerTypeSymbol.ContainingType;
	}

	private class DeclarationOrderTypeSymbolComparer : IComparer<Symbol>
	{
		public static readonly DeclarationOrderTypeSymbolComparer Instance = new DeclarationOrderTypeSymbolComparer();

		private DeclarationOrderTypeSymbolComparer()
		{
		}

		public int Compare(Symbol x, Symbol y)
		{
			return HandleComparer.Default.Compare(((PENamedTypeSymbol)x).Handle, ((PENamedTypeSymbol)y).Handle);
		}
	}

	private sealed class PENamedTypeSymbolNonGeneric : PENamedTypeSymbol
	{
		public override int Arity => 0;

		internal override bool MangleName => false;

		internal override int MetadataArity
		{
			get
			{
				if (_container is PENamedTypeSymbol pENamedTypeSymbol)
				{
					return pENamedTypeSymbol.MetadataArity;
				}
				return 0;
			}
		}

		internal override NamedTypeSymbol NativeIntegerUnderlyingType => null;

		internal PENamedTypeSymbolNonGeneric(PEModuleSymbol moduleSymbol, NamespaceOrTypeSymbol container, TypeDefinitionHandle handle, string emittedNamespaceName, PENamedTypeSymbol markerType, MethodDefinitionHandle markerMethod)
			: base(moduleSymbol, container, handle, emittedNamespaceName, 0, markerType, markerMethod, out var _)
		{
		}

		protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 2980);
		}

		internal override NamedTypeSymbol AsNativeInteger()
		{
			if (ContainingAssembly.RuntimeSupportsNumericIntPtr)
			{
				return this;
			}
			return ContainingAssembly.GetNativeIntegerType(this);
		}

		internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
		{
			if (!(t2 is NativeIntegerTypeSymbol nativeIntegerTypeSymbol))
			{
				return base.Equals(t2, comparison);
			}
			return nativeIntegerTypeSymbol.Equals(this, comparison);
		}
	}

	private sealed class PENamedTypeSymbolGeneric : PENamedTypeSymbol
	{
		private readonly GenericParameterHandleCollection _genericParameterHandles;

		private readonly ushort _arity;

		private readonly bool _mangleName;

		private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

		public override int Arity => _arity;

		internal override bool MangleName => _mangleName;

		internal override int MetadataArity => _genericParameterHandles.Count;

		internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

		public override ImmutableArray<TypeParameterSymbol> TypeParameters
		{
			get
			{
				EnsureTypeParametersAreLoaded();
				return _lazyTypeParameters;
			}
		}

		internal sealed override NamedTypeSymbol NativeIntegerUnderlyingType => null;

		internal PENamedTypeSymbolGeneric(PEModuleSymbol moduleSymbol, NamespaceOrTypeSymbol container, TypeDefinitionHandle handle, string emittedNamespaceName, GenericParameterHandleCollection genericParameterHandles, ushort arity, PENamedTypeSymbol markerType, MethodDefinitionHandle markerMethod)
			: base(moduleSymbol, container, handle, emittedNamespaceName, arity, markerType, markerMethod, out var mangleName)
		{
			_arity = arity;
			if (_arity == 0)
			{
				_lazyTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
			}
			_genericParameterHandles = genericParameterHandles;
			_mangleName = mangleName;
		}

		protected sealed override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 3069);
		}

		internal sealed override NamedTypeSymbol AsNativeInteger()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 3113);
		}

		private void EnsureTypeParametersAreLoaded()
		{
			if (_lazyTypeParameters.IsDefault)
			{
				PEModuleSymbol containingPEModule = base.ContainingPEModule;
				int num = _genericParameterHandles.Count - _arity;
				ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance(_arity);
				instance.Count = _arity;
				for (int i = 0; i < instance.Count; i++)
				{
					instance[i] = new PETypeParameterSymbol(containingPEModule, this, (ushort)i, _genericParameterHandles[num + i]);
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameters, instance.ToImmutableAndFree());
			}
		}

		protected override DiagnosticInfo GetUseSiteDiagnosticImpl()
		{
			DiagnosticInfo result = null;
			if (!MergeUseSiteDiagnostics(ref result, base.GetUseSiteDiagnosticImpl()))
			{
				if (!MatchesContainingTypeParameters())
				{
					result = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, this);
				}
				else if (IsExtension && !((PENamedTypeSymbolGeneric)_lazyUncommonProperties.extensionInfo.MarkerTypeSymbol).MatchesContainingTypeParameters())
				{
					result = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, this);
				}
			}
			return result;
		}

		private bool MatchesContainingTypeParameters()
		{
			NamedTypeSymbol containingType = ContainingType;
			if ((object)containingType == null)
			{
				return true;
			}
			ImmutableArray<TypeParameterSymbol> allTypeParameters = containingType.GetAllTypeParameters();
			int length = allTypeParameters.Length;
			if (length == 0)
			{
				return true;
			}
			ImmutableArray<TypeParameterSymbol> typeParameters = Create(base.ContainingPEModule, (PENamespaceSymbol)ContainingNamespace, _handle, null).TypeParameters;
			TypeMap typeMap = new TypeMap(allTypeParameters, IndexedTypeParameterSymbol.Take(length));
			TypeMap typeMap2 = new TypeMap(typeParameters, IndexedTypeParameterSymbol.Take(typeParameters.Length));
			for (int i = 0; i < length; i++)
			{
				TypeParameterSymbol typeParameter = allTypeParameters[i];
				TypeParameterSymbol typeParameter2 = typeParameters[i];
				if (!MemberSignatureComparer.HaveSameConstraints(typeParameter, typeMap, typeParameter2, typeMap2, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
				{
					return false;
				}
			}
			return true;
		}
	}

	private static readonly Dictionary<ReadOnlyMemory<char>, ImmutableArray<PENamedTypeSymbol>> s_emptyNestedTypes = new Dictionary<ReadOnlyMemory<char>, ImmutableArray<PENamedTypeSymbol>>(EmptyReadOnlyMemoryOfCharComparer.Instance);

	private readonly NamespaceOrTypeSymbol _container;

	private readonly TypeDefinitionHandle _handle;

	private readonly string _name;

	private readonly TypeAttributes _flags;

	private readonly ExtendedSpecialType _corTypeId;

	private ICollection<string> _lazyMemberNames;

	private ImmutableArray<Symbol> _lazyMembersInDeclarationOrder;

	private Dictionary<string, ImmutableArray<Symbol>> _lazyMembersByName;

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<PENamedTypeSymbol>> _lazyNestedTypes;

	private TypeKind _lazyKind;

	private NullableContextKind _lazyNullableContextValue;

	private NamedTypeSymbol _lazyBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyInterfaces;

	private NamedTypeSymbol _lazyDeclaredBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyDeclaredInterfaces;

	private Tuple<CultureInfo, string> _lazyDocComment;

	private CachedUseSiteInfo<AssemblySymbol> _lazyCachedUseSiteInfo = CachedUseSiteInfo<AssemblySymbol>.Uninitialized;

	private static readonly UncommonProperties s_noUncommonProperties = new UncommonProperties();

	private UncommonProperties _lazyUncommonProperties;

	public override ExtendedSpecialType ExtendedSpecialType => _corTypeId;

	internal sealed override ParameterSymbol? ExtensionParameter
	{
		get
		{
			if (!IsExtension)
			{
				return null;
			}
			ExtensionInfo extensionInfo = GetUncommonProperties().extensionInfo;
			if (extensionInfo.LazyExtensionParameter == null)
			{
				ParameterSymbol value = makeExtensionParameter(this, extensionInfo);
				Interlocked.CompareExchange(ref extensionInfo.LazyExtensionParameter, new StrongBox<ParameterSymbol>(value), null);
			}
			return extensionInfo.LazyExtensionParameter.Value;
			static ParameterSymbol? makeExtensionParameter(PENamedTypeSymbol @this, ExtensionInfo uncommon)
			{
				PEMethodSymbol markerMethodSymbol = GetMarkerMethodSymbol(@this, uncommon);
				if (markerMethodSymbol.IsGenericMethod || !markerMethodSymbol.ReturnsVoid || markerMethodSymbol.ParameterCount != 1)
				{
					return null;
				}
				return new ReceiverParameterSymbol(@this, markerMethodSymbol.Parameters[0]);
			}
		}
	}

	internal PEModuleSymbol ContainingPEModule
	{
		get
		{
			Symbol symbol = _container;
			while (symbol.Kind != SymbolKind.Namespace)
			{
				symbol = symbol.ContainingSymbol;
			}
			return ((PENamespaceSymbol)symbol).ContainingPEModule;
		}
	}

	internal override ModuleSymbol ContainingModule => ContainingPEModule;

	public abstract override int Arity { get; }

	internal abstract override bool MangleName { get; }

	internal sealed override bool IsFileLocal
	{
		get
		{
			UncommonProperties lazyUncommonProperties = _lazyUncommonProperties;
			if (lazyUncommonProperties != null)
			{
				ImmutableArray<byte> lazyFilePathChecksum = lazyUncommonProperties.lazyFilePathChecksum;
				if (!lazyFilePathChecksum.IsDefault)
				{
					return lazyUncommonProperties.lazyDisplayFileName != null;
				}
			}
			return false;
		}
	}

	internal sealed override FileIdentifier AssociatedFileIdentifier
	{
		get
		{
			UncommonProperties lazyUncommonProperties = _lazyUncommonProperties;
			if (lazyUncommonProperties != null)
			{
				ImmutableArray<byte> lazyFilePathChecksum = lazyUncommonProperties.lazyFilePathChecksum;
				if (!lazyFilePathChecksum.IsDefault)
				{
					string lazyDisplayFileName = lazyUncommonProperties.lazyDisplayFileName;
					if (lazyDisplayFileName != null)
					{
						return FileIdentifier.Create(lazyFilePathChecksum, lazyDisplayFileName);
					}
				}
			}
			return null;
		}
	}

	internal abstract int MetadataArity { get; }

	internal TypeDefinitionHandle Handle => _handle;

	public override int MetadataToken => MetadataTokens.GetToken(_handle);

	internal sealed override bool IsInterpolatedStringHandlerType
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyHasInterpolatedStringHandlerAttribute.HasValue())
			{
				uncommonProperties.lazyHasInterpolatedStringHandlerAttribute = ContainingPEModule.Module.HasInterpolatedStringHandlerAttribute(_handle).ToThreeState();
			}
			return uncommonProperties.lazyHasInterpolatedStringHandlerAttribute.Value();
		}
	}

	internal override bool HasCodeAnalysisEmbeddedAttribute
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyHasEmbeddedAttribute.HasValue())
			{
				uncommonProperties.lazyHasEmbeddedAttribute = ContainingPEModule.Module.HasCodeAnalysisEmbeddedAttribute(_handle).ToThreeState();
			}
			return uncommonProperties.lazyHasEmbeddedAttribute.Value();
		}
	}

	internal override bool HasCompilerLoweringPreserveAttribute
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyHasCompilerLoweringPreserveAttribute.HasValue())
			{
				uncommonProperties.lazyHasCompilerLoweringPreserveAttribute = ContainingPEModule.Module.HasCompilerLoweringPreserveAttribute(_handle).ToThreeState();
			}
			return uncommonProperties.lazyHasCompilerLoweringPreserveAttribute.Value();
		}
	}

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if ((object)_lazyBaseType == ErrorTypeSymbol.UnknownResultType)
			{
				Interlocked.CompareExchange(ref _lazyBaseType, MakeAcyclicBaseType(), ErrorTypeSymbol.UnknownResultType);
			}
			return _lazyBaseType;
		}
	}

	public override NamedTypeSymbol ConstructedFrom => this;

	public override Symbol ContainingSymbol => _container;

	public override NamedTypeSymbol ContainingType => _container as NamedTypeSymbol;

	internal override bool IsRecord
	{
		get
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			return SynthesizedRecordClone.FindValidCloneMethod(this, ref useSiteInfo) != null;
		}
	}

	internal override bool IsRecordStruct => false;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (IsExtension)
			{
				return Accessibility.Public;
			}
			Accessibility accessibility = Accessibility.Private;
			switch (_flags & TypeAttributes.VisibilityMask)
			{
			case TypeAttributes.NestedAssembly:
				return Accessibility.Internal;
			case TypeAttributes.VisibilityMask:
				return Accessibility.ProtectedOrInternal;
			case TypeAttributes.NestedFamANDAssem:
				return Accessibility.ProtectedAndInternal;
			case TypeAttributes.NestedPrivate:
				return Accessibility.Private;
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				return Accessibility.Public;
			case TypeAttributes.NestedFamily:
				return Accessibility.Protected;
			case TypeAttributes.NotPublic:
				return Accessibility.Internal;
			default:
				throw ExceptionUtilities.UnexpectedValue(_flags & TypeAttributes.VisibilityMask);
			}
		}
	}

	public override NamedTypeSymbol EnumUnderlyingType
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return null;
			}
			EnsureEnumUnderlyingTypeIsLoaded(uncommonProperties);
			return uncommonProperties.lazyEnumUnderlyingType;
		}
	}

	public override IEnumerable<string> MemberNames
	{
		get
		{
			EnsureNonTypeMemberNamesAreLoaded();
			return _lazyMemberNames;
		}
	}

	internal override bool HasDeclaredRequiredMembers
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (uncommonProperties.lazyHasRequiredMembers.HasValue())
			{
				return uncommonProperties.lazyHasRequiredMembers.Value();
			}
			bool flag = ContainingPEModule.Module.HasAttribute(_handle, AttributeDescription.RequiredMemberAttribute);
			uncommonProperties.lazyHasRequiredMembers = flag.ToThreeState();
			return flag;
		}
	}

	internal override FieldSymbol FixedElementField
	{
		get
		{
			FieldSymbol result = null;
			ImmutableArray<Symbol> members = GetMembers("FixedElementField");
			if (!members.IsDefault && members.Length == 1)
			{
				result = members[0] as FieldSymbol;
			}
			return result;
		}
	}

	public override ImmutableArray<Location> Locations => ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override string Name
	{
		get
		{
			if (!IsExtension)
			{
				return _name;
			}
			return "";
		}
	}

	public override string MetadataName
	{
		get
		{
			if (IsExtension)
			{
				return _name;
			}
			return base.MetadataName;
		}
	}

	internal override bool HasSpecialName => (_flags & TypeAttributes.SpecialName) != 0;

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => ImmutableArray<TypeWithAnnotations>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override bool IsStatic
	{
		get
		{
			if ((_flags & TypeAttributes.Sealed) != TypeAttributes.NotPublic)
			{
				return (_flags & TypeAttributes.Abstract) != 0;
			}
			return false;
		}
	}

	public override bool IsAbstract
	{
		get
		{
			if ((_flags & TypeAttributes.Abstract) != TypeAttributes.NotPublic)
			{
				return (_flags & TypeAttributes.Sealed) == 0;
			}
			return false;
		}
	}

	internal override bool IsMetadataAbstract => (_flags & TypeAttributes.Abstract) != 0;

	public override bool IsSealed
	{
		get
		{
			if ((_flags & TypeAttributes.Sealed) != TypeAttributes.NotPublic)
			{
				return (_flags & TypeAttributes.Abstract) == 0;
			}
			return false;
		}
	}

	internal override bool IsMetadataSealed => (_flags & TypeAttributes.Sealed) != 0;

	internal TypeAttributes Flags => _flags;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 1975);
		}
	}

	public override bool MightContainExtensionMethods
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyContainsExtensionMethods.HasValue())
			{
				ThreeState lazyContainsExtensionMethods = ThreeState.False;
				TypeKind typeKind = TypeKind;
				if (typeKind - 2 <= TypeKind.Array || typeKind == TypeKind.Struct)
				{
					bool flag = ContainingPEModule.Module.HasExtensionAttribute(_handle, ignoreCase: false);
					lazyContainsExtensionMethods = ((!(ContainingAssembly is PEAssemblySymbol pEAssemblySymbol)) ? flag.ToThreeState() : (flag && pEAssemblySymbol.MightContainExtensionMethods).ToThreeState());
				}
				uncommonProperties.lazyContainsExtensionMethods = lazyContainsExtensionMethods;
			}
			return uncommonProperties.lazyContainsExtensionMethods.Value();
		}
	}

	public override bool IsExtension => _lazyUncommonProperties?.extensionInfo != null;

	public override TypeKind TypeKind
	{
		get
		{
			TypeKind typeKind = (TypeKind)Volatile.Read(in Unsafe.As<TypeKind, byte>(ref _lazyKind));
			if (typeKind == TypeKind.Unknown)
			{
				if (IsExtension)
				{
					typeKind = TypeKind.Extension;
				}
				else if (_flags.IsInterface())
				{
					typeKind = TypeKind.Interface;
				}
				else
				{
					TypeSymbol declaredBaseType = GetDeclaredBaseType(skipTransformsIfNecessary: true);
					typeKind = TypeKind.Class;
					if ((object)declaredBaseType != null)
					{
						switch (declaredBaseType.SpecialType)
						{
						case SpecialType.System_Enum:
							typeKind = TypeKind.Enum;
							break;
						case SpecialType.System_MulticastDelegate:
							typeKind = TypeKind.Delegate;
							break;
						case SpecialType.System_ValueType:
							if (base.SpecialType != SpecialType.System_Enum)
							{
								typeKind = TypeKind.Struct;
							}
							break;
						}
					}
				}
				_lazyKind = typeKind;
			}
			return typeKind;
		}
	}

	internal sealed override bool IsInterface => _flags.IsInterface();

	internal string DefaultMemberName
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return "";
			}
			if (uncommonProperties.lazyDefaultMemberName == null)
			{
				ContainingPEModule.Module.HasDefaultMemberAttribute(_handle, out var memberName);
				Interlocked.CompareExchange(ref uncommonProperties.lazyDefaultMemberName, memberName ?? "", null);
			}
			return uncommonProperties.lazyDefaultMemberName;
		}
	}

	internal override bool IsComImport => (_flags & TypeAttributes.Import) != 0;

	internal override bool ShouldAddWinRTMembers => IsWindowsRuntimeImport;

	internal override bool IsWindowsRuntimeImport => (_flags & TypeAttributes.WindowsRuntime) != 0;

	internal override TypeLayout Layout => ContainingPEModule.Module.GetTypeLayout(_handle);

	internal override CharSet MarshallingCharSet
	{
		get
		{
			CharSet charSet = _flags.ToCharSet();
			if (charSet == (CharSet)0)
			{
				return CharSet.Ansi;
			}
			return charSet;
		}
	}

	public override bool IsSerializable => (_flags & TypeAttributes.Serializable) != 0;

	public override bool IsRefLikeType
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyIsByRefLike.HasValue())
			{
				ThreeState lazyIsByRefLike = ThreeState.False;
				if (TypeKind == TypeKind.Struct)
				{
					lazyIsByRefLike = ContainingPEModule.Module.HasIsByRefLikeAttribute(_handle).ToThreeState();
				}
				uncommonProperties.lazyIsByRefLike = lazyIsByRefLike;
			}
			return uncommonProperties.lazyIsByRefLike.Value();
		}
	}

	internal override string? ExtensionGroupingName
	{
		get
		{
			if (!IsExtension)
			{
				return null;
			}
			return _lazyUncommonProperties.extensionInfo.GroupingTypeSymbol.Name;
		}
	}

	internal PENamedTypeSymbol ExtensionGroupingType
	{
		get
		{
			if (!IsExtension)
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 2716);
			}
			return _lazyUncommonProperties.extensionInfo.GroupingTypeSymbol;
		}
	}

	internal override string? ExtensionMarkerName
	{
		get
		{
			if (!IsExtension)
			{
				return null;
			}
			return _name;
		}
	}

	public override bool IsReadOnly
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return false;
			}
			if (!uncommonProperties.lazyIsReadOnly.HasValue())
			{
				ThreeState lazyIsReadOnly = ThreeState.False;
				if (TypeKind == TypeKind.Struct)
				{
					lazyIsReadOnly = ContainingPEModule.Module.HasIsReadOnlyAttribute(_handle).ToThreeState();
				}
				uncommonProperties.lazyIsReadOnly = lazyIsReadOnly;
			}
			return uncommonProperties.lazyIsReadOnly.Value();
		}
	}

	internal override bool HasDeclarativeSecurity => (_flags & TypeAttributes.HasSecurity) != 0;

	internal override NamedTypeSymbol ComImportCoClass
	{
		get
		{
			if (!this.IsInterfaceType())
			{
				return null;
			}
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return null;
			}
			if ((object)uncommonProperties.lazyComImportCoClassType == ErrorTypeSymbol.UnknownResultType)
			{
				TypeSymbol typeSymbol = ContainingPEModule.TryDecodeAttributeWithTypeArgument(Handle, AttributeDescription.CoClassAttribute);
				NamedTypeSymbol value = (((object)typeSymbol != null && (typeSymbol.TypeKind == TypeKind.Class || typeSymbol.IsErrorType())) ? ((NamedTypeSymbol)typeSymbol) : null);
				Interlocked.CompareExchange(ref uncommonProperties.lazyComImportCoClassType, value, ErrorTypeSymbol.UnknownResultType);
			}
			return uncommonProperties.lazyComImportCoClassType;
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			UncommonProperties uncommonProperties = GetUncommonProperties();
			if (uncommonProperties == s_noUncommonProperties)
			{
				return null;
			}
			bool isRefLikeType = IsRefLikeType;
			ObsoleteAttributeHelpers.InitializeObsoleteDataFromMetadata(ref uncommonProperties.lazyObsoleteAttributeData, _handle, ContainingPEModule, isRefLikeType, ignoreRequiredMemberMarker: false);
			return uncommonProperties.lazyObsoleteAttributeData;
		}
	}

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	private UncommonProperties GetUncommonProperties()
	{
		UncommonProperties lazyUncommonProperties = _lazyUncommonProperties;
		if (lazyUncommonProperties != null)
		{
			return lazyUncommonProperties;
		}
		if (IsUncommon())
		{
			lazyUncommonProperties = new UncommonProperties();
			return Interlocked.CompareExchange(ref _lazyUncommonProperties, lazyUncommonProperties, null) ?? lazyUncommonProperties;
		}
		return _lazyUncommonProperties = s_noUncommonProperties;
	}

	private bool IsUncommon()
	{
		if (ContainingPEModule.HasAnyCustomAttributes(_handle))
		{
			return true;
		}
		TypeKind typeKind = TypeKind;
		if ((typeKind == TypeKind.Enum || typeKind == TypeKind.Extension) ? true : false)
		{
			return true;
		}
		return false;
	}

	internal static PENamedTypeSymbol Create(PEModuleSymbol moduleSymbol, PENamespaceSymbol containingNamespace, TypeDefinitionHandle handle, string emittedNamespaceName)
	{
		BadImageFormatException mrEx = null;
		GetGenericInfo(moduleSymbol, handle, out var genericParameterHandles, out var arity, out mrEx);
		PENamedTypeSymbol pENamedTypeSymbol = ((arity != 0) ? ((PENamedTypeSymbol)new PENamedTypeSymbolGeneric(moduleSymbol, containingNamespace, handle, emittedNamespaceName, genericParameterHandles, arity, null, default(MethodDefinitionHandle))) : ((PENamedTypeSymbol)new PENamedTypeSymbolNonGeneric(moduleSymbol, containingNamespace, handle, emittedNamespaceName, null, default(MethodDefinitionHandle))));
		if (mrEx != null)
		{
			pENamedTypeSymbol._lazyCachedUseSiteInfo.Initialize(pENamedTypeSymbol.DeriveCompilerFeatureRequiredDiagnostic() ?? new CSDiagnosticInfo(ErrorCode.ERR_BogusType, pENamedTypeSymbol));
		}
		return pENamedTypeSymbol;
	}

	private static void GetGenericInfo(PEModuleSymbol moduleSymbol, TypeDefinitionHandle handle, out GenericParameterHandleCollection genericParameterHandles, out ushort arity, out BadImageFormatException mrEx)
	{
		try
		{
			genericParameterHandles = moduleSymbol.Module.GetTypeDefGenericParamsOrThrow(handle);
			arity = (ushort)genericParameterHandles.Count;
			mrEx = null;
		}
		catch (BadImageFormatException ex)
		{
			arity = 0;
			genericParameterHandles = default(GenericParameterHandleCollection);
			mrEx = ex;
		}
	}

	internal static PENamedTypeSymbol Create(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, TypeDefinitionHandle handle, PENamedTypeSymbol markerType = null, MethodDefinitionHandle markerMethod = default(MethodDefinitionHandle))
	{
		BadImageFormatException mrEx = null;
		GetGenericInfo(moduleSymbol, handle, out var genericParameterHandles, out var arity, out mrEx);
		ushort arity2 = 0;
		int metadataArity = containingType.MetadataArity;
		if (arity > metadataArity)
		{
			arity2 = (ushort)(arity - metadataArity);
		}
		PENamedTypeSymbol pENamedTypeSymbol = ((arity != 0) ? ((PENamedTypeSymbol)new PENamedTypeSymbolGeneric(moduleSymbol, containingType, handle, null, genericParameterHandles, arity2, markerType, markerMethod)) : ((PENamedTypeSymbol)new PENamedTypeSymbolNonGeneric(moduleSymbol, containingType, handle, null, markerType, markerMethod)));
		if (mrEx != null || arity < metadataArity)
		{
			pENamedTypeSymbol._lazyCachedUseSiteInfo.Initialize(pENamedTypeSymbol.DeriveCompilerFeatureRequiredDiagnostic() ?? new CSDiagnosticInfo(ErrorCode.ERR_BogusType, pENamedTypeSymbol));
		}
		return pENamedTypeSymbol;
	}

	private PENamedTypeSymbol(PEModuleSymbol moduleSymbol, NamespaceOrTypeSymbol container, TypeDefinitionHandle handle, string emittedNamespaceName, ushort arity, PENamedTypeSymbol markerType, MethodDefinitionHandle markerMethod, out bool mangleName)
	{
		bool flag = false;
		string text;
		try
		{
			text = moduleSymbol.Module.GetTypeDefNameOrThrow(handle);
		}
		catch (BadImageFormatException)
		{
			text = string.Empty;
			flag = true;
		}
		_handle = handle;
		_container = container;
		try
		{
			_flags = moduleSymbol.Module.GetTypeDefFlagsOrThrow(handle);
		}
		catch (BadImageFormatException)
		{
			flag = true;
		}
		if (arity == 0 || (object)markerType != null)
		{
			_name = text;
			mangleName = false;
		}
		else
		{
			_name = MetadataHelpers.UnmangleMetadataNameForArity(text, arity);
			mangleName = (object)_name != text;
		}
		if (_lazyUncommonProperties != null)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 356);
		}
		if (container.IsNamespace && GeneratedNameParser.TryParseFileTypeName(_name, out string displayFileName, out byte[] checksum, out string originalTypeName))
		{
			_name = originalTypeName;
			_lazyUncommonProperties = new UncommonProperties
			{
				lazyFilePathChecksum = checksum.ToImmutableArray(),
				lazyDisplayFileName = displayFileName
			};
		}
		else if ((object)markerType != null)
		{
			_lazyUncommonProperties = new UncommonProperties
			{
				extensionInfo = new ExtensionInfo(markerType, markerMethod)
			};
		}
		if (emittedNamespaceName != null && moduleSymbol.ContainingAssembly.KeepLookingForDeclaredSpecialTypes && DeclaredAccessibility == Accessibility.Public)
		{
			_corTypeId = SpecialTypes.GetTypeFromMetadataName(MetadataHelpers.BuildQualifiedName(emittedNamespaceName, text));
		}
		else
		{
			_corTypeId = SpecialType.None;
		}
		if (flag)
		{
			_lazyCachedUseSiteInfo.Initialize(DeriveCompilerFeatureRequiredDiagnostic() ?? new CSDiagnosticInfo(ErrorCode.ERR_BogusType, this));
		}
	}

	internal MethodSymbol? GetMarkerMethodSymbol()
	{
		if (!IsExtension)
		{
			return null;
		}
		ExtensionInfo extensionInfo = GetUncommonProperties().extensionInfo;
		return GetMarkerMethodSymbol(this, extensionInfo);
	}

	private static PEMethodSymbol GetMarkerMethodSymbol(PENamedTypeSymbol @this, ExtensionInfo uncommon)
	{
		if ((object)uncommon.LazyMarkerMethodSymbol == null)
		{
			Interlocked.CompareExchange(ref uncommon.LazyMarkerMethodSymbol, new PEMethodSymbol(@this.ContainingPEModule, @this, uncommon.MarkerMethodHandle), null);
		}
		return uncommon.LazyMarkerMethodSymbol;
	}

	public sealed override MethodSymbol? TryGetCorrespondingExtensionImplementationMethod(MethodSymbol method)
	{
		if ((object)ContainingType == null)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 474);
		}
		if (!method.IsStatic && (object)ExtensionParameter == null)
		{
			return null;
		}
		ExtensionInfo extensionInfo = GetUncommonProperties().extensionInfo;
		if (extensionInfo.LazyImplementationMap == null)
		{
			Interlocked.CompareExchange(ref extensionInfo.LazyImplementationMap, new ConcurrentDictionary<MethodSymbol, MethodSymbol>(ReferenceEqualityComparer.Instance), null);
		}
		return ConcurrentDictionaryExtensions.GetOrAdd(extensionInfo.LazyImplementationMap, method, findCorrespondingExtensionImplementationMethod, this);
		static MethodSymbol? findCorrespondingExtensionImplementationMethod(MethodSymbol methodSymbol, PENamedTypeSymbol @this)
		{
			foreach (Symbol member in @this.ContainingType.GetMembers(methodSymbol.Name))
			{
				if (member is MethodSymbol methodSymbol2 && member.IsStatic && methodSymbol2.DeclaredAccessibility == methodSymbol.DeclaredAccessibility && methodSymbol2.Arity == @this.Arity + methodSymbol.Arity)
				{
					int num = ((!methodSymbol.IsStatic) ? 1 : 0);
					if (num + methodSymbol.ParameterCount == methodSymbol2.ParameterCount)
					{
						ImmutableArray<TypeParameterSymbol> immutableArray = @this.TypeParameters.Concat(methodSymbol.TypeParameters);
						TypeMap typeMap = (immutableArray.IsEmpty ? null : new TypeMap(immutableArray, methodSymbol2.TypeParameters));
						if (MemberSignatureComparer.HaveSameReturnTypes(methodSymbol2, null, methodSymbol, typeMap, TypeCompareKind.CLRSignatureCompareOptions))
						{
							ImmutableArray<ParameterSymbol> parameters;
							if (!methodSymbol.IsStatic)
							{
								parameters = methodSymbol2.Parameters;
								if (!MemberSignatureComparer.HaveSameParameterType(parameters[0], null, @this.ExtensionParameter, typeMap, MemberSignatureComparer.RefKindCompareMode.ConsiderDifferences, considerDefaultValues: false, TypeCompareKind.CLRSignatureCompareOptions))
								{
									continue;
								}
							}
							parameters = methodSymbol2.Parameters;
							ReadOnlySpan<ParameterSymbol> @params = parameters.AsSpan(num, methodSymbol2.ParameterCount - num);
							parameters = methodSymbol.Parameters;
							if (MemberSignatureComparer.HaveSameParameterTypes(@params, null, parameters.AsSpan(), typeMap, MemberSignatureComparer.RefKindCompareMode.ConsiderDifferences, considerDefaultValues: false, TypeCompareKind.CLRSignatureCompareOptions))
							{
								if (!MemberSignatureComparer.HaveSameConstraints(methodSymbol2.TypeParameters, null, immutableArray, typeMap, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
								{
									break;
								}
								return methodSymbol2;
							}
						}
					}
				}
			}
			return null;
		}
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved = null)
	{
		if (_lazyInterfaces.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyInterfaces, MakeAcyclicInterfaces(), default(ImmutableArray<NamedTypeSymbol>));
		}
		return _lazyInterfaces;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return InterfacesNoUseSiteDiagnostics();
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		return GetDeclaredBaseType(skipTransformsIfNecessary: false);
	}

	private NamedTypeSymbol GetDeclaredBaseType(bool skipTransformsIfNecessary)
	{
		if ((object)_lazyDeclaredBaseType == ErrorTypeSymbol.UnknownResultType)
		{
			NamedTypeSymbol namedTypeSymbol = MakeDeclaredBaseType();
			if ((object)namedTypeSymbol != null)
			{
				if (skipTransformsIfNecessary)
				{
					return namedTypeSymbol;
				}
				PEModuleSymbol containingPEModule = ContainingPEModule;
				namedTypeSymbol = (NamedTypeSymbol)NullableTypeDecoder.TransformType(TypeWithAnnotations.Create(TupleTypeDecoder.DecodeTupleTypesIfApplicable(NativeIntegerTypeDecoder.TransformType(DynamicTypeDecoder.TransformType(namedTypeSymbol, 0, _handle, containingPEModule), _handle, containingPEModule, this), _handle, containingPEModule)), _handle, containingPEModule, this, this).Type;
			}
			Interlocked.CompareExchange(ref _lazyDeclaredBaseType, namedTypeSymbol, ErrorTypeSymbol.UnknownResultType);
		}
		return _lazyDeclaredBaseType;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (_lazyDeclaredInterfaces.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyDeclaredInterfaces, MakeDeclaredInterfaces(), default(ImmutableArray<NamedTypeSymbol>));
		}
		return _lazyDeclaredInterfaces;
	}

	private NamedTypeSymbol MakeDeclaredBaseType()
	{
		if (!_flags.IsInterface())
		{
			try
			{
				PEModuleSymbol containingPEModule = ContainingPEModule;
				EntityHandle baseTypeOfTypeOrThrow = containingPEModule.Module.GetBaseTypeOfTypeOrThrow(_handle);
				if (!baseTypeOfTypeOrThrow.IsNil)
				{
					return (NamedTypeSymbol)new MetadataDecoder(containingPEModule, this).GetTypeOfToken(baseTypeOfTypeOrThrow);
				}
			}
			catch (BadImageFormatException mrEx)
			{
				return new UnsupportedMetadataTypeSymbol(mrEx);
			}
		}
		return null;
	}

	private ImmutableArray<NamedTypeSymbol> MakeDeclaredInterfaces()
	{
		try
		{
			PEModuleSymbol containingPEModule = ContainingPEModule;
			InterfaceImplementationHandleCollection interfaceImplementationsOrThrow = containingPEModule.Module.GetInterfaceImplementationsOrThrow(_handle);
			if (interfaceImplementationsOrThrow.Count > 0)
			{
				ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance(interfaceImplementationsOrThrow.Count);
				MetadataDecoder metadataDecoder = new MetadataDecoder(containingPEModule, this);
				foreach (InterfaceImplementationHandle item2 in interfaceImplementationsOrThrow)
				{
					EntityHandle token = containingPEModule.Module.MetadataReader.GetInterfaceImplementation(item2).Interface;
					NamedTypeSymbol item = (NullableTypeDecoder.TransformType(TypeWithAnnotations.Create(TupleTypeDecoder.DecodeTupleTypesIfApplicable(NativeIntegerTypeDecoder.TransformType(metadataDecoder.GetTypeOfToken(token), item2, containingPEModule, ContainingType), item2, containingPEModule)), item2, containingPEModule, this, this).Type as NamedTypeSymbol) ?? new UnsupportedMetadataTypeSymbol();
					instance.Add(item);
				}
				return instance.ToImmutableAndFree();
			}
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		catch (BadImageFormatException mrEx)
		{
			return ImmutableArray.Create((NamedTypeSymbol)new UnsupportedMetadataTypeSymbol(mrEx));
		}
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		UncommonProperties uncommonProperties = GetUncommonProperties();
		if (uncommonProperties == s_noUncommonProperties)
		{
			return ImmutableArray<CSharpAttributeData>.Empty;
		}
		if (uncommonProperties.lazyCustomAttributes.IsDefault)
		{
			ImmutableArray<CSharpAttributeData> value;
			CustomAttributeHandle filteredOutAttribute;
			if (IsExtension)
			{
				value = ImmutableArray<CSharpAttributeData>.Empty;
				filteredOutAttribute = default(CustomAttributeHandle);
			}
			else
			{
				value = ContainingPEModule.GetCustomAttributesForToken(Handle, out var _, MightContainExtensionMethods ? AttributeDescription.CaseSensitiveExtensionAttribute : default(AttributeDescription), out var _, (IsRefLikeType && ObsoleteAttributeData == null) ? AttributeDescription.ObsoleteAttribute : default(AttributeDescription), out var _, IsReadOnly ? AttributeDescription.IsReadOnlyAttribute : default(AttributeDescription), out var _, IsRefLikeType ? AttributeDescription.IsByRefLikeAttribute : default(AttributeDescription), out var _, (IsRefLikeType && DeriveCompilerFeatureRequiredDiagnostic() == null) ? AttributeDescription.CompilerFeatureRequiredAttribute : default(AttributeDescription), out filteredOutAttribute, AttributeDescription.RequiredMemberAttribute);
			}
			ImmutableInterlocked.InterlockedInitialize(ref uncommonProperties.lazyCustomAttributes, value);
			if (!uncommonProperties.lazyHasRequiredMembers.HasValue())
			{
				uncommonProperties.lazyHasRequiredMembers = (!filteredOutAttribute.IsNil).ToThreeState();
			}
		}
		return uncommonProperties.lazyCustomAttributes;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return GetAttributes();
	}

	internal override byte? GetNullableContextValue()
	{
		if (!_lazyNullableContextValue.TryGetByte(out var value))
		{
			value = (ContainingPEModule.Module.HasNullableContextAttribute(_handle, out var value2) ? new byte?(value2) : _container.GetNullableContextValue());
			_lazyNullableContextValue = value.ToNullableContextFlags();
		}
		return value;
	}

	internal override byte? GetLocalNullableContextValue()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 1000);
	}

	private void EnsureNonTypeMemberNamesAreLoaded()
	{
		if (_lazyMemberNames != null)
		{
			return;
		}
		PEModule module = ContainingPEModule.Module;
		HashSet<string> hashSet = new HashSet<string>();
		try
		{
			foreach (MethodDefinitionHandle item in module.GetMethodsOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetMethodDefNameOrThrow(item));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		try
		{
			foreach (PropertyDefinitionHandle item2 in module.GetPropertiesOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetPropertyDefNameOrThrow(item2));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		try
		{
			foreach (EventDefinitionHandle item3 in module.GetEventsOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetEventDefNameOrThrow(item3));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		try
		{
			foreach (FieldDefinitionHandle item4 in module.GetFieldsOfTypeOrThrow(_handle))
			{
				try
				{
					hashSet.Add(module.GetFieldDefNameOrThrow(item4));
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		if (IsValueType)
		{
			hashSet.Add(".ctor");
		}
		Interlocked.CompareExchange(ref _lazyMemberNames, CreateReadOnlyMemberNames(hashSet), null);
	}

	private static ICollection<string> CreateReadOnlyMemberNames(HashSet<string> names)
	{
		switch (names.Count)
		{
		case 0:
			return SpecializedCollections.EmptySet<string>();
		case 1:
			return SpecializedCollections.SingletonCollection(names.First());
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
			return ImmutableArray.CreateRange(names);
		default:
			return SpecializedCollections.ReadOnlySet(names);
		}
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		EnsureAllMembersAreLoaded();
		return _lazyMembersInDeclarationOrder;
	}

	private IEnumerable<FieldSymbol> GetEnumFieldsToEmit()
	{
		UncommonProperties uncommon = GetUncommonProperties();
		if (uncommon == s_noUncommonProperties)
		{
			yield break;
		}
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		ArrayBuilder<FieldDefinitionHandle> fieldDefs = ArrayBuilder<FieldDefinitionHandle>.GetInstance();
		try
		{
			foreach (FieldDefinitionHandle item in module.GetFieldsOfTypeOrThrow(_handle))
			{
				fieldDefs.Add(item);
			}
		}
		catch (BadImageFormatException)
		{
		}
		if (uncommon.lazyInstanceEnumFields.IsDefault)
		{
			ArrayBuilder<PEFieldSymbol> instance = ArrayBuilder<PEFieldSymbol>.GetInstance();
			foreach (FieldDefinitionHandle item2 in fieldDefs)
			{
				try
				{
					FieldAttributes fieldDefFlagsOrThrow = module.GetFieldDefFlagsOrThrow(item2);
					if ((fieldDefFlagsOrThrow & FieldAttributes.Static) == 0 && ModuleExtensions.ShouldImportField(fieldDefFlagsOrThrow, containingPEModule.ImportOptions))
					{
						instance.Add(new PEFieldSymbol(containingPEModule, this, item2));
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
			ImmutableInterlocked.InterlockedInitialize(ref uncommon.lazyInstanceEnumFields, instance.ToImmutableAndFree());
		}
		int staticIndex = 0;
		ImmutableArray<Symbol> staticFields = GetMembers();
		int instanceIndex = 0;
		foreach (FieldDefinitionHandle item3 in fieldDefs)
		{
			if (instanceIndex < uncommon.lazyInstanceEnumFields.Length && uncommon.lazyInstanceEnumFields[instanceIndex].Handle == item3)
			{
				yield return uncommon.lazyInstanceEnumFields[instanceIndex];
				instanceIndex++;
			}
			else if (staticIndex < staticFields.Length && staticFields[staticIndex].Kind == SymbolKind.Field)
			{
				PEFieldSymbol pEFieldSymbol = (PEFieldSymbol)staticFields[staticIndex];
				if (pEFieldSymbol.Handle == item3)
				{
					yield return pEFieldSymbol;
					staticIndex++;
				}
			}
		}
		fieldDefs.Free();
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		if (TypeKind == TypeKind.Enum)
		{
			return GetEnumFieldsToEmit();
		}
		IEnumerable<FieldSymbol> members = GetMembers<FieldSymbol>(GetMembers().WhereAsArray((Symbol m) => !(m is TupleErrorFieldSymbol)), SymbolKind.Field, 0);
		ArrayBuilder<FieldSymbol> arrayBuilder = null;
		foreach (EventSymbol item in GetEventsToEmit())
		{
			FieldSymbol associatedField = item.AssociatedField;
			if ((object)associatedField != null)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<FieldSymbol>.GetInstance();
				}
				arrayBuilder.Add(associatedField);
			}
		}
		if (arrayBuilder == null)
		{
			return members;
		}
		SmallDictionary<FieldDefinitionHandle, FieldSymbol> smallDictionary = new SmallDictionary<FieldDefinitionHandle, FieldSymbol>();
		int num = 0;
		foreach (PEFieldSymbol item2 in members)
		{
			smallDictionary.Add(item2.Handle, item2);
			num++;
		}
		foreach (PEFieldSymbol item3 in arrayBuilder)
		{
			smallDictionary.Add(item3.Handle, item3);
		}
		num += arrayBuilder.Count;
		arrayBuilder.Free();
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(num);
		try
		{
			foreach (FieldDefinitionHandle item4 in ContainingPEModule.Module.GetFieldsOfTypeOrThrow(_handle))
			{
				if (smallDictionary.TryGetValue(item4, out var value))
				{
					instance.Add(value);
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return instance.ToImmutableAndFree();
	}

	internal override IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		ImmutableArray<Symbol> members = GetMembers();
		int index = GetIndexOfFirstMember(members, SymbolKind.Method);
		if (!this.IsInterfaceType())
		{
			for (; index < members.Length && members[index].Kind == SymbolKind.Method; index++)
			{
				MethodSymbol methodSymbol = (MethodSymbol)members[index];
				if (!methodSymbol.IsDefaultValueTypeConstructor())
				{
					yield return methodSymbol;
				}
			}
		}
		else
		{
			if (index >= members.Length || members[index].Kind != SymbolKind.Method)
			{
				yield break;
			}
			PEMethodSymbol method = (PEMethodSymbol)members[index];
			PEModule module = ContainingPEModule.Module;
			ArrayBuilder<MethodDefinitionHandle> methodDefs = ArrayBuilder<MethodDefinitionHandle>.GetInstance();
			try
			{
				foreach (MethodDefinitionHandle item in module.GetMethodsOfTypeOrThrow(_handle))
				{
					methodDefs.Add(item);
				}
			}
			catch (BadImageFormatException)
			{
			}
			foreach (MethodDefinitionHandle item2 in methodDefs)
			{
				if (method.Handle == item2)
				{
					yield return method;
					index++;
					if (index == members.Length || members[index].Kind != SymbolKind.Method)
					{
						methodDefs.Free();
						yield break;
					}
					method = (PEMethodSymbol)members[index];
				}
				else
				{
					int gapSize;
					try
					{
						gapSize = ModuleExtensions.GetVTableGapSize(module.GetMethodDefNameOrThrow(item2));
					}
					catch (BadImageFormatException)
					{
						gapSize = 1;
					}
					do
					{
						yield return null;
						gapSize--;
					}
					while (gapSize > 0);
				}
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 1393);
		}
	}

	internal override IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		return GetMembers<PropertySymbol>(GetMembers(), SymbolKind.Property);
	}

	internal override IEnumerable<EventSymbol> GetEventsToEmit()
	{
		return GetMembers<EventSymbol>(GetMembers(), SymbolKind.Event);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetMembersUnordered();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return GetMembers(name);
	}

	private void EnsureEnumUnderlyingTypeIsLoaded(UncommonProperties uncommon)
	{
		if ((object)uncommon.lazyEnumUnderlyingType != null || TypeKind != TypeKind.Enum)
		{
			return;
		}
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		MetadataDecoder metadataDecoder = new MetadataDecoder(containingPEModule, this);
		NamedTypeSymbol namedTypeSymbol = null;
		try
		{
			foreach (FieldDefinitionHandle item in module.GetFieldsOfTypeOrThrow(_handle))
			{
				FieldAttributes fieldDefFlagsOrThrow;
				try
				{
					fieldDefFlagsOrThrow = module.GetFieldDefFlagsOrThrow(item);
				}
				catch (BadImageFormatException)
				{
					continue;
				}
				if ((fieldDefFlagsOrThrow & FieldAttributes.Static) == 0)
				{
					FieldInfo<TypeSymbol> fieldInfo = metadataDecoder.DecodeFieldSignature(item);
					TypeSymbol type = fieldInfo.Type;
					if (type.SpecialType.IsValidEnumUnderlyingType() && !fieldInfo.IsByRef && !fieldInfo.CustomModifiers.AnyRequired())
					{
						namedTypeSymbol = (((object)namedTypeSymbol != null) ? new UnsupportedMetadataTypeSymbol() : ((NamedTypeSymbol)type));
					}
				}
			}
			if ((object)namedTypeSymbol == null)
			{
				namedTypeSymbol = new UnsupportedMetadataTypeSymbol();
			}
		}
		catch (BadImageFormatException mrEx)
		{
			if ((object)namedTypeSymbol == null)
			{
				namedTypeSymbol = new UnsupportedMetadataTypeSymbol(mrEx);
			}
		}
		Interlocked.CompareExchange(ref uncommon.lazyEnumUnderlyingType, namedTypeSymbol, null);
	}

	private void EnsureAllMembersAreLoaded()
	{
		if (Volatile.Read(in _lazyMembersByName) == null)
		{
			LoadMembers();
		}
	}

	private void LoadMembers()
	{
		ArrayBuilder<Symbol> arrayBuilder = null;
		if (RoslynImmutableInterlocked.VolatileRead(in _lazyMembersInDeclarationOrder).IsDefault)
		{
			EnsureNestedTypesAreLoaded();
			arrayBuilder = ArrayBuilder<Symbol>.GetInstance();
			if (TypeKind == TypeKind.Enum)
			{
				EnsureEnumUnderlyingTypeIsLoaded(GetUncommonProperties());
				PEModuleSymbol containingPEModule = ContainingPEModule;
				PEModule module = containingPEModule.Module;
				try
				{
					foreach (FieldDefinitionHandle item3 in module.GetFieldsOfTypeOrThrow(_handle))
					{
						FieldAttributes fieldAttributes;
						try
						{
							fieldAttributes = module.GetFieldDefFlagsOrThrow(item3);
							if ((fieldAttributes & FieldAttributes.Static) == 0)
							{
								continue;
							}
						}
						catch (BadImageFormatException)
						{
							fieldAttributes = FieldAttributes.PrivateScope;
						}
						if (ModuleExtensions.ShouldImportField(fieldAttributes, containingPEModule.ImportOptions))
						{
							PEFieldSymbol item = new PEFieldSymbol(containingPEModule, this, item3);
							arrayBuilder.Add(item);
						}
					}
				}
				catch (BadImageFormatException)
				{
				}
				SynthesizedInstanceConstructor item2 = new SynthesizedInstanceConstructor(this);
				arrayBuilder.Add(item2);
			}
			else
			{
				ArrayBuilder<PEFieldSymbol> instance = ArrayBuilder<PEFieldSymbol>.GetInstance();
				ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
				MultiDictionary<string, PEFieldSymbol> privateFieldNameToSymbols = CreateFields(instance);
				PooledDictionary<MethodDefinitionHandle, PEMethodSymbol> pooledDictionary = CreateMethods(instance2);
				if (TypeKind == TypeKind.Struct)
				{
					bool flag = false;
					foreach (MethodSymbol item4 in instance2)
					{
						if (item4.IsParameterlessConstructor())
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						instance2.Insert(0, new SynthesizedInstanceConstructor(this));
					}
				}
				CreateProperties(pooledDictionary, instance2);
				CreateEvents(privateFieldNameToSymbols, pooledDictionary, instance2);
				foreach (PEFieldSymbol item5 in instance)
				{
					if ((object)item5.AssociatedSymbol == null)
					{
						arrayBuilder.Add(item5);
					}
				}
				arrayBuilder.AddRange(instance2);
				instance2.Free();
				instance.Free();
				pooledDictionary.Free();
			}
			int num = arrayBuilder.Count;
			foreach (ImmutableArray<PENamedTypeSymbol> value3 in _lazyNestedTypes.Values)
			{
				arrayBuilder.AddRange(value3);
			}
			arrayBuilder.Sort(num, DeclarationOrderTypeSymbolComparer.Instance);
			if (IsTupleType)
			{
				_ = arrayBuilder.Count;
				ImmutableArray<Symbol> immutableArray = arrayBuilder.ToImmutableAndFree();
				arrayBuilder = MakeSynthesizedTupleMembers(immutableArray);
				num += arrayBuilder.Count;
				arrayBuilder.AddRange(immutableArray);
			}
			ImmutableArray<Symbol> value = arrayBuilder.ToImmutable();
			if (!ImmutableInterlocked.InterlockedInitialize(ref _lazyMembersInDeclarationOrder, value))
			{
				arrayBuilder.Free();
				arrayBuilder = null;
			}
			else
			{
				arrayBuilder.Clip(num);
			}
		}
		if (Volatile.Read(in _lazyMembersByName) == null)
		{
			if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<Symbol>.GetInstance();
				foreach (Symbol item6 in _lazyMembersInDeclarationOrder)
				{
					if (item6.Kind == SymbolKind.NamedType)
					{
						break;
					}
					arrayBuilder.Add(item6);
				}
			}
			Dictionary<string, ImmutableArray<Symbol>> dictionary = GroupByName(arrayBuilder);
			if (Interlocked.CompareExchange(ref _lazyMembersByName, dictionary, null) == null)
			{
				ICollection<string> value2 = SpecializedCollections.ReadOnlyCollection(dictionary.Keys);
				Interlocked.Exchange(ref _lazyMemberNames, value2);
			}
		}
		arrayBuilder?.Free();
	}

	internal override ImmutableArray<Symbol> GetSimpleNonTypeMembers(string name)
	{
		EnsureAllMembersAreLoaded();
		if (!_lazyMembersByName.TryGetValue(name, out var value))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return value;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		EnsureAllMembersAreLoaded();
		if (!_lazyMembersByName.TryGetValue(name, out var value))
		{
			value = ImmutableArray<Symbol>.Empty;
		}
		if (_lazyNestedTypes.TryGetValue(System.MemoryExtensions.AsMemory(name), out var value2))
		{
			value = value.Concat(StaticCast<Symbol>.From(value2));
		}
		return value;
	}

	internal sealed override bool HasPossibleWellKnownCloneMethod()
	{
		return MemberNames.Contains("<Clone>$");
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return GetTypeMembers().ConditionallyDeOrder();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		EnsureNestedTypesAreLoaded();
		return GetMemberTypesPrivate();
	}

	private ImmutableArray<NamedTypeSymbol> GetMemberTypesPrivate()
	{
		PENamedTypeSymbol[] array = new PENamedTypeSymbol[_lazyNestedTypes.Values.Sum((ImmutableArray<PENamedTypeSymbol> a) => a.Length)];
		int num = 0;
		foreach (ImmutableArray<PENamedTypeSymbol> value in _lazyNestedTypes.Values)
		{
			value.CopyTo(array, num);
			num += value.Length;
		}
		NamedTypeSymbol[] array2 = array;
		return ImmutableCollectionsMarshal.AsImmutableArray(array2);
	}

	private void EnsureNestedTypesAreLoaded()
	{
		if (_lazyNestedTypes == null)
		{
			ArrayBuilder<PENamedTypeSymbol> instance = ArrayBuilder<PENamedTypeSymbol>.GetInstance();
			instance.AddRange(CreateNestedTypes());
			Dictionary<ReadOnlyMemory<char>, ImmutableArray<PENamedTypeSymbol>> dictionary = GroupByName(instance);
			if (Interlocked.CompareExchange(ref _lazyNestedTypes, dictionary, null) == null)
			{
				ContainingPEModule.OnNewTypeDeclarationsLoaded(dictionary);
			}
			instance.Free();
		}
	}

	private void SetExtensionGroupingTypeNestedTypes(ArrayBuilder<PENamedTypeSymbol> groupingNestedTypes)
	{
		Interlocked.CompareExchange(ref _lazyNestedTypes, GroupByName(groupingNestedTypes), null);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		EnsureNestedTypesAreLoaded();
		if (_lazyNestedTypes.TryGetValue(name, out var value))
		{
			return StaticCast<NamedTypeSymbol>.From(value);
		}
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol type, int num) => type.Arity == num, arity);
	}

	private MethodDefinitionHandle TryGetExtensionMarkerMethod()
	{
		PEModule module = ContainingPEModule.Module;
		try
		{
			MethodDefinitionHandle result = default(MethodDefinitionHandle);
			foreach (MethodDefinitionHandle item in module.GetMethodsOfTypeOrThrow(_handle))
			{
				module.GetMethodDefPropsOrThrow(item, out var name, out var _, out var flags, out var _);
				if ((flags & (MethodAttributes.Static | MethodAttributes.SpecialName)) == (MethodAttributes.Static | MethodAttributes.SpecialName) && name == "<Extension>$")
				{
					if (!result.IsNil)
					{
						return default(MethodDefinitionHandle);
					}
					result = item;
				}
			}
			return result;
		}
		catch (BadImageFormatException)
		{
		}
		return default(MethodDefinitionHandle);
	}

	private static ExtendedErrorTypeSymbol CyclicInheritanceError(TypeSymbol declaredBase)
	{
		CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_ImportedCircularBase, declaredBase);
		return new ExtendedErrorTypeSymbol(declaredBase, LookupResultKind.NotReferencable, errorInfo, unreported: true);
	}

	private NamedTypeSymbol MakeAcyclicBaseType()
	{
		NamedTypeSymbol declaredBaseType = GetDeclaredBaseType(null);
		if ((object)declaredBaseType == null)
		{
			return null;
		}
		if (BaseTypeAnalysis.TypeDependsOn(declaredBaseType, this))
		{
			return CyclicInheritanceError(declaredBaseType);
		}
		SetKnownToHaveNoDeclaredBaseCycles();
		return declaredBaseType;
	}

	private ImmutableArray<NamedTypeSymbol> MakeAcyclicInterfaces()
	{
		ImmutableArray<NamedTypeSymbol> declaredInterfaces = GetDeclaredInterfaces(null);
		if (!IsInterface)
		{
			return declaredInterfaces;
		}
		return declaredInterfaces.SelectAsArray((NamedTypeSymbol t) => (!BaseTypeAnalysis.TypeDependsOn(t, this)) ? t : CyclicInheritanceError(t));
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return PEDocumentationCommentUtils.GetDocumentationComment(this, ContainingPEModule, preferredCulture, cancellationToken, ref _lazyDocComment);
	}

	private IEnumerable<PENamedTypeSymbol> CreateNestedTypes()
	{
		if (IsExtension)
		{
			yield break;
		}
		PEModuleSymbol moduleSymbol = ContainingPEModule;
		PEModule module = moduleSymbol.Module;
		ImmutableArray<TypeDefinitionHandle> nestedTypeDefsOrThrow;
		try
		{
			nestedTypeDefsOrThrow = module.GetNestedTypeDefsOrThrow(_handle);
		}
		catch (BadImageFormatException)
		{
			yield break;
		}
		bool checkForExtensionGroup = IsStatic && (object)ContainingType == null && TypeKind == TypeKind.Class && module.HasExtensionAttribute(_handle, ignoreCase: false) && !base.IsGenericType;
		foreach (TypeDefinitionHandle item in nestedTypeDefsOrThrow)
		{
			PENamedTypeSymbol type = Create(moduleSymbol, this, item);
			if (checkForExtensionGroup && type.DeclaredAccessibility == Accessibility.Public && type.HasSpecialName && type.IsSealed && module.HasExtensionAttribute(type.Handle, ignoreCase: false) && type.TypeKind == TypeKind.Class && type.BaseTypeNoUseSiteDiagnostics.IsObjectType() && type.InterfacesNoUseSiteDiagnostics().IsEmpty)
			{
				ImmutableArray<TypeDefinitionHandle> nestedTypeDefsOrThrow2;
				try
				{
					nestedTypeDefsOrThrow2 = module.GetNestedTypeDefsOrThrow(type.Handle);
				}
				catch (BadImageFormatException)
				{
					break;
				}
				ArrayBuilder<PENamedTypeSymbol> groupingNestedTypes = ArrayBuilder<PENamedTypeSymbol>.GetInstance();
				foreach (TypeDefinitionHandle item2 in nestedTypeDefsOrThrow2)
				{
					PENamedTypeSymbol pENamedTypeSymbol = Create(moduleSymbol, type, item2);
					groupingNestedTypes.Add(pENamedTypeSymbol);
					if (pENamedTypeSymbol.HasSpecialName && pENamedTypeSymbol.IsStatic && pENamedTypeSymbol.DeclaredAccessibility == Accessibility.Public && pENamedTypeSymbol.TypeKind == TypeKind.Class && pENamedTypeSymbol.BaseTypeNoUseSiteDiagnostics.IsObjectType() && pENamedTypeSymbol.Arity == 0 && pENamedTypeSymbol.InterfacesNoUseSiteDiagnostics().IsEmpty && (type.Arity == 0 || pENamedTypeSymbol is PENamedTypeSymbolGeneric))
					{
						MethodDefinitionHandle markerMethod = pENamedTypeSymbol.TryGetExtensionMarkerMethod();
						if (!markerMethod.IsNil)
						{
							yield return Create(moduleSymbol, this, item2, pENamedTypeSymbol, markerMethod);
						}
					}
				}
				type.SetExtensionGroupingTypeNestedTypes(groupingNestedTypes);
				groupingNestedTypes.Free();
			}
			else
			{
				yield return type;
			}
		}
	}

	private MultiDictionary<string, PEFieldSymbol> CreateFields(ArrayBuilder<PEFieldSymbol> fieldMembers)
	{
		MultiDictionary<string, PEFieldSymbol> multiDictionary = new MultiDictionary<string, PEFieldSymbol>();
		if (IsExtension)
		{
			return multiDictionary;
		}
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		bool flag = false;
		bool flag2 = false;
		if (TypeKind == TypeKind.Struct)
		{
			if (base.SpecialType == SpecialType.None)
			{
				flag2 = ContainingAssembly.IsLinked;
			}
			switch (base.SpecialType)
			{
			case SpecialType.System_Void:
			case SpecialType.System_Boolean:
			case SpecialType.System_Char:
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
			case SpecialType.System_UInt32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
			case SpecialType.System_Decimal:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
			case SpecialType.System_IntPtr:
			case SpecialType.System_UIntPtr:
			case SpecialType.System_DateTime:
			case SpecialType.System_TypedReference:
			case SpecialType.System_ArgIterator:
			case SpecialType.System_RuntimeArgumentHandle:
			case SpecialType.System_RuntimeFieldHandle:
			case SpecialType.System_RuntimeMethodHandle:
			case SpecialType.System_RuntimeTypeHandle:
				flag = false;
				break;
			default:
				flag = true;
				break;
			}
		}
		try
		{
			foreach (FieldDefinitionHandle item in module.GetFieldsOfTypeOrThrow(_handle))
			{
				try
				{
					if (!flag2 && (!flag || (module.GetFieldDefFlagsOrThrow(item) & FieldAttributes.Static) != FieldAttributes.PrivateScope) && !module.ShouldImportField(item, containingPEModule.ImportOptions))
					{
						continue;
					}
				}
				catch (BadImageFormatException)
				{
				}
				PEFieldSymbol pEFieldSymbol = new PEFieldSymbol(containingPEModule, this, item);
				fieldMembers.Add(pEFieldSymbol);
				if (pEFieldSymbol.DeclaredAccessibility == Accessibility.Private)
				{
					string name = pEFieldSymbol.Name;
					if (name.Length > 0)
					{
						multiDictionary.Add(name, pEFieldSymbol);
					}
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return multiDictionary;
	}

	private PooledDictionary<MethodDefinitionHandle, PEMethodSymbol> CreateMethods(ArrayBuilder<Symbol> members)
	{
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		PooledDictionary<MethodDefinitionHandle, PEMethodSymbol> instance = PooledDictionary<MethodDefinitionHandle, PEMethodSymbol>.GetInstance();
		bool flag = TypeKind == TypeKind.Struct && base.SpecialType == SpecialType.None && ContainingAssembly.IsLinked;
		bool isExtension = IsExtension;
		try
		{
			foreach (MethodDefinitionHandle item in module.GetMethodsOfTypeOrThrow(isExtension ? _lazyUncommonProperties.extensionInfo.GroupingTypeSymbol.Handle : _handle))
			{
				if ((flag || module.ShouldImportMethod(_handle, item, containingPEModule.ImportOptions)) && (!isExtension || (module.HasExtensionMarkerAttribute(item, out var markerName) && !(markerName != MetadataName))))
				{
					PEMethodSymbol pEMethodSymbol = new PEMethodSymbol(containingPEModule, this, item);
					members.Add(pEMethodSymbol);
					instance.Add(item, pEMethodSymbol);
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return instance;
	}

	private void CreateProperties(Dictionary<MethodDefinitionHandle, PEMethodSymbol> methodHandleToSymbol, ArrayBuilder<Symbol> members)
	{
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		bool isExtension = IsExtension;
		try
		{
			foreach (PropertyDefinitionHandle item in module.GetPropertiesOfTypeOrThrow(isExtension ? _lazyUncommonProperties.extensionInfo.GroupingTypeSymbol.Handle : _handle))
			{
				try
				{
					if (!isExtension || (module.HasExtensionMarkerAttribute(item, out var markerName) && !(markerName != MetadataName)))
					{
						PropertyAccessors propertyMethodsOrThrow = module.GetPropertyMethodsOrThrow(item);
						PEMethodSymbol accessorMethod = GetAccessorMethod(module, methodHandleToSymbol, _handle, propertyMethodsOrThrow.Getter);
						PEMethodSymbol accessorMethod2 = GetAccessorMethod(module, methodHandleToSymbol, _handle, propertyMethodsOrThrow.Setter);
						if ((object)accessorMethod != null || (object)accessorMethod2 != null)
						{
							members.Add(PEPropertySymbol.Create(containingPEModule, this, item, accessorMethod, accessorMethod2));
						}
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
	}

	private void CreateEvents(MultiDictionary<string, PEFieldSymbol> privateFieldNameToSymbols, Dictionary<MethodDefinitionHandle, PEMethodSymbol> methodHandleToSymbol, ArrayBuilder<Symbol> members)
	{
		if (IsExtension)
		{
			return;
		}
		PEModuleSymbol containingPEModule = ContainingPEModule;
		PEModule module = containingPEModule.Module;
		try
		{
			foreach (EventDefinitionHandle item in module.GetEventsOfTypeOrThrow(_handle))
			{
				try
				{
					EventAccessors eventMethodsOrThrow = module.GetEventMethodsOrThrow(item);
					PEMethodSymbol accessorMethod = GetAccessorMethod(module, methodHandleToSymbol, _handle, eventMethodsOrThrow.Adder);
					PEMethodSymbol accessorMethod2 = GetAccessorMethod(module, methodHandleToSymbol, _handle, eventMethodsOrThrow.Remover);
					if ((object)accessorMethod != null || (object)accessorMethod2 != null)
					{
						members.Add(new PEEventSymbol(containingPEModule, this, item, accessorMethod, accessorMethod2, privateFieldNameToSymbols));
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
	}

	private PEMethodSymbol GetAccessorMethod(PEModule module, Dictionary<MethodDefinitionHandle, PEMethodSymbol> methodHandleToSymbol, TypeDefinitionHandle typeDef, MethodDefinitionHandle methodDef)
	{
		if (methodDef.IsNil)
		{
			return null;
		}
		methodHandleToSymbol.TryGetValue(methodDef, out var value);
		return value;
	}

	private static Dictionary<string, ImmutableArray<Symbol>> GroupByName(ArrayBuilder<Symbol> symbols)
	{
		return symbols.ToDictionary((Symbol s) => s.Name, StringOrdinalComparer.Instance);
	}

	private static Dictionary<ReadOnlyMemory<char>, ImmutableArray<PENamedTypeSymbol>> GroupByName(ArrayBuilder<PENamedTypeSymbol> symbols)
	{
		if (symbols.Count == 0)
		{
			return s_emptyNestedTypes;
		}
		return symbols.ToDictionary((PENamedTypeSymbol s) => System.MemoryExtensions.AsMemory(s.Name), ReadOnlyMemoryOfCharComparer.Instance);
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		AssemblySymbol primaryDependency = base.PrimaryDependency;
		if (!_lazyCachedUseSiteInfo.IsInitialized)
		{
			_lazyCachedUseSiteInfo.Initialize(primaryDependency, new UseSiteInfo<AssemblySymbol>(primaryDependency).AdjustDiagnosticInfo(GetUseSiteDiagnosticImpl()));
		}
		return _lazyCachedUseSiteInfo.ToUseSiteInfo(primaryDependency);
	}

	protected virtual DiagnosticInfo GetUseSiteDiagnosticImpl()
	{
		DiagnosticInfo result = DeriveCompilerFeatureRequiredDiagnostic();
		if (result != null)
		{
			return result;
		}
		if (!MergeUseSiteDiagnostics(ref result, CalculateUseSiteDiagnostic()))
		{
			if (ContainingPEModule.Module.HasRequiredAttributeAttribute(_handle))
			{
				result = new CSDiagnosticInfo(ErrorCode.ERR_BogusType, this);
			}
			else if (TypeKind == TypeKind.Class && base.SpecialType != SpecialType.System_Enum)
			{
				TypeSymbol declaredBaseType = GetDeclaredBaseType(null);
				if ((object)declaredBaseType != null && declaredBaseType.SpecialType == SpecialType.None)
				{
					AssemblySymbol containingAssembly = declaredBaseType.ContainingAssembly;
					if ((object)containingAssembly != null && containingAssembly.IsMissing && declaredBaseType is MissingMetadataTypeSymbol.TopLevel { Arity: 0 } topLevel)
					{
						SpecialType specialType = (SpecialType)SpecialTypes.GetTypeFromMetadataName(MetadataHelpers.BuildQualifiedName(topLevel.NamespaceName, topLevel.MetadataName));
						if ((uint)(specialType - 2) <= 1u || specialType == SpecialType.System_ValueType)
						{
							result = topLevel.GetUseSiteInfo().DiagnosticInfo;
						}
					}
				}
			}
		}
		return result;
	}

	internal DiagnosticInfo? GetCompilerFeatureRequiredDiagnostic()
	{
		DiagnosticInfo diagnosticInfo = GetUseSiteInfo().DiagnosticInfo;
		if (diagnosticInfo != null && diagnosticInfo.Code == 9041)
		{
			return diagnosticInfo;
		}
		return null;
	}

	private DiagnosticInfo? DeriveCompilerFeatureRequiredDiagnostic()
	{
		MetadataDecoder decoder = new MetadataDecoder(ContainingPEModule, this);
		DiagnosticInfo diagnosticInfo = PEUtilities.DeriveCompilerFeatureRequiredAttributeDiagnostic(this, ContainingPEModule, Handle, IsRefLikeType ? CompilerFeatureRequiredFeatures.RefStructs : CompilerFeatureRequiredFeatures.None, decoder);
		if (diagnosticInfo != null)
		{
			return diagnosticInfo;
		}
		foreach (PETypeParameterSymbol typeParameter in TypeParameters)
		{
			diagnosticInfo = typeParameter.DeriveCompilerFeatureRequiredDiagnostic(decoder);
			if (diagnosticInfo != null)
			{
				return diagnosticInfo;
			}
		}
		if (!(ContainingType is PENamedTypeSymbol pENamedTypeSymbol))
		{
			return ContainingPEModule.GetCompilerFeatureRequiredDiagnostic();
		}
		return pENamedTypeSymbol.GetCompilerFeatureRequiredDiagnostic();
	}

	internal override bool GetGuidString(out string guidString)
	{
		return ContainingPEModule.Module.HasGuidAttribute(_handle, out guidString);
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs", 2762);
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		UncommonProperties uncommonProperties = GetUncommonProperties();
		if (uncommonProperties == s_noUncommonProperties)
		{
			return ImmutableArray<string>.Empty;
		}
		if (uncommonProperties.lazyConditionalAttributeSymbols.IsDefault)
		{
			ImmutableArray<string> conditionalAttributeValues = ContainingPEModule.Module.GetConditionalAttributeValues(_handle);
			ImmutableInterlocked.InterlockedCompareExchange(ref uncommonProperties.lazyConditionalAttributeSymbols, conditionalAttributeValues, default(ImmutableArray<string>));
		}
		return uncommonProperties.lazyConditionalAttributeSymbols;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		UncommonProperties uncommonProperties = GetUncommonProperties();
		if (uncommonProperties == s_noUncommonProperties)
		{
			if ((object)BaseTypeNoUseSiteDiagnostics == null)
			{
				return AttributeUsageInfo.Default;
			}
			return BaseTypeNoUseSiteDiagnostics.GetAttributeUsageInfo();
		}
		if (uncommonProperties.lazyAttributeUsageInfo.IsNull)
		{
			uncommonProperties.lazyAttributeUsageInfo = DecodeAttributeUsageInfo();
		}
		return uncommonProperties.lazyAttributeUsageInfo;
	}

	private AttributeUsageInfo DecodeAttributeUsageInfo()
	{
		if (ContainingPEModule.Module.HasAttributeUsageAttribute(_handle, new MetadataDecoder(ContainingPEModule), out var usageInfo))
		{
			if (!usageInfo.HasValidAttributeTargets)
			{
				return AttributeUsageInfo.Default;
			}
			return usageInfo;
		}
		if ((object)BaseTypeNoUseSiteDiagnostics == null)
		{
			return AttributeUsageInfo.Default;
		}
		return BaseTypeNoUseSiteDiagnostics.GetAttributeUsageInfo();
	}

	private static int GetIndexOfFirstMember(ImmutableArray<Symbol> members, SymbolKind kind)
	{
		int length = members.Length;
		for (int i = 0; i < length; i++)
		{
			if (members[i].Kind == kind)
			{
				return i;
			}
		}
		return length;
	}

	private static IEnumerable<TSymbol> GetMembers<TSymbol>(ImmutableArray<Symbol> members, SymbolKind kind, int offset = -1) where TSymbol : Symbol
	{
		if (offset < 0)
		{
			offset = GetIndexOfFirstMember(members, kind);
		}
		int n = members.Length;
		for (int i = offset; i < n; i++)
		{
			Symbol symbol = members[i];
			if (symbol.Kind != kind)
			{
				break;
			}
			yield return (TSymbol)symbol;
		}
	}

	internal sealed override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		return SpecializedCollections.EmptyEnumerable<(MethodSymbol, MethodSymbol)>();
	}

	internal sealed override bool HasInlineArrayAttribute(out int length)
	{
		if (ContainingPEModule.Module.HasInlineArrayAttribute(_handle, out length) && length > 0)
		{
			return true;
		}
		length = 0;
		return false;
	}

	internal sealed override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		UncommonProperties uncommonProperties = GetUncommonProperties();
		if (uncommonProperties == s_noUncommonProperties)
		{
			builderType = null;
			methodName = null;
			return false;
		}
		if (uncommonProperties.lazyCollectionBuilderAttributeData == CollectionBuilderAttributeData.Uninitialized)
		{
			Interlocked.CompareExchange(ref uncommonProperties.lazyCollectionBuilderAttributeData, getCollectionBuilderAttributeData(), CollectionBuilderAttributeData.Uninitialized);
		}
		CollectionBuilderAttributeData lazyCollectionBuilderAttributeData = uncommonProperties.lazyCollectionBuilderAttributeData;
		if (lazyCollectionBuilderAttributeData == null)
		{
			builderType = null;
			methodName = null;
			return false;
		}
		builderType = lazyCollectionBuilderAttributeData.BuilderType;
		methodName = lazyCollectionBuilderAttributeData.MethodName;
		return true;
		CollectionBuilderAttributeData? getCollectionBuilderAttributeData()
		{
			if (ContainingPEModule.Module.HasCollectionBuilderAttribute(_handle, out var builderTypeName, out var methodName2))
			{
				return new CollectionBuilderAttributeData(new MetadataDecoder(ContainingPEModule).GetTypeSymbolForSerializedType(builderTypeName), methodName2);
			}
			return null;
		}
	}

	internal override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		builderArgument = ContainingPEModule.TryDecodeAttributeWithTypeArgument(Handle, AttributeDescription.AsyncMethodBuilderAttribute);
		return (object)builderArgument != null;
	}
}
