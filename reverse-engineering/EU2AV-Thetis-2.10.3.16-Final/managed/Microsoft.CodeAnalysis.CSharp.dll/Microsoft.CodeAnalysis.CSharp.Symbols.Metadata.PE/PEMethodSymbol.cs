using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.DocumentationComments;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PEMethodSymbol : MethodSymbol
{
	internal class SignatureData
	{
		public readonly SignatureHeader Header;

		public readonly ImmutableArray<ParameterSymbol> Parameters;

		public readonly PEParameterSymbol ReturnParam;

		public SignatureData(SignatureHeader header, ImmutableArray<ParameterSymbol> parameters, PEParameterSymbol returnParam)
		{
			Header = header;
			Parameters = parameters;
			ReturnParam = returnParam;
		}
	}

	private struct PackedFlags
	{
		private const int MethodKindOffset = 0;

		private const int MethodKindMask = 31;

		private const int MethodKindIsPopulatedBit = 32;

		private const int IsExtensionMethodBit = 64;

		private const int IsExtensionMethodIsPopulatedBit = 128;

		private const int IsExplicitFinalizerOverrideBit = 256;

		private const int IsExplicitClassOverrideBit = 512;

		private const int IsExplicitOverrideIsPopulatedBit = 1024;

		private const int IsObsoleteAttributePopulatedBit = 2048;

		private const int IsCustomAttributesPopulatedBit = 4096;

		private const int IsUseSiteDiagnosticPopulatedBit = 8192;

		private const int IsConditionalPopulatedBit = 16384;

		private const int IsOverriddenOrHiddenMembersPopulatedBit = 32768;

		private const int IsReadOnlyBit = 65536;

		private const int IsReadOnlyPopulatedBit = 131072;

		private const int NullableContextOffset = 18;

		private const int NullableContextMask = 7;

		private const int DoesNotReturnBit = 2097152;

		private const int IsDoesNotReturnPopulatedBit = 4194304;

		private const int IsMemberNotNullPopulatedBit = 8388608;

		private const int IsInitOnlyBit = 16777216;

		private const int IsInitOnlyPopulatedBit = 33554432;

		private const int IsUnmanagedCallersOnlyAttributePopulatedBit = 67108864;

		private const int HasSetsRequiredMembersBit = 134217728;

		private const int HasSetsRequiredMembersPopulatedBit = 268435456;

		private const int IsUnscopedRefBit = 536870912;

		private const int IsUnscopedRefPopulatedBit = 1073741824;

		private const int OverloadResolutionPriorityPopulatedBit = int.MinValue;

		private int _bits;

		public MethodKind MethodKind
		{
			get
			{
				return (MethodKind)(_bits & 0x1F);
			}
			set
			{
				_bits = (int)((uint)(_bits & -32) | (uint)(value & (MethodKind)0x1F) | 0x20);
			}
		}

		public bool MethodKindIsPopulated => (_bits & 0x20) != 0;

		public bool IsExtensionMethod => (_bits & 0x40) != 0;

		public bool IsExtensionMethodIsPopulated => (_bits & 0x80) != 0;

		public bool IsExplicitFinalizerOverride => (_bits & 0x100) != 0;

		public bool IsExplicitClassOverride => (_bits & 0x200) != 0;

		public bool IsExplicitOverrideIsPopulated => (_bits & 0x400) != 0;

		public bool IsObsoleteAttributePopulated => (Volatile.Read(in _bits) & 0x800) != 0;

		public bool IsCustomAttributesPopulated => (Volatile.Read(in _bits) & 0x1000) != 0;

		public bool IsUseSiteDiagnosticPopulated => (Volatile.Read(in _bits) & 0x2000) != 0;

		public bool IsConditionalPopulated => (Volatile.Read(in _bits) & 0x4000) != 0;

		public bool IsOverriddenOrHiddenMembersPopulated => (Volatile.Read(in _bits) & 0x8000) != 0;

		public bool IsReadOnly => (_bits & 0x10000) != 0;

		public bool IsReadOnlyPopulated => (_bits & 0x20000) != 0;

		public bool DoesNotReturn => (_bits & 0x200000) != 0;

		public bool IsDoesNotReturnPopulated => (_bits & 0x400000) != 0;

		public bool IsMemberNotNullPopulated => (Volatile.Read(in _bits) & 0x800000) != 0;

		public bool IsInitOnly => (_bits & 0x1000000) != 0;

		public bool IsInitOnlyPopulated => (_bits & 0x2000000) != 0;

		public bool IsUnmanagedCallersOnlyAttributePopulated => (Volatile.Read(in _bits) & 0x4000000) != 0;

		public bool HasSetsRequiredMembers => (_bits & 0x8000000) != 0;

		public bool HasSetsRequiredMembersPopulated => (_bits & 0x10000000) != 0;

		public bool IsUnscopedRef => (_bits & 0x20000000) != 0;

		public bool IsUnscopedRefPopulated => (_bits & 0x40000000) != 0;

		public bool IsOverloadResolutionPriorityPopulated => (Volatile.Read(in _bits) & int.MinValue) != 0;

		private static bool BitsAreUnsetOrSame(int bits, int mask)
		{
			if ((bits & mask) != 0)
			{
				return (bits & mask) == mask;
			}
			return true;
		}

		public void InitializeIsExtensionMethod(bool isExtensionMethod)
		{
			int toSet = (isExtensionMethod ? 64 : 0) | 0x80;
			ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public void InitializeIsReadOnly(bool isReadOnly)
		{
			int toSet = (isReadOnly ? 65536 : 0) | 0x20000;
			ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public void InitializeMethodKind(MethodKind methodKind)
		{
			int toSet = (int)((methodKind & (MethodKind)0x1F) | (MethodKind)0x20);
			ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public void InitializeIsExplicitOverride(bool isExplicitFinalizerOverride, bool isExplicitClassOverride)
		{
			int toSet = (isExplicitFinalizerOverride ? 256 : 0) | (isExplicitClassOverride ? 512 : 0) | 0x400;
			ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public void SetIsObsoleteAttributePopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 2048);
		}

		public void SetIsCustomAttributesPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 4096);
		}

		public void SetIsUseSiteDiagnosticPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 8192);
		}

		public void SetIsConditionalAttributePopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 16384);
		}

		public void SetIsOverriddenOrHiddenMembersPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 32768);
		}

		public bool TryGetNullableContext(out byte? value)
		{
			return ((NullableContextKind)((_bits >> 18) & 7)).TryGetByte(out value);
		}

		public bool SetNullableContext(byte? value)
		{
			return ThreadSafeFlagOperations.Set(ref _bits, (int)((uint)(value.ToNullableContextFlags() & (NullableContextKind)7) << 18));
		}

		public bool InitializeDoesNotReturn(bool value)
		{
			int num = 4194304;
			if (value)
			{
				num |= 0x200000;
			}
			return ThreadSafeFlagOperations.Set(ref _bits, num);
		}

		public void SetIsMemberNotNullPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 8388608);
		}

		public void InitializeIsInitOnly(bool isInitOnly)
		{
			int toSet = (isInitOnly ? 16777216 : 0) | 0x2000000;
			ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public void SetIsUnmanagedCallersOnlyAttributePopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 67108864);
		}

		public bool InitializeSetsRequiredMembersBit(bool value)
		{
			int num = 268435456;
			if (value)
			{
				num |= 0x8000000;
			}
			return ThreadSafeFlagOperations.Set(ref _bits, num);
		}

		public bool InitializeIsUnscopedRef(bool value)
		{
			int num = 1073741824;
			if (value)
			{
				num |= 0x20000000;
			}
			return ThreadSafeFlagOperations.Set(ref _bits, num);
		}

		public void SetIsOverloadResolutionPriorityPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, int.MinValue);
		}
	}

	private sealed class UncommonFields
	{
		public ParameterSymbol _lazyThisParameter;

		public Tuple<CultureInfo, string> _lazyDocComment;

		public OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembersResult;

		public ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

		public ImmutableArray<string> _lazyConditionalAttributeSymbols;

		public ObsoleteAttributeData _lazyObsoleteAttributeData;

		public UnmanagedCallersOnlyAttributeData _lazyUnmanagedCallersOnlyAttributeData;

		public CachedUseSiteInfo<AssemblySymbol> _lazyCachedUseSiteInfo;

		public ImmutableArray<string> _lazyNotNullMembers;

		public ImmutableArray<string> _lazyNotNullMembersWhenTrue;

		public ImmutableArray<string> _lazyNotNullMembersWhenFalse;

		public MethodSymbol _lazyExplicitClassOverride;

		public int _lazyOverloadResolutionPriority;
	}

	private readonly MethodDefinitionHandle _handle;

	private readonly string _name;

	private readonly PENamedTypeSymbol _containingType;

	private Symbol _associatedPropertyOrEventOpt;

	private PackedFlags _packedFlags;

	private readonly ushort _flags;

	private readonly ushort _implFlags;

	private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

	private SignatureData _lazySignature;

	private ImmutableArray<MethodSymbol> _lazyExplicitMethodImplementations;

	private UncommonFields _uncommonFields;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override string Name => _name;

	public override int MetadataToken => MetadataTokens.GetToken(_handle);

	internal MethodAttributes Flags => (MethodAttributes)_flags;

	internal override bool HasSpecialName => HasFlag(MethodAttributes.SpecialName);

	internal override bool HasRuntimeSpecialName => HasFlag(MethodAttributes.RTSpecialName);

	internal override MethodImplAttributes ImplementationAttributes => (MethodImplAttributes)_implFlags;

	internal override bool RequiresSecurityObject => HasFlag(MethodAttributes.RequireSecObject);

	internal override bool ReturnValueIsMarshalledExplicitly => ReturnTypeParameter.IsMarshalledExplicitly;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => ReturnTypeParameter.MarshallingInformation;

	internal override ImmutableArray<byte> ReturnValueMarshallingDescriptor => ReturnTypeParameter.MarshallingDescriptor;

	internal override bool IsAccessCheckedOnOverride => HasFlag(MethodAttributes.CheckAccessOnOverride);

	internal override bool HasDeclarativeSecurity => HasFlag(MethodAttributes.HasSecurity);

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			switch (Flags & MethodAttributes.MemberAccessMask)
			{
			case MethodAttributes.Assembly:
				return Accessibility.Internal;
			case MethodAttributes.FamORAssem:
				return Accessibility.ProtectedOrInternal;
			case MethodAttributes.FamANDAssem:
				return Accessibility.ProtectedAndInternal;
			case MethodAttributes.PrivateScope:
			case MethodAttributes.Private:
				return Accessibility.Private;
			case MethodAttributes.Public:
				return Accessibility.Public;
			case MethodAttributes.Family:
				return Accessibility.Protected;
			default:
				return Accessibility.Private;
			}
		}
	}

	public override bool IsExtern => HasFlag(MethodAttributes.PinvokeImpl);

	internal override bool IsExternal
	{
		get
		{
			if (!IsExtern)
			{
				return (ImplementationAttributes & MethodImplAttributes.CodeTypeMask) != 0;
			}
			return true;
		}
	}

	public override bool IsVararg => Signature.Header.CallingConvention == SignatureCallingConvention.VarArgs;

	public override bool IsGenericMethod => Arity > 0;

	public override bool IsAsync => false;

	public override int Arity
	{
		get
		{
			if (!_lazyTypeParameters.IsDefault)
			{
				return _lazyTypeParameters.Length;
			}
			try
			{
				MetadataDecoder<PEModuleSymbol, TypeSymbol, MethodSymbol, FieldSymbol, Symbol>.GetSignatureCountsOrThrow(_containingType.ContainingPEModule.Module, _handle, out var _, out var typeParameterCount);
				return typeParameterCount;
			}
			catch (BadImageFormatException)
			{
				return TypeParameters.Length;
			}
		}
	}

	internal MethodDefinitionHandle Handle => _handle;

	public override bool IsAbstract => HasFlag(MethodAttributes.Abstract);

	public override bool IsSealed
	{
		get
		{
			if (IsMetadataFinal)
			{
				if (!_containingType.IsInterface)
				{
					if (!IsAbstract)
					{
						return IsOverride;
					}
					return false;
				}
				if (IsAbstract && IsMetadataVirtual())
				{
					return !IsMetadataNewSlot();
				}
				return false;
			}
			return false;
		}
	}

	public override bool HidesBaseMethodsByName => !HasFlag(MethodAttributes.HideBySig);

	public override bool IsVirtual
	{
		get
		{
			if (IsMetadataVirtual() && !IsDestructor && !IsMetadataFinal && !IsAbstract)
			{
				if (!_containingType.IsInterface)
				{
					return !IsOverride;
				}
				if (!IsStatic)
				{
					return IsMetadataNewSlot();
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsOverride
	{
		get
		{
			if (!_containingType.IsInterface && IsMetadataVirtual() && !IsDestructor)
			{
				if (IsMetadataNewSlot() || (object)_containingType.BaseTypeNoUseSiteDiagnostics == null)
				{
					return IsExplicitClassOverride;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsStatic => HasFlag(MethodAttributes.Static);

	internal override bool IsMetadataFinal => HasFlag(MethodAttributes.Final);

	private bool IsExplicitFinalizerOverride
	{
		get
		{
			if (!_packedFlags.IsExplicitOverrideIsPopulated)
			{
				_ = ExplicitInterfaceImplementations;
			}
			return _packedFlags.IsExplicitFinalizerOverride;
		}
	}

	private bool IsExplicitClassOverride
	{
		get
		{
			if (!_packedFlags.IsExplicitOverrideIsPopulated)
			{
				_ = ExplicitInterfaceImplementations;
			}
			return _packedFlags.IsExplicitClassOverride;
		}
	}

	private bool IsDestructor => MethodKind == MethodKind.Destructor;

	public override bool ReturnsVoid => base.ReturnType.IsVoidType();

	internal override int ParameterCount
	{
		get
		{
			if (_lazySignature != null)
			{
				return _lazySignature.Parameters.Length;
			}
			try
			{
				MetadataDecoder<PEModuleSymbol, TypeSymbol, MethodSymbol, FieldSymbol, Symbol>.GetSignatureCountsOrThrow(_containingType.ContainingPEModule.Module, _handle, out var parameterCount, out var _);
				return parameterCount;
			}
			catch (BadImageFormatException)
			{
				return Parameters.Length;
			}
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters => Signature.Parameters;

	internal PEParameterSymbol ReturnTypeParameter => Signature.ReturnParam;

	public override RefKind RefKind => Signature.ReturnParam.RefKind;

	public override TypeWithAnnotations ReturnTypeWithAnnotations => Signature.ReturnParam.TypeWithAnnotations;

	public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => Signature.ReturnParam.FlowAnalysisAnnotations;

	public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => Signature.ReturnParam.NotNullIfParameterNotNull;

	public override FlowAnalysisAnnotations FlowAnalysisAnnotations
	{
		get
		{
			if (!_packedFlags.IsDoesNotReturnPopulated)
			{
				bool value = _containingType.ContainingPEModule.Module.HasDoesNotReturnAttribute(_handle);
				_packedFlags.InitializeDoesNotReturn(value);
			}
			if (!_packedFlags.DoesNotReturn)
			{
				return FlowAnalysisAnnotations.None;
			}
			return FlowAnalysisAnnotations.DoesNotReturn;
		}
	}

	internal override ImmutableArray<string> NotNullMembers
	{
		get
		{
			if (!_packedFlags.IsMemberNotNullPopulated)
			{
				PopulateMemberNotNullData();
			}
			UncommonFields uncommonFields = _uncommonFields;
			if (uncommonFields == null)
			{
				return ImmutableArray<string>.Empty;
			}
			ImmutableArray<string> lazyNotNullMembers = uncommonFields._lazyNotNullMembers;
			if (!lazyNotNullMembers.IsDefault)
			{
				return lazyNotNullMembers;
			}
			return ImmutableArray<string>.Empty;
		}
	}

	internal override ImmutableArray<string> NotNullWhenTrueMembers
	{
		get
		{
			if (!_packedFlags.IsMemberNotNullPopulated)
			{
				PopulateMemberNotNullData();
			}
			UncommonFields uncommonFields = _uncommonFields;
			if (uncommonFields == null)
			{
				return ImmutableArray<string>.Empty;
			}
			ImmutableArray<string> lazyNotNullMembersWhenTrue = uncommonFields._lazyNotNullMembersWhenTrue;
			if (!lazyNotNullMembersWhenTrue.IsDefault)
			{
				return lazyNotNullMembersWhenTrue;
			}
			return ImmutableArray<string>.Empty;
		}
	}

	internal override ImmutableArray<string> NotNullWhenFalseMembers
	{
		get
		{
			if (!_packedFlags.IsMemberNotNullPopulated)
			{
				PopulateMemberNotNullData();
			}
			UncommonFields uncommonFields = _uncommonFields;
			if (uncommonFields == null)
			{
				return ImmutableArray<string>.Empty;
			}
			ImmutableArray<string> lazyNotNullMembersWhenFalse = uncommonFields._lazyNotNullMembersWhenFalse;
			if (!lazyNotNullMembersWhenFalse.IsDefault)
			{
				return lazyNotNullMembersWhenFalse;
			}
			return ImmutableArray<string>.Empty;
		}
	}

	public override ImmutableArray<CustomModifier> RefCustomModifiers => Signature.ReturnParam.RefCustomModifiers;

	internal SignatureData Signature => _lazySignature ?? LoadSignature();

	public override BlobHandle MetadataSignatureHandle
	{
		get
		{
			try
			{
				return _containingType.ContainingPEModule.Module.GetMethodSignatureOrThrow(_handle);
			}
			catch (BadImageFormatException)
			{
				return default(BlobHandle);
			}
		}
	}

	public override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			DiagnosticInfo diagnosticInfo = null;
			ImmutableArray<TypeParameterSymbol> result = EnsureTypeParametersAreLoaded(ref diagnosticInfo);
			if (diagnosticInfo != null)
			{
				InitializeUseSiteDiagnostic(new UseSiteInfo<AssemblySymbol>(diagnosticInfo));
			}
			return result;
		}
	}

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations
	{
		get
		{
			if (!IsGenericMethod)
			{
				return ImmutableArray<TypeWithAnnotations>.Empty;
			}
			return GetTypeParametersAsTypeArguments();
		}
	}

	public override Symbol AssociatedSymbol => _associatedPropertyOrEventOpt;

	public override bool IsExtensionMethod
	{
		get
		{
			if (!_packedFlags.IsExtensionMethodIsPopulated)
			{
				bool isExtensionMethod = false;
				if (MethodKind == MethodKind.Ordinary && IsValidExtensionMethodSignature() && ContainingType.MightContainExtensionMethods)
				{
					isExtensionMethod = _containingType.ContainingPEModule.Module.HasExtensionAttribute(_handle, ignoreCase: false);
				}
				_packedFlags.InitializeIsExtensionMethod(isExtensionMethod);
			}
			return _packedFlags.IsExtensionMethod;
		}
	}

	public override ImmutableArray<Location> Locations => _containingType.ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override MethodKind MethodKind
	{
		get
		{
			if (!_packedFlags.MethodKindIsPopulated)
			{
				_packedFlags.InitializeMethodKind(ComputeMethodKind());
			}
			return _packedFlags.MethodKind;
		}
	}

	internal override CallingConvention CallingConvention => (CallingConvention)Signature.Header.RawValue;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
	{
		get
		{
			ImmutableArray<MethodSymbol> lazyExplicitMethodImplementations = _lazyExplicitMethodImplementations;
			if (!lazyExplicitMethodImplementations.IsDefault)
			{
				return lazyExplicitMethodImplementations;
			}
			ImmutableArray<MethodSymbol> explicitlyOverriddenMethods = new MetadataDecoder(_containingType.ContainingPEModule, _containingType).GetExplicitlyOverriddenMethods(_containingType.Handle, _handle, ContainingType);
			bool flag = false;
			bool flag2 = false;
			foreach (MethodSymbol item in explicitlyOverriddenMethods)
			{
				if (!item.ContainingType.IsInterface)
				{
					flag = true;
					flag2 = item.ContainingType.SpecialType == SpecialType.System_Object && item.Name == "Finalize" && item.MethodKind == MethodKind.Destructor;
				}
				if (flag & flag2)
				{
					break;
				}
			}
			lazyExplicitMethodImplementations = explicitlyOverriddenMethods;
			if (flag)
			{
				ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
				foreach (MethodSymbol item2 in explicitlyOverriddenMethods)
				{
					if (item2.ContainingType.IsInterface)
					{
						instance.Add(item2);
					}
				}
				lazyExplicitMethodImplementations = instance.ToImmutableAndFree();
				MethodSymbol methodSymbol = null;
				foreach (MethodSymbol item3 in explicitlyOverriddenMethods)
				{
					if (item3.ContainingType.IsClassType())
					{
						if ((object)methodSymbol != null)
						{
							methodSymbol = null;
							break;
						}
						methodSymbol = item3;
					}
				}
				if ((object)methodSymbol != null)
				{
					Interlocked.CompareExchange(ref AccessUncommonFields()._lazyExplicitClassOverride, methodSymbol, null);
				}
			}
			_packedFlags.InitializeIsExplicitOverride(flag2, flag);
			return InterlockedOperations.Initialize(ref _lazyExplicitMethodImplementations, lazyExplicitMethodImplementations);
		}
	}

	internal MethodSymbol ExplicitlyOverriddenClassMethod
	{
		get
		{
			if (!IsExplicitClassOverride)
			{
				return null;
			}
			return AccessUncommonFields()._lazyExplicitClassOverride;
		}
	}

	internal override bool IsDeclaredReadOnly
	{
		get
		{
			if (!_packedFlags.IsReadOnlyPopulated)
			{
				bool isReadOnly = false;
				if (base.IsValidReadOnlyTarget)
				{
					isReadOnly = _containingType.ContainingPEModule.Module.HasIsReadOnlyAttribute(_handle);
				}
				_packedFlags.InitializeIsReadOnly(isReadOnly);
			}
			return _packedFlags.IsReadOnly;
		}
	}

	internal override bool IsInitOnly
	{
		get
		{
			if (!_packedFlags.IsInitOnlyPopulated)
			{
				bool isInitOnly = !IsStatic && MethodKind == MethodKind.PropertySet && ReturnTypeWithAnnotations.CustomModifiers.HasIsExternalInitModifier();
				_packedFlags.InitializeIsInitOnly(isInitOnly);
			}
			return _packedFlags.IsInitOnly;
		}
	}

	protected override bool HasSetsRequiredMembersImpl
	{
		get
		{
			if (!_packedFlags.HasSetsRequiredMembersPopulated)
			{
				bool value = _containingType.ContainingPEModule.Module.HasAttribute(_handle, AttributeDescription.SetsRequiredMembersAttribute);
				_packedFlags.InitializeSetsRequiredMembersBit(value);
			}
			return _packedFlags.HasSetsRequiredMembers;
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!_packedFlags.IsObsoleteAttributePopulated)
			{
				ObsoleteAttributeData obsoleteAttributeData = ObsoleteAttributeHelpers.GetObsoleteDataFromMetadata(_handle, (PEModuleSymbol)ContainingModule, ignoreByRefLikeMarker: false, MethodKind == MethodKind.Constructor);
				if (obsoleteAttributeData != null)
				{
					obsoleteAttributeData = InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyObsoleteAttributeData, obsoleteAttributeData, ObsoleteAttributeData.Uninitialized);
				}
				_packedFlags.SetIsObsoleteAttributePopulated();
				return obsoleteAttributeData;
			}
			UncommonFields uncommonFields = _uncommonFields;
			if (uncommonFields == null)
			{
				return null;
			}
			ObsoleteAttributeData lazyObsoleteAttributeData = uncommonFields._lazyObsoleteAttributeData;
			if (lazyObsoleteAttributeData != ObsoleteAttributeData.Uninitialized)
			{
				return lazyObsoleteAttributeData;
			}
			return InterlockedOperations.Initialize(ref uncommonFields._lazyObsoleteAttributeData, (ObsoleteAttributeData)null, ObsoleteAttributeData.Uninitialized);
		}
	}

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 1693);
		}
	}

	internal override bool GenerateDebugInfo => false;

	internal override OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers
	{
		get
		{
			if (!_packedFlags.IsOverriddenOrHiddenMembersPopulated)
			{
				OverriddenOrHiddenMembersResult overriddenOrHiddenMembersResult = base.OverriddenOrHiddenMembers;
				if (overriddenOrHiddenMembersResult != OverriddenOrHiddenMembersResult.Empty)
				{
					overriddenOrHiddenMembersResult = InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyOverriddenOrHiddenMembersResult, overriddenOrHiddenMembersResult);
				}
				_packedFlags.SetIsOverriddenOrHiddenMembersPopulated();
				return overriddenOrHiddenMembersResult;
			}
			UncommonFields uncommonFields = _uncommonFields;
			if (uncommonFields == null)
			{
				return OverriddenOrHiddenMembersResult.Empty;
			}
			return uncommonFields._lazyOverriddenOrHiddenMembersResult ?? InterlockedOperations.Initialize(ref uncommonFields._lazyOverriddenOrHiddenMembersResult, OverriddenOrHiddenMembersResult.Empty);
		}
	}

	public override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 1737);
		}
	}

	internal override CSharpCompilation DeclaringCompilation => null;

	internal bool TestIsExtensionBitSet => _packedFlags.IsExtensionMethodIsPopulated;

	internal bool TestIsExtensionBitTrue => _packedFlags.IsExtensionMethod;

	internal sealed override bool HasUnscopedRefAttribute
	{
		get
		{
			if (!_packedFlags.IsUnscopedRefPopulated)
			{
				bool value = _containingType.ContainingPEModule.Module.HasUnscopedRefAttribute(_handle);
				_packedFlags.InitializeIsUnscopedRef(value);
			}
			return _packedFlags.IsUnscopedRef;
		}
	}

	internal sealed override bool UseUpdatedEscapeRules => ContainingModule.UseUpdatedEscapeRules;

	private UncommonFields AccessUncommonFields()
	{
		return _uncommonFields ?? InterlockedOperations.Initialize(ref _uncommonFields, createUncommonFields());
		UncommonFields createUncommonFields()
		{
			UncommonFields uncommonFields = new UncommonFields();
			if (!_packedFlags.IsObsoleteAttributePopulated)
			{
				uncommonFields._lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;
			}
			if (!_packedFlags.IsUnmanagedCallersOnlyAttributePopulated)
			{
				uncommonFields._lazyUnmanagedCallersOnlyAttributeData = UnmanagedCallersOnlyAttributeData.Uninitialized;
			}
			if (_packedFlags.IsCustomAttributesPopulated)
			{
				uncommonFields._lazyCustomAttributes = ImmutableArray<CSharpAttributeData>.Empty;
			}
			if (_packedFlags.IsConditionalPopulated)
			{
				uncommonFields._lazyConditionalAttributeSymbols = ImmutableArray<string>.Empty;
			}
			if (_packedFlags.IsOverriddenOrHiddenMembersPopulated)
			{
				uncommonFields._lazyOverriddenOrHiddenMembersResult = OverriddenOrHiddenMembersResult.Empty;
			}
			if (_packedFlags.IsMemberNotNullPopulated)
			{
				uncommonFields._lazyNotNullMembers = ImmutableArray<string>.Empty;
				uncommonFields._lazyNotNullMembersWhenTrue = ImmutableArray<string>.Empty;
				uncommonFields._lazyNotNullMembersWhenFalse = ImmutableArray<string>.Empty;
			}
			if (_packedFlags.IsExplicitOverrideIsPopulated)
			{
				uncommonFields._lazyExplicitClassOverride = null;
			}
			return uncommonFields;
		}
	}

	internal PEMethodSymbol(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, MethodDefinitionHandle methodDef)
	{
		_handle = methodDef;
		_containingType = containingType;
		MethodAttributes flags = MethodAttributes.PrivateScope;
		try
		{
			moduleSymbol.Module.GetMethodDefPropsOrThrow(methodDef, out _name, out var implFlags, out flags, out var _);
			_implFlags = (ushort)implFlags;
		}
		catch (BadImageFormatException)
		{
			if (_name == null)
			{
				_name = string.Empty;
			}
			InitializeUseSiteDiagnostic(new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this)));
		}
		_flags = (ushort)flags;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		thisParameter = ((IsStatic || this.IsExtensionBlockMember()) ? null : (_uncommonFields?._lazyThisParameter ?? InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyThisParameter, new ThisParameterSymbol(this))));
		return true;
	}

	private bool HasFlag(MethodAttributes flag)
	{
		return ((ushort)flag & _flags) != 0;
	}

	public override DllImportData GetDllImportData()
	{
		if (!HasFlag(MethodAttributes.PinvokeImpl))
		{
			return null;
		}
		return _containingType.ContainingPEModule.Module.GetDllImportData(_handle);
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 491);
	}

	internal override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return HasFlag(MethodAttributes.Virtual);
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return HasFlag(MethodAttributes.VtableLayoutMask);
	}

	private void PopulateMemberNotNullData()
	{
		PEModule module = _containingType.ContainingPEModule.Module;
		ImmutableArray<string> memberNotNullAttributeValues = module.GetMemberNotNullAttributeValues(_handle);
		if (!memberNotNullAttributeValues.IsEmpty)
		{
			InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyNotNullMembers, memberNotNullAttributeValues);
		}
		var (initializedValue, initializedValue2) = module.GetMemberNotNullWhenAttributeValues(_handle);
		if (!initializedValue.IsEmpty)
		{
			InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyNotNullMembersWhenTrue, initializedValue);
		}
		if (!initializedValue2.IsEmpty)
		{
			InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyNotNullMembersWhenFalse, initializedValue2);
		}
		_packedFlags.SetIsMemberNotNullPopulated();
	}

	internal bool SetAssociatedProperty(PEPropertySymbol propertySymbol, MethodKind methodKind)
	{
		return SetAssociatedPropertyOrEvent(propertySymbol, methodKind);
	}

	internal bool SetAssociatedEvent(PEEventSymbol eventSymbol, MethodKind methodKind)
	{
		return SetAssociatedPropertyOrEvent(eventSymbol, methodKind);
	}

	private bool SetAssociatedPropertyOrEvent(Symbol propertyOrEventSymbol, MethodKind methodKind)
	{
		if ((object)_associatedPropertyOrEventOpt == null)
		{
			_associatedPropertyOrEventOpt = propertyOrEventSymbol;
			_packedFlags.MethodKind = methodKind;
			return true;
		}
		return false;
	}

	private SignatureData LoadSignature()
	{
		PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
		ParamInfo<TypeSymbol>[] signatureForMethod = new MetadataDecoder(containingPEModule, this).GetSignatureForMethod(_handle, out var signatureHeader, out var metadataException);
		bool flag = metadataException != null;
		if (!signatureHeader.IsGeneric && _lazyTypeParameters.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameters, ImmutableArray<TypeParameterSymbol>.Empty);
		}
		int num = signatureForMethod.Length - 1;
		bool isBad;
		ImmutableArray<ParameterSymbol> parameters;
		if (num > 0)
		{
			ImmutableArray<ParameterSymbol>.Builder builder = ImmutableArray.CreateBuilder<ParameterSymbol>(num);
			for (int i = 0; i < num; i++)
			{
				builder.Add(PEParameterSymbol.Create(containingPEModule, this, IsMetadataVirtual(), i, signatureForMethod[i + 1], this, isReturn: false, out isBad));
				if (isBad)
				{
					flag = true;
				}
			}
			parameters = builder.ToImmutable();
		}
		else
		{
			parameters = ImmutableArray<ParameterSymbol>.Empty;
		}
		TypeSymbol type = signatureForMethod[0].Type.AsDynamicIfNoPia(_containingType);
		signatureForMethod[0].Type = type;
		PEParameterSymbol returnParam = PEParameterSymbol.Create(containingPEModule, this, IsMetadataVirtual(), 0, signatureForMethod[0], this, isReturn: true, out isBad);
		if (flag | isBad)
		{
			InitializeUseSiteDiagnostic(new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this)));
		}
		SignatureData value = new SignatureData(signatureHeader, parameters, returnParam);
		return InterlockedOperations.Initialize(ref _lazySignature, value);
	}

	private ImmutableArray<TypeParameterSymbol> EnsureTypeParametersAreLoaded(ref DiagnosticInfo diagnosticInfo)
	{
		ImmutableArray<TypeParameterSymbol> lazyTypeParameters = _lazyTypeParameters;
		if (!lazyTypeParameters.IsDefault)
		{
			return lazyTypeParameters;
		}
		return InterlockedOperations.Initialize(ref _lazyTypeParameters, LoadTypeParameters(ref diagnosticInfo));
	}

	private ImmutableArray<TypeParameterSymbol> LoadTypeParameters(ref DiagnosticInfo diagnosticInfo)
	{
		try
		{
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			GenericParameterHandleCollection genericParametersForMethodOrThrow = containingPEModule.Module.GetGenericParametersForMethodOrThrow(_handle);
			if (genericParametersForMethodOrThrow.Count == 0)
			{
				return ImmutableArray<TypeParameterSymbol>.Empty;
			}
			ImmutableArray<TypeParameterSymbol>.Builder builder = ImmutableArray.CreateBuilder<TypeParameterSymbol>(genericParametersForMethodOrThrow.Count);
			for (int i = 0; i < genericParametersForMethodOrThrow.Count; i++)
			{
				builder.Add(new PETypeParameterSymbol(containingPEModule, this, (ushort)i, genericParametersForMethodOrThrow[i]));
			}
			return builder.ToImmutable();
		}
		catch (BadImageFormatException)
		{
			diagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this);
			return ImmutableArray<TypeParameterSymbol>.Empty;
		}
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (!_packedFlags.IsCustomAttributesPopulated)
		{
			ImmutableArray<CSharpAttributeData> customAttributes = default(ImmutableArray<CSharpAttributeData>);
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			bool isExtensionMethodIsPopulated = _packedFlags.IsExtensionMethodIsPopulated;
			bool num = (isExtensionMethodIsPopulated ? _packedFlags.IsExtensionMethod : (MethodKind == MethodKind.Ordinary && IsValidExtensionMethodSignature() && _containingType.MightContainExtensionMethods));
			bool isReadOnlyPopulated = _packedFlags.IsReadOnlyPopulated;
			bool flag = (isReadOnlyPopulated ? _packedFlags.IsReadOnly : base.IsValidReadOnlyTarget);
			bool flag2 = this.ShouldCheckRequiredMembers() && ContainingType.HasAnyRequiredMembers;
			bool flag3 = SourceMethodSymbol.IsInstanceIncrementDecrementOrCompoundAssignmentOperator(this);
			bool flag4 = this.IsExtensionBlockMember();
			bool isExtensionMethod = false;
			bool isReadOnly = false;
			if (num | flag | flag2 | flag3 | flag4)
			{
				customAttributes = containingPEModule.GetCustomAttributesForToken(_handle, out var filteredOutAttribute, AttributeDescription.CaseSensitiveExtensionAttribute, out var filteredOutAttribute2, AttributeDescription.IsReadOnlyAttribute, out var _, ((flag2 | flag3) && DeriveCompilerFeatureRequiredDiagnostic() == null) ? AttributeDescription.CompilerFeatureRequiredAttribute : default(AttributeDescription), out var _, (flag2 && ObsoleteAttributeData == null) ? AttributeDescription.ObsoleteAttribute : default(AttributeDescription), out var _, AttributeDescription.ExtensionMarkerAttribute, out var _, default(AttributeDescription));
				isExtensionMethod = !filteredOutAttribute.IsNil;
				isReadOnly = !filteredOutAttribute2.IsNil;
			}
			else
			{
				containingPEModule.LoadCustomAttributes(_handle, ref customAttributes);
			}
			if (!isExtensionMethodIsPopulated)
			{
				_packedFlags.InitializeIsExtensionMethod(isExtensionMethod);
			}
			if (!isReadOnlyPopulated)
			{
				_packedFlags.InitializeIsReadOnly(isReadOnly);
			}
			if (!customAttributes.IsEmpty)
			{
				customAttributes = InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyCustomAttributes, customAttributes);
			}
			_packedFlags.SetIsCustomAttributesPopulated();
			return customAttributes;
		}
		UncommonFields uncommonFields = _uncommonFields;
		if (uncommonFields == null)
		{
			return ImmutableArray<CSharpAttributeData>.Empty;
		}
		ImmutableArray<CSharpAttributeData> lazyCustomAttributes = uncommonFields._lazyCustomAttributes;
		if (!lazyCustomAttributes.IsDefault)
		{
			return lazyCustomAttributes;
		}
		return InterlockedOperations.Initialize(ref uncommonFields._lazyCustomAttributes, ImmutableArray<CSharpAttributeData>.Empty);
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return GetAttributes();
	}

	public override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return Signature.ReturnParam.GetAttributes();
	}

	internal override byte? GetNullableContextValue()
	{
		if (!_packedFlags.TryGetNullableContext(out var value))
		{
			value = (_containingType.ContainingPEModule.Module.HasNullableContextAttribute(_handle, out var value2) ? new byte?(value2) : _containingType.GetNullableContextValue());
			_packedFlags.SetNullableContext(value);
		}
		return value;
	}

	internal override byte? GetLocalNullableContextValue()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 1103);
	}

	private bool IsValidExtensionMethodSignature()
	{
		if (!IsStatic)
		{
			return false;
		}
		ImmutableArray<ParameterSymbol> parameters = Parameters;
		if (parameters.Length == 0)
		{
			return false;
		}
		ParameterSymbol parameterSymbol = parameters[0];
		RefKind refKind = parameterSymbol.RefKind;
		if (refKind <= RefKind.Ref || refKind - 3 <= RefKind.Ref)
		{
			return !parameterSymbol.IsParams;
		}
		return false;
	}

	private bool IsValidStaticUserDefinedOperatorSignature(int parameterCount)
	{
		if (ReturnsVoid || IsGenericMethod || IsVararg || ParameterCount != parameterCount || this.IsParams())
		{
			return false;
		}
		return HasValidOperatorParameterRefKinds();
	}

	private bool HasValidOperatorParameterRefKinds()
	{
		if (base.ParameterRefKinds.IsDefault)
		{
			return true;
		}
		foreach (RefKind parameterRefKind in base.ParameterRefKinds)
		{
			switch (parameterRefKind)
			{
			case RefKind.Ref:
			case RefKind.Out:
			case RefKind.RefReadOnlyParameter:
				return false;
			default:
				throw ExceptionUtilities.UnexpectedValue(parameterRefKind);
			case RefKind.None:
			case RefKind.In:
				break;
			}
		}
		return true;
	}

	private bool IsValidInstanceUserDefinedOperatorSignature(int parameterCount)
	{
		if (!ReturnsVoid || IsGenericMethod || IsVararg || ParameterCount != parameterCount || this.IsParams())
		{
			return false;
		}
		return HasValidOperatorParameterRefKinds();
	}

	private MethodKind ComputeMethodKind()
	{
		if (HasSpecialName)
		{
			if (_name.StartsWith(".", StringComparison.Ordinal))
			{
				if ((Flags & (MethodAttributes.Virtual | MethodAttributes.RTSpecialName)) == MethodAttributes.RTSpecialName && _name.Equals(IsStatic ? ".cctor" : ".ctor") && ReturnsVoid && Arity == 0)
				{
					if (!IsStatic)
					{
						return MethodKind.Constructor;
					}
					if (Parameters.Length == 0)
					{
						return MethodKind.StaticConstructor;
					}
				}
				return MethodKind.Ordinary;
			}
			if (!HasRuntimeSpecialName && DeclaredAccessibility == Accessibility.Public)
			{
				if (IsStatic)
				{
					switch (_name)
					{
					case "op_CheckedAddition":
					case "op_CheckedDivision":
					case "op_LessThanOrEqual":
					case "op_CheckedMultiply":
					case "op_Multiply":
					case "op_Equality":
					case "op_Addition":
					case "op_LessThan":
					case "op_Division":
					case "op_RightShift":
					case "op_BitwiseAnd":
					case "op_Inequality":
					case "op_LeftShift":
					case "op_BitwiseOr":
					case "op_ExclusiveOr":
					case "op_GreaterThan":
					case "op_Subtraction":
					case "op_CheckedSubtraction":
					case "op_GreaterThanOrEqual":
					case "op_UnsignedRightShift":
					case "op_Modulus":
						if (!IsValidStaticUserDefinedOperatorSignature(2))
						{
							return MethodKind.Ordinary;
						}
						return MethodKind.UserDefinedOperator;
					case "op_LogicalNot":
					case "op_Increment":
					case "op_UnaryPlus":
					case "op_Decrement":
					case "op_CheckedDecrement":
					case "op_CheckedIncrement":
					case "op_False":
					case "op_OnesComplement":
					case "op_True":
					case "op_CheckedUnaryNegation":
					case "op_UnaryNegation":
						if (!IsValidStaticUserDefinedOperatorSignature(1))
						{
							return MethodKind.Ordinary;
						}
						return MethodKind.UserDefinedOperator;
					case "op_CheckedExplicit":
					case "op_Explicit":
					case "op_Implicit":
						if (!IsValidStaticUserDefinedOperatorSignature(1))
						{
							return MethodKind.Ordinary;
						}
						return MethodKind.Conversion;
					}
				}
				else
				{
					switch (_name)
					{
					case "op_CheckedDecrementAssignment":
					case "op_CheckedIncrementAssignment":
					case "op_IncrementAssignment":
					case "op_DecrementAssignment":
						if (!IsValidInstanceUserDefinedOperatorSignature(0))
						{
							return MethodKind.Ordinary;
						}
						return MethodKind.UserDefinedOperator;
					case "op_LeftShiftAssignment":
					case "op_BitwiseOrAssignment":
					case "op_AdditionAssignment":
					case "op_DivisionAssignment":
					case "op_ExclusiveOrAssignment":
					case "op_SubtractionAssignment":
					case "op_BitwiseAndAssignment":
					case "op_RightShiftAssignment":
					case "op_CheckedSubtractionAssignment":
					case "op_UnsignedRightShiftAssignment":
					case "op_CheckedAdditionAssignment":
					case "op_CheckedDivisionAssignment":
					case "op_MultiplicationAssignment":
					case "op_ModulusAssignment":
					case "op_CheckedMultiplicationAssignment":
						if (!IsValidInstanceUserDefinedOperatorSignature(1))
						{
							return MethodKind.Ordinary;
						}
						return MethodKind.UserDefinedOperator;
					}
				}
				return MethodKind.Ordinary;
			}
		}
		if (!IsStatic)
		{
			string name = _name;
			if (!(name == "Finalize"))
			{
				if (name == "Invoke" && _containingType.TypeKind == TypeKind.Delegate)
				{
					return MethodKind.DelegateInvoke;
				}
			}
			else if ((ContainingType.TypeKind == TypeKind.Class && this.IsRuntimeFinalizer(skipFirstMethodKindCheck: true)) || IsExplicitFinalizerOverride)
			{
				return MethodKind.Destructor;
			}
		}
		if (!SyntaxFacts.IsValidIdentifier(Name) && !ExplicitInterfaceImplementations.IsEmpty)
		{
			return MethodKind.ExplicitInterfaceImplementation;
		}
		return MethodKind.Ordinary;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return PEDocumentationCommentUtils.GetDocumentationComment(this, _containingType.ContainingPEModule, preferredCulture, cancellationToken, ref AccessUncommonFields()._lazyDocComment);
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		UseSiteInfo<AssemblySymbol> result;
		DiagnosticInfo result2;
		if (!_packedFlags.IsUseSiteDiagnosticPopulated)
		{
			result = new UseSiteInfo<AssemblySymbol>(base.PrimaryDependency);
			CalculateUseSiteDiagnostic(ref result);
			result2 = result.DiagnosticInfo;
			MergeUseSiteDiagnostics(ref result2, DeriveCompilerFeatureRequiredDiagnostic());
			EnsureTypeParametersAreLoaded(ref result2);
			if (_containingType.IsExtension)
			{
				if ((object)TryGetCorrespondingExtensionImplementationMethod() != null)
				{
					DiagnosticInfo? diagnosticInfo = _containingType.GetUseSiteInfo().DiagnosticInfo;
					if (diagnosticInfo == null || diagnosticInfo.DefaultSeverity != DiagnosticSeverity.Error)
					{
						goto IL_0090;
					}
				}
				result2 = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this);
			}
			goto IL_0090;
		}
		return GetCachedUseSiteInfo();
		IL_0090:
		if (result2 == null && _containingType.ContainingPEModule.RefSafetyRulesVersion == PEModuleSymbol.RefSafetyRulesAttributeVersion.UnrecognizedAttribute)
		{
			result2 = new CSDiagnosticInfo(ErrorCode.ERR_UnrecognizedRefSafetyRulesAttributeVersion, this);
		}
		if (result2 == null && GetUnmanagedCallersOnlyAttributeData(forceComplete: true) != null && CheckAndReportValidUnmanagedCallersOnlyTarget(null, null))
		{
			result2 = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this);
		}
		if (result2 == null && this.ShouldCheckRequiredMembers() && ContainingType.HasRequiredMembersError)
		{
			result2 = new CSDiagnosticInfo(ErrorCode.ERR_RequiredMembersInvalid, ContainingType);
		}
		return InitializeUseSiteDiagnostic(result.AdjustDiagnosticInfo(result2));
	}

	private DiagnosticInfo DeriveCompilerFeatureRequiredDiagnostic()
	{
		PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
		MetadataDecoder decoder = new MetadataDecoder(containingPEModule, this);
		DiagnosticInfo diagnosticInfo = PEUtilities.DeriveCompilerFeatureRequiredAttributeDiagnostic(this, containingPEModule, Handle, (MethodKind == MethodKind.Constructor) ? CompilerFeatureRequiredFeatures.RequiredMembers : (SourceMethodSymbol.IsInstanceIncrementDecrementOrCompoundAssignmentOperator(this) ? CompilerFeatureRequiredFeatures.UserDefinedCompoundAssignmentOperators : CompilerFeatureRequiredFeatures.None), decoder);
		if (diagnosticInfo != null)
		{
			return diagnosticInfo;
		}
		diagnosticInfo = Signature.ReturnParam.DeriveCompilerFeatureRequiredDiagnostic(decoder);
		if (diagnosticInfo != null)
		{
			return diagnosticInfo;
		}
		foreach (PEParameterSymbol parameter in Parameters)
		{
			diagnosticInfo = parameter.DeriveCompilerFeatureRequiredDiagnostic(decoder);
			if (diagnosticInfo != null)
			{
				return diagnosticInfo;
			}
		}
		foreach (PETypeParameterSymbol typeParameter in TypeParameters)
		{
			diagnosticInfo = typeParameter.DeriveCompilerFeatureRequiredDiagnostic(decoder);
			if (diagnosticInfo != null)
			{
				return diagnosticInfo;
			}
		}
		return _containingType.GetCompilerFeatureRequiredDiagnostic();
	}

	private UseSiteInfo<AssemblySymbol> GetCachedUseSiteInfo()
	{
		return (_uncommonFields?._lazyCachedUseSiteInfo ?? default(CachedUseSiteInfo<AssemblySymbol>)).ToUseSiteInfo(base.PrimaryDependency);
	}

	private UseSiteInfo<AssemblySymbol> InitializeUseSiteDiagnostic(UseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (_packedFlags.IsUseSiteDiagnosticPopulated)
		{
			return GetCachedUseSiteInfo();
		}
		if (useSiteInfo.DiagnosticInfo != null || !useSiteInfo.SecondaryDependencies.IsNullOrEmpty())
		{
			useSiteInfo = AccessUncommonFields()._lazyCachedUseSiteInfo.InterlockedInitializeFromDefault(base.PrimaryDependency, useSiteInfo);
		}
		_packedFlags.SetIsUseSiteDiagnosticPopulated();
		return useSiteInfo;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		if (!_packedFlags.IsConditionalPopulated)
		{
			ImmutableArray<string> immutableArray = _containingType.ContainingPEModule.Module.GetConditionalAttributeValues(_handle);
			if (!immutableArray.IsEmpty)
			{
				immutableArray = InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyConditionalAttributeSymbols, immutableArray);
			}
			_packedFlags.SetIsConditionalAttributePopulated();
			return immutableArray;
		}
		UncommonFields uncommonFields = _uncommonFields;
		if (uncommonFields == null)
		{
			return ImmutableArray<string>.Empty;
		}
		ImmutableArray<string> lazyConditionalAttributeSymbols = uncommonFields._lazyConditionalAttributeSymbols;
		if (!lazyConditionalAttributeSymbols.IsDefault)
		{
			return lazyConditionalAttributeSymbols;
		}
		return InterlockedOperations.Initialize(ref uncommonFields._lazyConditionalAttributeSymbols, ImmutableArray<string>.Empty);
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 1636);
	}

	internal override UnmanagedCallersOnlyAttributeData? GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		if (!_packedFlags.IsUnmanagedCallersOnlyAttributePopulated)
		{
			PEModuleSymbol pEModuleSymbol = (PEModuleSymbol)ContainingModule;
			UnmanagedCallersOnlyAttributeData initializedValue = pEModuleSymbol.Module.TryGetUnmanagedCallersOnlyAttribute(_handle, new MetadataDecoder(pEModuleSymbol), (string name, TypedConstant value, bool isField) => MethodSymbol.TryDecodeUnmanagedCallersOnlyCallConvsField(name, value, isField, null, null));
			UnmanagedCallersOnlyAttributeData result = InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyUnmanagedCallersOnlyAttributeData, initializedValue, UnmanagedCallersOnlyAttributeData.Uninitialized);
			_packedFlags.SetIsUnmanagedCallersOnlyAttributePopulated();
			return result;
		}
		return _uncommonFields?._lazyUnmanagedCallersOnlyAttributeData;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 1727);
	}

	internal override void AddSynthesizedReturnTypeAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 1732);
	}

	internal sealed override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEMethodSymbol.cs", 1749);
	}

	internal override bool HasAsyncMethodBuilderAttribute(out TypeSymbol builderArgument)
	{
		builderArgument = _containingType.ContainingPEModule.TryDecodeAttributeWithTypeArgument(Handle, AttributeDescription.AsyncMethodBuilderAttribute);
		return (object)builderArgument != null;
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		if (!_packedFlags.IsOverloadResolutionPriorityPopulated)
		{
			if (_containingType.ContainingPEModule.Module.TryGetOverloadResolutionPriorityValue(_handle, out var decodedPriority) && decodedPriority != 0)
			{
				Interlocked.CompareExchange(ref AccessUncommonFields()._lazyOverloadResolutionPriority, decodedPriority, 0);
			}
			_packedFlags.SetIsOverloadResolutionPriorityPopulated();
		}
		return _uncommonFields?._lazyOverloadResolutionPriority ?? 0;
	}
}
