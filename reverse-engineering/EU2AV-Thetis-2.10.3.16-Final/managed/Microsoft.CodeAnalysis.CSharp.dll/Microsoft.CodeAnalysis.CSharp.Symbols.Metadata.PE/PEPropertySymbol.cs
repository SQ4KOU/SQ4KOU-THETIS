using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
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

internal class PEPropertySymbol : PropertySymbol
{
	private struct PackedFlags(bool isSpecialName, bool isRuntimeSpecialName, bool callMethodsDirectly)
	{
		private const int IsSpecialNameFlag = 1;

		private const int IsRuntimeSpecialNameFlag = 2;

		private const int CallMethodsDirectlyFlag = 4;

		private const int HasRequiredMemberAttribute = 16;

		private const int RequiredMemberCompletionBit = 32;

		private const int HasUnscopedRefAttribute = 64;

		private const int UnscopedRefCompletionBit = 128;

		private const int IsUseSiteDiagnosticPopulatedBit = 256;

		private const int IsObsoleteAttributePopulatedBit = 512;

		private const int IsCustomAttributesPopulatedBit = 1024;

		private const int IsOverloadResolutionPriorityPopulatedBit = 2048;

		private int _bits = (int)((isSpecialName ? 1u : 0u) | (uint)(isRuntimeSpecialName ? 2 : 0)) | (callMethodsDirectly ? 4 : 0);

		public readonly bool IsSpecialName => (_bits & 1) != 0;

		public readonly bool IsRuntimeSpecialName => (_bits & 2) != 0;

		public readonly bool CallMethodsDirectly => (_bits & 4) != 0;

		public bool IsUseSiteDiagnosticPopulated => (Volatile.Read(in _bits) & 0x100) != 0;

		public bool IsObsoleteAttributePopulated => (Volatile.Read(in _bits) & 0x200) != 0;

		public bool IsCustomAttributesPopulated => (Volatile.Read(in _bits) & 0x400) != 0;

		public bool IsOverloadResolutionPriorityPopulated => (Volatile.Read(in _bits) & 0x800) != 0;

		public void SetHasRequiredMemberAttribute(bool isRequired)
		{
			int toSet = (isRequired ? 16 : 0) | 0x20;
			ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public readonly bool TryGetHasRequiredMemberAttribute(out bool hasRequiredMemberAttribute)
		{
			if ((_bits & 0x20) != 0)
			{
				hasRequiredMemberAttribute = (_bits & 0x10) != 0;
				return true;
			}
			hasRequiredMemberAttribute = false;
			return false;
		}

		public void SetHasUnscopedRefAttribute(bool unscopedRef)
		{
			int toSet = (unscopedRef ? 64 : 0) | 0x80;
			ThreadSafeFlagOperations.Set(ref _bits, toSet);
		}

		public readonly bool TryGetHasUnscopedRefAttribute(out bool hasUnscopedRefAttribute)
		{
			if ((_bits & 0x80) != 0)
			{
				hasUnscopedRefAttribute = (_bits & 0x40) != 0;
				return true;
			}
			hasUnscopedRefAttribute = false;
			return false;
		}

		public void SetUseSiteDiagnosticPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 256);
		}

		public void SetObsoleteAttributePopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 512);
		}

		public void SetCustomAttributesPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 1024);
		}

		public void SetOverloadResolutionPriorityPopulated()
		{
			ThreadSafeFlagOperations.Set(ref _bits, 2048);
		}
	}

	private sealed class UncommonFields
	{
		public ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

		public Tuple<CultureInfo, string> _lazyDocComment;

		public CachedUseSiteInfo<AssemblySymbol> _lazyCachedUseSiteInfo = CachedUseSiteInfo<AssemblySymbol>.Uninitialized;

		public ObsoleteAttributeData _lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

		public int _lazyOverloadResolutionPriority;
	}

	private sealed class PEPropertySymbolWithCustomModifiers : PEPropertySymbol
	{
		private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

		public PEPropertySymbolWithCustomModifiers(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, PropertyDefinitionHandle handle, PEMethodSymbol getMethod, PEMethodSymbol setMethod, ParamInfo<TypeSymbol>[] propertyParams, MetadataDecoder metadataDecoder)
			: base(moduleSymbol, containingType, handle, getMethod, setMethod, propertyParams, metadataDecoder)
		{
			ParamInfo<TypeSymbol> paramInfo = propertyParams[0];
			_refCustomModifiers = CSharpCustomModifier.Convert(paramInfo.RefCustomModifiers);
		}
	}

	private readonly string _name;

	private readonly PENamedTypeSymbol _containingType;

	private readonly PropertyDefinitionHandle _handle;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private readonly RefKind _refKind;

	private readonly TypeWithAnnotations _propertyTypeWithAnnotations;

	private readonly PEMethodSymbol _getMethod;

	private readonly PEMethodSymbol _setMethod;

	private UncommonFields? _uncommonFields;

	private const int UnsetAccessibility = -1;

	private int _declaredAccessibility = -1;

	private PackedFlags _flags;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override string Name
	{
		get
		{
			if (!IsIndexer)
			{
				return _name;
			}
			return "this[]";
		}
	}

	internal override bool HasSpecialName => _flags.IsSpecialName;

	public override string MetadataName => _name;

	public override int MetadataToken => MetadataTokens.GetToken(_handle);

	internal PropertyDefinitionHandle Handle => _handle;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (_declaredAccessibility == -1)
			{
				Accessibility declaredAccessibilityFromAccessors;
				if (IsOverride)
				{
					bool flag = false;
					Accessibility accessibility = Accessibility.NotApplicable;
					Accessibility accessibility2 = Accessibility.NotApplicable;
					PropertySymbol propertySymbol = this;
					while (true)
					{
						if (accessibility == Accessibility.NotApplicable)
						{
							MethodSymbol getMethod = propertySymbol.GetMethod;
							if ((object)getMethod != null)
							{
								Accessibility declaredAccessibility = getMethod.DeclaredAccessibility;
								accessibility = (((declaredAccessibility == Accessibility.ProtectedOrInternal) & flag) ? Accessibility.Protected : declaredAccessibility);
							}
						}
						if (accessibility2 == Accessibility.NotApplicable)
						{
							MethodSymbol setMethod = propertySymbol.SetMethod;
							if ((object)setMethod != null)
							{
								Accessibility declaredAccessibility2 = setMethod.DeclaredAccessibility;
								accessibility2 = (((declaredAccessibility2 == Accessibility.ProtectedOrInternal) & flag) ? Accessibility.Protected : declaredAccessibility2);
							}
						}
						if (accessibility != Accessibility.NotApplicable && accessibility2 != Accessibility.NotApplicable)
						{
							break;
						}
						PropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;
						if ((object)overriddenProperty == null)
						{
							break;
						}
						if (!flag && !propertySymbol.ContainingAssembly.HasInternalAccessTo(overriddenProperty.ContainingAssembly))
						{
							flag = true;
						}
						propertySymbol = overriddenProperty;
					}
					declaredAccessibilityFromAccessors = PEPropertyOrEventHelpers.GetDeclaredAccessibilityFromAccessors(accessibility, accessibility2);
				}
				else
				{
					declaredAccessibilityFromAccessors = PEPropertyOrEventHelpers.GetDeclaredAccessibilityFromAccessors(GetMethod, SetMethod);
				}
				Interlocked.CompareExchange(ref _declaredAccessibility, (int)declaredAccessibilityFromAccessors, -1);
			}
			return (Accessibility)_declaredAccessibility;
		}
	}

	public override bool IsExtern
	{
		get
		{
			if ((object)_getMethod == null || !_getMethod.IsExtern)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsExtern;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsAbstract
	{
		get
		{
			if ((object)_getMethod == null || !_getMethod.IsAbstract)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsAbstract;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsSealed
	{
		get
		{
			if ((object)_getMethod == null || _getMethod.IsSealed)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsSealed;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsVirtual
	{
		get
		{
			if (!IsOverride && !IsAbstract)
			{
				if ((object)_getMethod == null || !_getMethod.IsVirtual)
				{
					if ((object)_setMethod != null)
					{
						return _setMethod.IsVirtual;
					}
					return false;
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
			if ((object)_getMethod == null || !_getMethod.IsOverride)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsOverride;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsStatic
	{
		get
		{
			if ((object)_getMethod == null || _getMethod.IsStatic)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsStatic;
				}
				return true;
			}
			return false;
		}
	}

	internal override bool IsRequired
	{
		get
		{
			if (!_flags.TryGetHasRequiredMemberAttribute(out var hasRequiredMemberAttribute))
			{
				hasRequiredMemberAttribute = ((PEModuleSymbol)ContainingModule).Module.HasAttribute(_handle, AttributeDescription.RequiredMemberAttribute);
				_flags.SetHasRequiredMemberAttribute(hasRequiredMemberAttribute);
			}
			return hasRequiredMemberAttribute;
		}
	}

	internal sealed override bool HasUnscopedRefAttribute
	{
		get
		{
			if (!_flags.TryGetHasUnscopedRefAttribute(out var hasUnscopedRefAttribute))
			{
				hasUnscopedRefAttribute = ((PEModuleSymbol)ContainingModule).Module.HasUnscopedRefAttribute(_handle);
				_flags.SetHasUnscopedRefAttribute(hasUnscopedRefAttribute);
			}
			return hasUnscopedRefAttribute;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override bool IsIndexer
	{
		get
		{
			if (base.ParameterCount > 0)
			{
				string defaultMemberName = _containingType.DefaultMemberName;
				if (!(_name == defaultMemberName) && ((object)GetMethod == null || !(GetMethod.Name == defaultMemberName)))
				{
					if ((object)SetMethod != null)
					{
						return SetMethod.Name == defaultMemberName;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsIndexedProperty
	{
		get
		{
			if (base.ParameterCount > 0)
			{
				return _containingType.IsComImport;
			}
			return false;
		}
	}

	public override RefKind RefKind => _refKind;

	public override TypeWithAnnotations TypeWithAnnotations => _propertyTypeWithAnnotations;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override MethodSymbol GetMethod => _getMethod;

	public override MethodSymbol SetMethod => _setMethod;

	internal override CallingConvention CallingConvention => (CallingConvention)new MetadataDecoder(_containingType.ContainingPEModule, _containingType).GetSignatureHeaderForProperty(_handle).RawValue;

	public override ImmutableArray<Location> Locations => _containingType.ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations
	{
		get
		{
			if (((object)_getMethod == null || _getMethod.ExplicitInterfaceImplementations.Length == 0) && ((object)_setMethod == null || _setMethod.ExplicitInterfaceImplementations.Length == 0))
			{
				return ImmutableArray<PropertySymbol>.Empty;
			}
			ISet<PropertySymbol> propertiesForExplicitlyImplementedAccessor = PEPropertyOrEventHelpers.GetPropertiesForExplicitlyImplementedAccessor(_getMethod);
			ISet<PropertySymbol> propertiesForExplicitlyImplementedAccessor2 = PEPropertyOrEventHelpers.GetPropertiesForExplicitlyImplementedAccessor(_setMethod);
			ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance();
			foreach (PropertySymbol item in propertiesForExplicitlyImplementedAccessor)
			{
				if (!item.SetMethod.IsImplementable() || propertiesForExplicitlyImplementedAccessor2.Contains(item))
				{
					instance.Add(item);
				}
			}
			foreach (PropertySymbol item2 in propertiesForExplicitlyImplementedAccessor2)
			{
				if (!item2.GetMethod.IsImplementable())
				{
					instance.Add(item2);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	internal override bool MustCallMethodsDirectly => _flags.CallMethodsDirectly;

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!_flags.IsObsoleteAttributePopulated)
			{
				ObsoleteAttributeData obsoleteAttributeData = ObsoleteAttributeHelpers.GetObsoleteDataFromMetadata(_handle, (PEModuleSymbol)ContainingModule, ignoreByRefLikeMarker: false, ignoreRequiredMemberMarker: false);
				if (obsoleteAttributeData != null)
				{
					obsoleteAttributeData = InterlockedOperations.Initialize(ref AccessUncommonFields()._lazyObsoleteAttributeData, obsoleteAttributeData, ObsoleteAttributeData.Uninitialized);
				}
				_flags.SetObsoleteAttributePopulated();
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

	internal override bool HasRuntimeSpecialName => _flags.IsRuntimeSpecialName;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal static PEPropertySymbol Create(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, PropertyDefinitionHandle handle, PEMethodSymbol getMethod, PEMethodSymbol setMethod)
	{
		MetadataDecoder metadataDecoder = new MetadataDecoder(moduleSymbol, containingType);
		ParamInfo<TypeSymbol>[] signatureForProperty = metadataDecoder.GetSignatureForProperty(handle, out var _, out var BadImageFormatException);
		ParamInfo<TypeSymbol> paramInfo = signatureForProperty[0];
		PEPropertySymbol pEPropertySymbol = ((paramInfo.CustomModifiers.IsDefaultOrEmpty && paramInfo.RefCustomModifiers.IsDefaultOrEmpty) ? new PEPropertySymbol(moduleSymbol, containingType, handle, getMethod, setMethod, signatureForProperty, metadataDecoder) : new PEPropertySymbolWithCustomModifiers(moduleSymbol, containingType, handle, getMethod, setMethod, signatureForProperty, metadataDecoder));
		bool flag = pEPropertySymbol.RefKind == RefKind.In != pEPropertySymbol.RefCustomModifiers.HasInAttributeModifier();
		if ((BadImageFormatException != null) | flag)
		{
			pEPropertySymbol.AccessUncommonFields()._lazyCachedUseSiteInfo.Initialize(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, pEPropertySymbol));
			pEPropertySymbol._flags.SetUseSiteDiagnosticPopulated();
		}
		return pEPropertySymbol;
	}

	private PEPropertySymbol(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, PropertyDefinitionHandle handle, PEMethodSymbol getMethod, PEMethodSymbol setMethod, ParamInfo<TypeSymbol>[] propertyParams, MetadataDecoder metadataDecoder)
	{
		_containingType = containingType;
		PEModule module = moduleSymbol.Module;
		PropertyAttributes flags = PropertyAttributes.None;
		BadImageFormatException ex = null;
		try
		{
			module.GetPropertyDefPropsOrThrow(handle, out _name, out flags);
		}
		catch (BadImageFormatException ex2)
		{
			ex = ex2;
			if (_name == null)
			{
				_name = string.Empty;
			}
		}
		_getMethod = getMethod;
		_setMethod = setMethod;
		_handle = handle;
		BadImageFormatException metadataException = null;
		ParamInfo<TypeSymbol>[] array = (((object)getMethod == null) ? null : metadataDecoder.GetSignatureForMethod(getMethod.Handle, out var signatureHeader, out metadataException));
		BadImageFormatException metadataException2 = null;
		ParamInfo<TypeSymbol>[] array2 = (((object)setMethod == null) ? null : metadataDecoder.GetSignatureForMethod(setMethod.Handle, out signatureHeader, out metadataException2));
		_parameters = ((array2 == null) ? GetParameters(moduleSymbol, this, getMethod, propertyParams, array, out var anyParameterIsBad) : GetParameters(moduleSymbol, this, setMethod, propertyParams, array2, out anyParameterIsBad));
		if ((metadataException != null || metadataException2 != null || ex != null) | anyParameterIsBad)
		{
			AccessUncommonFields()._lazyCachedUseSiteInfo.Initialize(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this));
			_flags.SetUseSiteDiagnosticPopulated();
		}
		ParamInfo<TypeSymbol> paramInfo = propertyParams[0];
		ImmutableArray<CustomModifier> customModifiers = CSharpCustomModifier.Convert(paramInfo.CustomModifiers);
		if (paramInfo.IsByRef)
		{
			if (moduleSymbol.Module.HasIsReadOnlyAttribute(handle))
			{
				_refKind = RefKind.In;
			}
			else
			{
				_refKind = RefKind.Ref;
			}
		}
		else
		{
			_refKind = RefKind.None;
		}
		TypeWithAnnotations metadataType = TypeWithAnnotations.Create(NativeIntegerTypeDecoder.TransformType(DynamicTypeDecoder.TransformType(paramInfo.Type, customModifiers.Length, handle, moduleSymbol, _refKind), handle, moduleSymbol, _containingType).AsDynamicIfNoPia(_containingType), NullableAnnotation.Oblivious, customModifiers);
		metadataType = NullableTypeDecoder.TransformType(metadataType, handle, moduleSymbol, _containingType, _containingType);
		metadataType = TupleTypeDecoder.DecodeTupleTypesIfApplicable(metadataType, handle, moduleSymbol);
		_propertyTypeWithAnnotations = metadataType;
		bool flag = !DoSignaturesMatch(module, metadataDecoder, propertyParams, _getMethod, array, _setMethod, array2) || MustCallMethodsDirectlyCore() || anyUnexpectedRequiredModifiers(propertyParams);
		if (!flag)
		{
			if ((object)_getMethod != null)
			{
				_getMethod.SetAssociatedProperty(this, MethodKind.PropertyGet);
			}
			if ((object)_setMethod != null)
			{
				_setMethod.SetAssociatedProperty(this, MethodKind.PropertySet);
			}
		}
		_flags = new PackedFlags((flags & PropertyAttributes.SpecialName) != 0, (flags & PropertyAttributes.RTSpecialName) != 0, flag);
		static bool anyUnexpectedRequiredModifiers(ParamInfo<TypeSymbol>[] source)
		{
			return source.Any((ParamInfo<TypeSymbol> p) => (!p.RefCustomModifiers.IsDefaultOrEmpty && p.RefCustomModifiers.Any((ModifierInfo<TypeSymbol> m) => !m.IsOptional && !m.Modifier.IsWellKnownTypeInAttribute())) || p.CustomModifiers.AnyRequired());
		}
	}

	private UncommonFields AccessUncommonFields()
	{
		return _uncommonFields ?? InterlockedOperations.Initialize(ref _uncommonFields, createUncommonFields());
		UncommonFields createUncommonFields()
		{
			UncommonFields uncommonFields = new UncommonFields();
			if (!_flags.IsObsoleteAttributePopulated)
			{
				uncommonFields._lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;
			}
			if (!_flags.IsUseSiteDiagnosticPopulated)
			{
				uncommonFields._lazyCachedUseSiteInfo = CachedUseSiteInfo<AssemblySymbol>.Uninitialized;
			}
			if (_flags.IsCustomAttributesPopulated)
			{
				uncommonFields._lazyCustomAttributes = ImmutableArray<CSharpAttributeData>.Empty;
			}
			return uncommonFields;
		}
	}

	private bool MustCallMethodsDirectlyCore()
	{
		if (RefKind != RefKind.None && _setMethod != null)
		{
			return true;
		}
		if (base.ParameterCount == 0)
		{
			return false;
		}
		if (IsIndexedProperty)
		{
			return IsStatic;
		}
		if (IsIndexer)
		{
			return this.HasRefOrOutParameter();
		}
		return true;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (!_flags.IsCustomAttributesPopulated)
		{
			ImmutableArray<CSharpAttributeData> customAttributesForToken = ((PEModuleSymbol)ContainingModule).GetCustomAttributesForToken(_handle, out var _, (RefKind == RefKind.In) ? AttributeDescription.IsReadOnlyAttribute : default(AttributeDescription), out var filteredOutAttribute2, AttributeDescription.RequiredMemberAttribute, out var _, this.IsExtensionBlockMember() ? AttributeDescription.ExtensionMarkerAttribute : default(AttributeDescription), out var _, default(AttributeDescription), out var _, default(AttributeDescription), out var _, default(AttributeDescription));
			if (!customAttributesForToken.IsEmpty)
			{
				ImmutableInterlocked.InterlockedInitialize(ref AccessUncommonFields()._lazyCustomAttributes, customAttributesForToken);
			}
			_flags.SetCustomAttributesPopulated();
			_flags.SetHasRequiredMemberAttribute(!filteredOutAttribute2.IsNil);
		}
		UncommonFields uncommonFields = _uncommonFields;
		if (uncommonFields == null)
		{
			return ImmutableArray<CSharpAttributeData>.Empty;
		}
		ImmutableArray<CSharpAttributeData> immutableArray = uncommonFields._lazyCustomAttributes;
		if (immutableArray.IsDefault)
		{
			immutableArray = ImmutableArray<CSharpAttributeData>.Empty;
			ImmutableInterlocked.InterlockedInitialize(ref uncommonFields._lazyCustomAttributes, immutableArray);
		}
		return immutableArray;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return GetAttributes();
	}

	private static bool DoSignaturesMatch(PEModule module, MetadataDecoder metadataDecoder, ParamInfo<TypeSymbol>[] propertyParams, PEMethodSymbol getMethod, ParamInfo<TypeSymbol>[] getMethodParams, PEMethodSymbol setMethod, ParamInfo<TypeSymbol>[] setMethodParams)
	{
		bool flag = getMethodParams != null;
		bool flag2 = setMethodParams != null;
		if (flag && !metadataDecoder.DoPropertySignaturesMatch(propertyParams, getMethodParams, comparingToSetter: false, compareParamByRef: true, compareReturnType: true))
		{
			return false;
		}
		if (flag2 && !metadataDecoder.DoPropertySignaturesMatch(propertyParams, setMethodParams, comparingToSetter: true, compareParamByRef: true, compareReturnType: true))
		{
			return false;
		}
		if (flag & flag2)
		{
			int num = propertyParams.Length - 1;
			ParameterHandle handle = getMethodParams[num].Handle;
			ParameterHandle handle2 = setMethodParams[num].Handle;
			bool num2 = !handle.IsNil && module.HasParamArrayAttribute(handle);
			bool flag3 = !handle2.IsNil && module.HasParamArrayAttribute(handle2);
			bool flag4 = !handle.IsNil && module.HasParamCollectionAttribute(handle);
			bool flag5 = !handle2.IsNil && module.HasParamCollectionAttribute(handle2);
			if (num2 != flag3 || flag4 != flag5)
			{
				return false;
			}
			if (getMethod.IsExtern != setMethod.IsExtern || getMethod.IsSealed != setMethod.IsSealed || getMethod.IsOverride != setMethod.IsOverride || getMethod.IsStatic != setMethod.IsStatic)
			{
				return false;
			}
		}
		return true;
	}

	private static ImmutableArray<ParameterSymbol> GetParameters(PEModuleSymbol moduleSymbol, PEPropertySymbol property, PEMethodSymbol accessor, ParamInfo<TypeSymbol>[] propertyParams, ParamInfo<TypeSymbol>[] accessorParams, out bool anyParameterIsBad)
	{
		anyParameterIsBad = false;
		if (propertyParams.Length < 2)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		int num = accessorParams.Length;
		ParameterSymbol[] array = new ParameterSymbol[propertyParams.Length - 1];
		for (int i = 1; i < propertyParams.Length; i++)
		{
			ParamInfo<TypeSymbol> parameterInfo = propertyParams[i];
			ParameterHandle handle;
			Symbol nullableContext;
			if (i < num)
			{
				handle = accessorParams[i].Handle;
				nullableContext = accessor;
			}
			else
			{
				handle = parameterInfo.Handle;
				nullableContext = property;
			}
			int num2 = i - 1;
			array[num2] = PEParameterSymbol.Create(moduleSymbol, property, accessor.IsMetadataVirtual(), num2, handle, parameterInfo, nullableContext, out var isBad);
			if (isBad)
			{
				anyParameterIsBad = true;
			}
		}
		return array.AsImmutableOrNull();
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return PEDocumentationCommentUtils.GetDocumentationComment(this, _containingType.ContainingPEModule, preferredCulture, cancellationToken, ref AccessUncommonFields()._lazyDocComment);
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		AssemblySymbol primaryDependency = base.PrimaryDependency;
		if (!_flags.IsUseSiteDiagnosticPopulated)
		{
			UseSiteInfo<AssemblySymbol> result = new UseSiteInfo<AssemblySymbol>(primaryDependency);
			CalculateUseSiteDiagnostic(ref result);
			DiagnosticInfo result2 = deriveCompilerFeatureRequiredUseSiteInfo();
			MergeUseSiteDiagnostics(ref result2, result.DiagnosticInfo);
			result = result.AdjustDiagnosticInfo(result2);
			if (result.DiagnosticInfo != null || !result.SecondaryDependencies.IsNullOrEmpty())
			{
				AccessUncommonFields()._lazyCachedUseSiteInfo.InterlockedInitializeFromSentinel(base.PrimaryDependency, result);
			}
			_flags.SetUseSiteDiagnosticPopulated();
		}
		UncommonFields uncommonFields = _uncommonFields;
		if (uncommonFields == null)
		{
			return new UseSiteInfo<AssemblySymbol>(primaryDependency);
		}
		CachedUseSiteInfo<AssemblySymbol> lazyCachedUseSiteInfo = uncommonFields._lazyCachedUseSiteInfo;
		if (!lazyCachedUseSiteInfo.IsInitialized)
		{
			uncommonFields._lazyCachedUseSiteInfo.InterlockedInitializeFromSentinel(primaryDependency, new UseSiteInfo<AssemblySymbol>(primaryDependency));
			lazyCachedUseSiteInfo = uncommonFields._lazyCachedUseSiteInfo;
		}
		return lazyCachedUseSiteInfo.ToUseSiteInfo(primaryDependency);
		DiagnosticInfo deriveCompilerFeatureRequiredUseSiteInfo()
		{
			PENamedTypeSymbol pENamedTypeSymbol = (PENamedTypeSymbol)ContainingType;
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			MetadataDecoder decoder = new MetadataDecoder(containingPEModule, pENamedTypeSymbol);
			DiagnosticInfo diagnosticInfo = PEUtilities.DeriveCompilerFeatureRequiredAttributeDiagnostic(this, containingPEModule, Handle, CompilerFeatureRequiredFeatures.None, decoder);
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
			return pENamedTypeSymbol.GetCompilerFeatureRequiredDiagnostic();
		}
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		if (!_flags.IsOverloadResolutionPriorityPopulated)
		{
			if (_containingType.ContainingPEModule.Module.TryGetOverloadResolutionPriorityValue(_handle, out var decodedPriority) && decodedPriority != 0)
			{
				Interlocked.CompareExchange(ref AccessUncommonFields()._lazyOverloadResolutionPriority, decodedPriority, 0);
			}
			_flags.SetOverloadResolutionPriorityPopulated();
		}
		return _uncommonFields?._lazyOverloadResolutionPriority ?? 0;
	}
}
