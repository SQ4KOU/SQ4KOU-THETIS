using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourcePropertyAccessorSymbol : SourceMemberMethodSymbol
{
	private readonly SourcePropertySymbolBase _property;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeWithAnnotations _lazyReturnType;

	private ImmutableArray<CustomModifier> _lazyRefCustomModifiers;

	private ImmutableArray<MethodSymbol> _lazyExplicitInterfaceImplementations;

	private string _lazyName;

	private readonly bool _isAutoPropertyAccessor;

	private readonly bool _usesInit;

	internal sealed override ImmutableArray<string> NotNullMembers => _property.NotNullMembers.Concat(base.NotNullMembers);

	internal sealed override ImmutableArray<string> NotNullWhenTrueMembers => _property.NotNullWhenTrueMembers.Concat(base.NotNullWhenTrueMembers);

	internal sealed override ImmutableArray<string> NotNullWhenFalseMembers => _property.NotNullWhenFalseMembers.Concat(base.NotNullWhenFalseMembers);

	public sealed override Accessibility DeclaredAccessibility
	{
		get
		{
			Accessibility localAccessibility = LocalAccessibility;
			if (localAccessibility != Accessibility.NotApplicable)
			{
				return localAccessibility;
			}
			return _property.DeclaredAccessibility;
		}
	}

	public sealed override Symbol AssociatedSymbol => _property;

	public sealed override bool ReturnsVoid => base.ReturnType.IsVoidType();

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
			return _lazyParameters;
		}
	}

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public sealed override TypeWithAnnotations ReturnTypeWithAnnotations
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	public sealed override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations
	{
		get
		{
			if (MethodKind == MethodKind.PropertySet)
			{
				return FlowAnalysisAnnotations.None;
			}
			FlowAnalysisAnnotations flowAnalysisAnnotations = FlowAnalysisAnnotations.None;
			if (_property.HasMaybeNull)
			{
				flowAnalysisAnnotations |= FlowAnalysisAnnotations.MaybeNull;
			}
			if (_property.HasNotNull)
			{
				flowAnalysisAnnotations |= FlowAnalysisAnnotations.NotNull;
			}
			return flowAnalysisAnnotations;
		}
	}

	public sealed override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			LazyMethodChecks();
			return _lazyRefCustomModifiers;
		}
	}

	internal Accessibility LocalAccessibility => ModifierUtils.EffectiveAccessibility(DeclarationModifiers);

	internal bool LocalDeclaredReadOnly => (DeclarationModifiers & DeclarationModifiers.ReadOnly) != 0;

	internal sealed override bool IsDeclaredReadOnly
	{
		get
		{
			if (LocalDeclaredReadOnly || (_property.HasReadOnlyModifier && base.IsValidReadOnlyTarget))
			{
				return true;
			}
			if (!((CSharpParseOptions)base.SyntaxTree.Options).IsFeatureEnabled(MessageID.IDS_FeatureReadOnlyMembers))
			{
				return false;
			}
			if (!(DeclaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_IsReadOnlyAttribute__ctor) != null) && (DeclaringCompilation.Options.OutputKind == OutputKind.NetModule || !(DeclaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_IsReadOnlyAttribute) is MissingMetadataTypeSymbol)))
			{
				return false;
			}
			if (ContainingType.IsStructType() && !_property.IsStatic && _isAutoPropertyAccessor)
			{
				return MethodKind == MethodKind.PropertyGet;
			}
			return false;
		}
	}

	internal bool IsAutoPropertyAccessor => _isAutoPropertyAccessor;

	internal sealed override bool IsInitOnly
	{
		get
		{
			if (!IsStatic)
			{
				return _usesInit;
			}
			return false;
		}
	}

	internal sealed override bool IsExplicitInterfaceImplementation => _property.IsExplicitInterfaceImplementation;

	public sealed override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
	{
		get
		{
			if (_lazyExplicitInterfaceImplementations.IsDefault)
			{
				PropertySymbol propertySymbol = (IsExplicitInterfaceImplementation ? _property.ExplicitInterfaceImplementations.FirstOrDefault() : null);
				ImmutableArray<MethodSymbol> value;
				if ((object)propertySymbol == null)
				{
					value = ImmutableArray<MethodSymbol>.Empty;
				}
				else
				{
					MethodSymbol methodSymbol = ((MethodKind == MethodKind.PropertyGet) ? propertySymbol.GetMethod : propertySymbol.SetMethod);
					value = (((object)methodSymbol == null) ? ImmutableArray<MethodSymbol>.Empty : ImmutableArray.Create(methodSymbol));
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyExplicitInterfaceImplementations, value);
			}
			return _lazyExplicitInterfaceImplementations;
		}
	}

	private SyntaxList<AttributeListSyntax> AttributeDeclarationList
	{
		get
		{
			if (_property.ContainingType is SourceMemberContainerTypeSymbol { AnyMemberHasAttributes: not false })
			{
				CSharpSyntaxNode syntax = GetSyntax();
				SyntaxKind syntaxKind = syntax.Kind();
				if (syntaxKind - 8896 <= SyntaxKind.List || syntaxKind == SyntaxKind.InitAccessorDeclaration)
				{
					return ((AccessorDeclarationSyntax)syntax).AttributeLists;
				}
			}
			return default(SyntaxList<AttributeListSyntax>);
		}
	}

	public sealed override string Name
	{
		get
		{
			if (_lazyName == null)
			{
				bool flag = MethodKind == MethodKind.PropertyGet;
				string text = null;
				if (IsExplicitInterfaceImplementation)
				{
					PropertySymbol propertySymbol = _property.ExplicitInterfaceImplementations.FirstOrDefault();
					if ((object)propertySymbol != null)
					{
						MethodSymbol methodSymbol = (flag ? propertySymbol.GetMethod : propertySymbol.SetMethod);
						text = ExplicitInterfaceHelpers.GetMemberName(((object)methodSymbol != null) ? methodSymbol.Name : GetAccessorName(propertySymbol.MetadataName, flag, _property.IsCompilationOutputWinMdObj()), aliasQualifierOpt: _property.GetExplicitInterfaceSpecifier()?.Name.GetAliasQualifierOpt(), explicitInterfaceTypeOpt: propertySymbol.ContainingType);
					}
				}
				else if (IsOverride)
				{
					MethodSymbol overriddenMethod = base.OverriddenMethod;
					if ((object)overriddenMethod != null)
					{
						text = overriddenMethod.Name;
					}
				}
				if (text == null)
				{
					text = GetAccessorName(_property.SourceName, flag, _property.IsCompilationOutputWinMdObj());
				}
				InterlockedOperations.Initialize(ref _lazyName, text);
			}
			return _lazyName;
		}
	}

	public override bool IsImplicitlyDeclared
	{
		get
		{
			SyntaxKind syntaxKind = GetSyntax().Kind();
			if (syntaxKind - 8896 <= SyntaxKind.List || syntaxKind == SyntaxKind.ArrowExpressionClause || syntaxKind == SyntaxKind.InitAccessorDeclaration)
			{
				return false;
			}
			return true;
		}
	}

	internal sealed override bool GenerateDebugInfo => true;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			if (!_property.HasSkipLocalsInitAttribute)
			{
				return base.AreLocalsZeroed;
			}
			return false;
		}
	}

	protected sealed override SourceMemberMethodSymbol? BoundAttributesSource => (SourceMemberMethodSymbol)PartialDefinitionPart;

	public sealed override MethodSymbol? PartialImplementationPart
	{
		get
		{
			if (_property is SourcePropertySymbol { IsPartialDefinition: not false } sourcePropertySymbol)
			{
				SourcePropertySymbol otherPartOfPartial = sourcePropertySymbol.OtherPartOfPartial;
				if ((object)otherPartOfPartial != null)
				{
					if (MethodKind != MethodKind.PropertyGet)
					{
						return otherPartOfPartial.SetMethod;
					}
					return otherPartOfPartial.GetMethod;
				}
			}
			return null;
		}
	}

	public sealed override MethodSymbol? PartialDefinitionPart
	{
		get
		{
			if (_property is SourcePropertySymbol { IsPartialImplementation: not false } sourcePropertySymbol)
			{
				SourcePropertySymbol otherPartOfPartial = sourcePropertySymbol.OtherPartOfPartial;
				if ((object)otherPartOfPartial != null)
				{
					if (MethodKind != MethodKind.PropertyGet)
					{
						return otherPartOfPartial.SetMethod;
					}
					return otherPartOfPartial.GetMethod;
				}
			}
			return null;
		}
	}

	internal bool IsPartialDefinition
	{
		get
		{
			if (_property is SourcePropertySymbol sourcePropertySymbol)
			{
				return sourcePropertySymbol.IsPartialDefinition;
			}
			return false;
		}
	}

	internal bool IsPartialImplementation
	{
		get
		{
			if (_property is SourcePropertySymbol sourcePropertySymbol)
			{
				return sourcePropertySymbol.IsPartialImplementation;
			}
			return false;
		}
	}

	public sealed override bool IsExtern => PartialImplementationPart?.IsExtern ?? base.IsExtern;

	public static SourcePropertyAccessorSymbol CreateAccessorSymbol(NamedTypeSymbol containingType, SourcePropertySymbol property, DeclarationModifiers propertyModifiers, AccessorDeclarationSyntax syntax, bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		MethodKind methodKind = ((syntax.Kind() == SyntaxKind.GetAccessorDeclaration) ? MethodKind.PropertyGet : MethodKind.PropertySet);
		bool hasBlockBody = syntax.Body != null;
		bool hasExpressionBody = syntax.ExpressionBody != null;
		bool isNullableAnalysisEnabled = containingType.DeclaringCompilation.IsNullableAnalysisEnabledIn(syntax);
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
		return new SourcePropertyAccessorSymbol(containingType, property, propertyModifiers, syntax.Keyword.GetLocation(), syntax, hasBlockBody, hasExpressionBody, SyntaxFacts.HasYieldOperations(syntax.Body), syntax.Modifiers, methodKind, syntax.Keyword.IsKind(SyntaxKind.InitKeyword), isAutoPropertyAccessor, isNullableAnalysisEnabled, diagnostics);
	}

	public static SourcePropertyAccessorSymbol CreateAccessorSymbol(NamedTypeSymbol containingType, SourcePropertySymbol property, DeclarationModifiers propertyModifiers, ArrowExpressionClauseSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		bool isNullableAnalysisEnabled = containingType.DeclaringCompilation.IsNullableAnalysisEnabledIn(syntax);
		return new SourcePropertyAccessorSymbol(containingType, property, propertyModifiers, syntax.Expression.GetLocation(), syntax, isNullableAnalysisEnabled, diagnostics);
	}

	public static SourcePropertyAccessorSymbol CreateAccessorSymbol(bool isGetMethod, bool usesInit, NamedTypeSymbol containingType, SynthesizedRecordPropertySymbol property, DeclarationModifiers propertyModifiers, Location location, CSharpSyntaxNode syntax, BindingDiagnosticBag diagnostics)
	{
		MethodKind methodKind = (isGetMethod ? MethodKind.PropertyGet : MethodKind.PropertySet);
		return new SourcePropertyAccessorSymbol(containingType, property, propertyModifiers, location, syntax, hasBlockBody: false, hasExpressionBody: false, isIterator: false, default(SyntaxTokenList), methodKind, usesInit, isAutoPropertyAccessor: true, isNullableAnalysisEnabled: false, diagnostics);
	}

	public static SourcePropertyAccessorSymbol CreateAccessorSymbol(NamedTypeSymbol containingType, SynthesizedRecordEqualityContractProperty property, DeclarationModifiers propertyModifiers, Location location, CSharpSyntaxNode syntax, BindingDiagnosticBag diagnostics)
	{
		return new SynthesizedRecordEqualityContractProperty.GetAccessorSymbol(containingType, property, propertyModifiers, location, syntax, diagnostics);
	}

	private SourcePropertyAccessorSymbol(NamedTypeSymbol containingType, SourcePropertySymbol property, DeclarationModifiers propertyModifiers, Location location, ArrowExpressionClauseSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), location, isIterator: false, MakeModifiersAndFlags(containingType, property, propertyModifiers, location, hasBlockBody: false, hasExpressionBody: true, SyntaxTokenList.Create(default(ReadOnlySpan<SyntaxToken>)), MethodKind.PropertyGet, isNullableAnalysisEnabled, diagnostics, out var _))
	{
		_property = property;
		_isAutoPropertyAccessor = false;
		CheckFeatureAvailabilityAndRuntimeSupport(syntax, location, hasBody: true, diagnostics);
		CheckModifiersForBody(location, diagnostics);
		ModifierUtils.CheckAccessibility(DeclarationModifiers, this, property.IsExplicitInterfaceImplementation, diagnostics, location);
		CheckModifiers(location, hasBody: true, isAutoPropertyOrExpressionBodied: true, diagnostics);
	}

	protected SourcePropertyAccessorSymbol(NamedTypeSymbol containingType, SourcePropertySymbolBase property, DeclarationModifiers propertyModifiers, Location location, CSharpSyntaxNode syntax, bool hasBlockBody, bool hasExpressionBody, bool isIterator, SyntaxTokenList modifiers, MethodKind methodKind, bool usesInit, bool isAutoPropertyAccessor, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), location, isIterator, MakeModifiersAndFlags(containingType, property, propertyModifiers, location, hasBlockBody, hasExpressionBody, modifiers, methodKind, isNullableAnalysisEnabled, diagnostics, out var modifierErrors))
	{
		_property = property;
		_isAutoPropertyAccessor = isAutoPropertyAccessor;
		bool flag = hasBlockBody | hasExpressionBody;
		_usesInit = usesInit;
		if (_usesInit)
		{
			Binder.CheckFeatureAvailability(syntax, MessageID.IDS_FeatureInitOnlySetters, diagnostics, location);
		}
		CheckFeatureAvailabilityAndRuntimeSupport(syntax, location, flag | isAutoPropertyAccessor, diagnostics);
		if (flag)
		{
			CheckModifiersForBody(location, diagnostics);
		}
		ModifierUtils.CheckAccessibility(DeclarationModifiers, this, property.IsExplicitInterfaceImplementation, diagnostics, location);
		if (!modifierErrors)
		{
			CheckModifiers(location, flag, isAutoPropertyAccessor, diagnostics);
		}
		if (modifiers.Count > 0)
		{
			MessageID.IDS_FeaturePropertyAccessorMods.CheckFeatureAvailability(diagnostics, modifiers[0]);
		}
	}

	private static (DeclarationModifiers, Flags) MakeModifiersAndFlags(NamedTypeSymbol containingType, SourcePropertySymbolBase property, DeclarationModifiers propertyModifiers, Location location, bool hasBlockBody, bool hasExpressionBody, SyntaxTokenList modifiers, MethodKind methodKind, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics, out bool modifierErrors)
	{
		bool isExpressionBodied = !hasBlockBody & hasExpressionBody;
		bool hasBody = hasBlockBody | hasExpressionBody;
		bool isExplicitInterfaceImplementation = property.IsExplicitInterfaceImplementation;
		DeclarationModifiers declarationModifiers = MakeModifiers(containingType, modifiers, isExplicitInterfaceImplementation, hasBody, location, diagnostics, out modifierErrors);
		declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers | ((uint)GetAccessorModifiers(propertyModifiers) & 0xFFFFFC0Fu));
		if ((declarationModifiers & DeclarationModifiers.Private) != DeclarationModifiers.None)
		{
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFFDFFFFu);
		}
		Flags item = SourceMemberMethodSymbol.MakeFlags(methodKind, property.RefKind, declarationModifiers, returnsVoid: false, returnsVoidIsSet: false, isExpressionBodied, isExtensionMethod: false, isNullableAnalysisEnabled, isVarArg: false, isExplicitInterfaceImplementation, hasThisInitializer: false);
		return (declarationModifiers, item);
	}

	private static DeclarationModifiers GetAccessorModifiers(DeclarationModifiers propertyModifiers)
	{
		return (DeclarationModifiers)((uint)propertyModifiers & 0xFFF7FBFFu);
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return TryGetBodyBinderFromSyntax(binderFactoryOpt, ignoreAccessibility);
	}

	protected sealed override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		_lazyParameters = ComputeParameters();
		_lazyReturnType = ComputeReturnType(diagnostics);
		_lazyRefCustomModifiers = ImmutableArray<CustomModifier>.Empty;
		ImmutableArray<MethodSymbol> explicitInterfaceImplementations = ExplicitInterfaceImplementations;
		if (explicitInterfaceImplementations.Length > 0)
		{
			CustomModifierUtils.CopyMethodCustomModifiers(explicitInterfaceImplementations[0], this, out _lazyReturnType, out _lazyRefCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: false);
		}
		else if (IsOverride)
		{
			MethodSymbol overriddenMethod = base.OverriddenMethod;
			if ((object)overriddenMethod != null)
			{
				CustomModifierUtils.CopyMethodCustomModifiers(overriddenMethod, this, out _lazyReturnType, out _lazyRefCustomModifiers, out _lazyParameters, alsoCopyParamsModifier: true);
			}
		}
		else if (!_lazyReturnType.IsVoidType())
		{
			PropertySymbol property = _property;
			TypeWithAnnotations typeWithAnnotations = property.TypeWithAnnotations;
			_lazyReturnType = _lazyReturnType.WithTypeAndModifiers(CustomModifierUtils.CopyTypeCustomModifiers(typeWithAnnotations.Type, _lazyReturnType.Type, ContainingAssembly), typeWithAnnotations.CustomModifiers);
			_lazyRefCustomModifiers = property.RefCustomModifiers;
		}
	}

	public sealed override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public sealed override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	private TypeWithAnnotations ComputeReturnType(BindingDiagnosticBag diagnostics)
	{
		if (MethodKind == MethodKind.PropertyGet)
		{
			return _property.TypeWithAnnotations;
		}
		TypeWithAnnotations result = TypeWithAnnotations.Create(GetBinder().GetSpecialType(SpecialType.System_Void, diagnostics, GetSyntax()));
		if (IsInitOnly)
		{
			ImmutableArray<CustomModifier> customModifiers = ImmutableArray.Create(CSharpCustomModifier.CreateRequired(Binder.GetWellKnownType(DeclaringCompilation, WellKnownType.System_Runtime_CompilerServices_IsExternalInit, diagnostics, _location)));
			result = result.WithModifiers(customModifiers);
		}
		return result;
	}

	private Binder GetBinder()
	{
		CSharpSyntaxNode syntax = GetSyntax();
		return DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax);
	}

	private static DeclarationModifiers MakeModifiers(NamedTypeSymbol containingType, SyntaxTokenList modifiers, bool isExplicitInterfaceImplementation, bool hasBody, Location location, BindingDiagnosticBag diagnostics, out bool modifierErrors)
	{
		DeclarationModifiers declarationModifiers = ((!isExplicitInterfaceImplementation) ? DeclarationModifiers.AccessibilityMask : DeclarationModifiers.None);
		if (containingType.IsStructType())
		{
			declarationModifiers |= DeclarationModifiers.ReadOnly;
		}
		DeclarationModifiers defaultInterfaceImplementationModifiers = DeclarationModifiers.None;
		bool isInterface = containingType.IsInterface;
		if (isInterface && !isExplicitInterfaceImplementation)
		{
			defaultInterfaceImplementationModifiers = DeclarationModifiers.AccessibilityMask;
		}
		DeclarationModifiers declarationModifiers2 = ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: false, isInterface, modifiers, DeclarationModifiers.None, declarationModifiers, location, diagnostics, out modifierErrors, out var _);
		ModifierUtils.ReportDefaultInterfaceImplementationModifiers(hasBody, declarationModifiers2, defaultInterfaceImplementationModifiers, location, diagnostics);
		return declarationModifiers2;
	}

	private void CheckModifiers(Location location, bool hasBody, bool isAutoPropertyOrExpressionBodied, BindingDiagnosticBag diagnostics)
	{
		Accessibility localAccessibility = LocalAccessibility;
		if (IsAbstract && !ContainingType.IsAbstract && (ContainingType.TypeKind == TypeKind.Class || ContainingType.TypeKind == TypeKind.Submission))
		{
			diagnostics.Add(ErrorCode.ERR_AbstractInConcreteClass, location, this, ContainingType);
		}
		else if (IsVirtual && ContainingType.IsSealed && ContainingType.TypeKind != TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_NewVirtualInSealed, location, this, ContainingType);
		}
		else if (!hasBody && !IsExtern && !IsAbstract && !isAutoPropertyOrExpressionBodied && !IsPartialDefinition)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		else if (ContainingType.IsSealed && localAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
		}
		else if (LocalDeclaredReadOnly && _property.HasReadOnlyModifier)
		{
			diagnostics.Add(ErrorCode.ERR_InvalidPropertyReadOnlyMods, location, _property);
		}
		else if (LocalDeclaredReadOnly && IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_StaticMemberCantBeReadOnly, location, this);
		}
		else if (ContainingType.IsExtension && IsInitOnly)
		{
			diagnostics.Add(ErrorCode.ERR_InitInExtension, location, _property);
		}
		else if (LocalDeclaredReadOnly && IsInitOnly)
		{
			diagnostics.Add(ErrorCode.ERR_InitCannotBeReadonly, location, _property);
		}
		else if (LocalDeclaredReadOnly && _isAutoPropertyAccessor && MethodKind == MethodKind.PropertySet)
		{
			diagnostics.Add(ErrorCode.ERR_AutoSetterCantBeReadOnly, location, this);
		}
		else if (_usesInit && IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_BadInitAccessor, location);
		}
	}

	internal static string GetAccessorName(string propertyName, bool getNotSet, bool isWinMdOutput)
	{
		return (getNotSet ? "get_" : (isWinMdOutput ? "put_" : "set_")) + propertyName;
	}

	internal CSharpSyntaxNode GetSyntax()
	{
		return (CSharpSyntaxNode)syntaxReferenceOpt.GetSyntax();
	}

	internal sealed override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		MethodSymbol partialImplementationPart = PartialImplementationPart;
		if ((object)partialImplementationPart != null)
		{
			return OneOrMany.Create(AttributeDeclarationList, ((SourcePropertyAccessorSymbol)partialImplementationPart).AttributeDeclarationList);
		}
		MethodSymbol partialDefinitionPart = PartialDefinitionPart;
		if ((object)partialDefinitionPart != null)
		{
			return OneOrMany.Create(AttributeDeclarationList, ((SourcePropertyAccessorSymbol)partialDefinitionPart).AttributeDeclarationList);
		}
		return OneOrMany.Create(AttributeDeclarationList);
	}

	private ImmutableArray<ParameterSymbol> ComputeParameters()
	{
		bool flag = MethodKind == MethodKind.PropertyGet;
		ImmutableArray<ParameterSymbol> parameters = _property.Parameters;
		int num = parameters.Length + ((!flag) ? 1 : 0);
		if (num == 0)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(num);
		foreach (SourceParameterSymbol item in parameters)
		{
			instance.Add(new SourcePropertyClonedParameterSymbolForAccessors(item, this));
		}
		if (!flag)
		{
			instance.Add(new SynthesizedPropertyAccessorValueParameterSymbol(this, instance.Count));
		}
		return instance.ToImmutableAndFree();
	}

	internal sealed override void AddSynthesizedReturnTypeAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedReturnTypeAttributes(moduleBuilder, ref attributes);
		AddSynthesizedReturnTypeFlowAnalysisAttributes(ref attributes);
	}

	internal void AddSynthesizedReturnTypeFlowAnalysisAttributes(ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		FlowAnalysisAnnotations returnTypeFlowAnalysisAnnotations = ReturnTypeFlowAnalysisAnnotations;
		if ((returnTypeFlowAnalysisAnnotations & FlowAnalysisAnnotations.MaybeNull) != FlowAnalysisAnnotations.None)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(_property.MaybeNullAttributeIfExists));
		}
		if ((returnTypeFlowAnalysisAnnotations & FlowAnalysisAnnotations.NotNull) != FlowAnalysisAnnotations.None)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(_property.NotNullAttributeIfExists));
		}
	}

	internal void PartialAccessorChecks(SourcePropertyAccessorSymbol implementationAccessor, BindingDiagnosticBag diagnostics)
	{
		if (LocalAccessibility != implementationAccessor.LocalAccessibility)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberAccessibilityDifference, implementationAccessor.GetFirstLocation());
		}
		if (LocalDeclaredReadOnly != implementationAccessor.LocalDeclaredReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberReadOnlyDifference, implementationAccessor.GetFirstLocation());
		}
		if (_usesInit != implementationAccessor._usesInit)
		{
			string text = (_usesInit ? "init" : "set");
			diagnostics.Add(ErrorCode.ERR_PartialPropertyInitMismatch, implementationAccessor.GetFirstLocation(), implementationAccessor, text);
		}
	}
}
