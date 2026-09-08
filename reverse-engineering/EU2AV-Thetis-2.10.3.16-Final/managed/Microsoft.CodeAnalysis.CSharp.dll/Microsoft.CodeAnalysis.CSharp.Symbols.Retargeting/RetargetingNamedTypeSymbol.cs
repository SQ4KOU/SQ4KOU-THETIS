using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingNamedTypeSymbol : WrappedNamedTypeSymbol
{
	private sealed class ForwardedExtensionGroupingOrMarkerType : INestedTypeReference, INamedTypeReference, ITypeReference, IReference, INamedEntity, ITypeMemberReference
	{
		private readonly ITypeReference _containingType;

		private readonly INestedTypeReference _underlying;

		bool INestedTypeReference.InheritsEnclosingTypeTypeParameters
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 531);
			}
		}

		ushort INamedTypeReference.GenericParameterCount => _underlying.GenericParameterCount;

		bool INamedTypeReference.MangleName => _underlying.MangleName;

		string? INamedTypeReference.AssociatedFileIdentifier => _underlying.AssociatedFileIdentifier;

		bool ITypeReference.IsEnum
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 539);
			}
		}

		bool ITypeReference.IsValueType
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 541);
			}
		}

		Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 543);
			}
		}

		TypeDefinitionHandle ITypeReference.TypeDef
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 545);
			}
		}

		IGenericMethodParameterReference? ITypeReference.AsGenericMethodParameterReference => null;

		IGenericTypeInstanceReference? ITypeReference.AsGenericTypeInstanceReference => null;

		IGenericTypeParameterReference? ITypeReference.AsGenericTypeParameterReference => null;

		INamespaceTypeReference? ITypeReference.AsNamespaceTypeReference => null;

		INestedTypeReference? ITypeReference.AsNestedTypeReference => this;

		ISpecializedNestedTypeReference? ITypeReference.AsSpecializedNestedTypeReference => null;

		string? INamedEntity.Name => _underlying.Name;

		public ForwardedExtensionGroupingOrMarkerType(ITypeReference containingType, INestedTypeReference underlying)
		{
			_containingType = containingType;
			_underlying = underlying;
		}

		IDefinition? IReference.AsDefinition(EmitContext context)
		{
			return null;
		}

		INamespaceTypeDefinition? ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
		{
			return null;
		}

		INestedTypeDefinition? ITypeReference.AsNestedTypeDefinition(EmitContext context)
		{
			return null;
		}

		ITypeDefinition? ITypeReference.AsTypeDefinition(EmitContext context)
		{
			return null;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 571);
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 576);
		}

		ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
		{
			return _containingType;
		}

		ISymbolInternal? IReference.GetInternalSymbol()
		{
			return null;
		}

		ITypeDefinition? ITypeReference.GetResolvedType(EmitContext context)
		{
			return null;
		}
	}

	private readonly RetargetingModuleSymbol _retargetingModule;

	private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

	private NamedTypeSymbol _lazyBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyInterfaces;

	private NamedTypeSymbol _lazyDeclaredBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyDeclaredInterfaces;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private CachedUseSiteInfo<AssemblySymbol> _lazyCachedUseSiteInfo = CachedUseSiteInfo<AssemblySymbol>.Uninitialized;

	private StrongBox<ParameterSymbol> _lazyExtensionParameter;

	private ImmutableArray<(INestedTypeReference GroupingType, ImmutableArray<INestedTypeReference> MarkerTypes)> _lazyExtensionGroupingAndMarkerTypesForTypeForwarding;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			if (_lazyTypeParameters.IsDefault)
			{
				if (Arity == 0)
				{
					_lazyTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
				}
				else
				{
					ImmutableInterlocked.InterlockedCompareExchange(ref _lazyTypeParameters, RetargetingTranslator.Retarget(_underlyingType.TypeParameters), default(ImmutableArray<TypeParameterSymbol>));
				}
			}
			return _lazyTypeParameters;
		}
	}

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

	internal sealed override ParameterSymbol ExtensionParameter
	{
		get
		{
			if (_lazyExtensionParameter == null)
			{
				ParameterSymbol extensionParameter = _underlyingType.ExtensionParameter;
				RetargetingExtensionReceiverParameterSymbol value = (((object)extensionParameter != null) ? new RetargetingExtensionReceiverParameterSymbol(this, extensionParameter) : null);
				Interlocked.CompareExchange(ref _lazyExtensionParameter, new StrongBox<ParameterSymbol>(value), null);
			}
			return _lazyExtensionParameter.Value;
		}
	}

	public override NamedTypeSymbol ConstructedFrom => this;

	public override NamedTypeSymbol EnumUnderlyingType
	{
		get
		{
			NamedTypeSymbol enumUnderlyingType = _underlyingType.EnumUnderlyingType;
			if ((object)enumUnderlyingType != null)
			{
				return RetargetingTranslator.Retarget(enumUnderlyingType, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
			}
			return null;
		}
	}

	public override IEnumerable<string> MemberNames => _underlyingType.MemberNames;

	internal override bool HasDeclaredRequiredMembers => _underlyingType.HasDeclaredRequiredMembers;

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingType.ContainingSymbol);

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if ((object)_lazyBaseType == ErrorTypeSymbol.UnknownResultType)
			{
				NamedTypeSymbol namedTypeSymbol = GetDeclaredBaseType(null);
				if ((object)namedTypeSymbol == null)
				{
					NamedTypeSymbol baseTypeNoUseSiteDiagnostics = _underlyingType.BaseTypeNoUseSiteDiagnostics;
					if ((object)baseTypeNoUseSiteDiagnostics != null)
					{
						namedTypeSymbol = RetargetingTranslator.Retarget(baseTypeNoUseSiteDiagnostics, RetargetOptions.RetargetPrimitiveTypesByName);
					}
				}
				if ((object)namedTypeSymbol != null && BaseTypeAnalysis.TypeDependsOn(namedTypeSymbol, this))
				{
					return CyclicInheritanceError(namedTypeSymbol);
				}
				Interlocked.CompareExchange(ref _lazyBaseType, namedTypeSymbol, ErrorTypeSymbol.UnknownResultType);
			}
			return _lazyBaseType;
		}
	}

	internal override NamedTypeSymbol ComImportCoClass
	{
		get
		{
			NamedTypeSymbol comImportCoClass = _underlyingType.ComImportCoClass;
			if ((object)comImportCoClass != null)
			{
				return RetargetingTranslator.Retarget(comImportCoClass, RetargetOptions.RetargetPrimitiveTypesByName);
			}
			return null;
		}
	}

	internal override bool IsComImport => _underlyingType.IsComImport;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 423);
		}
	}

	internal override bool IsFileLocal => _underlyingType.IsFileLocal;

	internal override FileIdentifier AssociatedFileIdentifier => _underlyingType.AssociatedFileIdentifier;

	internal sealed override NamedTypeSymbol NativeIntegerUnderlyingType => null;

	internal sealed override bool IsRecord => _underlyingType.IsRecord;

	internal sealed override bool IsRecordStruct => _underlyingType.IsRecordStruct;

	internal override bool HasCompilerLoweringPreserveAttribute => _underlyingType.HasCompilerLoweringPreserveAttribute;

	internal override string? ExtensionGroupingName => _underlyingType.ExtensionGroupingName;

	internal override string? ExtensionMarkerName => _underlyingType.ExtensionMarkerName;

	public RetargetingNamedTypeSymbol(RetargetingModuleSymbol retargetingModule, NamedTypeSymbol underlyingType, TupleExtraData tupleData = null)
		: base(underlyingType, tupleData)
	{
		_retargetingModule = retargetingModule;
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		return new RetargetingNamedTypeSymbol(_retargetingModule, _underlyingType, newData);
	}

	public override MethodSymbol TryGetCorrespondingExtensionImplementationMethod(MethodSymbol method)
	{
		MethodSymbol methodSymbol = _underlyingType.TryGetCorrespondingExtensionImplementationMethod(((RetargetingMethodSymbol)method).UnderlyingMethod);
		if ((object)methodSymbol == null)
		{
			return null;
		}
		return RetargetingTranslator.Retarget(methodSymbol);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetMembers());
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetMembersUnordered());
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetMembers(name));
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		foreach (FieldSymbol item in _underlyingType.GetFieldsToEmit())
		{
			yield return RetargetingTranslator.Retarget(item);
		}
	}

	internal override IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		bool isInterface = _underlyingType.IsInterfaceType();
		foreach (MethodSymbol item in _underlyingType.GetMethodsToEmit())
		{
			int gapSize = (isInterface ? ModuleExtensions.GetVTableGapSize(item.MetadataName) : 0);
			if (gapSize > 0)
			{
				do
				{
					yield return null;
					gapSize--;
				}
				while (gapSize > 0);
			}
			else
			{
				yield return RetargetingTranslator.Retarget(item);
			}
		}
	}

	internal override IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		foreach (PropertySymbol item in _underlyingType.GetPropertiesToEmit())
		{
			yield return RetargetingTranslator.Retarget(item);
		}
	}

	internal override IEnumerable<EventSymbol> GetEventsToEmit()
	{
		foreach (EventSymbol item in _underlyingType.GetEventsToEmit())
		{
			yield return RetargetingTranslator.Retarget(item);
		}
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetEarlyAttributeDecodingMembers());
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetEarlyAttributeDecodingMembers(name));
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembersUnordered());
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembers());
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembers(name));
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetTypeMembers(name, arity));
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingType.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return RetargetingTranslator.RetargetAttributes(_underlyingType.GetCustomAttributesToEmit(moduleBuilder));
	}

	internal override NamedTypeSymbol? LookupMetadataType(ref MetadataTypeName typeName)
	{
		NamedTypeSymbol namedTypeSymbol = _underlyingType.LookupMetadataType(ref typeName);
		if ((object)namedTypeSymbol == null)
		{
			return null;
		}
		return RetargetingTranslator.Retarget(namedTypeSymbol, RetargetOptions.RetargetPrimitiveTypesByName);
	}

	private static ExtendedErrorTypeSymbol CyclicInheritanceError(TypeSymbol declaredBase)
	{
		CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_ImportedCircularBase, declaredBase);
		return new ExtendedErrorTypeSymbol(declaredBase, LookupResultKind.NotReferencable, errorInfo, unreported: true);
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (_lazyInterfaces.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> declaredInterfaces = GetDeclaredInterfaces(basesBeingResolved);
			if (!IsInterface)
			{
				return declaredInterfaces;
			}
			ImmutableArray<NamedTypeSymbol> value = declaredInterfaces.SelectAsArray((NamedTypeSymbol t) => (!BaseTypeAnalysis.TypeDependsOn(t, this)) ? t : CyclicInheritanceError(t));
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyInterfaces, value, default(ImmutableArray<NamedTypeSymbol>));
		}
		return _lazyInterfaces;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return RetargetingTranslator.Retarget(_underlyingType.GetInterfacesToEmit());
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		if ((object)_lazyDeclaredBaseType == ErrorTypeSymbol.UnknownResultType)
		{
			NamedTypeSymbol declaredBaseType = _underlyingType.GetDeclaredBaseType(basesBeingResolved);
			NamedTypeSymbol value = (((object)declaredBaseType != null) ? RetargetingTranslator.Retarget(declaredBaseType, RetargetOptions.RetargetPrimitiveTypesByName) : null);
			Interlocked.CompareExchange(ref _lazyDeclaredBaseType, value, ErrorTypeSymbol.UnknownResultType);
		}
		return _lazyDeclaredBaseType;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (_lazyDeclaredInterfaces.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> declaredInterfaces = _underlyingType.GetDeclaredInterfaces(basesBeingResolved);
			ImmutableArray<NamedTypeSymbol> value = RetargetingTranslator.Retarget(declaredInterfaces);
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyDeclaredInterfaces, value, default(ImmutableArray<NamedTypeSymbol>));
		}
		return _lazyDeclaredInterfaces;
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		if (!_lazyCachedUseSiteInfo.IsInitialized)
		{
			AssemblySymbol primaryDependency = base.PrimaryDependency;
			_lazyCachedUseSiteInfo.Initialize(primaryDependency, new UseSiteInfo<AssemblySymbol>(primaryDependency).AdjustDiagnosticInfo(CalculateUseSiteDiagnostic()));
		}
		return _lazyCachedUseSiteInfo.ToUseSiteInfo(base.PrimaryDependency);
	}

	internal sealed override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingNamedTypeSymbol.cs", 429);
	}

	internal sealed override bool HasPossibleWellKnownCloneMethod()
	{
		return _underlyingType.HasPossibleWellKnownCloneMethod();
	}

	internal override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		foreach (var item3 in _underlyingType.SynthesizedInterfaceMethodImpls())
		{
			MethodSymbol item = item3.Body;
			MethodSymbol item2 = item3.Implemented;
			MethodSymbol methodSymbol = RetargetingTranslator.Retarget(item, MemberSignatureComparer.RetargetedExplicitImplementationComparer);
			MethodSymbol methodSymbol2 = RetargetingTranslator.Retarget(item2, MemberSignatureComparer.RetargetedExplicitImplementationComparer);
			if ((object)methodSymbol != null && (object)methodSymbol2 != null)
			{
				yield return (Body: methodSymbol, Implemented: methodSymbol2);
			}
		}
	}

	internal override bool HasInlineArrayAttribute(out int length)
	{
		return _underlyingType.HasInlineArrayAttribute(out length);
	}

	internal sealed override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		bool result = _underlyingType.HasCollectionBuilderAttribute(out builderType, out methodName);
		if ((object)builderType != null)
		{
			builderType = RetargetingTranslator.Retarget(builderType, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
		}
		return result;
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		if (_underlyingType.HasAsyncMethodBuilderAttribute(out builderArgument))
		{
			builderArgument = RetargetingTranslator.Retarget(builderArgument, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
			return true;
		}
		builderArgument = null;
		return false;
	}

	internal ImmutableArray<(INestedTypeReference GroupingType, ImmutableArray<INestedTypeReference> MarkerTypes)> GetExtensionGroupingAndMarkerTypesForTypeForwarding(EmitContext context)
	{
		if (_lazyExtensionGroupingAndMarkerTypesForTypeForwarding.IsDefault)
		{
			ArrayBuilder<(INestedTypeReference, ImmutableArray<INestedTypeReference>)> instance = ArrayBuilder<(INestedTypeReference, ImmutableArray<INestedTypeReference>)>.GetInstance();
			ArrayBuilder<INestedTypeReference> instance2 = ArrayBuilder<INestedTypeReference>.GetInstance();
			foreach (INestedTypeDefinition groupingType in ((SourceMemberContainerTypeSymbol)_underlyingType).GetExtensionGroupingInfo().GetGroupingTypes())
			{
				ForwardedExtensionGroupingOrMarkerType forwardedExtensionGroupingOrMarkerType = new ForwardedExtensionGroupingOrMarkerType(GetCciAdapter(), groupingType);
				instance2.Clear();
				foreach (INestedTypeDefinition nestedType in groupingType.GetNestedTypes(context))
				{
					instance2.Add(new ForwardedExtensionGroupingOrMarkerType(forwardedExtensionGroupingOrMarkerType, nestedType));
				}
				instance.Add((forwardedExtensionGroupingOrMarkerType, instance2.ToImmutable()));
			}
			instance2.Free();
			ImmutableInterlocked.InterlockedInitialize<(INestedTypeReference, ImmutableArray<INestedTypeReference>)>(ref _lazyExtensionGroupingAndMarkerTypesForTypeForwarding, instance.ToImmutableAndFree());
		}
		return _lazyExtensionGroupingAndMarkerTypesForTypeForwarding;
	}
}
