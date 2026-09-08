using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceEventSymbol : EventSymbol, IAttributeTargetSymbol
{
	private SourceEventSymbol? _otherPartOfPartial;

	private readonly Location _location;

	private readonly SyntaxReference _syntaxRef;

	private readonly DeclarationModifiers _modifiers;

	private readonly bool _hasExplicitAccessModifier;

	internal readonly SourceMemberContainerTypeSymbol containingType;

	private SymbolCompletionState _state;

	private CustomAttributesBag<CSharpAttributeData>? _lazyCustomAttributesBag;

	private string? _lazyDocComment;

	private string? _lazyExpandedDocComment;

	private OverriddenOrHiddenMembersResult? _lazyOverriddenOrHiddenMembers;

	private ThreeState _lazyIsWindowsRuntimeEvent;

	public Location Location => _location;

	internal sealed override bool RequiresCompletion => true;

	public abstract override string Name { get; }

	public abstract override MethodSymbol? AddMethod { get; }

	public abstract override MethodSymbol? RemoveMethod { get; }

	public abstract override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations { get; }

	public abstract override TypeWithAnnotations TypeWithAnnotations { get; }

	public sealed override Symbol ContainingSymbol => containingType;

	public override NamedTypeSymbol ContainingType => containingType;

	public sealed override ImmutableArray<Location> Locations => ImmutableArray.Create(_location);

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_syntaxRef);

	internal SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (containingType.AnyMemberHasAttributes)
			{
				MemberDeclarationSyntax memberSyntax = MemberSyntax;
				if (memberSyntax != null)
				{
					return memberSyntax.AttributeLists;
				}
			}
			return default(SyntaxList<AttributeListSyntax>);
		}
	}

	internal MemberDeclarationSyntax? MemberSyntax
	{
		get
		{
			CSharpSyntaxNode cSharpSyntaxNode = CSharpSyntaxNode;
			if (cSharpSyntaxNode != null)
			{
				return cSharpSyntaxNode.Kind() switch
				{
					SyntaxKind.EventDeclaration => (EventDeclarationSyntax)cSharpSyntaxNode, 
					SyntaxKind.VariableDeclarator => (EventFieldDeclarationSyntax)cSharpSyntaxNode.Parent.Parent, 
					_ => throw ExceptionUtilities.UnexpectedValue(cSharpSyntaxNode.Kind()), 
				};
			}
			return null;
		}
	}

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => this;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Event;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations => AllowedAttributeLocations;

	protected abstract AttributeLocation AllowedAttributeLocations { get; }

	internal override ObsoleteAttributeData? ObsoleteAttributeData
	{
		get
		{
			if (!containingType.AnyMemberHasAttributes)
			{
				return null;
			}
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((CommonEventEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			return Microsoft.CodeAnalysis.ObsoleteAttributeData.Uninitialized;
		}
	}

	internal sealed override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal sealed override bool HasSpecialName => GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;

	public bool HasSkipLocalsInitAttribute => GetDecodedWellKnownAttributeData()?.HasSkipLocalsInitAttribute ?? false;

	public sealed override bool IsAbstract => (_modifiers & DeclarationModifiers.Abstract) != 0;

	private bool HasExternModifier => (_modifiers & DeclarationModifiers.Extern) != 0;

	public sealed override bool IsExtern => PartialImplementationPart?.IsExtern ?? HasExternModifier;

	public sealed override bool IsStatic => (_modifiers & DeclarationModifiers.Static) != 0;

	public sealed override bool IsOverride => (_modifiers & DeclarationModifiers.Override) != 0;

	public sealed override bool IsSealed => (_modifiers & DeclarationModifiers.Sealed) != 0;

	public sealed override bool IsVirtual => (_modifiers & DeclarationModifiers.Virtual) != 0;

	internal bool IsReadOnly => (_modifiers & DeclarationModifiers.ReadOnly) != 0;

	private bool IsUnsafe => (_modifiers & DeclarationModifiers.Unsafe) != 0;

	public sealed override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_modifiers);

	internal sealed override bool MustCallMethodsDirectly => false;

	internal SyntaxReference SyntaxReference => _syntaxRef;

	internal CSharpSyntaxNode CSharpSyntaxNode => (CSharpSyntaxNode)_syntaxRef.GetSyntax();

	internal SyntaxTree SyntaxTree => _syntaxRef.SyntaxTree;

	internal bool IsNew => (_modifiers & DeclarationModifiers.New) != 0;

	internal DeclarationModifiers Modifiers => _modifiers;

	internal override OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers
	{
		get
		{
			if (_lazyOverriddenOrHiddenMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyOverriddenOrHiddenMembers, this.MakeOverriddenOrHiddenMembers(), null);
			}
			return _lazyOverriddenOrHiddenMembers;
		}
	}

	public sealed override bool IsWindowsRuntimeEvent
	{
		get
		{
			if (!_lazyIsWindowsRuntimeEvent.HasValue())
			{
				_lazyIsWindowsRuntimeEvent = ComputeIsWindowsRuntimeEvent().ToThreeState();
			}
			return _lazyIsWindowsRuntimeEvent.Value();
		}
	}

	internal bool IsPartial => (Modifiers & DeclarationModifiers.Partial) != 0;

	protected abstract bool AccessorsHaveImplementation { get; }

	internal sealed override bool IsPartialDefinition
	{
		get
		{
			if (IsPartial && !AccessorsHaveImplementation)
			{
				return !HasExternModifier;
			}
			return false;
		}
	}

	internal bool IsPartialImplementation
	{
		get
		{
			if (IsPartial)
			{
				if (!AccessorsHaveImplementation)
				{
					return HasExternModifier;
				}
				return true;
			}
			return false;
		}
	}

	internal SourceEventSymbol? OtherPartOfPartial => _otherPartOfPartial;

	internal SourceEventSymbol? SourcePartialDefinitionPart
	{
		get
		{
			if (!IsPartialImplementation)
			{
				return null;
			}
			return OtherPartOfPartial;
		}
	}

	internal SourceEventSymbol? SourcePartialImplementationPart
	{
		get
		{
			if (!IsPartialDefinition)
			{
				return null;
			}
			return OtherPartOfPartial;
		}
	}

	internal sealed override EventSymbol? PartialDefinitionPart => SourcePartialDefinitionPart;

	internal sealed override EventSymbol? PartialImplementationPart => SourcePartialImplementationPart;

	internal SourceEventSymbol(SourceMemberContainerTypeSymbol containingType, CSharpSyntaxNode syntax, SyntaxTokenList modifiers, bool isFieldLike, ExplicitInterfaceSpecifierSyntax? interfaceSpecifierSyntaxOpt, SyntaxToken nameTokenSyntax, BindingDiagnosticBag diagnostics)
	{
		_location = nameTokenSyntax.GetLocation();
		this.containingType = containingType;
		_syntaxRef = syntax.GetReference();
		bool flag = interfaceSpecifierSyntaxOpt != null;
		_modifiers = MakeModifiers(modifiers, flag, isFieldLike, _location, diagnostics, out var _, out _hasExplicitAccessModifier);
		CheckAccessibility(_location, diagnostics, flag);
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		SourcePartialImplementationPart?.ForceComplete(locationOpt, filter, cancellationToken);
		if (filter == null || filter(this))
		{
			_state.DefaultForceComplete(this, cancellationToken);
		}
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		return new LexicalSortKey(_location, DeclaringCompilation);
	}

	public override Location TryGetFirstLocation()
	{
		return _location;
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		SourceEventSymbol sourcePartialDefinitionPart = SourcePartialDefinitionPart;
		bool flag;
		if ((object)sourcePartialDefinitionPart != null)
		{
			lazyCustomAttributesBag = sourcePartialDefinitionPart.GetAttributesBag();
			flag = Interlocked.CompareExchange(ref _lazyCustomAttributesBag, lazyCustomAttributesBag, null) == null;
		}
		else
		{
			flag = LoadAndValidateAttributes(GetAttributeDeclarations(), ref _lazyCustomAttributesBag);
		}
		if (flag)
		{
			DeclaringCompilation.SymbolDeclaredEvent(this);
			_state.NotePartComplete(CompletionPart.Attributes);
		}
		return _lazyCustomAttributesBag;
	}

	private OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		SourceEventSymbol sourcePartialImplementationPart = SourcePartialImplementationPart;
		if ((object)sourcePartialImplementationPart != null)
		{
			return OneOrMany.Create(AttributeDeclarationSyntaxList, sourcePartialImplementationPart.AttributeDeclarationSyntaxList);
		}
		return OneOrMany.Create(AttributeDeclarationSyntaxList);
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	protected CommonEventWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonEventWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal CommonEventEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonEventEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal override (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out CSharpAttributeData attributeData, out BoundAttribute boundAttribute, out ObsoleteAttributeData obsoleteData))
		{
			if (obsoleteData != null)
			{
				arguments.GetOrCreateData<CommonEventEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
			}
			return (attributeData, boundAttribute);
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	protected sealed override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		if (attribute.IsTargetAttribute(AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<CommonEventWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (!ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.TupleElementNamesAttribute | ReservedAttributes.NullableAttribute | ReservedAttributes.NativeIntegerAttribute | ReservedAttributes.ExtensionMarkerAttribute))
		{
			if (attribute.IsTargetAttribute(AttributeDescription.ExcludeFromCodeCoverageAttribute))
			{
				arguments.GetOrCreateData<CommonEventWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.SkipLocalsInitAttribute))
			{
				CSharpAttributeData.DecodeSkipLocalsInitAttribute<CommonEventWellKnownAttributeData>(DeclaringCompilation, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.UnscopedRefAttribute))
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, arguments.AttributeSyntaxOpt.Location);
			}
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData>? attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations;
		if (typeWithAnnotations.Type.ContainsDynamic())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers.Length));
		}
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(typeWithAnnotations.Type))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNativeIntegerAttribute(this, typeWithAnnotations.Type));
		}
		if (typeWithAnnotations.Type.ContainsTupleNames())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.SynthesizeTupleNamesAttribute(typeWithAnnotations.Type));
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableAttributeIfNecessary(this, containingType.GetNullableContextValue(), typeWithAnnotations));
		}
	}

	private void CheckAccessibility(Location location, BindingDiagnosticBag diagnostics, bool isExplicitInterfaceImplementation)
	{
		ModifierUtils.CheckAccessibility(_modifiers, this, isExplicitInterfaceImplementation, diagnostics, location);
	}

	private DeclarationModifiers MakeModifiers(SyntaxTokenList modifiers, bool explicitInterfaceImplementation, bool isFieldLike, Location location, BindingDiagnosticBag diagnostics, out bool modifierErrors, out bool hasExplicitAccessModifier)
	{
		bool isInterface = ContainingType.IsInterface;
		DeclarationModifiers defaultAccess = ((isInterface && !explicitInterfaceImplementation) ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.None;
		DeclarationModifiers declarationModifiers2 = DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
		if (!explicitInterfaceImplementation)
		{
			declarationModifiers2 |= DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Static | DeclarationModifiers.New | DeclarationModifiers.Virtual;
			if (!isInterface)
			{
				declarationModifiers2 |= DeclarationModifiers.Override;
			}
			else
			{
				defaultAccess = DeclarationModifiers.None;
				declarationModifiers2 |= DeclarationModifiers.Extern;
				declarationModifiers |= DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Static | DeclarationModifiers.Extern | DeclarationModifiers.Virtual;
			}
		}
		else
		{
			if (isInterface)
			{
				declarationModifiers2 |= DeclarationModifiers.Abstract;
			}
			declarationModifiers2 |= DeclarationModifiers.Static;
		}
		if (ContainingType.IsStructType())
		{
			declarationModifiers2 |= DeclarationModifiers.ReadOnly;
		}
		if (!isInterface)
		{
			declarationModifiers2 |= DeclarationModifiers.Extern;
		}
		DeclarationModifiers declarationModifiers3 = ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: false, isInterface, modifiers, defaultAccess, declarationModifiers2, location, diagnostics, out modifierErrors, out hasExplicitAccessModifier);
		ModifierUtils.CheckFeatureAvailabilityForStaticAbstractMembersInInterfacesIfNeeded(declarationModifiers3, explicitInterfaceImplementation, location, diagnostics);
		this.CheckUnsafeModifier(declarationModifiers3, diagnostics);
		ModifierUtils.ReportDefaultInterfaceImplementationModifiers(!isFieldLike, declarationModifiers3, declarationModifiers, location, diagnostics);
		if (isInterface)
		{
			declarationModifiers3 = ModifierUtils.AdjustModifiersForAnInterfaceMember(declarationModifiers3, !isFieldLike, explicitInterfaceImplementation, forMethod: false);
		}
		return declarationModifiers3;
	}

	protected void CheckModifiersAndType(BindingDiagnosticBag diagnostics)
	{
		Location firstLocation = GetFirstLocation();
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		bool flag = ContainingType.IsInterface && IsExplicitInterfaceImplementation;
		if (DeclaredAccessibility == Accessibility.Private && (IsVirtual || (IsAbstract && !flag) || IsOverride))
		{
			diagnostics.Add(ErrorCode.ERR_VirtualPrivate, firstLocation, this);
		}
		else if (IsReadOnly && IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_StaticMemberCantBeReadOnly, firstLocation, this);
		}
		else if (IsReadOnly && base.HasAssociatedField)
		{
			diagnostics.Add(ErrorCode.ERR_FieldLikeEventCantBeReadOnly, firstLocation, this);
		}
		else if (IsOverride && (IsNew || IsVirtual))
		{
			diagnostics.Add(ErrorCode.ERR_OverrideNotNew, firstLocation, this);
		}
		else if (IsSealed && !IsOverride && (!flag || !IsAbstract))
		{
			diagnostics.Add(ErrorCode.ERR_SealedNonOverride, firstLocation, this);
		}
		else if (IsPartial && !ContainingType.IsPartial())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberOnlyInPartialClass, firstLocation);
		}
		else if (IsPartial && IsExplicitInterfaceImplementation)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberNotExplicit, firstLocation);
		}
		else if (IsPartial && IsAbstract)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberCannotBeAbstract, firstLocation);
		}
		else if (IsAbstract && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, firstLocation, SyntaxFacts.GetText(SyntaxKind.AbstractKeyword));
		}
		else if (IsVirtual && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, firstLocation, SyntaxFacts.GetText(SyntaxKind.VirtualKeyword));
		}
		else if (IsAbstract && IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndExtern, firstLocation, this);
		}
		else if (IsAbstract && IsSealed && !flag)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndSealed, firstLocation, this);
		}
		else if (IsAbstract && IsVirtual)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractNotVirtual, firstLocation, Kind.Localize(), this);
		}
		else if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), firstLocation, this);
		}
		else if (ContainingType.IsStatic && !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_InstanceMemberInStaticClass, firstLocation, Name);
		}
		else if (!base.Type.IsVoidType())
		{
			if (!this.IsNoMoreVisibleThan(base.Type, ref useSiteInfo) && (CSharpSyntaxNode as EventDeclarationSyntax)?.ExplicitInterfaceSpecifier == null)
			{
				diagnostics.Add(ErrorCode.ERR_BadVisEventType, firstLocation, this, base.Type);
			}
			else if (!base.Type.IsDelegateType() && !base.Type.IsErrorType())
			{
				diagnostics.Add(ErrorCode.ERR_EventNotDelegate, firstLocation, this);
			}
			else if (IsAbstract && !ContainingType.IsAbstract && (ContainingType.TypeKind == TypeKind.Class || ContainingType.TypeKind == TypeKind.Submission))
			{
				diagnostics.Add(ErrorCode.ERR_AbstractInConcreteClass, firstLocation, this, ContainingType);
			}
			else if (IsVirtual && ContainingType.IsSealed)
			{
				diagnostics.Add(ErrorCode.ERR_NewVirtualInSealed, firstLocation, this, ContainingType);
			}
		}
		if (IsPartial)
		{
			ModifierUtils.CheckFeatureAvailabilityForPartialEventsAndConstructors(_location, diagnostics);
		}
		diagnostics.Add(firstLocation, useSiteInfo);
	}

	public override string GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref expandIncludes ? ref _lazyExpandedDocComment : ref _lazyDocComment);
	}

	protected static void CopyEventCustomModifiers(EventSymbol eventWithCustomModifiers, ref TypeWithAnnotations type, AssemblySymbol containingAssembly)
	{
		TypeSymbol type2 = eventWithCustomModifiers.Type;
		if (type.Type.Equals(type2, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds | TypeCompareKind.IgnoreDynamic | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
		{
			type = type.WithTypeAndModifiers(CustomModifierUtils.CopyTypeCustomModifiers(type2, type.Type, containingAssembly), eventWithCustomModifiers.TypeWithAnnotations.CustomModifiers);
		}
	}

	private bool ComputeIsWindowsRuntimeEvent()
	{
		EventSymbol partialDefinitionPart = PartialDefinitionPart;
		if ((object)partialDefinitionPart != null)
		{
			return partialDefinitionPart.IsWindowsRuntimeEvent;
		}
		ImmutableArray<EventSymbol> explicitInterfaceImplementations = ExplicitInterfaceImplementations;
		if (!explicitInterfaceImplementations.IsEmpty)
		{
			return explicitInterfaceImplementations[0].IsWindowsRuntimeEvent;
		}
		if (containingType.IsInterfaceType())
		{
			return this.IsCompilationOutputWinMdObj();
		}
		EventSymbol overriddenEvent = base.OverriddenEvent;
		if ((object)overriddenEvent != null)
		{
			return overriddenEvent.IsWindowsRuntimeEvent;
		}
		bool flag = false;
		foreach (NamedTypeSymbol key in containingType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Keys)
		{
			foreach (Symbol member in key.GetMembers(Name))
			{
				if (member.Kind == SymbolKind.Event && member.IsImplementableInterfaceMember() && this == containingType.FindImplementationForInterfaceMemberInNonInterface(member, ignoreImplementationInInterfacesIfResultIsNotReady: true))
				{
					flag = true;
					if (((EventSymbol)member).IsWindowsRuntimeEvent)
					{
						return true;
					}
				}
			}
		}
		if (flag)
		{
			return false;
		}
		return this.IsCompilationOutputWinMdObj();
	}

	internal static string GetAccessorName(string eventName, bool isAdder)
	{
		return (isAdder ? "add_" : "remove_") + eventName;
	}

	protected TypeWithAnnotations BindEventType(Binder binder, TypeSyntax typeSyntax, BindingDiagnosticBag diagnostics)
	{
		binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks | BinderFlags.SuppressUnsafeDiagnostics, this);
		return binder.BindType(typeSyntax, diagnostics);
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Location firstLocation = GetFirstLocation();
		CheckModifiersAndType(diagnostics);
		base.Type.CheckAllConstraints(declaringCompilation, conversions, firstLocation, diagnostics);
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(base.Type))
		{
			declaringCompilation.EnsureNativeIntegerAttributeExists(diagnostics, firstLocation, modifyCompilation: true);
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this) && TypeWithAnnotations.NeedsNullableAttribute())
		{
			declaringCompilation.EnsureNullableAttributeExists(diagnostics, firstLocation, modifyCompilation: true);
		}
		EventSymbol eventSymbol = ExplicitInterfaceImplementations.FirstOrDefault();
		if ((object)eventSymbol != null)
		{
			CheckExplicitImplementationAccessor(AddMethod, eventSymbol.AddMethod, eventSymbol, diagnostics);
			CheckExplicitImplementationAccessor(RemoveMethod, eventSymbol.RemoveMethod, eventSymbol, diagnostics);
		}
		if (IsPartialDefinition)
		{
			SourceEventSymbol otherPartOfPartial = OtherPartOfPartial;
			if ((object)otherPartOfPartial != null)
			{
				PartialEventChecks(otherPartOfPartial, diagnostics);
			}
		}
	}

	private void CheckExplicitImplementationAccessor(MethodSymbol? thisAccessor, MethodSymbol? otherAccessor, EventSymbol explicitlyImplementedEvent, BindingDiagnosticBag diagnostics)
	{
		if (!otherAccessor.IsImplementable() && (object)thisAccessor != null)
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitPropertyAddingAccessor, thisAccessor.GetFirstLocation(), thisAccessor, explicitlyImplementedEvent);
		}
	}

	private void PartialEventChecks(SourceEventSymbol implementation, BindingDiagnosticBag diagnostics)
	{
		if (!TypeWithAnnotations.Equals(implementation.TypeWithAnnotations, TypeCompareKind.AllIgnoreOptions))
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberTypeDifference, implementation.GetFirstLocation());
		}
		else if (MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(this, implementation))
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberInconsistentTupleNames, implementation.GetFirstLocation(), this, implementation);
		}
		else if (!MemberSignatureComparer.PartialMethodsStrictComparer.Equals(this, implementation))
		{
			diagnostics.Add(ErrorCode.WRN_PartialMemberSignatureDifference, implementation.GetFirstLocation(), new FormattedSymbol(this, SymbolDisplayFormat.MinimallyQualifiedFormat), new FormattedSymbol(implementation, SymbolDisplayFormat.MinimallyQualifiedFormat));
		}
		if (IsStatic != implementation.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberStaticDifference, implementation.GetFirstLocation());
		}
		if (IsUnsafe != implementation.IsUnsafe && this.CompilationAllowsUnsafe())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberUnsafeDifference, implementation.GetFirstLocation());
		}
		if (DeclaredAccessibility != implementation.DeclaredAccessibility || _hasExplicitAccessModifier != implementation._hasExplicitAccessModifier)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberAccessibilityDifference, implementation.GetFirstLocation());
		}
		if (IsVirtual != implementation.IsVirtual || IsOverride != implementation.IsOverride || IsSealed != implementation.IsSealed || IsNew != implementation.IsNew)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberExtendedModDifference, implementation.GetFirstLocation());
		}
	}

	internal static void InitializePartialEventParts(SourceEventSymbol definition, SourceEventSymbol implementation)
	{
		definition._otherPartOfPartial = implementation;
		implementation._otherPartOfPartial = definition;
	}
}
