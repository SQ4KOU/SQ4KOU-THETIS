using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourcePropertySymbolBase : PropertySymbol, IAttributeTargetSymbol
{
	[Flags]
	private enum Flags : ushort
	{
		IsExpressionBodied = 1,
		HasAutoPropertyGet = 2,
		HasAutoPropertySet = 4,
		GetterUsesFieldKeyword = 8,
		SetterUsesFieldKeyword = 0x10,
		IsExplicitInterfaceImplementation = 0x20,
		HasInitializer = 0x40,
		AccessorsHaveImplementation = 0x80,
		HasExplicitAccessModifier = 0x100,
		RequiresBackingField = 0x200
	}

	protected const string DefaultIndexerName = "Item";

	private readonly SourceMemberContainerTypeSymbol _containingType;

	private readonly string _name;

	private readonly SyntaxReference _syntaxRef;

	protected readonly DeclarationModifiers _modifiers;

	private ImmutableArray<CustomModifier> _lazyRefCustomModifiers;

	private readonly SourcePropertyAccessorSymbol? _getMethod;

	private readonly SourcePropertyAccessorSymbol? _setMethod;

	private readonly TypeSymbol _explicitInterfaceType;

	private ImmutableArray<PropertySymbol> _lazyExplicitInterfaceImplementations;

	private readonly Flags _propertyFlags;

	private readonly RefKind _refKind;

	private SymbolCompletionState _state;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeWithAnnotations.Boxed _lazyType;

	private string _lazySourceName;

	private string _lazyDocComment;

	private string _lazyExpandedDocComment;

	private OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembers;

	private SynthesizedSealedPropertyAccessor _lazySynthesizedSealedAccessor;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private SynthesizedBackingFieldSymbol? _lazyDeclaredBackingField;

	private StrongBox<SynthesizedBackingFieldSymbol?>? _lazyMergedBackingField;

	public Location Location { get; }

	protected abstract Location TypeLocation { get; }

	internal sealed override ImmutableArray<string> NotNullMembers => GetDecodedWellKnownAttributeData()?.NotNullMembers ?? ImmutableArray<string>.Empty;

	internal sealed override ImmutableArray<string> NotNullWhenTrueMembers => GetDecodedWellKnownAttributeData()?.NotNullWhenTrueMembers ?? ImmutableArray<string>.Empty;

	internal sealed override ImmutableArray<string> NotNullWhenFalseMembers => GetDecodedWellKnownAttributeData()?.NotNullWhenFalseMembers ?? ImmutableArray<string>.Empty;

	internal bool IsExpressionBodied => (_propertyFlags & Flags.IsExpressionBodied) != 0;

	public sealed override RefKind RefKind => _refKind;

	public sealed override TypeWithAnnotations TypeWithAnnotations
	{
		get
		{
			EnsureSignature();
			return _lazyType.Value;
		}
	}

	internal bool HasPointerType => TypeWithAnnotations.DefaultType.IsPointerOrFunctionPointer();

	public override string Name => _name;

	internal string SourceName
	{
		get
		{
			if (_lazySourceName == null)
			{
				SyntaxList<AttributeListSyntax> attributeLists = ((IndexerDeclarationSyntax)CSharpSyntaxNode).AttributeLists;
				string text = null;
				CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = null;
				Binder attributeBinder = GetAttributeBinder(attributeLists, DeclaringCompilation);
				LoadAndValidateAttributes(OneOrMany.Create(attributeLists), ref lazyCustomAttributesBag, AttributeLocation.None, earlyDecodingOnly: true, attributeBinder, this.IsExtensionBlockMember() ? new Func<AttributeSyntax, Binder, bool>(isPossibleIndexerNameAttributeInExtension) : new Func<AttributeSyntax, Binder, bool>(isPossibleIndexerNameAttribute));
				if (lazyCustomAttributesBag != null)
				{
					PropertyEarlyWellKnownAttributeData propertyEarlyWellKnownAttributeData = (PropertyEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData;
					if (propertyEarlyWellKnownAttributeData != null)
					{
						text = propertyEarlyWellKnownAttributeData.IndexerName;
					}
				}
				text = text ?? "Item";
				InterlockedOperations.Initialize(ref _lazySourceName, text);
			}
			return _lazySourceName;
			static bool isPossibleIndexerNameAttribute(AttributeSyntax node, Binder? rootBinderOpt)
			{
				return rootBinderOpt.QuickAttributeChecker.IsPossibleMatch(node, QuickAttributes.IndexerName);
			}
			static bool isPossibleIndexerNameAttributeInExtension(AttributeSyntax node, Binder? rootBinderOpt)
			{
				SeparatedSyntaxList<AttributeArgumentSyntax>? separatedSyntaxList = node.ArgumentList?.Arguments;
				if (separatedSyntaxList.HasValue)
				{
					SeparatedSyntaxList<AttributeArgumentSyntax> valueOrDefault = separatedSyntaxList.GetValueOrDefault();
					if (valueOrDefault.Count == 1)
					{
						AttributeArgumentSyntax attributeArgumentSyntax = valueOrDefault[0];
						if (attributeArgumentSyntax != null && attributeArgumentSyntax.NameColon == null && attributeArgumentSyntax.NameEquals == null)
						{
							ExpressionSyntax expression = attributeArgumentSyntax.Expression;
							if (expression is LiteralExpressionSyntax && expression.RawKind == 8750)
							{
								return isPossibleIndexerNameAttribute(node, rootBinderOpt);
							}
						}
					}
				}
				return false;
			}
		}
	}

	public override string MetadataName => SourceName.Replace(" ", "");

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(Location);

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_syntaxRef);

	public override bool IsAbstract => (_modifiers & DeclarationModifiers.Abstract) != 0;

	protected bool HasExternModifier => (_modifiers & DeclarationModifiers.Extern) != 0;

	public override bool IsExtern => HasExternModifier;

	public override bool IsStatic => (_modifiers & DeclarationModifiers.Static) != 0;

	public override bool IsIndexer => (_modifiers & DeclarationModifiers.Indexer) != 0;

	public override bool IsOverride => (_modifiers & DeclarationModifiers.Override) != 0;

	public override bool IsSealed => (_modifiers & DeclarationModifiers.Sealed) != 0;

	public override bool IsVirtual => (_modifiers & DeclarationModifiers.Virtual) != 0;

	internal sealed override bool IsRequired => (_modifiers & DeclarationModifiers.Required) != 0;

	internal bool IsNew => (_modifiers & DeclarationModifiers.New) != 0;

	internal bool HasReadOnlyModifier => (_modifiers & DeclarationModifiers.ReadOnly) != 0;

	public sealed override MethodSymbol? GetMethod => _getMethod;

	public sealed override MethodSymbol? SetMethod => _setMethod;

	internal override CallingConvention CallingConvention
	{
		get
		{
			if (!IsStatic)
			{
				return CallingConvention.HasThis;
			}
			return CallingConvention.Default;
		}
	}

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			EnsureSignature();
			return _lazyParameters;
		}
	}

	internal override bool IsExplicitInterfaceImplementation => (_propertyFlags & Flags.IsExplicitInterfaceImplementation) != 0;

	public sealed override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations
	{
		get
		{
			if (IsExplicitInterfaceImplementation)
			{
				EnsureSignature();
			}
			return _lazyExplicitInterfaceImplementations;
		}
	}

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			EnsureSignature();
			return _lazyRefCustomModifiers;
		}
	}

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_modifiers);

	public bool HasSkipLocalsInitAttribute => GetDecodedWellKnownAttributeData()?.HasSkipLocalsInitAttribute ?? false;

	internal bool IsAutoPropertyOrUsesFieldKeyword => IsSetOnEitherPart(Flags.HasAutoPropertyGet | Flags.HasAutoPropertySet | Flags.GetterUsesFieldKeyword | Flags.SetterUsesFieldKeyword);

	internal bool UsesFieldKeyword => IsSetOnEitherPart(Flags.GetterUsesFieldKeyword | Flags.SetterUsesFieldKeyword);

	protected bool HasExplicitAccessModifier => (_propertyFlags & Flags.HasExplicitAccessModifier) != 0;

	internal bool IsAutoProperty => IsSetOnEitherPart(Flags.HasAutoPropertyGet | Flags.HasAutoPropertySet);

	internal bool HasAutoPropertyGet => IsSetOnEitherPart(Flags.HasAutoPropertyGet);

	internal bool HasAutoPropertySet => IsSetOnEitherPart(Flags.HasAutoPropertySet);

	protected bool AccessorsHaveImplementation => (_propertyFlags & Flags.AccessorsHaveImplementation) != 0;

	internal SynthesizedBackingFieldSymbol BackingField
	{
		get
		{
			if (_lazyMergedBackingField == null)
			{
				SynthesizedBackingFieldSymbol declaredBackingField = DeclaredBackingField;
				Interlocked.CompareExchange(ref _lazyMergedBackingField, new StrongBox<SynthesizedBackingFieldSymbol>(declaredBackingField), null);
			}
			return _lazyMergedBackingField.Value;
		}
	}

	internal SynthesizedBackingFieldSymbol? DeclaredBackingField
	{
		get
		{
			if ((object)_lazyDeclaredBackingField == null && (_propertyFlags & Flags.RequiresBackingField) != 0)
			{
				Interlocked.CompareExchange(ref _lazyDeclaredBackingField, CreateBackingField(), null);
			}
			return _lazyDeclaredBackingField;
		}
	}

	internal override bool MustCallMethodsDirectly => false;

	internal SyntaxReference SyntaxReference => _syntaxRef;

	internal CSharpSyntaxNode CSharpSyntaxNode => (CSharpSyntaxNode)_syntaxRef.GetSyntax();

	internal SyntaxTree SyntaxTree => _syntaxRef.SyntaxTree;

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

	internal SynthesizedSealedPropertyAccessor SynthesizedSealedAccessorOpt
	{
		get
		{
			bool flag = (object)GetMethod != null;
			bool flag2 = (object)SetMethod != null;
			if (!IsSealed || (flag & flag2))
			{
				return null;
			}
			if ((object)_lazySynthesizedSealedAccessor == null)
			{
				Interlocked.CompareExchange(ref _lazySynthesizedSealedAccessor, MakeSynthesizedSealedAccessor(), null);
			}
			return _lazySynthesizedSealedAccessor;
		}
	}

	protected abstract SourcePropertySymbolBase BoundAttributesSource { get; }

	public abstract IAttributeTargetSymbol AttributesOwner { get; }

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => AttributesOwner;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Property;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			if (!IsAutoPropertyOrUsesFieldKeyword)
			{
				return AttributeLocation.Property;
			}
			return AttributeLocation.Field | AttributeLocation.Property;
		}
	}

	internal sealed override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal override bool HasSpecialName => GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!_containingType.AnyMemberHasAttributes)
			{
				return null;
			}
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((PropertyEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			return ObsoleteAttributeData.Uninitialized;
		}
	}

	internal bool HasDisallowNull => GetDecodedWellKnownAttributeData()?.HasDisallowNullAttribute ?? false;

	internal bool HasAllowNull => GetDecodedWellKnownAttributeData()?.HasAllowNullAttribute ?? false;

	internal bool HasMaybeNull => GetDecodedWellKnownAttributeData()?.HasMaybeNullAttribute ?? false;

	internal bool HasNotNull => GetDecodedWellKnownAttributeData()?.HasNotNullAttribute ?? false;

	internal SourceAttributeData DisallowNullAttributeIfExists => FindAttribute(AttributeDescription.DisallowNullAttribute);

	internal SourceAttributeData AllowNullAttributeIfExists => FindAttribute(AttributeDescription.AllowNullAttribute);

	internal SourceAttributeData MaybeNullAttributeIfExists => FindAttribute(AttributeDescription.MaybeNullAttribute);

	internal SourceAttributeData NotNullAttributeIfExists => FindAttribute(AttributeDescription.NotNullAttribute);

	internal ImmutableArray<SourceAttributeData> MemberNotNullAttributeIfExists => FindAttributes(AttributeDescription.MemberNotNullAttribute);

	internal ImmutableArray<SourceAttributeData> MemberNotNullWhenAttributeIfExists => FindAttributes(AttributeDescription.MemberNotNullWhenAttribute);

	internal sealed override bool HasUnscopedRefAttribute => GetDecodedWellKnownAttributeData()?.HasUnscopedRefAttribute ?? false;

	internal sealed override bool RequiresCompletion => true;

	internal bool IsPartial => (_modifiers & DeclarationModifiers.Partial) != 0;

	protected SourcePropertySymbolBase(SourceMemberContainerTypeSymbol containingType, CSharpSyntaxNode syntax, bool hasGetAccessor, bool hasSetAccessor, bool isExplicitInterfaceImplementation, TypeSymbol? explicitInterfaceType, string? aliasQualifierOpt, DeclarationModifiers modifiers, bool hasInitializer, bool hasExplicitAccessMod, bool hasAutoPropertyGet, bool hasAutoPropertySet, bool isExpressionBodied, bool accessorsHaveImplementation, bool getterUsesFieldKeyword, bool setterUsesFieldKeyword, RefKind refKind, string memberName, SyntaxList<AttributeListSyntax> indexerNameAttributeLists, Location location, BindingDiagnosticBag diagnostics)
	{
		_syntaxRef = syntax.GetReference();
		Location = location;
		_containingType = containingType;
		_refKind = refKind;
		_modifiers = modifiers;
		_explicitInterfaceType = explicitInterfaceType;
		if (isExplicitInterfaceImplementation)
		{
			_propertyFlags |= Flags.IsExplicitInterfaceImplementation;
		}
		else
		{
			_lazyExplicitInterfaceImplementations = ImmutableArray<PropertySymbol>.Empty;
		}
		if (hasExplicitAccessMod)
		{
			_propertyFlags |= Flags.HasExplicitAccessModifier;
		}
		if (hasAutoPropertyGet)
		{
			_propertyFlags |= Flags.HasAutoPropertyGet;
		}
		if (hasAutoPropertySet)
		{
			_propertyFlags |= Flags.HasAutoPropertySet;
		}
		if (getterUsesFieldKeyword)
		{
			_propertyFlags |= Flags.GetterUsesFieldKeyword;
		}
		if (setterUsesFieldKeyword)
		{
			_propertyFlags |= Flags.SetterUsesFieldKeyword;
		}
		if (hasInitializer)
		{
			_propertyFlags |= Flags.HasInitializer;
		}
		if (isExpressionBodied)
		{
			_propertyFlags |= Flags.IsExpressionBodied;
		}
		if (accessorsHaveImplementation)
		{
			_propertyFlags |= Flags.AccessorsHaveImplementation;
		}
		if (IsIndexer)
		{
			if ((indexerNameAttributeLists.Count == 0) | isExplicitInterfaceImplementation)
			{
				_lazySourceName = memberName;
			}
			_name = ExplicitInterfaceHelpers.GetMemberName("this[]", _explicitInterfaceType, aliasQualifierOpt);
		}
		else
		{
			_name = (_lazySourceName = memberName);
		}
		if (getterUsesFieldKeyword | setterUsesFieldKeyword | hasAutoPropertyGet | hasAutoPropertySet | hasInitializer)
		{
			_propertyFlags |= Flags.RequiresBackingField;
		}
		if (hasGetAccessor)
		{
			_getMethod = CreateGetAccessorSymbol(hasAutoPropertyGet, diagnostics);
		}
		if (hasSetAccessor)
		{
			_setMethod = CreateSetAccessorSymbol(hasAutoPropertySet, diagnostics);
		}
	}

	private void EnsureSignatureGuarded(BindingDiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = null;
		_lazyRefCustomModifiers = ImmutableArray<CustomModifier>.Empty;
		(TypeWithAnnotations, ImmutableArray<ParameterSymbol>) tuple = MakeParametersAndBindType(diagnostics);
		TypeWithAnnotations item = tuple.Item1;
		_lazyParameters = tuple.Item2;
		_lazyType = new TypeWithAnnotations.Boxed(item);
		bool isExplicitInterfaceImplementation = IsExplicitInterfaceImplementation;
		if (isExplicitInterfaceImplementation || IsOverride)
		{
			bool alsoCopyParamsModifier = false;
			PropertySymbol propertySymbol2;
			if (!isExplicitInterfaceImplementation)
			{
				alsoCopyParamsModifier = true;
				propertySymbol2 = base.OverriddenProperty;
			}
			else
			{
				CSharpSyntaxNode cSharpSyntaxNode = CSharpSyntaxNode;
				string interfacePropertyName = (IsIndexer ? "this[]" : ((PropertyDeclarationSyntax)cSharpSyntaxNode).Identifier.ValueText);
				propertySymbol = this.FindExplicitlyImplementedProperty(_explicitInterfaceType, interfacePropertyName, GetExplicitInterfaceSpecifier(), diagnostics);
				this.FindExplicitlyImplementedMemberVerification(propertySymbol, diagnostics);
				propertySymbol2 = propertySymbol;
			}
			if ((object)propertySymbol2 != null)
			{
				_lazyRefCustomModifiers = ((_refKind != RefKind.None) ? propertySymbol2.RefCustomModifiers : ImmutableArray<CustomModifier>.Empty);
				TypeWithAnnotations typeWithAnnotations = propertySymbol2.TypeWithAnnotations;
				if (item.Type.Equals(typeWithAnnotations.Type, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds | TypeCompareKind.IgnoreDynamic | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
				{
					item = item.WithTypeAndModifiers(CustomModifierUtils.CopyTypeCustomModifiers(typeWithAnnotations.Type, item.Type, ContainingAssembly), typeWithAnnotations.CustomModifiers);
					_lazyType = new TypeWithAnnotations.Boxed(item);
				}
				_lazyParameters = CustomModifierUtils.CopyParameterCustomModifiers(propertySymbol2.Parameters, _lazyParameters, alsoCopyParamsModifier);
			}
		}
		else if (_refKind == RefKind.In)
		{
			NamedTypeSymbol wellKnownType = Binder.GetWellKnownType(DeclaringCompilation, WellKnownType.System_Runtime_InteropServices_InAttribute, diagnostics, TypeLocation);
			_lazyRefCustomModifiers = ImmutableArray.Create(CSharpCustomModifier.CreateRequired(wellKnownType));
		}
		_lazyExplicitInterfaceImplementations = (((object)propertySymbol == null) ? ImmutableArray<PropertySymbol>.Empty : ImmutableArray.Create(propertySymbol));
	}

	protected void CheckInitializerIfNeeded(BindingDiagnosticBag diagnostics)
	{
		if ((_propertyFlags & Flags.HasInitializer) != 0)
		{
			if (ContainingType.IsInterface && !IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_InstancePropertyInitializerInInterface, Location);
			}
			else if (!IsAutoPropertyOrUsesFieldKeyword)
			{
				diagnostics.Add(ErrorCode.ERR_InitializerOnNonAutoProperty, Location);
			}
		}
	}

	private static void CheckFieldKeywordUsage(SourcePropertySymbolBase property, BindingDiagnosticBag diagnostics)
	{
		if (!property.DeclaringCompilation.IsFeatureEnabled(MessageID.IDS_FeatureFieldKeyword))
		{
			return;
		}
		SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol = null;
		Flags propertyFlags = property._propertyFlags;
		bool flag = (propertyFlags & Flags.GetterUsesFieldKeyword) != 0;
		bool flag2 = (propertyFlags & Flags.SetterUsesFieldKeyword) != 0;
		SourcePropertyAccessorSymbol setMethod = property._setMethod;
		if ((object)setMethod != null && !setMethod.IsAutoPropertyAccessor && !flag2 && !property.IsSetOnEitherPart(Flags.HasInitializer) && (property.HasAutoPropertyGet | flag))
		{
			sourcePropertyAccessorSymbol = setMethod;
		}
		else
		{
			SourcePropertyAccessorSymbol getMethod = property._getMethod;
			if ((object)getMethod != null && !getMethod.IsAutoPropertyAccessor && !flag && (property.HasAutoPropertySet | flag2))
			{
				sourcePropertyAccessorSymbol = getMethod;
			}
		}
		if ((object)sourcePropertyAccessorSymbol == null)
		{
			return;
		}
		if ((object)sourcePropertyAccessorSymbol != null)
		{
			MethodKind methodKind = sourcePropertyAccessorSymbol.MethodKind;
			string text;
			if (methodKind != MethodKind.PropertyGet)
			{
				if (methodKind != MethodKind.PropertySet)
				{
					goto IL_00d8;
				}
				text = (sourcePropertyAccessorSymbol.IsInitOnly ? SyntaxFacts.GetText(SyntaxKind.InitKeyword) : SyntaxFacts.GetText(SyntaxKind.SetKeyword));
			}
			else
			{
				if (sourcePropertyAccessorSymbol.IsInitOnly)
				{
					goto IL_00d8;
				}
				text = SyntaxFacts.GetText(SyntaxKind.GetKeyword);
			}
			string text2 = text;
			diagnostics.Add(ErrorCode.WRN_AccessorDoesNotUseBackingField, sourcePropertyAccessorSymbol.GetFirstLocation(), text2, property);
			return;
		}
		goto IL_00d8;
		IL_00d8:
		throw ExceptionUtilities.UnexpectedValue(sourcePropertyAccessorSymbol);
	}

	private void EnsureSignature()
	{
		if (_state.HasComplete(CompletionPart.FinishBaseType))
		{
			return;
		}
		lock (_syntaxRef)
		{
			if (_state.NotePartComplete(CompletionPart.StartBaseType))
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				try
				{
					EnsureSignatureGuarded(instance);
					AddDeclarationDiagnostics(instance);
					return;
				}
				finally
				{
					_state.NotePartComplete(CompletionPart.FinishBaseType);
					instance.Free();
				}
			}
		}
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		return new LexicalSortKey(Location, DeclaringCompilation);
	}

	public sealed override Location TryGetFirstLocation()
	{
		return Location;
	}

	protected abstract SourcePropertyAccessorSymbol CreateGetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics);

	protected abstract SourcePropertyAccessorSymbol CreateSetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics);

	internal bool CanUseBackingFieldDirectlyInConstructor(bool useAsLvalue)
	{
		if ((object)BackingField == null)
		{
			return false;
		}
		if (useAsLvalue)
		{
			if ((object)SetMethod != null)
			{
				return HasAutoPropertySet;
			}
			return true;
		}
		if ((object)GetMethod != null)
		{
			return HasAutoPropertyGet;
		}
		return true;
	}

	private bool IsSetOnEitherPart(Flags flags)
	{
		if ((_propertyFlags & flags) == 0)
		{
			if (this is SourcePropertySymbol sourcePropertySymbol)
			{
				SourcePropertySymbol otherPartOfPartial = sourcePropertySymbol.OtherPartOfPartial;
				if ((object)otherPartOfPartial != null)
				{
					return (otherPartOfPartial._propertyFlags & flags) != 0;
				}
			}
			return false;
		}
		return true;
	}

	internal void SetMergedBackingField(SynthesizedBackingFieldSymbol? backingField)
	{
		Interlocked.CompareExchange(ref _lazyMergedBackingField, new StrongBox<SynthesizedBackingFieldSymbol>(backingField), null);
	}

	private SynthesizedBackingFieldSymbol CreateBackingField()
	{
		string name = GeneratedNames.MakeBackingFieldName(_name);
		bool isReadOnly = (!IsStatic && ContainingType.IsReadOnly) || HasReadOnlyModifier || ((((object)_setMethod == null || _setMethod.IsInitOnly || _setMethod.IsDeclaredReadOnly) && ((object)_getMethod == null || (_propertyFlags & Flags.HasAutoPropertyGet) != 0 || _getMethod.IsDeclaredReadOnly)) ? true : false);
		return new SynthesizedBackingFieldSymbol(this, name, isReadOnly, IsStatic, (_propertyFlags & Flags.HasInitializer) != 0);
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		bool isExplicitInterfaceImplementation = IsExplicitInterfaceImplementation;
		CheckAccessibility(Location, diagnostics, isExplicitInterfaceImplementation);
		CheckModifiers(isExplicitInterfaceImplementation, Location, IsIndexer, diagnostics);
		CheckInitializerIfNeeded(diagnostics);
		CheckFieldKeywordUsage(((SourcePropertySymbolBase)PartialImplementationPart) ?? this, diagnostics);
		if (RefKind != RefKind.None && IsRequired)
		{
			diagnostics.Add(ErrorCode.ERR_RefReturningPropertiesCannotBeRequired, Location);
		}
		if (IsAutoPropertyOrUsesFieldKeyword)
		{
			if (!IsStatic && HasAutoPropertySet)
			{
				MethodSymbol setMethod = SetMethod;
				if ((object)setMethod != null && !setMethod.IsInitOnly)
				{
					if (ContainingType.IsReadOnly)
					{
						diagnostics.Add(ErrorCode.ERR_AutoPropsInRoStruct, Location);
					}
					else if (HasReadOnlyModifier)
					{
						diagnostics.Add(ErrorCode.ERR_AutoPropertyWithSetterCantBeReadOnly, Location, this);
					}
				}
			}
			Binder.ReportUseSiteDiagnosticForSynthesizedAttribute(DeclaringCompilation, WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor, diagnostics, Location);
			if (RefKind != RefKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_AutoPropertyCannotBeRefReturning, Location);
			}
			if (IsOverride)
			{
				PropertySymbol propertySymbol = (PropertySymbol)this.GetLeastOverriddenMember(ContainingType);
				if (((object)propertySymbol.GetMethod != null && (object)GetMethod == null) || ((object)propertySymbol.SetMethod != null && (object)SetMethod == null))
				{
					diagnostics.Add(ErrorCode.ERR_AutoPropertyMustOverrideSet, Location);
				}
			}
		}
		if (!IsStatic && ContainingType.IsInterface && IsSetOnEitherPart(Flags.RequiresBackingField) && !IsSetOnEitherPart(Flags.HasInitializer))
		{
			diagnostics.Add(ErrorCode.ERR_InterfacesCantContainFields, Location);
		}
		if (!IsExpressionBodied)
		{
			bool flag = (object)GetMethod != null;
			bool flag2 = (object)SetMethod != null;
			if (flag & flag2)
			{
				if (_refKind != RefKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_RefPropertyCannotHaveSetAccessor, _setMethod.GetFirstLocation());
				}
				else if (_getMethod.LocalAccessibility != Accessibility.NotApplicable && _setMethod.LocalAccessibility != Accessibility.NotApplicable)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicatePropertyAccessMods, Location, this);
				}
				else if (_getMethod.LocalDeclaredReadOnly && _setMethod.LocalDeclaredReadOnly)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicatePropertyReadOnlyMods, Location, this);
				}
				else if (IsAbstract)
				{
					CheckAbstractPropertyAccessorNotPrivate(_getMethod, diagnostics);
					CheckAbstractPropertyAccessorNotPrivate(_setMethod, diagnostics);
				}
			}
			else
			{
				if (!flag && !flag2)
				{
					diagnostics.Add(ErrorCode.ERR_PropertyWithNoAccessors, Location, this);
				}
				else if (RefKind != RefKind.None)
				{
					if (!flag)
					{
						diagnostics.Add(ErrorCode.ERR_RefPropertyMustHaveGetAccessor, Location);
					}
				}
				else if (!flag && HasAutoPropertySet)
				{
					diagnostics.Add(ErrorCode.ERR_AutoPropertyMustHaveGetAccessor, _setMethod.GetFirstLocation());
				}
				if (!IsOverride)
				{
					SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol = _getMethod ?? _setMethod;
					if ((object)sourcePropertyAccessorSymbol != null)
					{
						if (sourcePropertyAccessorSymbol.LocalAccessibility != Accessibility.NotApplicable)
						{
							diagnostics.Add(ErrorCode.ERR_AccessModMissingAccessor, Location, this);
						}
						if (sourcePropertyAccessorSymbol.LocalDeclaredReadOnly)
						{
							diagnostics.Add(ErrorCode.ERR_ReadOnlyModMissingAccessor, Location, this);
						}
					}
				}
			}
			CheckAccessibilityMoreRestrictive(_getMethod, diagnostics);
			CheckAccessibilityMoreRestrictive(_setMethod, diagnostics);
		}
		PropertySymbol propertySymbol2 = ExplicitInterfaceImplementations.FirstOrDefault();
		if ((object)propertySymbol2 != null)
		{
			CheckExplicitImplementationAccessor(GetMethod, propertySymbol2.GetMethod, propertySymbol2, diagnostics);
			CheckExplicitImplementationAccessor(SetMethod, propertySymbol2.SetMethod, propertySymbol2, diagnostics);
		}
		Location typeLocation = TypeLocation;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if ((object)_explicitInterfaceType != null)
		{
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = GetExplicitInterfaceSpecifier();
			_explicitInterfaceType.CheckAllConstraints(declaringCompilation, conversions, new SourceLocation(explicitInterfaceSpecifier.Name), diagnostics);
			if ((object)propertySymbol2 != null)
			{
				TypeSymbol.CheckModifierMismatchOnImplementingMember(ContainingType, this, propertySymbol2, isExplicit: true, diagnostics);
			}
		}
		if (_refKind == RefKind.In)
		{
			declaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, typeLocation, modifyCompilation: true);
		}
		ParameterHelpers.EnsureRefKindAttributesExist(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureParamCollectionAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(base.Type))
		{
			declaringCompilation.EnsureNativeIntegerAttributeExists(diagnostics, typeLocation, modifyCompilation: true);
		}
		ParameterHelpers.EnsureNativeIntegerAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureScopedRefAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		if (declaringCompilation.ShouldEmitNullableAttributes(this) && TypeWithAnnotations.NeedsNullableAttribute())
		{
			declaringCompilation.EnsureNullableAttributeExists(diagnostics, typeLocation, modifyCompilation: true);
		}
		ParameterHelpers.EnsureNullableAttributeExists(declaringCompilation, this, Parameters, diagnostics, modifyCompilation: true);
		if (this.IsExtensionBlockMember())
		{
			ParameterHelpers.CheckUnderspecifiedGenericExtension(this, Parameters, diagnostics);
			declaringCompilation.EnsureExtensionMarkerAttributeExists(diagnostics, GetFirstLocation(), modifyCompilation: true);
		}
	}

	private void CheckAccessibility(Location location, BindingDiagnosticBag diagnostics, bool isExplicitInterfaceImplementation)
	{
		ModifierUtils.CheckAccessibility(_modifiers, this, isExplicitInterfaceImplementation, diagnostics, location);
	}

	private void CheckModifiers(bool isExplicitInterfaceImplementation, Location location, bool isIndexer, BindingDiagnosticBag diagnostics)
	{
		bool flag = isExplicitInterfaceImplementation && ContainingType.IsInterface;
		if (DeclaredAccessibility == Accessibility.Private && (IsVirtual || (IsAbstract && !flag) || IsOverride))
		{
			diagnostics.Add(ErrorCode.ERR_VirtualPrivate, location, this);
			return;
		}
		if (IsStatic && HasReadOnlyModifier)
		{
			diagnostics.Add(ErrorCode.ERR_StaticMemberCantBeReadOnly, location, this);
			return;
		}
		if (IsOverride && (IsNew || IsVirtual))
		{
			diagnostics.Add(ErrorCode.ERR_OverrideNotNew, location, this);
			return;
		}
		if (IsSealed && !IsOverride && !(IsAbstract & flag))
		{
			diagnostics.Add(ErrorCode.ERR_SealedNonOverride, location, this);
			return;
		}
		if (IsPartial && !ContainingType.IsPartial())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberOnlyInPartialClass, location);
			return;
		}
		if (IsPartial & isExplicitInterfaceImplementation)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberNotExplicit, location);
			return;
		}
		if (IsPartial && IsAbstract)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberCannotBeAbstract, location);
			return;
		}
		if (IsAbstract && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.AbstractKeyword));
			return;
		}
		if (IsVirtual && ContainingType.TypeKind == TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.VirtualKeyword));
			return;
		}
		if (IsAbstract && IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndExtern, location, this);
			return;
		}
		if (IsAbstract && IsSealed && !flag)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractAndSealed, location, this);
			return;
		}
		if (IsAbstract && IsVirtual)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractNotVirtual, location, Kind.Localize(), this);
			return;
		}
		if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
			return;
		}
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null && containingType.IsExtension)
		{
			ParameterSymbol extensionParameter = containingType.ExtensionParameter;
			if ((object)extensionParameter != null && extensionParameter.Name == "" && !IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_InstanceMemberWithUnnamedExtensionsParameter, location, Name);
				return;
			}
		}
		if (ContainingType.IsStatic && !IsStatic)
		{
			ErrorCode code = (isIndexer ? ErrorCode.ERR_IndexerInStaticClass : ErrorCode.ERR_InstanceMemberInStaticClass);
			diagnostics.Add(code, location, this);
		}
	}

	private void CheckAccessibilityMoreRestrictive(SourcePropertyAccessorSymbol accessor, BindingDiagnosticBag diagnostics)
	{
		if ((object)accessor != null && !IsAccessibilityMoreRestrictive(DeclaredAccessibility, accessor.LocalAccessibility))
		{
			diagnostics.Add(ErrorCode.ERR_InvalidPropertyAccessMod, accessor.GetFirstLocation(), accessor, this);
		}
	}

	private static bool IsAccessibilityMoreRestrictive(Accessibility property, Accessibility accessor)
	{
		if (accessor == Accessibility.NotApplicable)
		{
			return true;
		}
		if (accessor < property)
		{
			if (accessor == Accessibility.Protected)
			{
				return property != Accessibility.Internal;
			}
			return true;
		}
		return false;
	}

	private static void CheckAbstractPropertyAccessorNotPrivate(SourcePropertyAccessorSymbol accessor, BindingDiagnosticBag diagnostics)
	{
		if (accessor.LocalAccessibility == Accessibility.Private)
		{
			diagnostics.Add(ErrorCode.ERR_PrivateAbstractAccessor, accessor.GetFirstLocation(), accessor);
		}
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref expandIncludes ? ref _lazyExpandedDocComment : ref _lazyDocComment);
	}

	private void CheckExplicitImplementationAccessor(MethodSymbol thisAccessor, MethodSymbol otherAccessor, PropertySymbol explicitlyImplementedProperty, BindingDiagnosticBag diagnostics)
	{
		bool flag = (object)thisAccessor != null;
		bool flag2 = otherAccessor.IsImplementable();
		if (flag2 && !flag)
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitPropertyMissingAccessor, Location, this, otherAccessor);
		}
		else if (!flag2 & flag)
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitPropertyAddingAccessor, thisAccessor.GetFirstLocation(), thisAccessor, explicitlyImplementedProperty);
		}
		else if (TypeSymbol.HaveInitOnlyMismatch(thisAccessor, otherAccessor))
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitPropertyMismatchInitOnly, thisAccessor.GetFirstLocation(), thisAccessor, otherAccessor);
		}
	}

	private SynthesizedSealedPropertyAccessor MakeSynthesizedSealedAccessor()
	{
		if ((object)GetMethod != null)
		{
			MethodSymbol ownOrInheritedSetMethod = this.GetOwnOrInheritedSetMethod();
			if ((object)ownOrInheritedSetMethod != null)
			{
				return new SynthesizedSealedPropertyAccessor(this, ownOrInheritedSetMethod);
			}
			return null;
		}
		if ((object)SetMethod != null)
		{
			MethodSymbol ownOrInheritedGetMethod = this.GetOwnOrInheritedGetMethod();
			if ((object)ownOrInheritedGetMethod != null)
			{
				return new SynthesizedSealedPropertyAccessor(this, ownOrInheritedGetMethod);
			}
			return null;
		}
		return null;
	}

	public abstract OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations();

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		SourcePropertySymbolBase boundAttributesSource = BoundAttributesSource;
		BackingField?.GetAttributes();
		bool flag;
		if ((object)boundAttributesSource != null)
		{
			CustomAttributesBag<CSharpAttributeData> attributesBag = boundAttributesSource.GetAttributesBag();
			flag = Interlocked.CompareExchange(ref _lazyCustomAttributesBag, attributesBag, null) == null;
		}
		else
		{
			flag = LoadAndValidateAttributes(GetAttributeDeclarations(), ref _lazyCustomAttributesBag);
		}
		if (flag)
		{
			_state.NotePartComplete(CompletionPart.Attributes);
		}
		return _lazyCustomAttributesBag;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	private PropertyWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (PropertyWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal PropertyEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (PropertyEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations;
		if (typeWithAnnotations.Type.ContainsDynamic())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers.Length + RefCustomModifiers.Length, _refKind));
		}
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(typeWithAnnotations.Type))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNativeIntegerAttribute(this, typeWithAnnotations.Type));
		}
		if (typeWithAnnotations.Type.ContainsTupleNames())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(typeWithAnnotations.Type));
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableAttributeIfNecessary(this, ContainingType.GetNullableContextValue(), typeWithAnnotations));
		}
		if (base.ReturnsByRefReadonly)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
		}
		if (IsRequired)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_RequiredMemberAttribute__ctor));
		}
		if (this.IsExtensionBlockMember())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeExtensionMarkerAttribute(this, ((SourceNamedTypeSymbol)ContainingType).ExtensionMarkerName));
		}
	}

	internal override (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out CSharpAttributeData attributeData, out BoundAttribute boundAttribute, out ObsoleteAttributeData obsoleteData))
		{
			if (obsoleteData != null)
			{
				arguments.GetOrCreateData<PropertyEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
			}
			return (attributeData, boundAttribute);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.IndexerNameAttribute))
		{
			(attributeData, boundAttribute) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out var generatedDiagnostics);
			if (!attributeData.HasErrors)
			{
				string text = attributeData.CommonConstructorArguments[0].DecodeValue<string>(SpecialType.System_String);
				if (text != null)
				{
					arguments.GetOrCreateData<PropertyEarlyWellKnownAttributeData>().IndexerName = text;
				}
				if (!generatedDiagnostics)
				{
					return (attributeData, boundAttribute);
				}
			}
			return (null, null);
		}
		if ((IsIndexer || this.IsExtensionBlockMember()) && CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.OverloadResolutionPriorityAttribute))
		{
			(CSharpAttributeData, BoundAttribute) attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out var generatedDiagnostics2);
			attributeData = attribute.Item1;
			boundAttribute = attribute.Item2;
			ImmutableArray<TypedConstant> commonConstructorArguments = attributeData.CommonConstructorArguments;
			if (commonConstructorArguments.Length == 1 && commonConstructorArguments[0].ValueInternal is int overloadResolutionPriority)
			{
				arguments.GetOrCreateData<PropertyEarlyWellKnownAttributeData>().OverloadResolutionPriority = overloadResolutionPriority;
				if (!generatedDiagnostics2)
				{
					return (attributeData, boundAttribute);
				}
			}
			return (null, null);
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	protected override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(AttributeDescription.IndexerNameAttribute))
		{
			ValidateIndexerNameAttribute(attribute, arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<PropertyWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.ExcludeFromCodeCoverageAttribute))
		{
			arguments.GetOrCreateData<PropertyWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.SkipLocalsInitAttribute))
		{
			CSharpAttributeData.DecodeSkipLocalsInitAttribute<PropertyWellKnownAttributeData>(DeclaringCompilation, ref arguments);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.DynamicAttribute))
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ExplicitDynamicAttr, arguments.AttributeSyntaxOpt.Location);
		}
		else
		{
			if (ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.DynamicAttribute | ReservedAttributes.IsReadOnlyAttribute | ReservedAttributes.IsUnmanagedAttribute | ReservedAttributes.IsByRefLikeAttribute | ReservedAttributes.TupleElementNamesAttribute | ReservedAttributes.NullableAttribute | ReservedAttributes.NativeIntegerAttribute | ReservedAttributes.RequiredMemberAttribute | ReservedAttributes.RequiresLocationAttribute | ReservedAttributes.ExtensionMarkerAttribute))
			{
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.DisallowNullAttribute))
			{
				arguments.GetOrCreateData<PropertyWellKnownAttributeData>().HasDisallowNullAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.AllowNullAttribute))
			{
				arguments.GetOrCreateData<PropertyWellKnownAttributeData>().HasAllowNullAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.MaybeNullAttribute))
			{
				arguments.GetOrCreateData<PropertyWellKnownAttributeData>().HasMaybeNullAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.NotNullAttribute))
			{
				arguments.GetOrCreateData<PropertyWellKnownAttributeData>().HasNotNullAttribute = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.MemberNotNullAttribute))
			{
				MessageID.IDS_FeatureMemberNotNull.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				CSharpAttributeData.DecodeMemberNotNullAttribute<PropertyWellKnownAttributeData>(ContainingType, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.MemberNotNullWhenAttribute))
			{
				MessageID.IDS_FeatureMemberNotNull.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				CSharpAttributeData.DecodeMemberNotNullWhenAttribute<PropertyWellKnownAttributeData>(ContainingType, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.UnscopedRefAttribute))
			{
				if (!ContainingModule.UseUpdatedEscapeRules)
				{
					bindingDiagnosticBag.Add(ErrorCode.WRN_UnscopedRefAttributeOldRules, arguments.AttributeSyntaxOpt.Location);
				}
				if (IsValidUnscopedRefAttributeTarget())
				{
					arguments.GetOrCreateData<PropertyWellKnownAttributeData>().HasUnscopedRefAttribute = true;
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
			else if (attribute.IsTargetAttribute(AttributeDescription.OverloadResolutionPriorityAttribute))
			{
				MessageID.IDS_FeatureOverloadResolutionPriority.CheckFeatureAvailability(bindingDiagnosticBag, arguments.AttributeSyntaxOpt);
				if (!base.CanHaveOverloadResolutionPriority)
				{
					bindingDiagnosticBag.Add(IsOverride ? ErrorCode.ERR_CannotApplyOverloadResolutionPriorityToOverride : ErrorCode.ERR_CannotApplyOverloadResolutionPriorityToMember, arguments.AttributeSyntaxOpt);
				}
			}
		}
	}

	private bool IsValidUnscopedRefAttributeTarget()
	{
		if (isNullOrValidAccessor(_getMethod))
		{
			return isNullOrValidAccessor(_setMethod);
		}
		return false;
		static bool isNullOrValidAccessor(MethodSymbol? accessor)
		{
			return accessor?.IsValidUnscopedRefAttributeTarget() ?? true;
		}
	}

	private SourceAttributeData FindAttribute(AttributeDescription attributeDescription)
	{
		return (SourceAttributeData)GetAttributes().First((CSharpAttributeData a) => a.IsTargetAttribute(attributeDescription));
	}

	private ImmutableArray<SourceAttributeData> FindAttributes(AttributeDescription attributeDescription)
	{
		return (from a in GetAttributes()
			where a.IsTargetAttribute(attributeDescription)
			select a).Cast<SourceAttributeData>().ToImmutableArray();
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	private void ValidateIndexerNameAttribute(CSharpAttributeData attribute, AttributeSyntax node, BindingDiagnosticBag diagnostics)
	{
		if (!IsIndexer || IsExplicitInterfaceImplementation)
		{
			diagnostics.Add(ErrorCode.ERR_BadIndexerNameAttr, node.Name.Location, node.GetErrorDisplayName());
			return;
		}
		string text = attribute.CommonConstructorArguments[0].DecodeValue<string>(SpecialType.System_String);
		if (text == null || !SyntaxFacts.IsValidIdentifier(text))
		{
			diagnostics.Add(ErrorCode.ERR_BadArgumentToAttribute, node.ArgumentList.Arguments[0].Location, node.GetErrorDisplayName());
		}
		else if (this.IsExtensionBlockMember() && SourceName != text)
		{
			diagnostics.Add(ErrorCode.ERR_InsufficientStack, node.ArgumentList.Arguments[0].Location);
		}
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return GetEarlyDecodedWellKnownAttributeData()?.OverloadResolutionPriority ?? 0;
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		if (filter != null && !filter(this))
		{
			return;
		}
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.StartBaseType:
			case CompletionPart.FinishBaseType:
				EnsureSignature();
				break;
			case CompletionPart.StartInterfaces:
			case CompletionPart.FinishInterfaces:
				if (_state.NotePartComplete(CompletionPart.StartInterfaces))
				{
					if (Parameters.Length > 0)
					{
						BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance();
						TypeConversions typeConversions2 = ContainingAssembly.CorLibrary.TypeConversions;
						foreach (ParameterSymbol parameter in Parameters)
						{
							parameter.ForceComplete(locationOpt, null, cancellationToken);
							parameter.Type.CheckAllConstraints(DeclaringCompilation, typeConversions2, parameter.GetFirstLocation(), instance2);
						}
						AddDeclarationDiagnostics(instance2);
						instance2.Free();
					}
					DeclaringCompilation.SymbolDeclaredEvent(this);
					if (this.IsPartialDefinition())
					{
						if ((object)_getMethod != null)
						{
							DeclaringCompilation.SymbolDeclaredEvent(_getMethod);
						}
						if ((object)_setMethod != null)
						{
							DeclaringCompilation.SymbolDeclaredEvent(_setMethod);
						}
					}
					_state.NotePartComplete(CompletionPart.FinishInterfaces);
				}
				else
				{
					_state.SpinWaitComplete(CompletionPart.FinishInterfaces, cancellationToken);
				}
				break;
			case CompletionPart.EnumUnderlyingType:
			case CompletionPart.TypeArguments:
				if (_state.NotePartComplete(CompletionPart.EnumUnderlyingType))
				{
					BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
					TypeConversions typeConversions = ContainingAssembly.CorLibrary.TypeConversions;
					base.Type.CheckAllConstraints(DeclaringCompilation, typeConversions, Location, instance);
					ValidatePropertyType(instance);
					AddDeclarationDiagnostics(instance);
					_state.NotePartComplete(CompletionPart.TypeArguments);
					instance.Free();
				}
				else
				{
					_state.SpinWaitComplete(CompletionPart.TypeArguments, cancellationToken);
				}
				break;
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(CompletionPart.NamespaceSymbolAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type | CompletionPart.TypeParameters | CompletionPart.TypeMembers | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompletedChecksStarted);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	protected virtual void ValidatePropertyType(BindingDiagnosticBag diagnostics)
	{
		TypeSymbol type = base.Type;
		if (type.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_FieldCantBeRefAny, TypeLocation, type);
		}
		else if (IsAutoPropertyOrUsesFieldKeyword)
		{
			if (!IsStatic && (ContainingType.IsRecord || ContainingType.IsRecordStruct) && type.IsPointerOrFunctionPointer())
			{
				diagnostics.Add(ErrorCode.ERR_BadFieldTypeInRecord, TypeLocation, type);
			}
			else if (type.IsRefLikeOrAllowsRefLikeType() && (IsStatic || !ContainingType.IsRefLikeType))
			{
				diagnostics.Add(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, TypeLocation, type);
			}
		}
		if (type.IsStatic)
		{
			if ((object)GetMethod != null)
			{
				diagnostics.Add(ErrorFacts.GetStaticClassReturnCode(ContainingType.IsInterfaceType()), TypeLocation, type);
			}
			else if ((object)SetMethod != null)
			{
				diagnostics.Add(ErrorFacts.GetStaticClassParameterCode(ContainingType.IsInterfaceType()), TypeLocation, type);
			}
		}
	}

	protected abstract (TypeWithAnnotations Type, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindType(BindingDiagnosticBag diagnostics);

	protected static ExplicitInterfaceSpecifierSyntax? GetExplicitInterfaceSpecifier(SyntaxNode syntax)
	{
		return (syntax as BasePropertyDeclarationSyntax)?.ExplicitInterfaceSpecifier;
	}

	internal ExplicitInterfaceSpecifierSyntax? GetExplicitInterfaceSpecifier()
	{
		return GetExplicitInterfaceSpecifier(CSharpSyntaxNode);
	}
}
