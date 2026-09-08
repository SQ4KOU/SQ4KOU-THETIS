using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal class PEParameterSymbol : ParameterSymbol
{
	[Flags]
	private enum WellKnownAttributeFlags
	{
		HasIDispatchConstantAttribute = 1,
		HasIUnknownConstantAttribute = 2,
		HasCallerFilePathAttribute = 4,
		HasCallerLineNumberAttribute = 8,
		HasCallerMemberNameAttribute = 0x10,
		IsCallerFilePath = 0x20,
		IsCallerLineNumber = 0x40,
		IsCallerMemberName = 0x80
	}

	private struct PackedFlags
	{
		private const int WellKnownAttributeDataOffset = 0;

		private const int WellKnownAttributeCompletionFlagOffset = 8;

		private const int RefKindOffset = 16;

		private const int FlowAnalysisAnnotationsOffset = 21;

		private const int ScopeOffset = 29;

		private const int RefKindMask = 7;

		private const int WellKnownAttributeDataMask = 255;

		private const int WellKnownAttributeCompletionFlagMask = 255;

		private const int FlowAnalysisAnnotationsMask = 255;

		private const int ScopeMask = 3;

		private const int HasNameInMetadataBit = 524288;

		private const int FlowAnalysisAnnotationsCompletionBit = 1048576;

		private const int HasUnscopedRefAttributeBit = int.MinValue;

		private const int AllWellKnownAttributesCompleteNoData = 65280;

		private int _bits;

		public RefKind RefKind => (RefKind)((_bits >> 16) & 7);

		public bool HasNameInMetadata => (_bits & 0x80000) != 0;

		public ScopedKind Scope => (ScopedKind)((_bits >> 29) & 3);

		public bool HasUnscopedRefAttribute => (_bits & int.MinValue) != 0;

		public PackedFlags(RefKind refKind, bool attributesAreComplete, bool hasNameInMetadata, ScopedKind scope, bool hasUnscopedRefAttribute)
		{
			int num = (int)((uint)(refKind & (RefKind)7) << 16);
			int num2 = (attributesAreComplete ? 65280 : 0);
			int num3 = (hasNameInMetadata ? 524288 : 0);
			int num4 = (int)((uint)(scope & (ScopedKind)3) << 29);
			int num5 = (hasUnscopedRefAttribute ? int.MinValue : 0);
			_bits = num | num2 | num3 | num4 | num5;
		}

		public bool SetWellKnownAttribute(WellKnownAttributeFlags flag, bool value)
		{
			int num = (int)flag << 8;
			if (value)
			{
				num |= (int)flag;
			}
			ThreadSafeFlagOperations.Set(ref _bits, num);
			return value;
		}

		public bool TryGetWellKnownAttribute(WellKnownAttributeFlags flag, out bool value)
		{
			int bits = _bits;
			value = ((uint)bits & (uint)flag) != 0;
			return (bits & ((int)flag << 8)) != 0;
		}

		public bool SetFlowAnalysisAnnotations(FlowAnalysisAnnotations value)
		{
			int toSet = 0x100000 | ((int)(value & (FlowAnalysisAnnotations.MaybeNull | FlowAnalysisAnnotations.NotNull | FlowAnalysisAnnotations.DoesNotReturn | FlowAnalysisAnnotations.AllowNull | FlowAnalysisAnnotations.DisallowNull)) << 21);
			return ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public bool TryGetFlowAnalysisAnnotations(out FlowAnalysisAnnotations value)
		{
			int bits = _bits;
			value = (FlowAnalysisAnnotations)((bits >> 21) & 0xFF);
			return (bits & 0x100000) != 0;
		}
	}

	[Flags]
	private enum IsParamsValues : byte
	{
		NotInitialized = 0,
		Initialized = 1,
		Array = 2,
		Collection = 4
	}

	private sealed class PEParameterSymbolWithCustomModifiers : PEParameterSymbol
	{
		private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

		public PEParameterSymbolWithCustomModifiers(PEModuleSymbol moduleSymbol, Symbol containingSymbol, int ordinal, bool isByRef, ImmutableArray<ModifierInfo<TypeSymbol>> refCustomModifiers, TypeWithAnnotations type, ParameterHandle handle, Symbol nullableContext, bool isReturn, out bool isBad)
			: base(moduleSymbol, containingSymbol, ordinal, isByRef, type, handle, nullableContext, refCustomModifiers.NullToEmpty().Length + type.CustomModifiers.Length, isReturn, out isBad)
		{
			_refCustomModifiers = CSharpCustomModifier.Convert(refCustomModifiers);
		}
	}

	private readonly Symbol _containingSymbol;

	private readonly string _name;

	private readonly TypeWithAnnotations _typeWithAnnotations;

	private readonly ParameterHandle _handle;

	private readonly ParameterAttributes _flags;

	private readonly PEModuleSymbol _moduleSymbol;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private ConstantValue? _lazyDefaultValue = ConstantValue.Unset;

	private IsParamsValues _lazyIsParams;

	private static readonly ImmutableArray<int> s_defaultStringHandlerAttributeIndexes = ImmutableArray.Create(int.MinValue);

	private ImmutableArray<int> _lazyInterpolatedStringHandlerAttributeIndexes = s_defaultStringHandlerAttributeIndexes;

	private int _lazyCallerArgumentExpressionParameterIndex = -2;

	private ImmutableArray<CSharpAttributeData> _lazyHiddenAttributes;

	private readonly ushort _ordinal;

	private PackedFlags _packedFlags;

	private bool HasNameInMetadata => _packedFlags.HasNameInMetadata;

	public override RefKind RefKind => _packedFlags.RefKind;

	public override string Name => _name;

	public override string MetadataName
	{
		get
		{
			if (!HasNameInMetadata)
			{
				return string.Empty;
			}
			return _name;
		}
	}

	public override int MetadataToken => MetadataTokens.GetToken(_handle);

	internal ParameterAttributes Flags => _flags;

	public override int Ordinal => _ordinal;

	public override bool IsDiscard => false;

	internal ParameterHandle Handle => _handle;

	public override Symbol ContainingSymbol => _containingSymbol;

	internal override bool HasMetadataConstantValue => (_flags & ParameterAttributes.HasDefault) != 0;

	internal sealed override ConstantValue? ExplicitDefaultConstantValue
	{
		get
		{
			if (_lazyDefaultValue == ConstantValue.Unset)
			{
				ConstantValue value = ImportConstantValue(!IsMetadataOptional);
				Interlocked.CompareExchange(ref _lazyDefaultValue, value, ConstantValue.Unset);
			}
			return _lazyDefaultValue;
		}
	}

	internal sealed override ConstantValue? DefaultValueFromAttributes => null;

	internal override bool IsMetadataOptional => (_flags & ParameterAttributes.Optional) != 0;

	internal override bool IsIDispatchConstant
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.HasIDispatchConstantAttribute, out var value))
			{
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.HasIDispatchConstantAttribute, _moduleSymbol.Module.HasAttribute(_handle, AttributeDescription.IDispatchConstantAttribute));
			}
			return value;
		}
	}

	internal override bool IsIUnknownConstant
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.HasIUnknownConstantAttribute, out var value))
			{
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.HasIUnknownConstantAttribute, _moduleSymbol.Module.HasAttribute(_handle, AttributeDescription.IUnknownConstantAttribute));
			}
			return value;
		}
	}

	private bool HasCallerLineNumberAttribute
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.HasCallerLineNumberAttribute, out var value))
			{
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.HasCallerLineNumberAttribute, _moduleSymbol.Module.HasAttribute(_handle, AttributeDescription.CallerLineNumberAttribute));
			}
			return value;
		}
	}

	private bool HasCallerFilePathAttribute
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.HasCallerFilePathAttribute, out var value))
			{
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.HasCallerFilePathAttribute, _moduleSymbol.Module.HasAttribute(_handle, AttributeDescription.CallerFilePathAttribute));
			}
			return value;
		}
	}

	private bool HasCallerMemberNameAttribute
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.HasCallerMemberNameAttribute, out var value))
			{
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.HasCallerMemberNameAttribute, _moduleSymbol.Module.HasAttribute(_handle, AttributeDescription.CallerMemberNameAttribute));
			}
			return value;
		}
	}

	internal override bool IsCallerLineNumber
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.IsCallerLineNumber, out var value))
			{
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				bool value2 = HasCallerLineNumberAttribute && ContainingAssembly.TypeConversions.HasCallerLineNumberConversion(base.Type, ref useSiteInfo);
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.IsCallerLineNumber, value2);
			}
			return value;
		}
	}

	internal override bool IsCallerFilePath
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.IsCallerFilePath, out var value))
			{
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				bool value2 = !HasCallerLineNumberAttribute && HasCallerFilePathAttribute && ContainingAssembly.TypeConversions.HasCallerInfoStringConversion(base.Type, ref useSiteInfo);
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.IsCallerFilePath, value2);
			}
			return value;
		}
	}

	internal override bool IsCallerMemberName
	{
		get
		{
			if (!_packedFlags.TryGetWellKnownAttribute(WellKnownAttributeFlags.IsCallerMemberName, out var value))
			{
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				bool value2 = !HasCallerLineNumberAttribute && !HasCallerFilePathAttribute && HasCallerMemberNameAttribute && ContainingAssembly.TypeConversions.HasCallerInfoStringConversion(base.Type, ref useSiteInfo);
				return _packedFlags.SetWellKnownAttribute(WellKnownAttributeFlags.IsCallerMemberName, value2);
			}
			return value;
		}
	}

	internal override int CallerArgumentExpressionParameterIndex
	{
		get
		{
			if (_lazyCallerArgumentExpressionParameterIndex != -2)
			{
				return _lazyCallerArgumentExpressionParameterIndex;
			}
			PEModule.AttributeInfo attributeInfo = _moduleSymbol.Module.FindTargetAttribute(_handle, AttributeDescription.CallerArgumentExpressionAttribute);
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			bool num = attributeInfo.HasValue && !HasCallerLineNumberAttribute && !HasCallerFilePathAttribute && !HasCallerMemberNameAttribute && ContainingAssembly.TypeConversions.HasCallerInfoStringConversion(base.Type, ref useSiteInfo);
			int num2 = -1;
			if (num && _moduleSymbol.Module.TryExtractStringValueFromAttribute(attributeInfo.Handle, out string value) && value != null)
			{
				num2 = SourceComplexParameterSymbolBase.GetCallerArgumentExpressionParameterIndex(this, value);
			}
			_lazyCallerArgumentExpressionParameterIndex = num2;
			return num2;
		}
	}

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations
	{
		get
		{
			if (!_packedFlags.TryGetFlowAnalysisAnnotations(out var value))
			{
				value = DecodeFlowAnalysisAttributes(_moduleSymbol.Module, _handle);
				_packedFlags.SetFlowAnalysisAnnotations(value);
			}
			return value;
		}
	}

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes
	{
		get
		{
			EnsureInterpolatedStringHandlerArgumentAttributeDecoded();
			return _lazyInterpolatedStringHandlerAttributeIndexes.NullToEmpty();
		}
	}

	internal override bool HasInterpolatedStringHandlerArgumentError
	{
		get
		{
			EnsureInterpolatedStringHandlerArgumentAttributeDecoded();
			ImmutableArray<int> lazyInterpolatedStringHandlerAttributeIndexes = _lazyInterpolatedStringHandlerAttributeIndexes;
			return lazyInterpolatedStringHandlerAttributeIndexes.IsDefault;
		}
	}

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => _moduleSymbol.Module.GetStringValuesOfNotNullIfNotNullAttribute(_handle);

	public override TypeWithAnnotations TypeWithAnnotations => _typeWithAnnotations;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal sealed override bool HasEnumeratorCancellationAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEParameterSymbol.cs", 968);
		}
	}

	internal override bool IsMetadataIn => (_flags & ParameterAttributes.In) != 0;

	internal override bool IsMetadataOut => (_flags & ParameterAttributes.Out) != 0;

	internal override bool IsMarshalledExplicitly => (_flags & ParameterAttributes.HasFieldMarshal) != 0;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

	internal override ImmutableArray<byte> MarshallingDescriptor
	{
		get
		{
			if ((_flags & ParameterAttributes.HasFieldMarshal) == 0)
			{
				return default(ImmutableArray<byte>);
			}
			return _moduleSymbol.Module.GetMarshallingDescriptor(_handle);
		}
	}

	internal override UnmanagedType MarshallingType
	{
		get
		{
			if ((_flags & ParameterAttributes.HasFieldMarshal) == 0)
			{
				return (UnmanagedType)0;
			}
			return _moduleSymbol.Module.GetMarshallingType(_handle);
		}
	}

	public override bool IsParamsArray => (GetIsParamsValues() & IsParamsValues.Array) != 0;

	public override bool IsParamsCollection => (GetIsParamsValues() & IsParamsValues.Collection) != 0;

	public override ImmutableArray<Location> Locations => _containingSymbol.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal sealed override ScopedKind DeclaredScope
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEParameterSymbol.cs", 1082);
		}
	}

	internal sealed override ScopedKind EffectiveScope => _packedFlags.Scope;

	internal override bool HasUnscopedRefAttribute => _packedFlags.HasUnscopedRefAttribute;

	internal sealed override bool UseUpdatedEscapeRules => _moduleSymbol.UseUpdatedEscapeRules;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public override bool HasUnsupportedMetadata
	{
		get
		{
			PEModuleSymbol moduleSymbol = (PEModuleSymbol)ContainingModule;
			Symbol containingSymbol = ContainingSymbol;
			MetadataDecoder metadataDecoder;
			if (!(containingSymbol is PEMethodSymbol context))
			{
				if (!(containingSymbol is PEPropertySymbol))
				{
					throw ExceptionUtilities.UnexpectedValue(ContainingSymbol.Kind);
				}
				metadataDecoder = new MetadataDecoder(moduleSymbol, (PENamedTypeSymbol)ContainingType);
			}
			else
			{
				metadataDecoder = new MetadataDecoder(moduleSymbol, context);
			}
			MetadataDecoder decoder = metadataDecoder;
			DiagnosticInfo diagnosticInfo = DeriveCompilerFeatureRequiredDiagnostic(decoder);
			if (diagnosticInfo == null || diagnosticInfo.Code != 9041)
			{
				return base.HasUnsupportedMetadata;
			}
			return true;
		}
	}

	internal static PEParameterSymbol Create(PEModuleSymbol moduleSymbol, PEMethodSymbol containingSymbol, bool isContainingSymbolVirtual, int ordinal, ParamInfo<TypeSymbol> parameterInfo, Symbol nullableContext, bool isReturn, out bool isBad)
	{
		return Create(moduleSymbol, containingSymbol, isContainingSymbolVirtual, ordinal, parameterInfo.IsByRef, parameterInfo.RefCustomModifiers, parameterInfo.Type, parameterInfo.Handle, nullableContext, parameterInfo.CustomModifiers, isReturn, out isBad);
	}

	internal static PEParameterSymbol Create(PEModuleSymbol moduleSymbol, PEPropertySymbol containingSymbol, bool isContainingSymbolVirtual, int ordinal, ParameterHandle handle, ParamInfo<TypeSymbol> parameterInfo, Symbol nullableContext, out bool isBad)
	{
		return Create(moduleSymbol, containingSymbol, isContainingSymbolVirtual, ordinal, parameterInfo.IsByRef, parameterInfo.RefCustomModifiers, parameterInfo.Type, handle, nullableContext, parameterInfo.CustomModifiers, isReturn: false, out isBad);
	}

	private PEParameterSymbol(PEModuleSymbol moduleSymbol, Symbol containingSymbol, int ordinal, bool isByRef, TypeWithAnnotations typeWithAnnotations, ParameterHandle handle, Symbol nullableContext, int countOfCustomModifiers, bool isReturn, out bool isBad)
	{
		isBad = false;
		_moduleSymbol = moduleSymbol;
		_containingSymbol = containingSymbol;
		_ordinal = (ushort)ordinal;
		_handle = handle;
		RefKind refKind = RefKind.None;
		ScopedKind scope = ScopedKind.None;
		bool flag = false;
		if (handle.IsNil)
		{
			refKind = (isByRef ? RefKind.Ref : RefKind.None);
			byte? nullableContextValue = nullableContext.GetNullableContextValue();
			if (nullableContextValue.HasValue)
			{
				typeWithAnnotations = NullableTypeDecoder.TransformType(typeWithAnnotations, nullableContextValue.GetValueOrDefault(), default(ImmutableArray<byte>));
			}
			_lazyCustomAttributes = ImmutableArray<CSharpAttributeData>.Empty;
			_lazyHiddenAttributes = ImmutableArray<CSharpAttributeData>.Empty;
			_lazyDefaultValue = null;
			_lazyIsParams = IsParamsValues.Initialized;
		}
		else
		{
			try
			{
				moduleSymbol.Module.GetParamPropsOrThrow(handle, out _name, out _flags);
			}
			catch (BadImageFormatException)
			{
				isBad = true;
			}
			if (isByRef)
			{
				refKind = (((_flags & (ParameterAttributes.In | ParameterAttributes.Out)) == ParameterAttributes.Out) ? RefKind.Out : ((!isReturn && moduleSymbol.Module.HasRequiresLocationAttribute(handle)) ? RefKind.RefReadOnlyParameter : ((!moduleSymbol.Module.HasIsReadOnlyAttribute(handle)) ? RefKind.Ref : RefKind.In)));
			}
			TypeSymbol type = DynamicTypeDecoder.TransformType(typeWithAnnotations.Type, countOfCustomModifiers, handle, moduleSymbol, refKind);
			type = NativeIntegerTypeDecoder.TransformType(type, handle, moduleSymbol, containingSymbol.ContainingType);
			typeWithAnnotations = typeWithAnnotations.WithTypeAndModifiers(type, typeWithAnnotations.CustomModifiers);
			Symbol accessSymbol = ((containingSymbol.Kind == SymbolKind.Property) ? containingSymbol.ContainingSymbol : containingSymbol);
			typeWithAnnotations = NullableTypeDecoder.TransformType(typeWithAnnotations, handle, moduleSymbol, accessSymbol, nullableContext);
			typeWithAnnotations = TupleTypeDecoder.DecodeTupleTypesIfApplicable(typeWithAnnotations, handle, moduleSymbol);
			flag = _moduleSymbol.Module.HasUnscopedRefAttribute(_handle);
			if (flag)
			{
				if (_moduleSymbol.Module.HasScopedRefAttribute(_handle))
				{
					isBad = true;
				}
				scope = ScopedKind.None;
			}
			else if (_moduleSymbol.Module.HasScopedRefAttribute(_handle))
			{
				if (isByRef)
				{
					scope = ScopedKind.ScopedRef;
				}
				else if (typeWithAnnotations.Type.IsRefLikeOrAllowsRefLikeType())
				{
					scope = ScopedKind.ScopedValue;
				}
				else
				{
					isBad = true;
				}
			}
			else if (ParameterHelpers.IsRefScopedByDefault(_moduleSymbol.UseUpdatedEscapeRules, refKind))
			{
				scope = ScopedKind.ScopedRef;
			}
		}
		_typeWithAnnotations = typeWithAnnotations;
		bool flag2 = !string.IsNullOrEmpty(_name);
		if (!flag2)
		{
			if (isExtensionMarkerParameter(containingSymbol, ordinal))
			{
				_name = "";
			}
			else
			{
				_name = "value";
			}
		}
		_packedFlags = new PackedFlags(refKind, handle.IsNil, flag2, scope, flag);
		static bool isExtensionMarkerParameter(Symbol symbol, int num)
		{
			if (symbol.MetadataName != "<Extension>$")
			{
				return false;
			}
			if ((object)((PENamedTypeSymbol)symbol.ContainingType).GetMarkerMethodSymbol() == symbol)
			{
				return num == 0;
			}
			return false;
		}
	}

	private static PEParameterSymbol Create(PEModuleSymbol moduleSymbol, Symbol containingSymbol, bool isContainingSymbolVirtual, int ordinal, bool isByRef, ImmutableArray<ModifierInfo<TypeSymbol>> refCustomModifiers, TypeSymbol type, ParameterHandle handle, Symbol nullableContext, ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers, bool isReturn, out bool isBad)
	{
		TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations.Create(type, NullableAnnotation.Oblivious, CSharpCustomModifier.Convert(customModifiers));
		PEParameterSymbol pEParameterSymbol = ((customModifiers.IsDefaultOrEmpty && refCustomModifiers.IsDefaultOrEmpty) ? new PEParameterSymbol(moduleSymbol, containingSymbol, ordinal, isByRef, typeWithAnnotations, handle, nullableContext, 0, isReturn, out isBad) : new PEParameterSymbolWithCustomModifiers(moduleSymbol, containingSymbol, ordinal, isByRef, refCustomModifiers, typeWithAnnotations, handle, nullableContext, isReturn, out isBad));
		bool flag = pEParameterSymbol.RefCustomModifiers.HasInAttributeModifier();
		if (isReturn)
		{
			isBad |= pEParameterSymbol.RefKind == RefKind.In != flag;
		}
		else
		{
			RefKind refKind = pEParameterSymbol.RefKind;
			if (refKind - 3 <= RefKind.Ref)
			{
				isBad |= isContainingSymbolVirtual != flag;
			}
			else if (flag)
			{
				isBad = true;
			}
		}
		return pEParameterSymbol;
	}

	internal ConstantValue? ImportConstantValue(bool ignoreAttributes = false)
	{
		ConstantValue constantValue = null;
		if ((_flags & ParameterAttributes.HasDefault) != ParameterAttributes.None)
		{
			constantValue = _moduleSymbol.Module.GetParamDefaultValue(_handle);
		}
		if (constantValue == null && !ignoreAttributes)
		{
			constantValue = GetDefaultDecimalOrDateTimeValue();
		}
		return constantValue;
	}

	private ConstantValue? GetDefaultDecimalOrDateTimeValue()
	{
		ConstantValue defaultValue = null;
		if (_moduleSymbol.Module.HasDateTimeConstantAttribute(_handle, out defaultValue))
		{
			return defaultValue;
		}
		_moduleSymbol.Module.HasDecimalConstantAttribute(_handle, out defaultValue);
		return defaultValue;
	}

	private static FlowAnalysisAnnotations DecodeFlowAnalysisAttributes(PEModule module, ParameterHandle handle)
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
		bool when;
		if (module.HasAttribute(handle, AttributeDescription.MaybeNullAttribute))
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.MaybeNull;
		}
		else if (module.HasMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(handle, AttributeDescription.MaybeNullWhenAttribute, out when))
		{
			flowAnalysisAnnotations = (FlowAnalysisAnnotations)((int)flowAnalysisAnnotations | (when ? 4 : 8));
		}
		bool when2;
		if (module.HasAttribute(handle, AttributeDescription.NotNullAttribute))
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.NotNull;
		}
		else if (module.HasMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(handle, AttributeDescription.NotNullWhenAttribute, out when2))
		{
			flowAnalysisAnnotations = (FlowAnalysisAnnotations)((int)flowAnalysisAnnotations | (when2 ? 16 : 32));
		}
		if (module.HasMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(handle, AttributeDescription.DoesNotReturnIfAttribute, out var when3))
		{
			flowAnalysisAnnotations = (FlowAnalysisAnnotations)((int)flowAnalysisAnnotations | (when3 ? 128 : 64));
		}
		return flowAnalysisAnnotations;
	}

	private void EnsureInterpolatedStringHandlerArgumentAttributeDecoded()
	{
		ImmutableArray<int> lazyInterpolatedStringHandlerAttributeIndexes = _lazyInterpolatedStringHandlerAttributeIndexes;
		if (lazyInterpolatedStringHandlerAttributeIndexes == s_defaultStringHandlerAttributeIndexes)
		{
			lazyInterpolatedStringHandlerAttributeIndexes = DecodeInterpolatedStringHandlerArgumentAttribute();
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyInterpolatedStringHandlerAttributeIndexes, lazyInterpolatedStringHandlerAttributeIndexes, s_defaultStringHandlerAttributeIndexes);
		}
	}

	private ImmutableArray<int> DecodeInterpolatedStringHandlerArgumentAttribute()
	{
		(ImmutableArray<string?> Names, bool FoundAttribute) interpolatedStringHandlerArgumentAttributeValues = _moduleSymbol.Module.GetInterpolatedStringHandlerArgumentAttributeValues(_handle);
		var (immutableArray, _) = interpolatedStringHandlerArgumentAttributeValues;
		if (!interpolatedStringHandlerArgumentAttributeValues.FoundAttribute)
		{
			return ImmutableArray<int>.Empty;
		}
		if (immutableArray.IsDefault || !(base.Type is NamedTypeSymbol { IsInterpolatedStringHandlerType: not false }))
		{
			return default(ImmutableArray<int>);
		}
		Symbol containingSymbol = ContainingSymbol;
		if (containingSymbol is MethodSymbol && containingSymbol.Name == "<Extension>$" && ContainingType is PENamedTypeSymbol { IsExtension: not false } pENamedTypeSymbol)
		{
			MethodSymbol markerMethodSymbol = pENamedTypeSymbol.GetMarkerMethodSymbol();
			if ((object)markerMethodSymbol != null && (object)markerMethodSymbol == ContainingSymbol)
			{
				return default(ImmutableArray<int>);
			}
		}
		if (immutableArray.IsEmpty)
		{
			return ImmutableArray<int>.Empty;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(immutableArray.Length);
		ImmutableArray<ParameterSymbol> parameters = ContainingSymbol.GetParameters();
		foreach (string item in immutableArray)
		{
			string text = item;
			if (text != null)
			{
				if (!(text == ""))
				{
					containingSymbol = ContainingSymbol;
					if ((object)containingSymbol != null && !containingSymbol.IsStatic && containingSymbol.ContainingSymbol is NamedTypeSymbol { IsExtension: not false } namedTypeSymbol2)
					{
						ParameterSymbol extensionParameter = namedTypeSymbol2.ExtensionParameter;
						if ((object)extensionParameter != null)
						{
							string name = extensionParameter.Name;
							if (string.Equals(name, item, StringComparison.Ordinal))
							{
								instance.Add(-2);
								continue;
							}
						}
					}
					ParameterSymbol parameterSymbol = parameters.FirstOrDefault((ParameterSymbol p, string b) => string.Equals(p.Name, b, StringComparison.Ordinal), item);
					if ((object)parameterSymbol != null && (object)parameterSymbol != this)
					{
						instance.Add(parameterSymbol.Ordinal);
						continue;
					}
					instance.Free();
					return default(ImmutableArray<int>);
				}
				bool flag = !ContainingSymbol.RequiresInstanceReceiver();
				if (!flag)
				{
					bool flag2 = ((ContainingSymbol is MethodSymbol { MethodKind: var methodKind } && (methodKind == MethodKind.Constructor || methodKind == MethodKind.DelegateInvoke)) ? true : false);
					flag = flag2;
				}
				if (!flag && !ContainingSymbol.IsExtensionBlockMember())
				{
					instance.Add(-1);
					continue;
				}
			}
			instance.Free();
			return default(ImmutableArray<int>);
		}
		return instance.ToImmutableAndFree();
	}

	private IsParamsValues GetIsParamsValues()
	{
		if ((_lazyIsParams & IsParamsValues.Initialized) == 0)
		{
			IsParamsValues isParamsValues = IsParamsValues.Initialized;
			if (_moduleSymbol.Module.HasParamArrayAttribute(_handle))
			{
				isParamsValues |= IsParamsValues.Array;
			}
			if (_moduleSymbol.Module.HasParamCollectionAttribute(_handle))
			{
				isParamsValues |= IsParamsValues.Collection;
			}
			_lazyIsParams = isParamsValues;
		}
		return _lazyIsParams;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_lazyCustomAttributes.IsDefault)
		{
			PEModuleSymbol pEModuleSymbol = (PEModuleSymbol)ContainingModule;
			IsParamsValues isParamsValues = _lazyIsParams & (IsParamsValues.Initialized | IsParamsValues.Array);
			bool flag = ((isParamsValues == IsParamsValues.NotInitialized || isParamsValues == (IsParamsValues.Initialized | IsParamsValues.Array)) ? true : false);
			bool flag2 = flag;
			isParamsValues = _lazyIsParams & (IsParamsValues.Initialized | IsParamsValues.Collection);
			flag = ((isParamsValues == IsParamsValues.NotInitialized || isParamsValues == (IsParamsValues.Initialized | IsParamsValues.Collection)) ? true : false);
			bool flag3 = flag;
			ConstantValue explicitDefaultConstantValue = ExplicitDefaultConstantValue;
			AttributeDescription filterOut = default(AttributeDescription);
			if ((object)explicitDefaultConstantValue != null)
			{
				if (explicitDefaultConstantValue.Discriminator == ConstantValueTypeDiscriminator.DateTime)
				{
					filterOut = AttributeDescription.DateTimeConstantAttribute;
				}
				else if (explicitDefaultConstantValue.Discriminator == ConstantValueTypeDiscriminator.Decimal)
				{
					filterOut = AttributeDescription.DecimalConstantAttribute;
				}
			}
			bool flag4 = RefKind == RefKind.In;
			bool flag5 = RefKind == RefKind.RefReadOnlyParameter;
			ImmutableArray<CSharpAttributeData> customAttributesForToken = pEModuleSymbol.GetCustomAttributesForToken(_handle, out var filteredOutAttribute, flag2 ? AttributeDescription.ParamArrayAttribute : default(AttributeDescription), out var filteredOutAttribute2, flag3 ? AttributeDescription.ParamCollectionAttribute : default(AttributeDescription), out var filteredOutAttribute3, filterOut, out var _, flag4 ? AttributeDescription.IsReadOnlyAttribute : default(AttributeDescription), out var _, flag5 ? AttributeDescription.RequiresLocationAttribute : default(AttributeDescription), out var _, AttributeDescription.ScopedRefAttribute);
			if (!filteredOutAttribute.IsNil || !filteredOutAttribute3.IsNil || !filteredOutAttribute2.IsNil)
			{
				ArrayBuilder<CSharpAttributeData> instance = ArrayBuilder<CSharpAttributeData>.GetInstance();
				if (!filteredOutAttribute.IsNil)
				{
					instance.Add(new PEAttributeData(pEModuleSymbol, filteredOutAttribute));
				}
				if (!filteredOutAttribute2.IsNil)
				{
					instance.Add(new PEAttributeData(pEModuleSymbol, filteredOutAttribute2));
				}
				if (!filteredOutAttribute3.IsNil)
				{
					instance.Add(new PEAttributeData(pEModuleSymbol, filteredOutAttribute3));
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyHiddenAttributes, instance.ToImmutableAndFree());
			}
			else
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyHiddenAttributes, ImmutableArray<CSharpAttributeData>.Empty);
			}
			if ((_lazyIsParams & IsParamsValues.Initialized) == 0)
			{
				IsParamsValues isParamsValues2 = IsParamsValues.Initialized;
				if (!filteredOutAttribute.IsNil)
				{
					isParamsValues2 |= IsParamsValues.Array;
				}
				if (!filteredOutAttribute2.IsNil)
				{
					isParamsValues2 |= IsParamsValues.Collection;
				}
				_lazyIsParams = isParamsValues2;
			}
			ImmutableInterlocked.InterlockedInitialize(ref _lazyCustomAttributes, customAttributesForToken);
		}
		return _lazyCustomAttributes;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		foreach (CSharpAttributeData attribute in GetAttributes())
		{
			yield return attribute;
		}
		foreach (CSharpAttributeData lazyHiddenAttribute in _lazyHiddenAttributes)
		{
			yield return lazyHiddenAttribute;
		}
	}

	public sealed override bool Equals(Symbol other, TypeCompareKind compareKind)
	{
		if (!(other is NativeIntegerParameterSymbol nativeIntegerParameterSymbol))
		{
			return base.Equals(other, compareKind);
		}
		return nativeIntegerParameterSymbol.Equals(this, compareKind);
	}

	internal DiagnosticInfo? DeriveCompilerFeatureRequiredDiagnostic(MetadataDecoder decoder)
	{
		return PEUtilities.DeriveCompilerFeatureRequiredAttributeDiagnostic(this, (PEModuleSymbol)ContainingModule, Handle, CompilerFeatureRequiredFeatures.None, decoder);
	}
}
