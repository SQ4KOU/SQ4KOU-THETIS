using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourcePropertySymbol : SourcePropertySymbolBase
{
	private SourcePropertySymbol? _otherPartOfPartial;

	protected override Location TypeLocation => GetTypeSyntax(base.CSharpSyntaxNode).Location;

	private SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (ContainingType is SourceMemberContainerTypeSymbol { AnyMemberHasAttributes: not false })
			{
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax)base.CSharpSyntaxNode).AttributeLists;
			}
			return default(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>);
		}
	}

	protected override SourcePropertySymbolBase? BoundAttributesSource => SourcePartialDefinitionPart;

	public override IAttributeTargetSymbol AttributesOwner => this;

	public sealed override bool IsExtern => PartialImplementationPart?.IsExtern ?? base.HasExternModifier;

	internal SourcePropertySymbol? OtherPartOfPartial => _otherPartOfPartial;

	internal bool IsPartialDefinition
	{
		get
		{
			if (base.IsPartial && !base.AccessorsHaveImplementation)
			{
				return !base.HasExternModifier;
			}
			return false;
		}
	}

	internal bool IsPartialImplementation
	{
		get
		{
			if (base.IsPartial)
			{
				if (!base.AccessorsHaveImplementation)
				{
					return base.HasExternModifier;
				}
				return true;
			}
			return false;
		}
	}

	internal SourcePropertySymbol? SourcePartialDefinitionPart
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

	internal SourcePropertySymbol? SourcePartialImplementationPart
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

	internal sealed override PropertySymbol? PartialDefinitionPart => SourcePartialDefinitionPart;

	internal sealed override PropertySymbol? PartialImplementationPart => SourcePartialImplementationPart;

	internal static SourcePropertySymbol Create(SourceMemberContainerTypeSymbol containingType, Binder bodyBinder, Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		SyntaxToken identifier = syntax.Identifier;
		Location location = identifier.GetLocation();
		return Create(containingType, bodyBinder, syntax, identifier.ValueText, location, diagnostics);
	}

	internal static SourcePropertySymbol Create(SourceMemberContainerTypeSymbol containingType, Binder bodyBinder, Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		Location location = syntax.ThisKeyword.GetLocation();
		return Create(containingType, bodyBinder, syntax, "Item", location, diagnostics);
	}

	private static SourcePropertySymbol Create(SourceMemberContainerTypeSymbol containingType, Binder binder, Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax syntax, string name, Location location, BindingDiagnosticBag diagnostics)
	{
		GetAccessorDeclarations(syntax, diagnostics, out bool isExpressionBodied, out bool hasGetAccessorImplementation, out bool hasSetAccessorImplementation, out bool getterUsesFieldKeyword, out bool setterUsesFieldKeyword, out Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax getSyntax, out Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax setSyntax);
		bool accessorsHaveImplementation = hasGetAccessorImplementation | hasSetAccessorImplementation;
		Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = SourcePropertySymbolBase.GetExplicitInterfaceSpecifier(syntax);
		SyntaxTokenList modifierTokensSyntax = GetModifierTokensSyntax(syntax);
		bool isExplicitInterfaceImplementation = explicitInterfaceSpecifier != null;
		var (declarationModifiers, hasExplicitAccessMod) = MakeModifiers(containingType, modifierTokensSyntax, isExplicitInterfaceImplementation, syntax.Kind() == SyntaxKind.IndexerDeclaration, accessorsHaveImplementation, location, diagnostics, out var _);
		int num;
		int num2;
		if ((declarationModifiers & (DeclarationModifiers.Abstract | DeclarationModifiers.Extern | DeclarationModifiers.Indexer)) == 0 && ((!containingType.IsInterface | hasGetAccessorImplementation | hasSetAccessorImplementation) || (declarationModifiers & DeclarationModifiers.Static) != DeclarationModifiers.None))
		{
			num = ((((declarationModifiers & DeclarationModifiers.Partial) == 0) | hasGetAccessorImplementation | hasSetAccessorImplementation) ? 1 : 0);
			if (num != 0 && getSyntax != null)
			{
				num2 = ((!hasGetAccessorImplementation) ? 1 : 0);
				goto IL_009f;
			}
		}
		else
		{
			num = 0;
		}
		num2 = 0;
		goto IL_009f;
		IL_009f:
		bool hasAutoPropertyGet = (byte)num2 != 0;
		bool hasAutoPropertySet = num != 0 && setSyntax != null && !hasSetAccessorImplementation;
		string memberNameAndInterfaceSymbol = ExplicitInterfaceHelpers.GetMemberNameAndInterfaceSymbol(binder, modifierTokensSyntax, explicitInterfaceSpecifier, name, diagnostics, out var explicitInterfaceTypeOpt, out var aliasQualifierOpt);
		return new SourcePropertySymbol(containingType, syntax, (getSyntax != null) | isExpressionBodied, setSyntax != null, isExplicitInterfaceImplementation, explicitInterfaceTypeOpt, aliasQualifierOpt, declarationModifiers, hasExplicitAccessMod, hasAutoPropertyGet, hasAutoPropertySet, isExpressionBodied, accessorsHaveImplementation, getterUsesFieldKeyword, setterUsesFieldKeyword, memberNameAndInterfaceSymbol, location, diagnostics);
	}

	private SourcePropertySymbol(SourceMemberContainerTypeSymbol containingType, Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax syntax, bool hasGetAccessor, bool hasSetAccessor, bool isExplicitInterfaceImplementation, TypeSymbol? explicitInterfaceType, string? aliasQualifierOpt, DeclarationModifiers modifiers, bool hasExplicitAccessMod, bool hasAutoPropertyGet, bool hasAutoPropertySet, bool isExpressionBodied, bool accessorsHaveImplementation, bool getterUsesFieldKeyword, bool setterUsesFieldKeyword, string memberName, Location location, BindingDiagnosticBag diagnostics)
		: base(containingType, syntax, hasGetAccessor, hasSetAccessor, isExplicitInterfaceImplementation, explicitInterfaceType, aliasQualifierOpt, modifiers, HasInitializer(syntax), hasExplicitAccessMod, hasAutoPropertyGet, hasAutoPropertySet, isExpressionBodied, accessorsHaveImplementation, getterUsesFieldKeyword, setterUsesFieldKeyword, syntax.Type.SkipScoped(out var _).GetRefKindInLocalOrReturn(diagnostics), memberName, syntax.AttributeLists, location, diagnostics)
	{
		if (hasAutoPropertyGet | hasAutoPropertySet)
		{
			Binder.CheckFeatureAvailability(syntax, (!(hasGetAccessor & hasSetAccessor)) ? (hasAutoPropertyGet ? MessageID.IDS_FeatureReadonlyAutoImplementedProperties : MessageID.IDS_FeatureAutoImplementedProperties) : ((hasAutoPropertyGet & hasAutoPropertySet) ? MessageID.IDS_FeatureAutoImplementedProperties : MessageID.IDS_FeatureFieldKeyword), diagnostics, location);
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.AccessorList, syntax.GetExpressionBodySyntax(), syntax, diagnostics);
		if (syntax is Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax propertyDeclarationSyntax)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax initializer = propertyDeclarationSyntax.Initializer;
			if (initializer != null)
			{
				MessageID.IDS_FeatureAutoPropertyInitializer.CheckFeatureAvailability(diagnostics, initializer.EqualsToken);
			}
		}
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		PartialImplementationPart?.ForceComplete(locationOpt, filter, cancellationToken);
		base.ForceComplete(locationOpt, filter, cancellationToken);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax GetTypeSyntax(SyntaxNode syntax)
	{
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax)syntax).Type;
	}

	private static SyntaxTokenList GetModifierTokensSyntax(SyntaxNode syntax)
	{
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax)syntax).Modifiers;
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax? GetArrowExpression(SyntaxNode syntax)
	{
		if (!(syntax is Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax propertyDeclarationSyntax))
		{
			if (syntax is Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax indexerDeclarationSyntax)
			{
				return indexerDeclarationSyntax.ExpressionBody;
			}
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
		}
		return propertyDeclarationSyntax.ExpressionBody;
	}

	private static bool HasInitializer(SyntaxNode syntax)
	{
		if (syntax is Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax propertyDeclarationSyntax)
		{
			return propertyDeclarationSyntax.Initializer != null;
		}
		return false;
	}

	public override OneOrMany<SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>> GetAttributeDeclarations()
	{
		SourcePropertySymbol sourcePartialImplementationPart = SourcePartialImplementationPart;
		if ((object)sourcePartialImplementationPart != null)
		{
			return OneOrMany.Create(AttributeDeclarationSyntaxList, sourcePartialImplementationPart.AttributeDeclarationSyntaxList);
		}
		return OneOrMany.Create(AttributeDeclarationSyntaxList);
	}

	private static void GetAccessorDeclarations(CSharpSyntaxNode syntaxNode, BindingDiagnosticBag diagnostics, out bool isExpressionBodied, out bool hasGetAccessorImplementation, out bool hasSetAccessorImplementation, out bool getterUsesFieldKeyword, out bool setterUsesFieldKeyword, out Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax? getSyntax, out Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax? setSyntax)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax basePropertyDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax)syntaxNode;
		isExpressionBodied = basePropertyDeclarationSyntax.AccessorList == null;
		getSyntax = null;
		setSyntax = null;
		if (!isExpressionBodied)
		{
			getterUsesFieldKeyword = false;
			setterUsesFieldKeyword = false;
			hasGetAccessorImplementation = false;
			hasSetAccessorImplementation = false;
			foreach (Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax accessor in basePropertyDeclarationSyntax.AccessorList.Accessors)
			{
				switch (accessor.Kind())
				{
				case SyntaxKind.GetAccessorDeclaration:
					if (getSyntax == null)
					{
						getSyntax = accessor;
						hasGetAccessorImplementation = hasImplementation(accessor);
						getterUsesFieldKeyword = containsFieldExpressionInAccessor(accessor);
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateAccessor, accessor.Keyword.GetLocation());
					}
					break;
				case SyntaxKind.SetAccessorDeclaration:
				case SyntaxKind.InitAccessorDeclaration:
					if (setSyntax == null)
					{
						setSyntax = accessor;
						hasSetAccessorImplementation = hasImplementation(accessor);
						setterUsesFieldKeyword = containsFieldExpressionInAccessor(accessor);
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateAccessor, accessor.Keyword.GetLocation());
					}
					break;
				case SyntaxKind.AddAccessorDeclaration:
				case SyntaxKind.RemoveAccessorDeclaration:
					diagnostics.Add(ErrorCode.ERR_GetOrSetExpected, accessor.Keyword.GetLocation());
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(accessor.Kind());
				case SyntaxKind.UnknownAccessorDeclaration:
					break;
				}
			}
		}
		else
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax arrowExpression = GetArrowExpression(basePropertyDeclarationSyntax);
			hasGetAccessorImplementation = arrowExpression != null;
			hasSetAccessorImplementation = false;
			getterUsesFieldKeyword = arrowExpression != null && containsFieldExpressionInGreenNode(arrowExpression.Green);
			setterUsesFieldKeyword = false;
		}
		static bool containsFieldExpressionInAccessor(Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax syntax)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorDeclarationSyntax accessorDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorDeclarationSyntax)syntax.Green;
			foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax attributeList in accessorDeclarationSyntax.AttributeLists)
			{
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeSyntax> attributes = attributeList.Attributes;
				for (int i = 0; i < attributes.Count; i++)
				{
					if (containsFieldExpressionInGreenNode(attributes[i]))
					{
						return true;
					}
				}
			}
			if (!containsFieldExpressionInGreenNode(accessorDeclarationSyntax.Body))
			{
				return containsFieldExpressionInGreenNode(accessorDeclarationSyntax.ExpressionBody);
			}
			return true;
		}
		static bool containsFieldExpressionInGreenNode(GreenNode? green)
		{
			if (green != null)
			{
				foreach (GreenNode item in green.EnumerateNodes())
				{
					if (item.RawKind == 8757)
					{
						return true;
					}
				}
			}
			return false;
		}
		static bool hasImplementation(Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax accessor)
		{
			return (((object)accessor.Body) ?? ((object)accessor.ExpressionBody)) != null;
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax GetGetAccessorDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax syntax)
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax accessor in syntax.AccessorList.Accessors)
		{
			if (accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
			{
				return accessor;
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourcePropertySymbol.cs", 353);
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax GetSetAccessorDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax syntax)
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax accessor in syntax.AccessorList.Accessors)
		{
			SyntaxKind syntaxKind = accessor.Kind();
			if (syntaxKind == SyntaxKind.SetAccessorDeclaration || syntaxKind == SyntaxKind.InitAccessorDeclaration)
			{
				return accessor;
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourcePropertySymbol.cs", 368);
	}

	private static (DeclarationModifiers modifiers, bool hasExplicitAccessMod) MakeModifiers(NamedTypeSymbol containingType, SyntaxTokenList modifiers, bool isExplicitInterfaceImplementation, bool isIndexer, bool accessorsHaveImplementation, Location location, BindingDiagnosticBag diagnostics, out bool modifierErrors)
	{
		bool isInterface = containingType.IsInterface;
		bool isExtension = containingType.IsExtension;
		DeclarationModifiers defaultAccess = ((isInterface && !isExplicitInterfaceImplementation) ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
		DeclarationModifiers declarationModifiers2 = DeclarationModifiers.None;
		if (!isExplicitInterfaceImplementation)
		{
			declarationModifiers |= DeclarationModifiers.AccessibilityMask;
			if (!isExtension)
			{
				declarationModifiers |= DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.New | DeclarationModifiers.Virtual;
			}
			if (!isIndexer)
			{
				declarationModifiers |= DeclarationModifiers.Static;
			}
			if (!isExtension)
			{
				if (!isInterface)
				{
					declarationModifiers |= DeclarationModifiers.Override;
					if (!isIndexer)
					{
						declarationModifiers |= DeclarationModifiers.Required;
					}
				}
				else
				{
					defaultAccess = DeclarationModifiers.None;
					declarationModifiers2 = (DeclarationModifiers)((uint)declarationModifiers2 | (uint)(3 | ((!isIndexer) ? 4 : 0) | 0x20000 | 0x2000 | 0x3F0));
				}
			}
		}
		else
		{
			if (isInterface)
			{
				declarationModifiers |= DeclarationModifiers.Abstract;
			}
			if (!isIndexer)
			{
				declarationModifiers |= DeclarationModifiers.Static;
			}
		}
		if (containingType.IsStructType())
		{
			declarationModifiers |= DeclarationModifiers.ReadOnly;
		}
		declarationModifiers |= DeclarationModifiers.Extern;
		DeclarationModifiers declarationModifiers3 = ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: false, isInterface, modifiers, defaultAccess, declarationModifiers, location, diagnostics, out modifierErrors, out var hasExplicitAccessModifier);
		if ((declarationModifiers3 & DeclarationModifiers.Partial) != DeclarationModifiers.None)
		{
			LanguageVersion languageVersion = ((CSharpParseOptions)location.SourceTree.Options).LanguageVersion;
			LanguageVersion languageVersion2 = MessageID.IDS_FeaturePartialProperties.RequiredVersion();
			if (languageVersion < languageVersion2)
			{
				ModifierUtils.ReportUnsupportedModifiersForLanguageVersion(declarationModifiers3, DeclarationModifiers.Partial, location, diagnostics, languageVersion, languageVersion2);
			}
		}
		ModifierUtils.CheckFeatureAvailabilityForStaticAbstractMembersInInterfacesIfNeeded(declarationModifiers3, isExplicitInterfaceImplementation, location, diagnostics);
		containingType.CheckUnsafeModifier(declarationModifiers3, location, diagnostics);
		ModifierUtils.ReportDefaultInterfaceImplementationModifiers(accessorsHaveImplementation, declarationModifiers3, declarationModifiers2, location, diagnostics);
		if (isInterface)
		{
			declarationModifiers3 = ModifierUtils.AdjustModifiersForAnInterfaceMember(declarationModifiers3, accessorsHaveImplementation, isExplicitInterfaceImplementation, forMethod: false);
		}
		if (isIndexer)
		{
			declarationModifiers3 |= DeclarationModifiers.Indexer;
		}
		if ((declarationModifiers3 & DeclarationModifiers.Static) != DeclarationModifiers.None && (declarationModifiers3 & DeclarationModifiers.Required) != DeclarationModifiers.None)
		{
			diagnostics.Add(ErrorCode.ERR_BadMemberFlag, location, SyntaxFacts.GetText(SyntaxKind.RequiredKeyword));
			declarationModifiers3 = (DeclarationModifiers)((uint)declarationModifiers3 & 0xFFBFFFFFu);
		}
		return (modifiers: declarationModifiers3, hasExplicitAccessMod: hasExplicitAccessModifier);
	}

	protected override SourcePropertyAccessorSymbol CreateGetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax basePropertyDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax)base.CSharpSyntaxNode;
		Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax arrowExpression = GetArrowExpression(basePropertyDeclarationSyntax);
		if (basePropertyDeclarationSyntax.AccessorList == null && arrowExpression != null)
		{
			return CreateExpressionBodiedAccessor(arrowExpression, diagnostics);
		}
		return CreateAccessorSymbol(GetGetAccessorDeclaration(basePropertyDeclarationSyntax), isAutoPropertyAccessor, diagnostics);
	}

	protected override SourcePropertyAccessorSymbol CreateSetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax syntax = (Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax)base.CSharpSyntaxNode;
		return CreateAccessorSymbol(GetSetAccessorDeclaration(syntax), isAutoPropertyAccessor, diagnostics);
	}

	private SourcePropertyAccessorSymbol CreateAccessorSymbol(Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax syntax, bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		return SourcePropertyAccessorSymbol.CreateAccessorSymbol(ContainingType, this, _modifiers, syntax, isAutoPropertyAccessor, diagnostics);
	}

	private SourcePropertyAccessorSymbol CreateExpressionBodiedAccessor(Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		return SourcePropertyAccessorSymbol.CreateAccessorSymbol(ContainingType, this, _modifiers, syntax, diagnostics);
	}

	private Binder CreateBinderForTypeAndParameters()
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		SyntaxTree syntaxTree = base.SyntaxTree;
		CSharpSyntaxNode cSharpSyntaxNode = base.CSharpSyntaxNode;
		Binder binder = declaringCompilation.GetBinderFactory(syntaxTree).GetBinder(cSharpSyntaxNode, cSharpSyntaxNode, this);
		SyntaxTokenList modifierTokensSyntax = GetModifierTokensSyntax(cSharpSyntaxNode);
		return binder.SetOrClearUnsafeRegionIfNecessary(modifierTokensSyntax).WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
	}

	protected override (TypeWithAnnotations Type, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindType(BindingDiagnosticBag diagnostics)
	{
		Binder binder = CreateBinderForTypeAndParameters();
		CSharpSyntaxNode cSharpSyntaxNode = base.CSharpSyntaxNode;
		return (Type: ComputeType(binder, cSharpSyntaxNode, diagnostics), Parameters: ComputeParameters(binder, cSharpSyntaxNode, diagnostics));
	}

	private TypeWithAnnotations ComputeType(Binder binder, SyntaxNode syntax, BindingDiagnosticBag diagnostics)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax typeSyntax = GetTypeSyntax(syntax);
		typeSyntax = typeSyntax.SkipScoped(out var _).SkipRef();
		TypeWithAnnotations typeWithAnnotations = binder.BindType(typeSyntax, diagnostics);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = binder.GetNewCompoundUseSiteInfo(diagnostics);
		if (GetExplicitInterfaceSpecifier() == null && !this.IsNoMoreVisibleThan(typeWithAnnotations, ref useSiteInfo))
		{
			diagnostics.Add(IsIndexer ? ErrorCode.ERR_BadVisIndexerReturn : ErrorCode.ERR_BadVisPropertyType, base.Location, this, typeWithAnnotations.Type);
		}
		if (typeWithAnnotations.Type.HasFileLocalTypes())
		{
			NamedTypeSymbol namedTypeSymbol = ContainingType;
			if ((object)namedTypeSymbol != null && namedTypeSymbol.IsExtension)
			{
				NamedTypeSymbol containingType = namedTypeSymbol.ContainingType;
				if ((object)containingType != null)
				{
					namedTypeSymbol = containingType;
				}
			}
			if (!namedTypeSymbol.HasFileLocalTypes())
			{
				diagnostics.Add(ErrorCode.ERR_FileTypeDisallowedInSignature, base.Location, typeWithAnnotations.Type, namedTypeSymbol);
			}
		}
		diagnostics.Add(base.Location, useSiteInfo);
		if (typeWithAnnotations.IsVoidType())
		{
			if (IsIndexer)
			{
				diagnostics.Add(ErrorCode.ERR_IndexerCantHaveVoidType, base.Location);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_PropertyCantHaveVoidType, base.Location, this);
			}
		}
		return typeWithAnnotations;
	}

	private static ImmutableArray<ParameterSymbol> MakeParameters(Binder binder, SourcePropertySymbolBase owner, Microsoft.CodeAnalysis.CSharp.Syntax.BaseParameterListSyntax? parameterSyntaxOpt, BindingDiagnosticBag diagnostics, bool addRefReadOnlyModifier)
	{
		if (parameterSyntaxOpt == null)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		if (parameterSyntaxOpt.Parameters.Count < 1)
		{
			diagnostics.Add(ErrorCode.ERR_IndexerNeedsParam, parameterSyntaxOpt.GetLastToken().GetLocation());
		}
		bool addRefReadOnlyModifier2 = addRefReadOnlyModifier;
		ImmutableArray<ParameterSymbol> result = ParameterHelpers.MakeParameters(binder, owner, parameterSyntaxOpt, out var arglistToken, diagnostics, allowRefOrOut: false, allowThis: false, addRefReadOnlyModifier2).Cast<SourceParameterSymbol, ParameterSymbol>();
		if (arglistToken.Kind() != SyntaxKind.None)
		{
			diagnostics.Add(ErrorCode.ERR_IllegalVarArgs, arglistToken.GetLocation());
		}
		if (result.Length == 1 && !owner.IsExplicitInterfaceImplementation)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameterSyntax = parameterSyntaxOpt.Parameters[0];
			if (parameterSyntax.Default != null)
			{
				SyntaxToken identifier = parameterSyntax.Identifier;
				diagnostics.Add(ErrorCode.WRN_DefaultValueForUnconsumedLocation, identifier.GetLocation(), identifier.ValueText);
			}
		}
		return result;
	}

	private ImmutableArray<ParameterSymbol> ComputeParameters(Binder binder, CSharpSyntaxNode syntax, BindingDiagnosticBag diagnostics)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.BaseParameterListSyntax parameterListSyntax = GetParameterListSyntax(syntax);
		return MakeParameters(binder, this, parameterListSyntax, diagnostics, IsVirtual || IsAbstract);
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		NamedTypeSymbol namedTypeSymbol = ContainingType;
		if ((object)namedTypeSymbol != null && namedTypeSymbol.IsExtension)
		{
			NamedTypeSymbol containingType = namedTypeSymbol.ContainingType;
			if ((object)containingType != null)
			{
				namedTypeSymbol = containingType;
			}
		}
		foreach (ParameterSymbol parameter in Parameters)
		{
			if (!IsExplicitInterfaceImplementation && !this.IsNoMoreVisibleThan(parameter.Type, ref useSiteInfo))
			{
				diagnostics.Add(ErrorCode.ERR_BadVisIndexerParam, base.Location, this, parameter.Type);
			}
			else if (parameter.Type.HasFileLocalTypes() && !namedTypeSymbol.HasFileLocalTypes())
			{
				diagnostics.Add(ErrorCode.ERR_FileTypeDisallowedInSignature, base.Location, parameter.Type, namedTypeSymbol);
			}
			else if ((object)SetMethod != null && parameter.Name == "value")
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateGeneratedName, parameter.TryGetFirstLocation() ?? base.Location, parameter.Name);
			}
		}
		MethodSymbol setMethod = SetMethod;
		if ((object)setMethod != null && this.IsExtensionBlockMember())
		{
			if (ContainingType.TypeParameters.Any((TypeParameterSymbol tp) => tp.Name == "value"))
			{
				diagnostics.Add(ErrorCode.ERR_ValueParameterSameNameAsExtensionTypeParameter, setMethod.GetFirstLocationOrNone());
			}
			ParameterSymbol extensionParameter = ContainingType.ExtensionParameter;
			if ((object)extensionParameter != null && extensionParameter.Name == "value")
			{
				diagnostics.Add(ErrorCode.ERR_ValueParameterSameNameAsExtensionParameter, setMethod.GetFirstLocationOrNone());
			}
		}
		if (this.IsExtensionBlockMember())
		{
			ParameterSymbol extensionParameter2 = ContainingType.ExtensionParameter;
			if ((object)extensionParameter2 != null && !this.IsNoMoreVisibleThan(extensionParameter2.Type, ref useSiteInfo))
			{
				diagnostics.Add(ErrorCode.ERR_BadVisIndexerParam, base.Location, this, extensionParameter2.Type);
			}
		}
		diagnostics.Add(base.Location, useSiteInfo);
		if (IsPartialDefinition)
		{
			SourcePropertySymbol otherPartOfPartial = OtherPartOfPartial;
			if ((object)otherPartOfPartial != null)
			{
				PartialPropertyChecks(otherPartOfPartial, diagnostics);
				otherPartOfPartial.CheckInitializerIfNeeded(diagnostics);
			}
		}
	}

	private void PartialPropertyChecks(SourcePropertySymbol implementation, BindingDiagnosticBag diagnostics)
	{
		bool flag = !TypeWithAnnotations.Equals(implementation.TypeWithAnnotations, TypeCompareKind.AllIgnoreOptions);
		if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberTypeDifference, implementation.GetFirstLocation());
		}
		else if (MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(this, implementation))
		{
			flag = true;
			diagnostics.Add(ErrorCode.ERR_PartialMemberInconsistentTupleNames, implementation.GetFirstLocation(), this, implementation);
		}
		if (RefKind != implementation.RefKind)
		{
			flag = true;
			diagnostics.Add(ErrorCode.ERR_PartialMemberRefReturnDifference, implementation.GetFirstLocation());
		}
		if ((!flag && !MemberSignatureComparer.PartialMethodsStrictComparer.Equals(this, implementation)) || !Parameters.SequenceEqual(implementation.Parameters, (ParameterSymbol a, ParameterSymbol b) => a.Name == b.Name))
		{
			diagnostics.Add(ErrorCode.WRN_PartialMemberSignatureDifference, implementation.GetFirstLocation(), new FormattedSymbol(this, SymbolDisplayFormat.MinimallyQualifiedFormat), new FormattedSymbol(implementation, SymbolDisplayFormat.MinimallyQualifiedFormat));
		}
		if (IsRequired != implementation.IsRequired)
		{
			diagnostics.Add(ErrorCode.ERR_PartialPropertyRequiredDifference, implementation.GetFirstLocation());
		}
		if (IsStatic != implementation.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberStaticDifference, implementation.GetFirstLocation());
		}
		if (base.HasReadOnlyModifier != implementation.HasReadOnlyModifier)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberReadOnlyDifference, implementation.GetFirstLocation());
		}
		if ((_modifiers & DeclarationModifiers.Unsafe) != (implementation._modifiers & DeclarationModifiers.Unsafe) && this.CompilationAllowsUnsafe())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberUnsafeDifference, implementation.GetFirstLocation());
		}
		if (this.IsParams() != implementation.IsParams())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberParamsDifference, implementation.GetFirstLocation());
		}
		if (DeclaredAccessibility != implementation.DeclaredAccessibility || base.HasExplicitAccessModifier != implementation.HasExplicitAccessModifier)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberAccessibilityDifference, implementation.GetFirstLocation());
		}
		if (IsVirtual != implementation.IsVirtual || IsOverride != implementation.IsOverride || IsSealed != implementation.IsSealed || base.IsNew != implementation.IsNew)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberExtendedModDifference, implementation.GetFirstLocation());
		}
		for (int num = 0; num < base.ParameterCount; num++)
		{
			SourceParameterSymbol obj = (SourceParameterSymbol)Parameters[num];
			SourceParameterSymbol sourceParameterSymbol = (SourceParameterSymbol)implementation.Parameters[num];
			if (obj.DeclaredScope != sourceParameterSymbol.DeclaredScope)
			{
				diagnostics.Add(ErrorCode.ERR_ScopedMismatchInParameterOfPartial, implementation.GetFirstLocation(), new FormattedSymbol(implementation.Parameters[num], SymbolDisplayFormat.ShortFormat));
			}
		}
		MethodSymbol getMethod = GetMethod;
		if ((object)getMethod != null)
		{
			MethodSymbol getMethod2 = implementation.GetMethod;
			if ((object)getMethod2 != null)
			{
				((SourcePropertyAccessorSymbol)getMethod).PartialAccessorChecks((SourcePropertyAccessorSymbol)getMethod2, diagnostics);
			}
		}
		MethodSymbol setMethod = SetMethod;
		if ((object)setMethod != null)
		{
			MethodSymbol setMethod2 = implementation.SetMethod;
			if ((object)setMethod2 != null)
			{
				((SourcePropertyAccessorSymbol)setMethod).PartialAccessorChecks((SourcePropertyAccessorSymbol)setMethod2, diagnostics);
			}
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.BaseParameterListSyntax? GetParameterListSyntax(CSharpSyntaxNode syntax)
	{
		return (syntax as Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax)?.ParameterList;
	}

	internal static void InitializePartialPropertyParts(SourcePropertySymbol definition, SourcePropertySymbol implementation)
	{
		definition._otherPartOfPartial = implementation;
		implementation._otherPartOfPartial = definition;
		SynthesizedBackingFieldSymbol mergedBackingField = definition.DeclaredBackingField ?? implementation.DeclaredBackingField;
		definition.SetMergedBackingField(mergedBackingField);
		implementation.SetMergedBackingField(mergedBackingField);
	}
}
