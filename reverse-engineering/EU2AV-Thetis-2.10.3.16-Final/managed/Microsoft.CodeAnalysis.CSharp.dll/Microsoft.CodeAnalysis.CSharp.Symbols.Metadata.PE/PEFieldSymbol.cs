using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.DocumentationComments;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PEFieldSymbol : FieldSymbol
{
	private struct PackedFlags
	{
		private const int HasDisallowNullAttribute = 1;

		private const int HasAllowNullAttribute = 2;

		private const int HasMaybeNullAttribute = 4;

		private const int HasNotNullAttribute = 8;

		private const int FlowAnalysisAnnotationsCompletionBit = 16;

		private const int IsVolatileBit = 32;

		private const int RefKindOffset = 6;

		private const int RefKindMask = 3;

		private const int HasRequiredMemberAttribute = 256;

		private const int RequiredMemberCompletionBit = 512;

		private int _bits;

		public bool IsVolatile => (_bits & 0x20) != 0;

		public RefKind RefKind => (RefKind)((_bits >> 6) & 3);

		public bool SetFlowAnalysisAnnotations(FlowAnalysisAnnotations value)
		{
			int num = 16;
			if ((value & FlowAnalysisAnnotations.DisallowNull) != FlowAnalysisAnnotations.None)
			{
				num |= 1;
			}
			if ((value & FlowAnalysisAnnotations.AllowNull) != FlowAnalysisAnnotations.None)
			{
				num |= 2;
			}
			if ((value & FlowAnalysisAnnotations.MaybeNull) != FlowAnalysisAnnotations.None)
			{
				num |= 4;
			}
			if ((value & FlowAnalysisAnnotations.NotNull) != FlowAnalysisAnnotations.None)
			{
				num |= 8;
			}
			return ThreadSafeFlagOperations.Set(ref _bits, num);
		}

		public bool TryGetFlowAnalysisAnnotations(out FlowAnalysisAnnotations value)
		{
			int bits = _bits;
			value = FlowAnalysisAnnotations.None;
			if ((bits & 1) != 0)
			{
				value |= FlowAnalysisAnnotations.DisallowNull;
			}
			if ((bits & 2) != 0)
			{
				value |= FlowAnalysisAnnotations.AllowNull;
			}
			if ((bits & 4) != 0)
			{
				value |= FlowAnalysisAnnotations.MaybeNull;
			}
			if ((bits & 8) != 0)
			{
				value |= FlowAnalysisAnnotations.NotNull;
			}
			return (bits & 0x10) != 0;
		}

		public void SetIsVolatile(bool isVolatile)
		{
			if (isVolatile)
			{
				ThreadSafeFlagOperations.Set(ref _bits, 32);
			}
		}

		public void SetRefKind(RefKind refKind)
		{
			int num = (int)((uint)(refKind & RefKind.In) << 6);
			if (num != 0)
			{
				ThreadSafeFlagOperations.Set(ref _bits, num);
			}
		}

		public bool SetHasRequiredMemberAttribute(bool isRequired)
		{
			int toSet = 0x200 | (isRequired ? 256 : 0);
			return ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public bool TryGetHasRequiredMemberAttribute(out bool hasRequiredMemberAttribute)
		{
			if ((_bits & 0x200) != 0)
			{
				hasRequiredMemberAttribute = (_bits & 0x100) != 0;
				return true;
			}
			hasRequiredMemberAttribute = false;
			return false;
		}
	}

	private readonly FieldDefinitionHandle _handle;

	private readonly string _name;

	private readonly FieldAttributes _flags;

	private readonly PENamedTypeSymbol _containingType;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private ConstantValue _lazyConstantValue = Microsoft.CodeAnalysis.ConstantValue.Unset;

	private Tuple<CultureInfo, string> _lazyDocComment;

	private CachedUseSiteInfo<AssemblySymbol> _lazyCachedUseSiteInfo = CachedUseSiteInfo<AssemblySymbol>.Uninitialized;

	private ObsoleteAttributeData _lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

	private TypeWithAnnotations.Boxed _lazyType;

	private int _lazyFixedSize;

	private NamedTypeSymbol _lazyFixedImplementationType;

	private PEEventSymbol _associatedEventOpt;

	private PackedFlags _packedFlags;

	private ImmutableArray<CustomModifier> _lazyRefCustomModifiers;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override string Name => _name;

	public override int MetadataToken => MetadataTokens.GetToken(_handle);

	internal FieldAttributes Flags => _flags;

	internal override bool HasSpecialName => (_flags & FieldAttributes.SpecialName) != 0;

	internal override bool HasRuntimeSpecialName => (_flags & FieldAttributes.RTSpecialName) != 0;

	internal override bool IsNotSerialized => (_flags & FieldAttributes.NotSerialized) != 0;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

	internal override bool IsMarshalledExplicitly => (_flags & FieldAttributes.HasFieldMarshal) != 0;

	internal override UnmanagedType MarshallingType
	{
		get
		{
			if ((_flags & FieldAttributes.HasFieldMarshal) == 0)
			{
				return (UnmanagedType)0;
			}
			return _containingType.ContainingPEModule.Module.GetMarshallingType(_handle);
		}
	}

	internal override ImmutableArray<byte> MarshallingDescriptor
	{
		get
		{
			if ((_flags & FieldAttributes.HasFieldMarshal) == 0)
			{
				return default(ImmutableArray<byte>);
			}
			return _containingType.ContainingPEModule.Module.GetMarshallingDescriptor(_handle);
		}
	}

	internal override int? TypeLayoutOffset => _containingType.ContainingPEModule.Module.GetFieldOffset(_handle);

	internal FieldDefinitionHandle Handle => _handle;

	private PEModuleSymbol ContainingPEModule => ((PENamespaceSymbol)ContainingNamespace).ContainingPEModule;

	public override RefKind RefKind
	{
		get
		{
			EnsureSignatureIsLoaded();
			return _packedFlags.RefKind;
		}
	}

	public override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			EnsureSignatureIsLoaded();
			return _lazyRefCustomModifiers;
		}
	}

	public override FlowAnalysisAnnotations FlowAnalysisAnnotations
	{
		get
		{
			if (!_packedFlags.TryGetFlowAnalysisAnnotations(out var value))
			{
				value = DecodeFlowAnalysisAttributes(_containingType.ContainingPEModule.Module, _handle);
				_packedFlags.SetFlowAnalysisAnnotations(value);
			}
			return value;
		}
	}

	public override bool IsFixedSizeBuffer
	{
		get
		{
			EnsureSignatureIsLoaded();
			return (object)_lazyFixedImplementationType != null;
		}
	}

	public override int FixedSize
	{
		get
		{
			EnsureSignatureIsLoaded();
			return _lazyFixedSize;
		}
	}

	public override Symbol AssociatedSymbol => _associatedEventOpt;

	public override bool IsReadOnly => (_flags & FieldAttributes.InitOnly) != 0;

	public override bool IsVolatile
	{
		get
		{
			EnsureSignatureIsLoaded();
			return _packedFlags.IsVolatile;
		}
	}

	public override bool IsConst
	{
		get
		{
			if ((_flags & FieldAttributes.Literal) == 0)
			{
				return GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false) != null;
			}
			return true;
		}
	}

	public override ImmutableArray<Location> Locations => _containingType.ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			Accessibility accessibility = Accessibility.Private;
			switch (_flags & FieldAttributes.FieldAccessMask)
			{
			case FieldAttributes.Assembly:
				return Accessibility.Internal;
			case FieldAttributes.FamORAssem:
				return Accessibility.ProtectedOrInternal;
			case FieldAttributes.FamANDAssem:
				return Accessibility.ProtectedAndInternal;
			case FieldAttributes.PrivateScope:
			case FieldAttributes.Private:
				return Accessibility.Private;
			case FieldAttributes.Public:
				return Accessibility.Public;
			case FieldAttributes.Family:
				return Accessibility.Protected;
			default:
				return Accessibility.Private;
			}
		}
	}

	public override bool IsStatic => (_flags & FieldAttributes.Static) != 0;

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			ObsoleteAttributeHelpers.InitializeObsoleteDataFromMetadata(ref _lazyObsoleteAttributeData, _handle, (PEModuleSymbol)ContainingModule, ignoreByRefLikeMarker: false, ignoreRequiredMemberMarker: false);
			return _lazyObsoleteAttributeData;
		}
	}

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal override bool IsRequired
	{
		get
		{
			if (!_packedFlags.TryGetHasRequiredMemberAttribute(out var hasRequiredMemberAttribute))
			{
				hasRequiredMemberAttribute = ContainingPEModule.Module.HasAttribute(_handle, AttributeDescription.RequiredMemberAttribute);
				_packedFlags.SetHasRequiredMemberAttribute(hasRequiredMemberAttribute);
			}
			return hasRequiredMemberAttribute;
		}
	}

	internal PEFieldSymbol(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, FieldDefinitionHandle fieldDef)
	{
		_handle = fieldDef;
		_containingType = containingType;
		_packedFlags = default(PackedFlags);
		try
		{
			moduleSymbol.Module.GetFieldDefPropsOrThrow(fieldDef, out _name, out _flags);
		}
		catch (BadImageFormatException)
		{
			if (_name == null)
			{
				_name = string.Empty;
			}
			_lazyCachedUseSiteInfo.Initialize(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this));
		}
	}

	internal void SetAssociatedEvent(PEEventSymbol eventSymbol)
	{
		if ((object)_associatedEventOpt == null)
		{
			_associatedEventOpt = eventSymbol;
		}
	}

	private void EnsureSignatureIsLoaded()
	{
		if (_lazyType == null)
		{
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			FieldInfo<TypeSymbol> fieldInfo = new MetadataDecoder(containingPEModule, _containingType).DecodeFieldSignature(_handle);
			TypeSymbol type = fieldInfo.Type;
			ImmutableArray<CustomModifier> immutableArray = CSharpCustomModifier.Convert(fieldInfo.CustomModifiers);
			TypeWithAnnotations metadataType = TypeWithAnnotations.Create(NativeIntegerTypeDecoder.TransformType(DynamicTypeDecoder.TransformType(type, immutableArray.Length, _handle, containingPEModule), _handle, containingPEModule, _containingType), NullableAnnotation.Oblivious, immutableArray);
			metadataType = NullableTypeDecoder.TransformType(metadataType, _handle, containingPEModule, this, _containingType);
			metadataType = TupleTypeDecoder.DecodeTupleTypesIfApplicable(metadataType, _handle, containingPEModule);
			RefKind refKind = (fieldInfo.IsByRef ? ((!containingPEModule.Module.HasIsReadOnlyAttribute(_handle)) ? RefKind.Ref : RefKind.In) : RefKind.None);
			_packedFlags.SetRefKind(refKind);
			_packedFlags.SetIsVolatile(immutableArray.Any((CustomModifier m) => !m.IsOptional && ((CSharpCustomModifier)m).ModifierSymbol.SpecialType == SpecialType.System_Runtime_CompilerServices_IsVolatile));
			if (immutableArray.IsEmpty && IsFixedBuffer(out var fixedSize, out var fixedElementType))
			{
				_lazyFixedSize = fixedSize;
				_lazyFixedImplementationType = metadataType.Type as NamedTypeSymbol;
				metadataType = TypeWithAnnotations.Create(new PointerTypeSymbol(TypeWithAnnotations.Create(fixedElementType)));
			}
			ImmutableInterlocked.InterlockedInitialize(ref _lazyRefCustomModifiers, CSharpCustomModifier.Convert(fieldInfo.RefCustomModifiers));
			Interlocked.CompareExchange(ref _lazyType, new TypeWithAnnotations.Boxed(metadataType), null);
		}
	}

	private bool IsFixedBuffer(out int fixedSize, out TypeSymbol fixedElementType)
	{
		fixedSize = 0;
		fixedElementType = null;
		PEModuleSymbol containingPEModule = ContainingPEModule;
		if (containingPEModule.Module.HasFixedBufferAttribute(_handle, out var elementTypeName, out var bufferSize))
		{
			TypeSymbol typeSymbolForSerializedType = new MetadataDecoder(containingPEModule).GetTypeSymbolForSerializedType(elementTypeName);
			if (typeSymbolForSerializedType.FixedBufferElementSizeInBytes() != 0)
			{
				fixedSize = bufferSize;
				fixedElementType = typeSymbolForSerializedType;
				return true;
			}
		}
		return false;
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		EnsureSignatureIsLoaded();
		return _lazyType.Value;
	}

	private static FlowAnalysisAnnotations DecodeFlowAnalysisAttributes(PEModule module, FieldDefinitionHandle handle)
	{
		FlowAnalysisAnnotations flowAnalysisAnnotations = FlowAnalysisAnnotations.None;
		if (module.HasAttribute(handle, AttributeDescription.AllowNullAttribute))
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.AllowNull;
		}
		if (module.HasAttribute(handle, AttributeDescription.DisallowNullAttribute))
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.DisallowNull;
		}
		if (module.HasAttribute(handle, AttributeDescription.MaybeNullAttribute))
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.MaybeNull;
		}
		if (module.HasAttribute(handle, AttributeDescription.NotNullAttribute))
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.NotNull;
		}
		return flowAnalysisAnnotations;
	}

	internal override NamedTypeSymbol FixedImplementationType(PEModuleBuilder emitModule)
	{
		EnsureSignatureIsLoaded();
		return _lazyFixedImplementationType;
	}

	internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
	{
		if (_lazyConstantValue == Microsoft.CodeAnalysis.ConstantValue.Unset)
		{
			ConstantValue value = null;
			if ((_flags & FieldAttributes.Literal) != FieldAttributes.PrivateScope)
			{
				value = _containingType.ContainingPEModule.Module.GetConstantFieldValue(_handle);
			}
			if (base.Type.SpecialType == SpecialType.System_Decimal && _containingType.ContainingPEModule.Module.HasDecimalConstantAttribute(Handle, out var defaultValue))
			{
				value = defaultValue;
			}
			Interlocked.CompareExchange(ref _lazyConstantValue, value, Microsoft.CodeAnalysis.ConstantValue.Unset);
		}
		return _lazyConstantValue;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_lazyCustomAttributes.IsDefault)
		{
			ImmutableArray<CSharpAttributeData> customAttributesForToken = ((PEModuleSymbol)ContainingModule).GetCustomAttributesForToken(_handle, out var _, FilterOutDecimalConstantAttribute() ? AttributeDescription.DecimalConstantAttribute : default(AttributeDescription), out var filteredOutAttribute2, AttributeDescription.RequiredMemberAttribute);
			ImmutableInterlocked.InterlockedInitialize(ref _lazyCustomAttributes, customAttributesForToken);
			_packedFlags.SetHasRequiredMemberAttribute(!filteredOutAttribute2.IsNil);
		}
		return _lazyCustomAttributes;
	}

	private bool FilterOutDecimalConstantAttribute()
	{
		ConstantValue constantValue;
		if (base.Type.SpecialType == SpecialType.System_Decimal && (object)(constantValue = GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false)) != null)
		{
			return constantValue.Discriminator == ConstantValueTypeDiscriminator.Decimal;
		}
		return false;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		foreach (CSharpAttributeData attribute in GetAttributes())
		{
			yield return attribute;
		}
		if (FilterOutDecimalConstantAttribute())
		{
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			yield return new PEAttributeData(containingPEModule, containingPEModule.Module.FindLastTargetAttribute(_handle, AttributeDescription.DecimalConstantAttribute).Handle);
		}
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return PEDocumentationCommentUtils.GetDocumentationComment(this, _containingType.ContainingPEModule, preferredCulture, cancellationToken, ref _lazyDocComment);
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		AssemblySymbol primaryDependency = base.PrimaryDependency;
		if (!_lazyCachedUseSiteInfo.IsInitialized)
		{
			UseSiteInfo<AssemblySymbol> result = new UseSiteInfo<AssemblySymbol>(primaryDependency);
			CalculateUseSiteDiagnostic(ref result);
			if (RefKind != RefKind.None && (IsFixedSizeBuffer || base.Type.IsRefLikeOrAllowsRefLikeType()))
			{
				MergeUseSiteInfo(ref result, new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this)));
			}
			deriveCompilerFeatureRequiredUseSiteInfo(ref result);
			_lazyCachedUseSiteInfo.Initialize(primaryDependency, result);
		}
		return _lazyCachedUseSiteInfo.ToUseSiteInfo(primaryDependency);
		void deriveCompilerFeatureRequiredUseSiteInfo(ref UseSiteInfo<AssemblySymbol> reference)
		{
			PENamedTypeSymbol pENamedTypeSymbol = (PENamedTypeSymbol)ContainingType;
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			DiagnosticInfo diagnosticInfo = PEUtilities.DeriveCompilerFeatureRequiredAttributeDiagnostic(this, containingPEModule, Handle, CompilerFeatureRequiredFeatures.None, new MetadataDecoder(containingPEModule, pENamedTypeSymbol));
			if (diagnosticInfo == null)
			{
				diagnosticInfo = pENamedTypeSymbol.GetCompilerFeatureRequiredDiagnostic();
			}
			if (diagnosticInfo != null)
			{
				reference = new UseSiteInfo<AssemblySymbol>(diagnosticInfo);
			}
		}
	}
}
