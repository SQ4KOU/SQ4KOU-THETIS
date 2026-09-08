using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMethodSymbol : MethodSymbol, IAttributeTargetSymbol
{
	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private CustomAttributesBag<CSharpAttributeData> _lazyReturnTypeCustomAttributesBag;

	protected readonly SyntaxReference syntaxReferenceOpt;

	protected bool AreContainingSymbolLocalsZeroed
	{
		get
		{
			if (ContainingSymbol is SourceMethodSymbol sourceMethodSymbol)
			{
				return sourceMethodSymbol.AreLocalsZeroed;
			}
			if (ContainingType is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
			{
				return sourceMemberContainerTypeSymbol.AreLocalsZeroed;
			}
			return true;
		}
	}

	protected override bool HasSetsRequiredMembersImpl
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceMethodSymbol.cs", 94);
		}
	}

	internal sealed override bool UseUpdatedEscapeRules => ContainingModule.UseUpdatedEscapeRules;

	internal virtual Binder? OuterBinder => null;

	internal virtual Binder? WithTypeParametersBinder => null;

	internal SyntaxReference SyntaxRef => syntaxReferenceOpt;

	internal CSharpSyntaxNode SyntaxNode
	{
		get
		{
			if (syntaxReferenceOpt != null)
			{
				return (CSharpSyntaxNode)syntaxReferenceOpt.GetSyntax();
			}
			return null;
		}
	}

	internal SyntaxTree SyntaxTree
	{
		get
		{
			if (syntaxReferenceOpt != null)
			{
				return syntaxReferenceOpt.SyntaxTree;
			}
			return null;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			if (syntaxReferenceOpt != null)
			{
				return ImmutableArray.Create(syntaxReferenceOpt);
			}
			return ImmutableArray<SyntaxReference>.Empty;
		}
	}

	public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => DecodeReturnTypeAnnotationAttributes(GetDecodedReturnTypeWellKnownAttributeData());

	public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => GetDecodedReturnTypeWellKnownAttributeData()?.NotNullIfParameterNotNull ?? ImmutableHashSet<string>.Empty;

	protected virtual SourceMemberMethodSymbol BoundAttributesSource => null;

	protected virtual IAttributeTargetSymbol AttributeOwner => this;

	protected virtual AttributeLocation AttributeLocationForLoadAndValidateAttributes => AttributeLocation.None;

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => AttributeOwner;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Method;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			switch (MethodKind)
			{
			case MethodKind.Constructor:
			case MethodKind.Destructor:
			case MethodKind.StaticConstructor:
				return AttributeLocation.Method;
			case MethodKind.EventAdd:
			case MethodKind.EventRemove:
			case MethodKind.PropertySet:
				return AttributeLocation.Method | AttributeLocation.Parameter | AttributeLocation.Return;
			default:
				return AttributeLocation.Method | AttributeLocation.Return;
			}
		}
	}

	public override bool AreLocalsZeroed
	{
		get
		{
			MethodWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null || !decodedWellKnownAttributeData.HasSkipLocalsInitAttribute)
			{
				return AreContainingSymbolLocalsZeroed;
			}
			return false;
		}
	}

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (ContainingSymbol is SourceMemberContainerTypeSymbol { AnyMemberHasAttributes: false })
			{
				return null;
			}
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((MethodEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			if (syntaxReferenceOpt == null)
			{
				return null;
			}
			return ObsoleteAttributeData.Uninitialized;
		}
	}

	internal ThreeState IsRuntimeAsyncEnabledInMethod => GetDecodedWellKnownAttributeData()?.RuntimeAsyncMethodGenerationSetting ?? ThreeState.Unknown;

	internal override ImmutableArray<string> NotNullMembers => GetDecodedWellKnownAttributeData()?.NotNullMembers ?? ImmutableArray<string>.Empty;

	internal override ImmutableArray<string> NotNullWhenTrueMembers => GetDecodedWellKnownAttributeData()?.NotNullWhenTrueMembers ?? ImmutableArray<string>.Empty;

	internal override ImmutableArray<string> NotNullWhenFalseMembers => GetDecodedWellKnownAttributeData()?.NotNullWhenFalseMembers ?? ImmutableArray<string>.Empty;

	public override FlowAnalysisAnnotations FlowAnalysisAnnotations => DecodeFlowAnalysisAttributes(GetDecodedWellKnownAttributeData());

	internal sealed override bool HasUnscopedRefAttribute => GetDecodedWellKnownAttributeData()?.HasUnscopedRefAttribute ?? false;

	public sealed override bool HidesBaseMethodsByName => false;

	internal sealed override bool HasRuntimeSpecialName
	{
		get
		{
			if (!base.HasRuntimeSpecialName)
			{
				return IsVtableGapInterfaceMethod();
			}
			return true;
		}
	}

	internal override bool HasSpecialName
	{
		get
		{
			switch (MethodKind)
			{
			case MethodKind.Constructor:
			case MethodKind.Conversion:
			case MethodKind.EventAdd:
			case MethodKind.EventRemove:
			case MethodKind.UserDefinedOperator:
			case MethodKind.PropertyGet:
			case MethodKind.PropertySet:
			case MethodKind.StaticConstructor:
				return true;
			default:
				if (IsVtableGapInterfaceMethod())
				{
					return true;
				}
				return HasSpecialNameAttribute;
			}
		}
	}

	internal sealed override bool HasSpecialNameAttribute => GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;

	internal sealed override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal override bool RequiresSecurityObject => GetDecodedWellKnownAttributeData()?.HasDynamicSecurityMethodAttribute ?? false;

	internal override bool HasDeclarativeSecurity => GetDecodedWellKnownAttributeData()?.HasDeclarativeSecurity ?? false;

	internal override MarshalPseudoCustomAttributeData? ReturnValueMarshallingInformation => GetDecodedReturnTypeWellKnownAttributeData()?.MarshallingInformation;

	internal override MethodImplAttributes ImplementationAttributes
	{
		get
		{
			MethodImplAttributes result = GetDecodedWellKnownAttributeData()?.MethodImplAttributes ?? MethodImplAttributes.IL;
			if (ContainingType.IsComImport && MethodKind == MethodKind.Constructor)
			{
				result |= (MethodImplAttributes)0x1003;
			}
			AddAsyncImplAttributeIfNeeded(ref result);
			return result;
		}
	}

	public abstract ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes();

	public abstract ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds();

	protected static void ReportBadRefToken(TypeSyntax returnTypeSyntax, BindingDiagnosticBag diagnostics)
	{
		if (!returnTypeSyntax.HasErrors)
		{
			SyntaxToken firstToken = returnTypeSyntax.GetFirstToken();
			diagnostics.Add(ErrorCode.ERR_UnexpectedToken, firstToken.GetLocation(), firstToken.ToString());
		}
	}

	internal void ReportAsyncParameterErrors(BindingDiagnosticBag diagnostics, Location location)
	{
		foreach (ParameterSymbol item in this.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: true))
		{
			bool flag = item.IsExtensionParameter();
			if (item.RefKind != RefKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_BadAsyncArgType, getLocation(item, location, flag));
			}
			else if (item.Type.IsPointerOrFunctionPointer() && !flag)
			{
				diagnostics.Add(ErrorCode.ERR_UnsafeAsyncArgType, getLocation(item, location, flag));
			}
			else if (item.Type.IsRestrictedType())
			{
				diagnostics.Add(ErrorCode.ERR_BadSpecialByRefParameter, getLocation(item, location, flag), item.Type);
			}
		}
		static Location getLocation(ParameterSymbol parameter, Location location3, bool isReceiverParameter)
		{
			Location location2;
			if (!isReceiverParameter)
			{
				location2 = parameter.TryGetFirstLocation();
				if ((object)location2 == null)
				{
					return location3;
				}
			}
			else
			{
				location2 = location3;
			}
			return location2;
		}
	}

	internal override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		return SourceMemberContainerTypeSymbol.HasAsyncMethodBuilderAttribute(this, out builderArgument);
	}

	internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		AddSynthesizedAttributes(this, moduleBuilder, ref attributes);
	}

	internal static void AddSynthesizedAttributes(MethodSymbol target, PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		if (target.IsDeclaredReadOnly && !target.ContainingType.IsReadOnly)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(target));
		}
		CSharpCompilation declaringCompilation = target.DeclaringCompilation;
		if (declaringCompilation.ShouldEmitNullableAttributes(target) && target.ShouldEmitNullableContextValue(out var value))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableContextAttribute(target, value));
		}
		if (target.RequiresExplicitOverride(out var warnAmbiguous))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizePreserveBaseOverridesAttribute());
		}
		bool isAsync = target.IsAsync;
		bool isIterator = target.IsIterator;
		if ((isAsync | isIterator) && !target.IsExtensionBlockMember())
		{
			if (moduleBuilder.CompilationState.TryGetStateMachineType(target, out NamedTypeSymbol stateMachineType))
			{
				TypedConstant item = new TypedConstant(declaringCompilation.GetWellKnownType(WellKnownType.System_Type), TypedConstantKind.Type, stateMachineType.GetUnboundGenericTypeOrSelf());
				if (isAsync & isIterator)
				{
					Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_AsyncIteratorStateMachineAttribute__ctor, ImmutableArray.Create(item)));
				}
				else if (isAsync)
				{
					Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_AsyncStateMachineAttribute__ctor, ImmutableArray.Create(item)));
				}
				else if (isIterator)
				{
					Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_IteratorStateMachineAttribute__ctor, ImmutableArray.Create(item)));
				}
			}
			if (isAsync && !isIterator && !declaringCompilation.IsRuntimeAsyncEnabledIn(target))
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDebuggerStepThroughAttribute());
			}
		}
		warnAmbiguous = ((target.IsImplicitlyDeclared && !(target is SourceFieldLikeEventSymbol.SourceEventDefinitionAccessorSymbol { PartialImplementationPart: { IsImplicitlyDeclared: false } })) || target is SourcePropertyAccessorSymbol { IsAutoPropertyAccessor: not false }) && !target.ContainingType.IsImplicitlyDeclared;
		if (warnAmbiguous)
		{
			bool flag = ((target is SynthesizedMethodBaseSymbol || target is SourcePropertyAccessorSymbol || target is SynthesizedSourceOrdinaryMethodSymbol || target is SynthesizedRecordEqualityOperatorBase || target is SynthesizedEventAccessorSymbol || target is SourceFieldLikeEventSymbol.SourceEventDefinitionAccessorSymbol) ? true : false);
			warnAmbiguous = flag;
		}
		if (warnAmbiguous)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
		if (target is SourceConstructorSymbolBase)
		{
			MethodSymbol.AddRequiredMembersMarkerAttributes(ref attributes, target);
		}
		if (target.IsExtensionMethod)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor));
		}
		if (target is SourcePropertyAccessorSymbol { AssociatedSymbol: SourcePropertySymbolBase associatedSymbol })
		{
			if (!target.NotNullMembers.IsEmpty)
			{
				foreach (SourceAttributeData memberNotNullAttributeIfExist in associatedSymbol.MemberNotNullAttributeIfExists)
				{
					Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(memberNotNullAttributeIfExist));
				}
			}
			if (!target.NotNullWhenTrueMembers.IsEmpty || !target.NotNullWhenFalseMembers.IsEmpty)
			{
				foreach (SourceAttributeData memberNotNullWhenAttributeIfExist in associatedSymbol.MemberNotNullWhenAttributeIfExists)
				{
					Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(memberNotNullWhenAttributeIfExist));
				}
			}
		}
		if (target is MethodToClassRewriter.BaseMethodWrapperSymbol)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerHiddenAttribute__ctor));
		}
		if (IsInstanceIncrementDecrementOrCompoundAssignmentOperator(target))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerFeatureRequiredAttribute__ctor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, "UserDefinedCompoundAssignmentOperators"))));
		}
		if (target.IsExtensionBlockMember())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeExtensionMarkerAttribute(target, ((SourceNamedTypeSymbol)target.ContainingType).ExtensionMarkerName));
		}
	}

	internal static bool IsInstanceIncrementDecrementOrCompoundAssignmentOperator(MethodSymbol target)
	{
		if (target.MethodKind == MethodKind.UserDefinedOperator && !target.IsStatic)
		{
			SyntaxKind operatorKind = SyntaxFacts.GetOperatorKind(target.Name);
			if (operatorKind - 8262 > SyntaxKind.List)
			{
				return SyntaxFacts.IsOverloadableCompoundAssignmentOperator(operatorKind);
			}
			return true;
		}
		return false;
	}

	protected SourceMethodSymbol(SyntaxReference syntaxReferenceOpt)
	{
		this.syntaxReferenceOpt = syntaxReferenceOpt;
	}

	protected CSharpSyntaxNode? GetInMethodSyntaxNode()
	{
		CSharpSyntaxNode syntaxNode = SyntaxNode;
		if (!(syntaxNode is ConstructorDeclarationSyntax constructorDeclarationSyntax))
		{
			if (!(syntaxNode is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax))
			{
				if (!(syntaxNode is AccessorDeclarationSyntax accessorDeclarationSyntax))
				{
					if (!(syntaxNode is ArrowExpressionClauseSyntax result))
					{
						if (!(syntaxNode is LocalFunctionStatementSyntax localFunctionStatementSyntax))
						{
							if (!(syntaxNode is CompilationUnitSyntax))
							{
								if (syntaxNode is RecordDeclarationSyntax result2)
								{
									return result2;
								}
								if (syntaxNode is ClassDeclarationSyntax result3)
								{
									return result3;
								}
							}
							else if (this is SynthesizedSimpleProgramEntryPointSymbol synthesizedSimpleProgramEntryPointSymbol)
							{
								return (CSharpSyntaxNode)synthesizedSimpleProgramEntryPointSymbol.ReturnTypeSyntax;
							}
							return null;
						}
						return (CSharpSyntaxNode?)(((object)localFunctionStatementSyntax.Body) ?? ((object)localFunctionStatementSyntax.ExpressionBody));
					}
					return result;
				}
				return (CSharpSyntaxNode?)(((object)accessorDeclarationSyntax.Body) ?? ((object)accessorDeclarationSyntax.ExpressionBody));
			}
			return (CSharpSyntaxNode?)(((object)baseMethodDeclarationSyntax.Body) ?? ((object)baseMethodDeclarationSyntax.ExpressionBody));
		}
		return (CSharpSyntaxNode?)(constructorDeclarationSyntax.Initializer ?? ((object)constructorDeclarationSyntax.Body) ?? ((object)constructorDeclarationSyntax.ExpressionBody));
	}

	internal virtual OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
	}

	internal virtual OneOrMany<SyntaxList<AttributeListSyntax>> GetReturnTypeAttributeDeclarations()
	{
		return GetAttributeDeclarations();
	}

	internal MethodEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (MethodEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	protected MethodWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (MethodWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal ReturnTypeWellKnownAttributeData GetDecodedReturnTypeWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyReturnTypeCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetReturnTypeAttributesBag();
		}
		return (ReturnTypeWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		return GetAttributesBag(ref _lazyCustomAttributesBag, forReturnType: false);
	}

	private CustomAttributesBag<CSharpAttributeData> GetReturnTypeAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyReturnTypeCustomAttributesBag = _lazyReturnTypeCustomAttributesBag;
		if (lazyReturnTypeCustomAttributesBag != null && lazyReturnTypeCustomAttributesBag.IsSealed)
		{
			return lazyReturnTypeCustomAttributesBag;
		}
		return GetAttributesBag(ref _lazyReturnTypeCustomAttributesBag, forReturnType: true);
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag(ref CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag, bool forReturnType)
	{
		SourceMemberMethodSymbol boundAttributesSource = BoundAttributesSource;
		bool flag;
		if ((object)boundAttributesSource != null)
		{
			CustomAttributesBag<CSharpAttributeData> value = (forReturnType ? boundAttributesSource.GetReturnTypeAttributesBag() : boundAttributesSource.GetAttributesBag());
			flag = Interlocked.CompareExchange(ref lazyCustomAttributesBag, value, null) == null;
		}
		else
		{
			AttributeLocation symbolPart;
			OneOrMany<SyntaxList<AttributeListSyntax>> attributesSyntaxLists;
			if (!forReturnType)
			{
				OneOrMany<SyntaxList<AttributeListSyntax>> attributeDeclarations = GetAttributeDeclarations();
				AttributeLocation attributeLocationForLoadAndValidateAttributes = AttributeLocationForLoadAndValidateAttributes;
				symbolPart = attributeLocationForLoadAndValidateAttributes;
				attributesSyntaxLists = attributeDeclarations;
			}
			else
			{
				OneOrMany<SyntaxList<AttributeListSyntax>> returnTypeAttributeDeclarations = GetReturnTypeAttributeDeclarations();
				symbolPart = AttributeLocation.Return;
				attributesSyntaxLists = returnTypeAttributeDeclarations;
			}
			flag = LoadAndValidateAttributes(attributesSyntaxLists, ref lazyCustomAttributesBag, symbolPart, earlyDecodingOnly: false, OuterBinder);
		}
		if (flag)
		{
			NoteAttributesComplete(forReturnType);
		}
		return lazyCustomAttributesBag;
	}

	protected abstract void NoteAttributesComplete(bool forReturnType);

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	public override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return GetReturnTypeAttributesBag().Attributes;
	}

	internal override (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (arguments.SymbolPart == AttributeLocation.None)
		{
			bool generatedDiagnostics;
			if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.ConditionalAttribute))
			{
				var (cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
				if (!cSharpAttributeData.HasErrors)
				{
					string constructorArgument = cSharpAttributeData.GetConstructorArgument<string>(0, SpecialType.System_String);
					arguments.GetOrCreateData<MethodEarlyWellKnownAttributeData>().AddConditionalSymbol(constructorArgument);
					if (!generatedDiagnostics)
					{
						return (cSharpAttributeData, item);
					}
				}
				return (null, null);
			}
			if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out CSharpAttributeData attributeData, out BoundAttribute boundAttribute, out ObsoleteAttributeData obsoleteData))
			{
				if (obsoleteData != null)
				{
					arguments.GetOrCreateData<MethodEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
				}
				return (attributeData, boundAttribute);
			}
			if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.UnmanagedCallersOnlyAttribute))
			{
				arguments.GetOrCreateData<MethodEarlyWellKnownAttributeData>().UnmanagedCallersOnlyAttributePresent = true;
				return (null, null);
			}
			if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.OverloadResolutionPriorityAttribute))
			{
				if (!base.CanHaveOverloadResolutionPriority)
				{
					return (null, null);
				}
				(CSharpAttributeData, BoundAttribute) attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
				attributeData = attribute.Item1;
				boundAttribute = attribute.Item2;
				ImmutableArray<TypedConstant> commonConstructorArguments = attributeData.CommonConstructorArguments;
				if (commonConstructorArguments.Length == 1 && commonConstructorArguments[0].ValueInternal is int overloadResolutionPriority)
				{
					arguments.GetOrCreateData<MethodEarlyWellKnownAttributeData>().OverloadResolutionPriority = overloadResolutionPriority;
					if (!generatedDiagnostics)
					{
						return (attributeData, boundAttribute);
					}
				}
				return (null, null);
			}
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	public ImmutableArray<(CSharpAttributeData, BoundAttribute)> BindMethodAttributes()
	{
		return BindAttributes(GetAttributeDeclarations(), OuterBinder);
	}

	internal sealed override UnmanagedCallersOnlyAttributeData? GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		if (syntaxReferenceOpt == null)
		{
			return null;
		}
		if (forceComplete)
		{
			GetAttributes();
		}
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag == null || !lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			return UnmanagedCallersOnlyAttributeData.Uninitialized;
		}
		if (lazyCustomAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			return ((MethodWellKnownAttributeData)lazyCustomAttributesBag.DecodedWellKnownAttributeData)?.UnmanagedCallersOnlyAttributeData;
		}
		MethodEarlyWellKnownAttributeData obj = (MethodEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData;
		if (obj == null || !obj.UnmanagedCallersOnlyAttributePresent)
		{
			return null;
		}
		return UnmanagedCallersOnlyAttributeData.AttributePresentDataNotBound;
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return GetEarlyDecodedWellKnownAttributeData()?.ConditionalSymbols ?? ImmutableArray<string>.Empty;
	}

	protected override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (arguments.SymbolPart == AttributeLocation.None)
		{
			DecodeWellKnownAttributeAppliedToMethod(ref arguments);
		}
		else
		{
			DecodeWellKnownAttributeAppliedToReturnValue(ref arguments);
		}
	}

	private void DecodeWellKnownAttributeAppliedToMethod(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		if (attribute.IsTargetAttribute(AttributeDescription.PreserveSigAttribute))
		{
			arguments.GetOrCreateData<MethodWellKnownAttributeData>().SetPreserveSignature(arguments.Index);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.MethodImplAttribute))
		{
			AttributeData.DecodeMethodImplAttribute<MethodWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>(ref arguments, MessageProvider.Instance, ContainingType);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.DllImportAttribute))
		{
			DecodeDllImportAttribute(ref arguments);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<MethodWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.ExcludeFromCodeCoverageAttribute))
		{
			arguments.GetOrCreateData<MethodWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.ConditionalAttribute))
		{
			ValidateConditionalAttribute(attribute, arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.SuppressUnmanagedCodeSecurityAttribute))
		{
			arguments.GetOrCreateData<MethodWellKnownAttributeData>().HasSuppressUnmanagedCodeSecurityAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.DynamicSecurityMethodAttribute))
		{
			arguments.GetOrCreateData<MethodWellKnownAttributeData>().HasDynamicSecurityMethodAttribute = true;
		}
		else
		{
			if (VerifyObsoleteAttributeAppliedToMethod(ref arguments, AttributeDescription.ObsoleteAttribute) || VerifyObsoleteAttributeAppliedToMethod(ref arguments, AttributeDescription.DeprecatedAttribute) || ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.IsReadOnlyAttribute | ReservedAttributes.IsUnmanagedAttribute | ReservedAttributes.IsByRefLikeAttribute | ReservedAttributes.NullableContextAttribute | ReservedAttributes.CaseSensitiveExtensionAttribute | ReservedAttributes.RequiresLocationAttribute | ReservedAttributes.ExtensionMarkerAttribute))
			{
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.SecurityCriticalAttribute) || attribute.IsTargetAttribute(AttributeDescription.SecuritySafeCriticalAttribute))
			{
				if (IsAsync)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_SecurityCriticalOrSecuritySafeCriticalOnAsync, arguments.AttributeSyntaxOpt.Location, arguments.AttributeSyntaxOpt.GetErrorDisplayName());
				}
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.SkipLocalsInitAttribute))
			{
				CSharpAttributeData.DecodeSkipLocalsInitAttribute<MethodWellKnownAttributeData>(DeclaringCompilation, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.DoesNotReturnAttribute))
			{
				arguments.GetOrCreateData<MethodWellKnownAttributeData>().HasDoesNotReturnAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.MemberNotNullAttribute))
			{
				MessageID.IDS_FeatureMemberNotNull.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				CSharpAttributeData.DecodeMemberNotNullAttribute<MethodWellKnownAttributeData>(ContainingType, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.MemberNotNullWhenAttribute))
			{
				MessageID.IDS_FeatureMemberNotNull.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				CSharpAttributeData.DecodeMemberNotNullWhenAttribute<MethodWellKnownAttributeData>(ContainingType, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.ModuleInitializerAttribute))
			{
				MessageID.IDS_FeatureModuleInitializers.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				DecodeModuleInitializerAttribute(arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.UnmanagedCallersOnlyAttribute))
			{
				DecodeUnmanagedCallersOnlyAttribute(ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.UnscopedRefAttribute))
			{
				if (!UseUpdatedEscapeRules)
				{
					bindingDiagnosticBag.Add(ErrorCode.WRN_UnscopedRefAttributeOldRules, arguments.AttributeSyntaxOpt.Location);
				}
				if (this.IsValidUnscopedRefAttributeTarget())
				{
					arguments.GetOrCreateData<MethodWellKnownAttributeData>().HasUnscopedRefAttribute = true;
					if (ContainingType.IsInterface || IsExplicitInterfaceImplementation)
					{
						MessageID.IDS_FeatureRefStructInterfaces.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
					}
				}
				else
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, arguments.AttributeSyntaxOpt.Location);
				}
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.InterceptsLocationAttribute))
			{
				DecodeInterceptsLocationAttribute(arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.OverloadResolutionPriorityAttribute))
			{
				MessageID.IDS_FeatureOverloadResolutionPriority.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				if (!base.CanHaveOverloadResolutionPriority)
				{
					bindingDiagnosticBag.Add(IsOverride ? ErrorCode.ERR_CannotApplyOverloadResolutionPriorityToOverride : ErrorCode.ERR_CannotApplyOverloadResolutionPriorityToMember, arguments.AttributeSyntaxOpt);
				}
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.RuntimeAsyncMethodGenerationAttribute))
			{
				arguments.GetOrCreateData<MethodWellKnownAttributeData>().RuntimeAsyncMethodGenerationSetting = ((!attribute.CommonConstructorArguments[0].DecodeValue<bool>(SpecialType.System_Boolean)) ? ThreeState.False : ThreeState.True);
			}
			else
			{
				CSharpCompilation declaringCompilation = DeclaringCompilation;
				if (attribute.IsSecurityAttribute(declaringCompilation))
				{
					attribute.DecodeSecurityAttribute<MethodWellKnownAttributeData>(this, declaringCompilation, ref arguments);
				}
			}
		}
	}

	private static FlowAnalysisAnnotations DecodeFlowAnalysisAttributes(MethodWellKnownAttributeData attributeData)
	{
		if (attributeData == null || !attributeData.HasDoesNotReturnAttribute)
		{
			return FlowAnalysisAnnotations.None;
		}
		return FlowAnalysisAnnotations.DoesNotReturn;
	}

	private bool VerifyObsoleteAttributeAppliedToMethod(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments, AttributeDescription description)
	{
		if (arguments.Attribute.IsTargetAttribute(description))
		{
			if (this.IsAccessor())
			{
				BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
				if (this is SourceEventAccessorSymbol)
				{
					AttributeUsageInfo attributeUsageInfo = arguments.Attribute.AttributeClass.GetAttributeUsageInfo();
					bindingDiagnosticBag.Add(ErrorCode.ERR_AttributeNotOnEventAccessor, arguments.AttributeSyntaxOpt.Name.Location, description.FullName, attributeUsageInfo.GetValidTargetsErrorArgument());
				}
				else
				{
					MessageID.IDS_FeatureObsoleteOnPropertyAccessor.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				}
			}
			return true;
		}
		return false;
	}

	private void ValidateConditionalAttribute(CSharpAttributeData attribute, AttributeSyntax node, BindingDiagnosticBag diagnostics)
	{
		if (this.IsAccessor())
		{
			AttributeUsageInfo attributeUsageInfo = attribute.AttributeClass.GetAttributeUsageInfo();
			diagnostics.Add(ErrorCode.ERR_AttributeNotOnAccessor, node.Name.Location, node.GetErrorDisplayName(), attributeUsageInfo.GetValidTargetsErrorArgument());
			return;
		}
		if (ContainingType.IsInterfaceType())
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnInterfaceMethod, node.Location);
			return;
		}
		if (IsOverride)
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnOverride, node.Location, this);
			return;
		}
		if (!base.CanBeReferencedByName || MethodKind == MethodKind.Destructor)
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnSpecialMethod, node.Location, this);
			return;
		}
		if (!ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalMustReturnVoid, node.Location, this);
			return;
		}
		if (HasAnyOutParameter())
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalWithOutParam, node.Location, this);
			return;
		}
		if ((object)this != null && MethodKind == MethodKind.LocalFunction && !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnLocalFunction, node.Location, this);
			return;
		}
		string constructorArgument = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
		if (constructorArgument == null || !SyntaxFacts.IsValidIdentifier(constructorArgument))
		{
			diagnostics.Add(ErrorCode.ERR_BadArgumentToAttribute, attribute.GetAttributeArgumentLocation(0), node.GetErrorDisplayName());
		}
	}

	private bool HasAnyOutParameter()
	{
		foreach (ParameterSymbol parameter in Parameters)
		{
			if (parameter.RefKind == RefKind.Out)
			{
				return true;
			}
		}
		return false;
	}

	private void DecodeWellKnownAttributeAppliedToReturnValue(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		_ = (BindingDiagnosticBag)arguments.Diagnostics;
		if (attribute.IsTargetAttribute(AttributeDescription.MarshalAsAttribute))
		{
			MarshalAsAttributeDecoder<ReturnTypeWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>.Decode(ref arguments, AttributeTargets.ReturnValue, MessageProvider.Instance);
		}
		else if (!ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.DynamicAttribute | ReservedAttributes.IsReadOnlyAttribute | ReservedAttributes.IsUnmanagedAttribute | ReservedAttributes.IsByRefLikeAttribute | ReservedAttributes.TupleElementNamesAttribute | ReservedAttributes.NullableAttribute | ReservedAttributes.NativeIntegerAttribute | ReservedAttributes.RequiresLocationAttribute | ReservedAttributes.ExtensionMarkerAttribute))
		{
			if (attribute.IsTargetAttribute(AttributeDescription.MaybeNullAttribute))
			{
				arguments.GetOrCreateData<ReturnTypeWellKnownAttributeData>().HasMaybeNullAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.NotNullAttribute))
			{
				arguments.GetOrCreateData<ReturnTypeWellKnownAttributeData>().HasNotNullAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.NotNullIfNotNullAttribute))
			{
				arguments.GetOrCreateData<ReturnTypeWellKnownAttributeData>().AddNotNullIfParameterNotNull(attribute.DecodeNotNullIfNotNullAttribute());
			}
		}
	}

	private void DecodeDllImportAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		bool flag = false;
		MethodSymbol methodSymbol = PartialImplementationPart ?? this;
		if (!methodSymbol.IsExtern || (!methodSymbol.IsStatic && !methodSymbol.IsExtensionBlockMember()))
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_DllImportOnInvalidMethod, arguments.AttributeSyntaxOpt.Name.Location);
			flag = true;
		}
		bool flag2 = false;
		MethodSymbol methodSymbol2 = this;
		while ((object)methodSymbol2 != null)
		{
			if (methodSymbol2.IsGenericMethod)
			{
				flag2 = true;
				break;
			}
			methodSymbol2 = methodSymbol2.ContainingSymbol as MethodSymbol;
		}
		if (!flag2)
		{
			NamedTypeSymbol containingType = ContainingType;
			if ((object)containingType == null || !containingType.IsGenericType)
			{
				goto IL_00b5;
			}
		}
		bindingDiagnosticBag.Add(ErrorCode.ERR_DllImportOnGenericMethod, arguments.AttributeSyntaxOpt.Name.Location);
		flag = true;
		goto IL_00b5;
		IL_00b5:
		string text = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
		if (!MetadataHelpers.IsValidMetadataIdentifier(text))
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_InvalidAttributeArgument, attribute.GetAttributeArgumentLocation(0), arguments.AttributeSyntaxOpt.GetErrorDisplayName());
			flag = true;
			text = null;
		}
		CharSet charSet = GetEffectiveDefaultMarshallingCharSet() ?? CharSet.None;
		string text2 = null;
		bool preserveSig = true;
		System.Runtime.InteropServices.CallingConvention callingConvention = System.Runtime.InteropServices.CallingConvention.Winapi;
		bool setLastError = false;
		bool exactSpelling = false;
		bool? useBestFit = null;
		bool? throwOnUnmappable = null;
		int num = 1;
		foreach (KeyValuePair<string, TypedConstant> commonNamedArgument in attribute.CommonNamedArguments)
		{
			switch (commonNamedArgument.Key)
			{
			case "EntryPoint":
				text2 = commonNamedArgument.Value.ValueInternal as string;
				if (!MetadataHelpers.IsValidMetadataIdentifier(text2))
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_InvalidNamedArgument, arguments.AttributeSyntaxOpt.ArgumentList.Arguments[num].Location, commonNamedArgument.Key);
					flag = true;
					text2 = null;
				}
				break;
			case "CharSet":
				charSet = commonNamedArgument.Value.DecodeValue<CharSet>(SpecialType.System_Enum);
				break;
			case "SetLastError":
				setLastError = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "ExactSpelling":
				exactSpelling = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "PreserveSig":
				preserveSig = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "CallingConvention":
				callingConvention = commonNamedArgument.Value.DecodeValue<System.Runtime.InteropServices.CallingConvention>(SpecialType.System_Enum);
				break;
			case "BestFitMapping":
				useBestFit = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "ThrowOnUnmappableChar":
				throwOnUnmappable = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			}
			num++;
		}
		if (!flag)
		{
			arguments.GetOrCreateData<MethodWellKnownAttributeData>().SetDllImport(arguments.Index, text, text2 ?? Name, DllImportData.MakeFlags(exactSpelling, charSet, setLastError, callingConvention, useBestFit, throwOnUnmappable), preserveSig);
		}
	}

	private void DecodeModuleInitializerAttribute(DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		if (MethodKind != MethodKind.Ordinary || this.IsExtensionBlockMember())
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ModuleInitializerMethodMustBeOrdinary, arguments.AttributeSyntaxOpt.Location);
			return;
		}
		bool flag = false;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(bindingDiagnosticBag, ContainingAssembly);
		if (!AccessCheck.IsSymbolAccessible(this, ContainingAssembly, ref useSiteInfo))
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ModuleInitializerMethodMustBeAccessibleOutsideTopLevelType, arguments.AttributeSyntaxOpt.Location, Name);
			flag = true;
		}
		bindingDiagnosticBag.Add(arguments.AttributeSyntaxOpt, useSiteInfo);
		if (!IsStatic || ParameterCount > 0 || !ReturnsVoid || IsAbstract || IsVirtual)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ModuleInitializerMethodMustBeStaticParameterlessVoid, arguments.AttributeSyntaxOpt.Location, Name);
			flag = true;
		}
		if (IsGenericMethod || ContainingType.IsGenericType)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ModuleInitializerMethodAndContainingTypesMustNotBeGeneric, arguments.AttributeSyntaxOpt.Location, Name);
			flag = true;
		}
		if (_lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData is MethodEarlyWellKnownAttributeData { UnmanagedCallersOnlyAttributePresent: not false })
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ModuleInitializerCannotBeUnmanagedCallersOnly, arguments.AttributeSyntaxOpt.Location);
			flag = true;
		}
		if (!flag && !CallsAreOmitted(arguments.AttributeSyntaxOpt.SyntaxTree))
		{
			DeclaringCompilation.AddModuleInitializerMethod(this);
		}
	}

	private void DecodeInterceptsLocationAttribute(DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		ImmutableArray<TypedConstant> commonConstructorArguments = arguments.Attribute.CommonConstructorArguments;
		if (commonConstructorArguments.Length == 3)
		{
			ITypeSymbol type = commonConstructorArguments[0].Type;
			if (type != null && type.SpecialType == SpecialType.System_String)
			{
				TypedConstant typedConstant = commonConstructorArguments[1];
				if (typedConstant.Kind != TypedConstantKind.Array && typedConstant.Value is int lineNumberOneBased)
				{
					TypedConstant typedConstant2 = commonConstructorArguments[2];
					if (typedConstant2.Kind != TypedConstantKind.Array && typedConstant2.Value is int characterNumberOneBased)
					{
						DecodeInterceptsLocationAttributeExperimentalCompat(arguments, (string)commonConstructorArguments[0].Value, lineNumberOneBased, characterNumberOneBased);
						return;
					}
				}
			}
		}
		DecodeInterceptsLocationChecksumBased(arguments, (int)commonConstructorArguments[0].Value, (string)commonConstructorArguments[1].Value);
	}

	private void DecodeInterceptsLocationChecksumBased(DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments, int version, string? data)
	{
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		NameSyntax name = arguments.AttributeSyntaxOpt.Name;
		Location location = name.Location;
		if (version != 1)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptsLocationUnsupportedVersion, location, version);
			return;
		}
		(ReadOnlyMemory<byte>, int, string)? tuple = InterceptableLocation1.Decode(data);
		int item2;
		string item3;
		SyntaxTree syntaxTree;
		SyntaxToken syntaxToken;
		if (tuple.HasValue)
		{
			(ReadOnlyMemory<byte>, int, string) valueOrDefault = tuple.GetValueOrDefault();
			ReadOnlyMemory<byte> item = valueOrDefault.Item1;
			item2 = valueOrDefault.Item2;
			item3 = valueOrDefault.Item3;
			ImmutableArray<ImmutableArray<string>> interceptorsNamespaces = ((CSharpParseOptions)name.SyntaxTree.Options).InterceptorsNamespaces;
			ArrayBuilder<string> arrayBuilder = getNamespaceNames(this);
			if (!interceptorsNamespaces.Any<ImmutableArray<string>, ArrayBuilder<string>>((ImmutableArray<string> ns, ArrayBuilder<string> thisNamespaceNames) => isDeclaredInNamespace(thisNamespaceNames, ns), arrayBuilder))
			{
				reportFeatureNotEnabled(bindingDiagnosticBag, location, arrayBuilder);
				arrayBuilder.Free();
				return;
			}
			arrayBuilder.Free();
			if (ReportBadInterceptsLocation(bindingDiagnosticBag, location))
			{
				return;
			}
			OneOrMany<SyntaxTree> syntaxTreesByContentHash = DeclaringCompilation.GetSyntaxTreesByContentHash(item);
			if (syntaxTreesByContentHash.Count > 1)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptsLocationDuplicateFile, location, item3);
				return;
			}
			if (syntaxTreesByContentHash.Count == 0)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptsLocationFileNotFound, location, item3);
				return;
			}
			syntaxTree = syntaxTreesByContentHash[0];
			SyntaxNode root = syntaxTree.GetRoot();
			if (item2 < 0 || item2 > root.EndPosition)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptsLocationDataInvalidPosition, location, item3);
				return;
			}
			_ = syntaxTree.GetText().Lines.Count;
			syntaxToken = root.FindToken(item2);
			SyntaxToken syntaxToken2 = syntaxToken;
			if (syntaxToken2.Parent is SimpleNameSyntax simpleNameSyntax)
			{
				CSharpSyntaxNode parent = simpleNameSyntax.Parent;
				if (parent is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
				{
					if (parent.Parent is InvocationExpressionSyntax && memberAccessExpressionSyntax.Name == simpleNameSyntax)
					{
						goto IL_02a2;
					}
					SimpleNameSyntax simpleNameSyntax2 = simpleNameSyntax;
					if (memberAccessExpressionSyntax.Name != simpleNameSyntax2)
					{
						goto IL_0284;
					}
				}
				else if (parent is MemberBindingExpressionSyntax memberBindingExpressionSyntax)
				{
					if (parent.Parent is InvocationExpressionSyntax)
					{
						SimpleNameSyntax simpleNameSyntax3 = simpleNameSyntax;
						if (memberBindingExpressionSyntax.Name == simpleNameSyntax3)
						{
							goto IL_02a2;
						}
					}
					SimpleNameSyntax simpleNameSyntax4 = simpleNameSyntax;
					if (memberBindingExpressionSyntax.Name != simpleNameSyntax4)
					{
						goto IL_0284;
					}
				}
				else if (parent is InvocationExpressionSyntax invocationExpressionSyntax)
				{
					SimpleNameSyntax simpleNameSyntax5 = simpleNameSyntax;
					if (invocationExpressionSyntax.Expression == simpleNameSyntax5)
					{
						goto IL_02a2;
					}
				}
				bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorNameNotInvoked, location, syntaxToken.Text);
				return;
			}
			goto IL_0284;
		}
		bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptsLocationDataInvalidFormat, location);
		return;
		IL_02a2:
		if (item2 != syntaxToken.Position)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptsLocationDataInvalidPosition, location, item3);
		}
		else
		{
			DeclaringCompilation.AddInterception(syntaxTree.GetText().GetContentHash(), item2, location, this);
		}
		return;
		IL_0284:
		bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorPositionBadToken, location, syntaxToken.Text);
		static ArrayBuilder<string> getNamespaceNames(SourceMethodSymbol @this)
		{
			ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
			NamespaceSymbol containingNamespace = @this.ContainingNamespace;
			while ((object)containingNamespace != null && !containingNamespace.IsGlobalNamespace)
			{
				instance.Add(containingNamespace.Name);
				containingNamespace = containingNamespace.ContainingNamespace;
			}
			instance.ReverseContents();
			return instance;
		}
		static bool isDeclaredInNamespace(ArrayBuilder<string> thisNamespaceNames, ImmutableArray<string> namespaceSegments)
		{
			if (namespaceSegments.Length == 1 && namespaceSegments[0] == "global")
			{
				return true;
			}
			if (namespaceSegments.Length > thisNamespaceNames.Count)
			{
				return false;
			}
			for (int i = 0; i < namespaceSegments.Length; i++)
			{
				if (namespaceSegments[i] != thisNamespaceNames[i])
				{
					return false;
				}
			}
			return true;
		}
		static void reportFeatureNotEnabled(BindingDiagnosticBag diagnostics, Location attributeLocation, ArrayBuilder<string> namespaceNames)
		{
			if (namespaceNames.Count == 0)
			{
				diagnostics.Add(ErrorCode.ERR_InterceptorGlobalNamespace, attributeLocation);
			}
			else
			{
				string text = "<InterceptorsNamespaces>$(InterceptorsNamespaces);" + string.Join(".", namespaceNames) + "</InterceptorsNamespaces>";
				diagnostics.Add(ErrorCode.ERR_InterceptorsFeatureNotEnabled, attributeLocation, text);
			}
		}
	}

	private void DecodeInterceptsLocationAttributeExperimentalCompat(DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments, string? attributeFilePath, int lineNumberOneBased, int characterNumberOneBased)
	{
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		AttributeSyntax attributeSyntaxOpt = arguments.AttributeSyntaxOpt;
		Location location = attributeSyntaxOpt.Location;
		bindingDiagnosticBag.Add(ErrorCode.WRN_InterceptsLocationAttributeUnsupportedSignature, location);
		ImmutableArray<ImmutableArray<string>> interceptorsNamespaces = ((CSharpParseOptions)attributeSyntaxOpt.SyntaxTree.Options).InterceptorsNamespaces;
		ArrayBuilder<string> thisNamespaceNames = getNamespaceNames();
		if (!interceptorsNamespaces.Any<ImmutableArray<string>>((ImmutableArray<string> ns) => isDeclaredInNamespace(thisNamespaceNames, ns)))
		{
			reportFeatureNotEnabled(bindingDiagnosticBag, attributeSyntaxOpt, thisNamespaceNames);
			thisNamespaceNames.Free();
			return;
		}
		thisNamespaceNames.Free();
		CSharpAttributeData attribute = arguments.Attribute;
		if (attributeFilePath == null)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorFilePathCannotBeNull, attribute.GetAttributeArgumentLocation(0));
			return;
		}
		if (ReportBadInterceptsLocation(bindingDiagnosticBag, location))
		{
			return;
		}
		string normalizedPathOrOriginalPath = FileUtilities.GetNormalizedPathOrOriginalPath(attributeFilePath, SyntaxTree.FilePath);
		OneOrMany<SyntaxTree> oneOrMany = DeclaringCompilation.GetSyntaxTreesByPath(normalizedPathOrOriginalPath);
		if (oneOrMany.Count > 1)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorNonUniquePath, attribute.GetAttributeArgumentLocation(0), normalizedPathOrOriginalPath);
			return;
		}
		if (oneOrMany.Count == 0)
		{
			oneOrMany = DeclaringCompilation.GetSyntaxTreesByMappedPath(attributeFilePath);
			if (oneOrMany.Count > 1)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorNonUniquePath, attribute.GetAttributeArgumentLocation(0), attributeFilePath);
				return;
			}
		}
		if (oneOrMany.Count == 0)
		{
			SyntaxTree syntaxTree = DeclaringCompilation.SyntaxTrees.FirstOrDefault((SyntaxTree tree, string attributeFilePathWithForwardSlashes) => tree.FilePath.Replace('\\', '/').EndsWith(attributeFilePathWithForwardSlashes), attributeFilePath.Replace('\\', '/'));
			if (syntaxTree != null)
			{
				string text = (PathUtilities.IsAbsolute(SyntaxTree.FilePath) ? PathUtilities.GetRelativePath(PathUtilities.GetDirectoryName(SyntaxTree.FilePath), syntaxTree.FilePath) : syntaxTree.FilePath);
				bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorPathNotInCompilationWithCandidate, attribute.GetAttributeArgumentLocation(0), attributeFilePath, text);
			}
			else
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorPathNotInCompilation, attribute.GetAttributeArgumentLocation(0), normalizedPathOrOriginalPath);
			}
			return;
		}
		SyntaxTree syntaxTree2 = oneOrMany[0];
		int num = lineNumberOneBased - 1;
		int num2 = characterNumberOneBased - 1;
		if (num < 0 || num2 < 0)
		{
			Location attributeArgumentLocation = attribute.GetAttributeArgumentLocation((num < 0) ? 1 : 2);
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorLineCharacterMustBePositive, attributeArgumentLocation);
			return;
		}
		TextLineCollection lines = syntaxTree2.GetText().Lines;
		int count = lines.Count;
		if (num >= count)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorLineOutOfRange, attribute.GetAttributeArgumentLocation(1), count, lineNumberOneBased);
			return;
		}
		TextLine textLine = lines[num];
		int num3 = textLine.End - textLine.Start;
		if (num2 >= num3)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorCharacterOutOfRange, attribute.GetAttributeArgumentLocation(2), num3, characterNumberOneBased);
			return;
		}
		int num4 = textLine.Start + num2;
		SyntaxToken syntaxToken = syntaxTree2.GetRoot().FindToken(num4);
		SyntaxToken syntaxToken2 = syntaxToken;
		if (syntaxToken2.Parent is SimpleNameSyntax simpleNameSyntax)
		{
			CSharpSyntaxNode parent = simpleNameSyntax.Parent;
			if (parent is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
			{
				if (parent.Parent is InvocationExpressionSyntax && memberAccessExpressionSyntax.Name == simpleNameSyntax)
				{
					goto IL_03d2;
				}
				SimpleNameSyntax simpleNameSyntax2 = simpleNameSyntax;
				if (memberAccessExpressionSyntax.Name != simpleNameSyntax2)
				{
					goto IL_03b4;
				}
			}
			else if (parent is InvocationExpressionSyntax invocationExpressionSyntax)
			{
				SimpleNameSyntax simpleNameSyntax3 = simpleNameSyntax;
				if (invocationExpressionSyntax.Expression == simpleNameSyntax3)
				{
					goto IL_03d2;
				}
			}
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorNameNotInvoked, location, syntaxToken.Text);
			return;
		}
		goto IL_03b4;
		IL_03b4:
		bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorPositionBadToken, location, syntaxToken.Text);
		return;
		IL_03d2:
		if (num4 != syntaxToken.SpanStart)
		{
			LinePosition startLinePosition = syntaxToken.GetLocation().GetLineSpan().StartLinePosition;
			bindingDiagnosticBag.Add(ErrorCode.ERR_InterceptorMustReferToStartOfTokenPosition, location, syntaxToken.Text, startLinePosition.Line + 1, startLinePosition.Character + 1);
		}
		else
		{
			DeclaringCompilation.AddInterception(syntaxTree2.GetText().GetContentHash(), syntaxToken.Position, location, this);
		}
		ArrayBuilder<string> getNamespaceNames()
		{
			ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
			NamespaceSymbol containingNamespace = ContainingNamespace;
			while ((object)containingNamespace != null && !containingNamespace.IsGlobalNamespace)
			{
				instance.Add(containingNamespace.Name);
				containingNamespace = containingNamespace.ContainingNamespace;
			}
			instance.ReverseContents();
			return instance;
		}
		static bool isDeclaredInNamespace(ArrayBuilder<string> arrayBuilder, ImmutableArray<string> namespaceSegments)
		{
			if (namespaceSegments.Length == 1 && namespaceSegments[0] == "global")
			{
				return true;
			}
			if (namespaceSegments.Length > arrayBuilder.Count)
			{
				return false;
			}
			for (int i = 0; i < namespaceSegments.Length; i++)
			{
				if (namespaceSegments[i] != arrayBuilder[i])
				{
					return false;
				}
			}
			return true;
		}
		static void reportFeatureNotEnabled(BindingDiagnosticBag diagnostics, AttributeSyntax attributeSyntax, ArrayBuilder<string> namespaceNames)
		{
			if (namespaceNames.Count == 0)
			{
				diagnostics.Add(ErrorCode.ERR_InterceptorGlobalNamespace, attributeSyntax);
			}
			else
			{
				string text2 = "<InterceptorsNamespaces>$(InterceptorsNamespaces);" + string.Join(".", namespaceNames) + "</InterceptorsNamespaces>";
				diagnostics.Add(ErrorCode.ERR_InterceptorsFeatureNotEnabled, attributeSyntax, text2);
			}
		}
	}

	private bool ReportBadInterceptsLocation(BindingDiagnosticBag diagnostics, Location attributeLocation)
	{
		if (!this.IsExtensionBlockMember() && ContainingType.IsGenericType)
		{
			diagnostics.Add(ErrorCode.ERR_InterceptorContainingTypeCannotBeGeneric, attributeLocation, this);
			return true;
		}
		if (MethodKind != MethodKind.Ordinary)
		{
			diagnostics.Add(ErrorCode.ERR_InterceptorMethodMustBeOrdinary, attributeLocation);
			return true;
		}
		if (GetUnmanagedCallersOnlyAttributeData(forceComplete: false) != null)
		{
			diagnostics.Add(ErrorCode.ERR_InterceptorCannotUseUnmanagedCallersOnly, attributeLocation);
			return true;
		}
		return false;
	}

	private void DecodeUnmanagedCallersOnlyAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		BindingDiagnosticBag diagnostics = (BindingDiagnosticBag)arguments.Diagnostics;
		arguments.GetOrCreateData<MethodWellKnownAttributeData>().UnmanagedCallersOnlyAttributeData = DecodeUnmanagedCallersOnlyAttributeData(this, arguments.Attribute, arguments.AttributeSyntaxOpt.Location, diagnostics);
		CheckAndReportValidUnmanagedCallersOnlyTarget(arguments.AttributeSyntaxOpt.Name, diagnostics);
		CSharpSyntaxNode cSharpSyntaxNode = this.ExtractReturnTypeSyntax();
		if (cSharpSyntaxNode != CSharpSyntaxTree.Dummy.GetRoot())
		{
			checkAndReportManagedTypes(base.ReturnType, RefKind, cSharpSyntaxNode, isParam: false, diagnostics);
			foreach (ParameterSymbol parameter in Parameters)
			{
				checkAndReportManagedTypes(parameter.Type, parameter.RefKind, parameter.GetNonNullSyntaxNode(), isParam: true, diagnostics);
			}
		}
		static UnmanagedCallersOnlyAttributeData DecodeUnmanagedCallersOnlyAttributeData(SourceMethodSymbol @this, CSharpAttributeData attribute, Location location, BindingDiagnosticBag diagnostics2)
		{
			ImmutableHashSet<INamedTypeSymbolInternal> callingConventionTypes = null;
			if (!attribute.CommonNamedArguments.IsDefaultOrEmpty)
			{
				NamedTypeSymbol wellKnownType = @this.DeclaringCompilation.GetWellKnownType(WellKnownType.System_Type);
				foreach (var (text2, value) in attribute.CommonNamedArguments)
				{
					bool isField = attribute.AttributeClass.GetMembers(text2).Any((Symbol m, NamedTypeSymbol systemType) => m is FieldSymbol { Type: ArrayTypeSymbol { ElementType: NamedTypeSymbol elementType } } && elementType.Equals(systemType, TypeCompareKind.ConsiderEverything), wellKnownType);
					(bool, ImmutableHashSet<INamedTypeSymbolInternal>) tuple = MethodSymbol.TryDecodeUnmanagedCallersOnlyCallConvsField(text2, value, isField, location, diagnostics2);
					if (tuple.Item1)
					{
						callingConventionTypes = tuple.Item2;
					}
				}
			}
			return UnmanagedCallersOnlyAttributeData.Create(callingConventionTypes);
		}
		static void checkAndReportManagedTypes(TypeSymbol type, RefKind refKind, SyntaxNode syntax, bool isParam, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (refKind != RefKind.None)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_CannotUseRefInUnmanagedCallersOnly, syntax.Location);
			}
			switch (type.ManagedKindNoUseSiteDiagnostics)
			{
			case ManagedKind.Unmanaged:
			case ManagedKind.UnmanagedWithGenerics:
				break;
			case ManagedKind.Managed:
				bindingDiagnosticBag.Add(ErrorCode.ERR_CannotUseManagedTypeInUnmanagedCallersOnly, syntax.Location, type, (isParam ? MessageID.IDS_Parameter : MessageID.IDS_Return).Localize());
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(type.ManagedKindNoUseSiteDiagnostics);
			}
		}
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		if (symbolPart != AttributeLocation.Return)
		{
			if ((ContainingSymbol is NamedTypeSymbol { IsComImport: not false, TypeKind: var typeKind } && (typeKind == TypeKind.Class || typeKind == TypeKind.Interface)) ? true : false)
			{
				MethodKind methodKind = MethodKind;
				if (methodKind == MethodKind.Constructor || methodKind == MethodKind.StaticConstructor)
				{
					if (!IsImplicitlyDeclared)
					{
						diagnostics.Add(ErrorCode.ERR_ComImportWithUserCtor, GetFirstLocation());
					}
				}
				else if (!IsAbstract && !IsExtern)
				{
					diagnostics.Add(ErrorCode.ERR_ComImportWithImpl, GetFirstLocation(), this, ContainingType);
				}
			}
			if (IsExtern && !IsAbstract && !this.IsPartialMember() && GetInMethodSyntaxNode() == null && boundAttributes.IsEmpty && !ContainingType.IsComImport)
			{
				ErrorCode code = ((MethodKind == MethodKind.Constructor || MethodKind == MethodKind.StaticConstructor) ? ErrorCode.WRN_ExternCtorNoImplementation : ErrorCode.WRN_ExternMethodNoImplementation);
				diagnostics.Add(code, GetFirstLocation(), this);
			}
		}
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	protected void AsyncMethodChecks(BindingDiagnosticBag diagnostics)
	{
		AsyncMethodChecks(verifyReturnType: true, GetFirstLocation(), diagnostics);
	}

	protected void AsyncMethodChecks(bool verifyReturnType, Location errorLocation, BindingDiagnosticBag diagnostics)
	{
		if (!IsAsync)
		{
			return;
		}
		bool flag = false;
		TypeSyntax returnTypeSyntax;
		if (verifyReturnType)
		{
			if (RefKind != RefKind.None)
			{
				CSharpSyntaxNode syntaxNode = SyntaxNode;
				if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
				{
					TypeSyntax returnType = methodDeclarationSyntax.ReturnType;
					returnTypeSyntax = returnType;
				}
				else
				{
					if (!(syntaxNode is LocalFunctionStatementSyntax localFunctionStatementSyntax))
					{
						if (syntaxNode is ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax)
						{
							TypeSyntax returnType2 = parenthesizedLambdaExpressionSyntax.ReturnType;
							if (returnType2 != null)
							{
								returnTypeSyntax = returnType2;
								goto IL_0085;
							}
						}
						throw ExceptionUtilities.UnexpectedValue(syntaxNode);
					}
					TypeSyntax returnType3 = localFunctionStatementSyntax.ReturnType;
					returnTypeSyntax = returnType3;
				}
				goto IL_0085;
			}
			if (isBadAsyncReturn(this))
			{
				diagnostics.Add(ErrorCode.ERR_BadAsyncReturn, errorLocation);
				flag = true;
			}
		}
		goto IL_00a8;
		IL_0085:
		ReportBadRefToken(returnTypeSyntax, diagnostics);
		flag = true;
		goto IL_00a8;
		IL_00a8:
		if (HasAsyncMethodBuilderAttribute(out TypeSymbol _))
		{
			MessageID.IDS_AsyncMethodBuilderOverride.CheckFeatureAvailability(diagnostics, DeclaringCompilation, errorLocation);
		}
		if (MethodKind != MethodKind.AnonymousFunction)
		{
			NamedTypeSymbol containingType = ContainingType;
			while ((object)containingType != null)
			{
				if (containingType is SourceNamedTypeSymbol { HasSecurityCriticalAttributes: not false })
				{
					diagnostics.Add(ErrorCode.ERR_SecurityCriticalOrSecuritySafeCriticalOnAsyncInClassOrStruct, errorLocation);
					flag = true;
					break;
				}
				containingType = containingType.ContainingType;
			}
		}
		if ((ImplementationAttributes & MethodImplAttributes.Synchronized) != MethodImplAttributes.IL)
		{
			diagnostics.Add(ErrorCode.ERR_SynchronizedAsyncMethod, errorLocation);
			flag = true;
		}
		if (!flag)
		{
			ReportAsyncParameterErrors(diagnostics, errorLocation);
		}
		NamedTypeSymbol wellKnownType = DeclaringCompilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IAsyncEnumerable_T);
		if (base.ReturnType.OriginalDefinition.Equals(wellKnownType) && GetInMethodSyntaxNode() != null)
		{
			NamedTypeSymbol wellKnownType2 = DeclaringCompilation.GetWellKnownType(WellKnownType.System_Threading_CancellationToken);
			int num = Parameters.Count((ParameterSymbol p) => p.HasEnumeratorCancellationAttribute);
			if (num == 0 && base.ParameterTypesWithAnnotations.Any((TypeWithAnnotations p, NamedTypeSymbol cancellationTokenType) => p.Type.Equals(cancellationTokenType), wellKnownType2))
			{
				diagnostics.Add(ErrorCode.WRN_UndecoratedCancellationTokenParameter, errorLocation, this);
			}
			if (num > 1)
			{
				diagnostics.Add(ErrorCode.ERR_MultipleEnumeratorCancellationAttributes, errorLocation);
			}
		}
		static bool isBadAsyncReturn(MethodSymbol methodSymbol)
		{
			TypeSymbol returnType4 = methodSymbol.ReturnType;
			CSharpCompilation declaringCompilation = methodSymbol.DeclaringCompilation;
			if (!returnType4.IsErrorType() && !returnType4.IsVoidType() && !returnType4.IsIAsyncEnumerableType(declaringCompilation) && !returnType4.IsIAsyncEnumeratorType(declaringCompilation) && !methodSymbol.IsAsyncEffectivelyReturningTask(declaringCompilation))
			{
				return !methodSymbol.IsAsyncEffectivelyReturningGenericTask(declaringCompilation);
			}
			return false;
		}
	}

	private static FlowAnalysisAnnotations DecodeReturnTypeAnnotationAttributes(ReturnTypeWellKnownAttributeData attributeData)
	{
		FlowAnalysisAnnotations flowAnalysisAnnotations = FlowAnalysisAnnotations.None;
		if (attributeData != null)
		{
			if (attributeData.HasMaybeNullAttribute)
			{
				flowAnalysisAnnotations |= FlowAnalysisAnnotations.MaybeNull;
			}
			if (attributeData.HasNotNullAttribute)
			{
				flowAnalysisAnnotations |= FlowAnalysisAnnotations.NotNull;
			}
		}
		return flowAnalysisAnnotations;
	}

	private bool IsVtableGapInterfaceMethod()
	{
		if (ContainingType.IsInterface)
		{
			return ModuleExtensions.GetVTableGapSize(MetadataName) > 0;
		}
		return false;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		CustomAttributesBag<CSharpAttributeData> attributesBag = GetAttributesBag();
		MethodWellKnownAttributeData methodWellKnownAttributeData = (MethodWellKnownAttributeData)attributesBag.DecodedWellKnownAttributeData;
		if (methodWellKnownAttributeData != null)
		{
			SecurityWellKnownAttributeData securityInformation = methodWellKnownAttributeData.SecurityInformation;
			if (securityInformation != null)
			{
				return securityInformation.GetSecurityAttributes(attributesBag.Attributes);
			}
		}
		return SpecializedCollections.EmptyEnumerable<SecurityAttribute>();
	}

	public override DllImportData? GetDllImportData()
	{
		return GetDecodedWellKnownAttributeData()?.DllImportPlatformInvokeData;
	}

	protected void AddAsyncImplAttributeIfNeeded(ref MethodImplAttributes result)
	{
		if (IsAsync && DeclaringCompilation.IsRuntimeAsyncEnabledIn(this))
		{
			result |= MethodImplAttributeExtensions.get_Async();
		}
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		return GetEarlyDecodedWellKnownAttributeData()?.OverloadResolutionPriority ?? 0;
	}
}
