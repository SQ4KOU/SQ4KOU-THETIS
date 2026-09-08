using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class MethodSymbol : Symbol, ITypeMemberReference, IReference, INamedEntity, IMethodReference, ISignature, IGenericMethodInstanceReference, ISpecializedMethodReference, ITypeDefinitionMember, IDefinition, IMethodDefinition, IMethodSymbolInternal, ISymbolInternal
{
	internal enum IsMetadataVirtualOption
	{
		None,
		IgnoreInterfaceImplementationChanges
	}

	internal const MethodSymbol None = null;

	private ParameterSignature _lazyParameterSignature;

	bool IDefinition.IsEncDeleted => false;

	IGenericMethodInstanceReference IMethodReference.AsGenericMethodInstanceReference
	{
		get
		{
			if (!AdaptedMethodSymbol.IsDefinition && AdaptedMethodSymbol.IsGenericMethod)
			{
				return this;
			}
			return null;
		}
	}

	ISpecializedMethodReference IMethodReference.AsSpecializedMethodReference
	{
		get
		{
			if (!AdaptedMethodSymbol.IsDefinition && (!AdaptedMethodSymbol.IsGenericMethod || PEModuleBuilder.IsGenericType(AdaptedMethodSymbol.ContainingType)))
			{
				return this;
			}
			return null;
		}
	}

	string INamedEntity.Name => AdaptedMethodSymbol.MetadataName;

	bool IMethodReference.AcceptsExtraArguments => AdaptedMethodSymbol.IsVararg;

	ushort IMethodReference.GenericParameterCount => (ushort)AdaptedMethodSymbol.Arity;

	ushort ISignature.ParameterCount => (ushort)AdaptedMethodSymbol.ParameterCount;

	ImmutableArray<IParameterTypeInformation> IMethodReference.ExtraParameters => ImmutableArray<IParameterTypeInformation>.Empty;

	CallingConvention ISignature.CallingConvention => AdaptedMethodSymbol.CallingConvention;

	ImmutableArray<ICustomModifier> ISignature.ReturnValueCustomModifiers => ImmutableArray<ICustomModifier>.CastUp(AdaptedMethodSymbol.ReturnTypeWithAnnotations.CustomModifiers);

	ImmutableArray<ICustomModifier> ISignature.RefCustomModifiers => ImmutableArray<ICustomModifier>.CastUp(AdaptedMethodSymbol.RefCustomModifiers);

	bool ISignature.ReturnValueIsByRef => AdaptedMethodSymbol.RefKind.IsManagedReference();

	IMethodReference ISpecializedMethodReference.UnspecializedVersion => AdaptedMethodSymbol.OriginalDefinition.GetCciAdapter();

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition
	{
		get
		{
			if (AdaptedMethodSymbol.OriginalDefinition is SynthesizedGlobalMethodSymbol synthesizedGlobalMethodSymbol)
			{
				return synthesizedGlobalMethodSymbol.ContainingPrivateImplementationDetailsType;
			}
			if (AdaptedMethodSymbol is SynthesizedExtensionMarker markerMethod)
			{
				return ((SourceMemberContainerTypeSymbol)AdaptedMethodSymbol.ContainingType.ContainingType).GetExtensionGroupingInfo().GetCorrespondingMarkerType(markerMethod);
			}
			if (AdaptedMethodSymbol.IsExtensionBlockMember())
			{
				NamedTypeSymbol containingType = AdaptedMethodSymbol.ContainingType;
				return ((SourceMemberContainerTypeSymbol)containingType.ContainingType).GetExtensionGroupingInfo().GetCorrespondingGroupingType((SourceNamedTypeSymbol)containingType);
			}
			return AdaptedMethodSymbol.ContainingType.GetCciAdapter();
		}
	}

	TypeMemberVisibility ITypeDefinitionMember.Visibility => AdaptedMethodSymbol.MetadataVisibility;

	bool IMethodDefinition.HasBody => DefaultImplementations.HasBody(this);

	IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters
	{
		get
		{
			foreach (TypeParameterSymbol typeParameter in AdaptedMethodSymbol.TypeParameters)
			{
				yield return typeParameter.GetCciAdapter();
			}
		}
	}

	bool IMethodDefinition.HasDeclarativeSecurity => AdaptedMethodSymbol.HasDeclarativeSecurity;

	IEnumerable<SecurityAttribute> IMethodDefinition.SecurityAttributes => AdaptedMethodSymbol.GetSecurityInformation();

	bool IMethodDefinition.IsAbstract => AdaptedMethodSymbol.IsAbstract;

	bool IMethodDefinition.IsAccessCheckedOnOverride => AdaptedMethodSymbol.IsAccessCheckedOnOverride;

	bool IMethodDefinition.IsConstructor => AdaptedMethodSymbol.MethodKind == MethodKind.Constructor;

	bool IMethodDefinition.IsExternal
	{
		get
		{
			if (!AdaptedMethodSymbol.ContainingType.IsExtension)
			{
				return AdaptedMethodSymbol.IsExternal;
			}
			return false;
		}
	}

	bool IMethodDefinition.IsHiddenBySignature => !AdaptedMethodSymbol.HidesBaseMethodsByName;

	bool IMethodDefinition.IsNewSlot => AdaptedMethodSymbol.IsMetadataNewSlot();

	bool IMethodDefinition.IsPlatformInvoke
	{
		get
		{
			if (!AdaptedMethodSymbol.ContainingType.IsExtension)
			{
				return AdaptedMethodSymbol.GetDllImportData() != null;
			}
			return false;
		}
	}

	IPlatformInvokeInformation IMethodDefinition.PlatformInvokeData
	{
		get
		{
			if (!AdaptedMethodSymbol.ContainingType.IsExtension)
			{
				return AdaptedMethodSymbol.GetDllImportData();
			}
			return null;
		}
	}

	bool IMethodDefinition.IsRuntimeSpecial => AdaptedMethodSymbol.HasRuntimeSpecialName;

	bool IMethodDefinition.IsSealed => AdaptedMethodSymbol.IsMetadataFinal;

	bool IMethodDefinition.IsSpecialName => AdaptedMethodSymbol.HasSpecialName;

	bool IMethodDefinition.IsStatic => AdaptedMethodSymbol.IsStatic;

	bool IMethodDefinition.IsVirtual => AdaptedMethodSymbol.IsMetadataVirtual();

	ImmutableArray<IParameterDefinition> IMethodDefinition.Parameters => EnumerateDefinitionParameters();

	bool IMethodDefinition.RequiresSecurityObject => AdaptedMethodSymbol.RequiresSecurityObject;

	bool IMethodDefinition.ReturnValueIsMarshalledExplicitly => AdaptedMethodSymbol.ReturnValueIsMarshalledExplicitly;

	IMarshallingInformation IMethodDefinition.ReturnValueMarshallingInformation => AdaptedMethodSymbol.ReturnValueMarshallingInformation;

	ImmutableArray<byte> IMethodDefinition.ReturnValueMarshallingDescriptor => AdaptedMethodSymbol.ReturnValueMarshallingDescriptor;

	INamespace IMethodDefinition.ContainingNamespace => AdaptedMethodSymbol.ContainingNamespace.GetCciAdapter();

	internal MethodSymbol AdaptedMethodSymbol => this;

	internal virtual bool IsAccessCheckedOnOverride
	{
		get
		{
			Accessibility declaredAccessibility = DeclaredAccessibility;
			if ((declaredAccessibility == Accessibility.Private || declaredAccessibility == Accessibility.ProtectedAndInternal || declaredAccessibility == Accessibility.Internal) && IsMetadataVirtual())
			{
				return !IsMetadataFinal;
			}
			return false;
		}
	}

	internal virtual bool IsExternal
	{
		get
		{
			if (!IsExtern)
			{
				if ((object)ContainingType != null)
				{
					return ContainingType.TypeKind == TypeKind.Delegate;
				}
				return false;
			}
			return true;
		}
	}

	internal virtual bool HasRuntimeSpecialName
	{
		get
		{
			if (MethodKind != MethodKind.Constructor)
			{
				return MethodKind == MethodKind.StaticConstructor;
			}
			return true;
		}
	}

	internal virtual bool IsMetadataFinal
	{
		get
		{
			if (!IsSealed)
			{
				if (IsMetadataVirtual())
				{
					if (!IsVirtual && !IsOverride && !IsAbstract)
					{
						return MethodKind != MethodKind.Destructor;
					}
					return false;
				}
				return false;
			}
			return true;
		}
	}

	internal virtual bool ReturnValueIsMarshalledExplicitly => ReturnValueMarshallingInformation != null;

	internal virtual ImmutableArray<byte> ReturnValueMarshallingDescriptor => default(ImmutableArray<byte>);

	public new virtual MethodSymbol OriginalDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalDefinition;

	public abstract MethodKind MethodKind { get; }

	public abstract int Arity { get; }

	public virtual bool IsGenericMethod => Arity != 0;

	public virtual bool RequiresInstanceReceiver => !IsStatic;

	internal virtual bool IsDirectlyExcludedFromCodeCoverage => false;

	internal abstract bool HasSpecialNameAttribute { get; }

	internal virtual ImmutableArray<string> NotNullMembers => ImmutableArray<string>.Empty;

	internal virtual ImmutableArray<string> NotNullWhenTrueMembers => ImmutableArray<string>.Empty;

	internal virtual ImmutableArray<string> NotNullWhenFalseMembers => ImmutableArray<string>.Empty;

	public abstract bool IsExtensionMethod { get; }

	internal abstract bool HasSpecialName { get; }

	internal abstract MethodImplAttributes ImplementationAttributes { get; }

	internal abstract bool HasDeclarativeSecurity { get; }

	internal abstract MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation { get; }

	internal abstract bool RequiresSecurityObject { get; }

	public abstract bool HidesBaseMethodsByName { get; }

	public abstract bool IsVararg { get; }

	public virtual bool IsCheckedBuiltin => false;

	public abstract bool ReturnsVoid { get; }

	public abstract bool IsAsync { get; }

	public bool ReturnsByRef => RefKind == RefKind.Ref;

	public bool ReturnsByRefReadonly => RefKind == RefKind.In;

	public abstract RefKind RefKind { get; }

	public abstract TypeWithAnnotations ReturnTypeWithAnnotations { get; }

	public TypeSymbol ReturnType => ReturnTypeWithAnnotations.Type;

	public abstract FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations { get; }

	public abstract ImmutableHashSet<string> ReturnNotNullIfParameterNotNull { get; }

	public abstract FlowAnalysisAnnotations FlowAnalysisAnnotations { get; }

	public abstract ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations { get; }

	public abstract ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

	internal ParameterSymbol? ThisParameter
	{
		get
		{
			if (!TryGetThisParameter(out ParameterSymbol thisParameter))
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/MethodSymbol.cs", 290);
			}
			return thisParameter;
		}
	}

	internal virtual int ParameterCount => Parameters.Length;

	public abstract ImmutableArray<ParameterSymbol> Parameters { get; }

	public virtual MethodSymbol ConstructedFrom => this;

	internal virtual bool IsExplicitInterfaceImplementation => ExplicitInterfaceImplementations.Any();

	internal abstract bool IsDeclaredReadOnly { get; }

	internal abstract bool IsInitOnly { get; }

	internal virtual bool IsEffectivelyReadOnly
	{
		get
		{
			if (!IsDeclaredReadOnly)
			{
				NamedTypeSymbol containingType = ContainingType;
				if ((object)containingType == null || !containingType.IsReadOnly)
				{
					return false;
				}
			}
			return IsValidReadOnlyTarget;
		}
	}

	protected bool IsValidReadOnlyTarget
	{
		get
		{
			if (!IsStatic && ContainingType.IsStructType() && MethodKind != MethodKind.Constructor)
			{
				return !IsInitOnly;
			}
			return false;
		}
	}

	public abstract ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations { get; }

	public abstract ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	public abstract Symbol AssociatedSymbol { get; }

	public MethodSymbol OverriddenMethod
	{
		get
		{
			if (IsOverride && (object)ConstructedFrom == this)
			{
				if (base.IsDefinition)
				{
					return (MethodSymbol)OverriddenOrHiddenMembers.GetOverriddenMember();
				}
				return (MethodSymbol)OverriddenOrHiddenMembersResult.GetOverriddenMember(this, OriginalDefinition.OverriddenMethod);
			}
			return null;
		}
	}

	public bool IsConditional
	{
		get
		{
			if (GetAppliedConditionalSymbols().Any())
			{
				return true;
			}
			if (IsOverride)
			{
				MethodSymbol overriddenMethod = OverriddenMethod;
				if ((object)overriddenMethod != null)
				{
					return overriddenMethod.IsConditional;
				}
			}
			return false;
		}
	}

	internal bool HasSetsRequiredMembers
	{
		get
		{
			if (MethodKind == MethodKind.Constructor)
			{
				return HasSetsRequiredMembersImpl;
			}
			return false;
		}
	}

	protected abstract bool HasSetsRequiredMembersImpl { get; }

	internal abstract bool HasUnscopedRefAttribute { get; }

	internal abstract bool UseUpdatedEscapeRules { get; }

	internal virtual OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers => this.MakeOverriddenOrHiddenMembers();

	public sealed override SymbolKind Kind => SymbolKind.Method;

	internal bool IsScriptConstructor
	{
		get
		{
			if (MethodKind == MethodKind.Constructor)
			{
				return ContainingType.IsScriptClass;
			}
			return false;
		}
	}

	internal virtual bool IsScriptInitializer => false;

	internal bool IsImplicitConstructor
	{
		get
		{
			if (MethodKind == MethodKind.Constructor || MethodKind == MethodKind.StaticConstructor)
			{
				return IsImplicitlyDeclared;
			}
			return false;
		}
	}

	internal bool IsImplicitInstanceConstructor
	{
		get
		{
			if (MethodKind == MethodKind.Constructor)
			{
				return IsImplicitlyDeclared;
			}
			return false;
		}
	}

	internal bool IsSubmissionConstructor
	{
		get
		{
			if (IsScriptConstructor)
			{
				return ContainingAssembly.IsInteractive;
			}
			return false;
		}
	}

	internal bool IsSubmissionInitializer
	{
		get
		{
			if (IsScriptInitializer)
			{
				return ContainingAssembly.IsInteractive;
			}
			return false;
		}
	}

	internal bool IsEntryPointCandidate
	{
		get
		{
			if (this.IsPartialDefinition() && (object)PartialImplementationPart == null)
			{
				return false;
			}
			if (IsStatic && !IsAbstract && !IsVirtual)
			{
				return Name == "Main";
			}
			return false;
		}
	}

	internal virtual MethodSymbol CallsiteReducedFromMethod => null;

	public virtual MethodSymbol PartialImplementationPart => null;

	public virtual MethodSymbol PartialDefinitionPart => null;

	public virtual MethodSymbol ReducedFrom => null;

	public virtual TypeSymbol ReceiverType => ContainingType;

	internal ImmutableArray<TypeWithAnnotations> ParameterTypesWithAnnotations
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref _lazyParameterSignature);
			return _lazyParameterSignature.parameterTypesWithAnnotations;
		}
	}

	internal ImmutableArray<RefKind> ParameterRefKinds
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref _lazyParameterSignature);
			return _lazyParameterSignature.parameterRefKinds;
		}
	}

	internal abstract CallingConvention CallingConvention { get; }

	internal virtual ImmutableArray<NamedTypeSymbol> UnmanagedCallingConventionTypes => ImmutableArray<NamedTypeSymbol>.Empty;

	internal virtual TypeMap TypeSubstitution => null;

	public sealed override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo diagnosticInfo = GetUseSiteInfo().DiagnosticInfo;
			bool flag = diagnosticInfo != null;
			if (flag)
			{
				int code = diagnosticInfo.Code;
				bool flag2 = ((code == 570 || code == 9041) ? true : false);
				flag = flag2;
			}
			return flag;
		}
	}

	internal virtual bool IsIterator => false;

	internal virtual TypeWithAnnotations IteratorElementTypeWithAnnotations => default(TypeWithAnnotations);

	internal virtual bool SynthesizesLoweredBoundBody => false;

	internal abstract bool GenerateDebugInfo { get; }

	internal virtual Microsoft.CodeAnalysis.NullableAnnotation ReceiverNullableAnnotation
	{
		get
		{
			if (!RequiresInstanceReceiver)
			{
				return Microsoft.CodeAnalysis.NullableAnnotation.None;
			}
			return Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated;
		}
	}

	public abstract bool AreLocalsZeroed { get; }

	internal int OverloadResolutionPriority
	{
		get
		{
			if (!CanHaveOverloadResolutionPriority)
			{
				return 0;
			}
			return TryGetOverloadResolutionPriority();
		}
	}

	internal bool CanHaveOverloadResolutionPriority
	{
		get
		{
			MethodKind methodKind = MethodKind;
			if ((methodKind == MethodKind.Constructor || (uint)(methodKind - 9) <= 1u || methodKind == MethodKind.ReducedExtension) ? true : false)
			{
				return !IsOverride;
			}
			return false;
		}
	}

	bool IMethodSymbolInternal.HasDeclarativeSecurity => HasDeclarativeSecurity;

	bool IMethodSymbolInternal.IsAccessCheckedOnOverride => IsAccessCheckedOnOverride;

	bool IMethodSymbolInternal.IsExternal => IsExternal;

	bool IMethodSymbolInternal.IsHiddenBySignature => !HidesBaseMethodsByName;

	bool IMethodSymbolInternal.IsMetadataNewSlot => IsMetadataNewSlot();

	bool IMethodSymbolInternal.IsPlatformInvoke => GetDllImportData() != null;

	bool IMethodSymbolInternal.HasRuntimeSpecialName => HasRuntimeSpecialName;

	bool IMethodSymbolInternal.IsMetadataFinal => IsSealed;

	bool IMethodSymbolInternal.HasSpecialName => HasSpecialName;

	bool IMethodSymbolInternal.RequiresSecurityObject => RequiresSecurityObject;

	MethodImplAttributes IMethodSymbolInternal.ImplementationAttributes => ImplementationAttributes;

	bool IMethodSymbolInternal.IsIterator => IsIterator;

	ISymbolInternal IMethodSymbolInternal.AssociatedSymbol => AssociatedSymbol;

	IMethodSymbolInternal IMethodSymbolInternal.PartialImplementationPart => PartialImplementationPart;

	IMethodSymbolInternal IMethodSymbolInternal.PartialDefinitionPart => PartialDefinitionPart;

	public virtual BlobHandle MetadataSignatureHandle => default(BlobHandle);

	int IMethodSymbolInternal.ParameterCount => ParameterCount;

	ImmutableArray<IParameterSymbolInternal> IMethodSymbolInternal.Parameters => Parameters.Cast<ParameterSymbol, IParameterSymbolInternal>();

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return ResolvedMethodImpl(context);
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		if (AdaptedMethodSymbol.OriginalDefinition is SynthesizedGlobalMethodSymbol synthesizedGlobalMethodSymbol)
		{
			return synthesizedGlobalMethodSymbol.ContainingPrivateImplementationDetailsType;
		}
		NamedTypeSymbol containingType = AdaptedMethodSymbol.ContainingType;
		if (AdaptedMethodSymbol is SynthesizedExtensionMarker markerMethod)
		{
			return ((SourceMemberContainerTypeSymbol)containingType.ContainingType).GetExtensionGroupingInfo().GetCorrespondingMarkerType(markerMethod);
		}
		if (AdaptedMethodSymbol.IsExtensionBlockMember())
		{
			return ((SourceMemberContainerTypeSymbol)containingType.ContainingType).GetExtensionGroupingInfo().GetCorrespondingGroupingType((SourceNamedTypeSymbol)containingType);
		}
		return ((PEModuleBuilder)context.Module).Translate(containingType, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics, fromImplements: false, AdaptedMethodSymbol.IsDefinition);
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		if (!AdaptedMethodSymbol.IsDefinition)
		{
			if (AdaptedMethodSymbol.IsGenericMethod)
			{
				visitor.Visit((IGenericMethodInstanceReference)this);
			}
			else
			{
				visitor.Visit((IMethodReference)this);
			}
			return;
		}
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)visitor.Context.Module;
		if (AdaptedMethodSymbol.ContainingModule == pEModuleBuilder.SourceModule)
		{
			visitor.Visit((IMethodDefinition)this);
		}
		else
		{
			visitor.Visit((IMethodReference)this);
		}
	}

	IMethodDefinition IMethodReference.GetResolvedMethod(EmitContext context)
	{
		return ResolvedMethodImpl(context);
	}

	private IMethodDefinition ResolvedMethodImpl(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if (AdaptedMethodSymbol.IsDefinition && AdaptedMethodSymbol.ContainingModule == pEModuleBuilder.SourceModule)
		{
			return this;
		}
		return null;
	}

	ImmutableArray<IParameterTypeInformation> ISignature.GetParameters(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if (AdaptedMethodSymbol.IsDefinition && AdaptedMethodSymbol.ContainingModule == pEModuleBuilder.SourceModule)
		{
			return StaticCast<IParameterTypeInformation>.From(EnumerateDefinitionParameters());
		}
		return pEModuleBuilder.Translate(AdaptedMethodSymbol.Parameters);
	}

	private ImmutableArray<IParameterDefinition> EnumerateDefinitionParameters()
	{
		return StaticCast<IParameterDefinition>.From(AdaptedMethodSymbol.Parameters);
	}

	ITypeReference ISignature.GetType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(AdaptedMethodSymbol.ReturnType, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics);
	}

	IEnumerable<ITypeReference> IGenericMethodInstanceReference.GetGenericArguments(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (TypeWithAnnotations typeArgumentsWithAnnotation in AdaptedMethodSymbol.TypeArgumentsWithAnnotations)
		{
			yield return moduleBeingBuilt.Translate(typeArgumentsWithAnnotation.Type, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics);
		}
	}

	IMethodReference IGenericMethodInstanceReference.GetGenericMethod(EmitContext context)
	{
		if (!PEModuleBuilder.IsGenericType(AdaptedMethodSymbol.ContainingType))
		{
			return ((PEModuleBuilder)context.Module).Translate(AdaptedMethodSymbol.OriginalDefinition, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics, null, needDeclaration: true);
		}
		return new SpecializedMethodReference(AdaptedMethodSymbol.ConstructedFrom);
	}

	IMethodBody? IMethodDefinition.GetBody(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).GetMethodBody(AdaptedMethodSymbol);
	}

	MethodImplAttributes IMethodDefinition.GetImplementationAttributes(EmitContext context)
	{
		if (!AdaptedMethodSymbol.ContainingType.IsExtension)
		{
			return AdaptedMethodSymbol.ImplementationAttributes;
		}
		return MethodImplAttributes.IL;
	}

	IEnumerable<ICustomAttribute> IMethodDefinition.GetReturnValueAttributes(EmitContext context)
	{
		ImmutableArray<CSharpAttributeData> returnTypeAttributes = AdaptedMethodSymbol.GetReturnTypeAttributes();
		ArrayBuilder<CSharpAttributeData> attributes = null;
		AdaptedMethodSymbol.AddSynthesizedReturnTypeAttributes((PEModuleBuilder)context.Module, ref attributes);
		return AdaptedMethodSymbol.GetCustomAttributesToEmit(returnTypeAttributes, attributes, isReturnType: true, emittingAssemblyAttributesInNetModule: false);
	}

	internal new MethodSymbol GetCciAdapter()
	{
		return this;
	}

	internal abstract bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false);

	internal abstract bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None);

	internal abstract UnmanagedCallersOnlyAttributeData? GetUnmanagedCallersOnlyAttributeData(bool forceComplete);

	internal abstract bool HasAsyncMethodBuilderAttribute(out TypeSymbol builderArgument);

	public abstract DllImportData? GetDllImportData();

	internal abstract IEnumerable<SecurityAttribute> GetSecurityInformation();

	internal ImmutableArray<TypeWithAnnotations> GetTypeParametersAsTypeArguments()
	{
		return TypeMap.TypeParametersAsTypeSymbolsWithAnnotations(TypeParameters);
	}

	internal virtual bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		thisParameter = null;
		return false;
	}

	public virtual ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return ImmutableArray<CSharpAttributeData>.Empty;
	}

	internal MethodSymbol GetLeastOverriddenMethod(NamedTypeSymbol accessingTypeOpt)
	{
		return GetLeastOverriddenMethodCore(accessingTypeOpt, requireSameReturnType: false);
	}

	private MethodSymbol GetLeastOverriddenMethodCore(NamedTypeSymbol accessingTypeOpt, bool requireSameReturnType)
	{
		accessingTypeOpt = accessingTypeOpt?.OriginalDefinition;
		MethodSymbol methodSymbol = this;
		while (methodSymbol.IsOverride && !methodSymbol.HidesBaseMethodsByName)
		{
			MethodSymbol overriddenMethod = methodSymbol.OverriddenMethod;
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			if ((object)overriddenMethod == null || ((object)accessingTypeOpt != null && !AccessCheck.IsSymbolAccessible(overriddenMethod, accessingTypeOpt, ref useSiteInfo)) || (requireSameReturnType && !ReturnType.Equals(overriddenMethod.ReturnType, TypeCompareKind.AllIgnoreOptions)))
			{
				break;
			}
			methodSymbol = overriddenMethod;
		}
		return methodSymbol;
	}

	internal MethodSymbol GetConstructedLeastOverriddenMethod(NamedTypeSymbol accessingTypeOpt, bool requireSameReturnType)
	{
		MethodSymbol leastOverriddenMethodCore = ConstructedFrom.GetLeastOverriddenMethodCore(accessingTypeOpt, requireSameReturnType);
		if (!leastOverriddenMethodCore.IsGenericMethod)
		{
			return leastOverriddenMethodCore;
		}
		return leastOverriddenMethodCore.Construct(TypeArgumentsWithAnnotations);
	}

	internal virtual bool CallsAreOmitted(SyntaxTree syntaxTree)
	{
		if (syntaxTree != null)
		{
			return CallsAreConditionallyOmitted(syntaxTree);
		}
		return false;
	}

	private bool CallsAreConditionallyOmitted(SyntaxTree syntaxTree)
	{
		if (IsConditional)
		{
			ImmutableArray<string> appliedConditionalSymbols = GetAppliedConditionalSymbols();
			if (syntaxTree.IsAnyPreprocessorSymbolDefined(appliedConditionalSymbols))
			{
				return false;
			}
			if (IsOverride)
			{
				MethodSymbol overriddenMethod = OverriddenMethod;
				if ((object)overriddenMethod != null && overriddenMethod.IsConditional)
				{
					return overriddenMethod.CallsAreConditionallyOmitted(syntaxTree);
				}
			}
			return true;
		}
		return false;
	}

	internal abstract ImmutableArray<string> GetAppliedConditionalSymbols();

	internal static bool CanOverrideOrHide(MethodKind kind)
	{
		switch (kind)
		{
		case MethodKind.AnonymousFunction:
		case MethodKind.Constructor:
		case MethodKind.Destructor:
		case MethodKind.ExplicitInterfaceImplementation:
		case MethodKind.ReducedExtension:
		case MethodKind.StaticConstructor:
			return false;
		case MethodKind.Conversion:
		case MethodKind.DelegateInvoke:
		case MethodKind.EventAdd:
		case MethodKind.EventRemove:
		case MethodKind.UserDefinedOperator:
		case MethodKind.Ordinary:
		case MethodKind.PropertyGet:
		case MethodKind.PropertySet:
		case MethodKind.LocalFunction:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitMethod(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitMethod(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitMethod(this);
	}

	public MethodSymbol? ReduceExtensionMethod(TypeSymbol receiverType, CSharpCompilation? compilation)
	{
		bool wasFullyInferred;
		return ReduceExtensionMethod(receiverType, compilation, out wasFullyInferred);
	}

	public MethodSymbol? ReduceExtensionMethod(TypeSymbol receiverType, CSharpCompilation? compilation, out bool wasFullyInferred)
	{
		if ((object)receiverType == null)
		{
			throw new ArgumentNullException("receiverType");
		}
		if (!IsExtensionMethod || MethodKind == MethodKind.ReducedExtension || receiverType.IsVoidType())
		{
			wasFullyInferred = false;
			return null;
		}
		return ReducedExtensionMethodSymbol.Create(this, receiverType, compilation, out wasFullyInferred);
	}

	public MethodSymbol ReduceExtensionMethod()
	{
		if (!IsExtensionMethod || MethodKind == MethodKind.ReducedExtension)
		{
			return null;
		}
		return ReducedExtensionMethodSymbol.Create(this);
	}

	public virtual TypeWithAnnotations GetTypeInferredDuringReduction(TypeParameterSymbol reducedFromTypeParameter)
	{
		throw new InvalidOperationException();
	}

	public MethodSymbol Construct(params TypeSymbol[] typeArguments)
	{
		return Construct(ImmutableArray.Create(typeArguments));
	}

	public MethodSymbol Construct(ImmutableArray<TypeSymbol> typeArguments)
	{
		return Construct(typeArguments.SelectAsArray((TypeSymbol a) => TypeWithAnnotations.Create(a)));
	}

	internal MethodSymbol Construct(ImmutableArray<TypeWithAnnotations> typeArguments)
	{
		if ((object)this != ConstructedFrom || Arity == 0)
		{
			throw new InvalidOperationException();
		}
		if (typeArguments.IsDefault)
		{
			throw new ArgumentNullException("typeArguments");
		}
		if (typeArguments.Any(NamedTypeSymbol.TypeWithAnnotationsIsNullFunction))
		{
			throw new ArgumentException(CSharpResources.TypeArgumentCannotBeNull, "typeArguments");
		}
		if (typeArguments.Length != Arity)
		{
			throw new ArgumentException(CSharpResources.WrongNumberOfTypeArguments, "typeArguments");
		}
		if (ConstructedNamedTypeSymbol.TypeParametersMatchTypeArguments(TypeParameters, typeArguments))
		{
			return this;
		}
		return new ConstructedMethodSymbol(this, typeArguments);
	}

	internal MethodSymbol AsMember(NamedTypeSymbol newOwner)
	{
		if (!newOwner.IsDefinition)
		{
			return new SubstitutedMethodSymbol(newOwner, this);
		}
		return this;
	}

	internal TypeSymbol GetParameterType(int index)
	{
		return ParameterTypesWithAnnotations[index].Type;
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		if (base.IsDefinition)
		{
			return new UseSiteInfo<AssemblySymbol>(base.PrimaryDependency);
		}
		return OriginalDefinition.GetUseSiteInfo();
	}

	internal bool CalculateUseSiteDiagnostic(ref UseSiteInfo<AssemblySymbol> result)
	{
		if (DeriveUseSiteInfoFromType(ref result, ReturnTypeWithAnnotations, IsInitOnly ? AllowedRequiredModifierType.System_Runtime_CompilerServices_IsExternalInit : AllowedRequiredModifierType.None) || DeriveUseSiteInfoFromCustomModifiers(ref result, RefCustomModifiers, AllowedRequiredModifierType.System_Runtime_InteropServices_InAttribute) || DeriveUseSiteInfoFromParameters(ref result, Parameters))
		{
			return true;
		}
		ModuleSymbol containingModule = ContainingModule;
		if ((object)containingModule != null && containingModule.HasUnifiedReferences)
		{
			HashSet<TypeSymbol> checkedTypes = null;
			DiagnosticInfo result2 = result.DiagnosticInfo;
			if (ReturnTypeWithAnnotations.GetUnificationUseSiteDiagnosticRecursive(ref result2, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result2, RefCustomModifiers, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result2, Parameters, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result2, TypeParameters, this, ref checkedTypes))
			{
				result = result.AdjustDiagnosticInfo(result2);
				return true;
			}
			result = result.AdjustDiagnosticInfo(result2);
		}
		return false;
	}

	internal static (bool IsCallConvs, ImmutableHashSet<INamedTypeSymbolInternal>? CallConvs) TryDecodeUnmanagedCallersOnlyCallConvsField(string key, TypedConstant value, bool isField, Location? location, BindingDiagnosticBag? diagnostics)
	{
		ImmutableHashSet<INamedTypeSymbolInternal> item = null;
		if (!UnmanagedCallersOnlyAttributeData.IsCallConvsTypedConstant(key, isField, in value))
		{
			return (IsCallConvs: false, CallConvs: item);
		}
		if (value.Values.IsDefaultOrEmpty)
		{
			item = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
			return (IsCallConvs: true, CallConvs: item);
		}
		PooledHashSet<INamedTypeSymbolInternal> instance = PooledHashSet<INamedTypeSymbolInternal>.GetInstance();
		foreach (TypedConstant value2 in value.Values)
		{
			if (!(value2.ValueInternal is NamedTypeSymbol namedTypeSymbol) || !FunctionPointerTypeSymbol.IsCallingConventionModifier(namedTypeSymbol))
			{
				diagnostics?.Add(ErrorCode.ERR_InvalidUnmanagedCallersOnlyCallConv, location, value2.ValueInternal ?? "null");
			}
			else
			{
				instance.Add(namedTypeSymbol);
			}
		}
		item = instance.ToImmutableHashSet();
		instance.Free();
		return (IsCallConvs: true, CallConvs: item);
	}

	internal bool CheckAndReportValidUnmanagedCallersOnlyTarget(SyntaxNode? node, BindingDiagnosticBag? diagnostics)
	{
		bool flag = !IsStatic || IsAbstract || IsVirtual;
		if (!flag)
		{
			MethodKind methodKind = MethodKind;
			bool flag2 = ((methodKind == MethodKind.Ordinary || methodKind == MethodKind.LocalFunction) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			diagnostics?.Add(ErrorCode.ERR_UnmanagedCallersOnlyRequiresStatic, node.Location);
			return true;
		}
		if (isGenericMethod(this) || ContainingType.IsGenericType)
		{
			diagnostics?.Add(ErrorCode.ERR_UnmanagedCallersOnlyMethodOrTypeCannotBeGeneric, node.Location);
			return true;
		}
		return false;
		static bool isGenericMethod([DisallowNull] MethodSymbol? method)
		{
			do
			{
				if (method.IsGenericMethod)
				{
					return true;
				}
				method = method.ContainingSymbol as MethodSymbol;
			}
			while ((object)method != null);
			return false;
		}
	}

	protected sealed override bool IsHighestPriorityUseSiteErrorCode(int code)
	{
		if (code == 570 || code == 9041)
		{
			return true;
		}
		return false;
	}

	internal virtual void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/MethodSymbol.cs", 1122);
	}

	internal abstract int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree);

	internal virtual void AddSynthesizedReturnTypeAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		if (ReturnsByRefReadonly)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		TypeWithAnnotations returnTypeWithAnnotations = ReturnTypeWithAnnotations;
		if (returnTypeWithAnnotations.Type.ContainsDynamic() && declaringCompilation.HasDynamicEmitAttributes(BindingDiagnosticBag.Discarded, Location.None))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(returnTypeWithAnnotations.Type, returnTypeWithAnnotations.CustomModifiers.Length + RefCustomModifiers.Length, RefKind));
		}
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(returnTypeWithAnnotations.Type))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNativeIntegerAttribute(this, returnTypeWithAnnotations.Type));
		}
		if (returnTypeWithAnnotations.Type.ContainsTupleNames() && declaringCompilation.HasTupleNamesAttributes(BindingDiagnosticBag.Discarded, Location.None))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(returnTypeWithAnnotations.Type));
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableAttributeIfNecessary(this, GetNullableContextValue(), returnTypeWithAnnotations));
		}
	}

	internal abstract bool IsNullableAnalysisEnabled();

	internal abstract int TryGetOverloadResolutionPriority();

	int IMethodSymbolInternal.CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return CalculateLocalSyntaxOffset(localPosition, localTree);
	}

	IMethodSymbolInternal IMethodSymbolInternal.Construct(params ITypeSymbolInternal[] typeArguments)
	{
		return Construct((TypeSymbol[])typeArguments);
	}

	protected sealed override ISymbol CreateISymbol()
	{
		return new Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.MethodSymbol(this);
	}

	public override bool Equals(Symbol other, TypeCompareKind compareKind)
	{
		if (other is SubstitutedMethodSymbol substitutedMethodSymbol)
		{
			return substitutedMethodSymbol.Equals(this, compareKind);
		}
		if (other is NativeIntegerMethodSymbol nativeIntegerMethodSymbol)
		{
			return nativeIntegerMethodSymbol.Equals(this, compareKind);
		}
		return base.Equals(other, compareKind);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	protected static void AddRequiredMembersMarkerAttributes(ref ArrayBuilder<CSharpAttributeData> attributes, MethodSymbol methodToAttribute)
	{
		if (methodToAttribute.ShouldCheckRequiredMembers() && methodToAttribute.ContainingType.HasAnyRequiredMembers)
		{
			ObsoleteAttributeData? obsoleteAttributeData = methodToAttribute.ObsoleteAttributeData;
			CSharpCompilation declaringCompilation = methodToAttribute.DeclaringCompilation;
			if (obsoleteAttributeData == null)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_ObsoleteAttribute__ctor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, "Constructors of types with required members are not supported in this version of your compiler."), new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_Boolean), TypedConstantKind.Primitive, true))));
			}
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerFeatureRequiredAttribute__ctor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, "RequiredMembers"))));
		}
	}

	public MethodSymbol? TryGetCorrespondingExtensionImplementationMethod()
	{
		return ContainingType.TryGetCorrespondingExtensionImplementationMethod(this);
	}
}
