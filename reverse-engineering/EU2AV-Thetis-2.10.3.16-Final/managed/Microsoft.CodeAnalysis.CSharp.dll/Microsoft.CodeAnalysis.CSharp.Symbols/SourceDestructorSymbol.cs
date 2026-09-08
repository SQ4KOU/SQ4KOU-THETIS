using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceDestructorSymbol : SourceMemberMethodSymbol
{
	private TypeWithAnnotations _lazyReturnType;

	internal override int ParameterCount => 0;

	public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override TypeWithAnnotations ReturnTypeWithAnnotations
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override string Name => "Finalize";

	internal override bool IsMetadataFinal => false;

	internal override bool GenerateDebugInfo => true;

	internal SourceDestructorSymbol(SourceMemberContainerTypeSymbol containingType, DestructorDeclarationSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(containingType, syntax.GetReference(), GetSymbolLocation(syntax, out var location), SyntaxFacts.HasYieldOperations(syntax.Body), MakeModifiersAndFlags(containingType, syntax, isNullableAnalysisEnabled, location, diagnostics, out var modifierErrors))
	{
		this.CheckUnsafeModifier(DeclarationModifiers, diagnostics);
		bool flag = syntax.Body != null;
		bool isExpressionBodied = base.IsExpressionBodied;
		if (syntax.Identifier.ValueText != containingType.Name && !containingType.IsExtension)
		{
			diagnostics.Add(ErrorCode.ERR_BadDestructorName, syntax.Identifier.GetLocation());
		}
		if ((flag | isExpressionBodied) && IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_ExternHasBody, location, this);
		}
		if (!modifierErrors && !flag && !isExpressionBodied && !IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		if (containingType.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_DestructorInStaticClass, location);
		}
		else if (!containingType.IsReferenceType)
		{
			diagnostics.Add(ErrorCode.ERR_OnlyClassesCanContainDestructors, location);
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	private static (DeclarationModifiers, Flags) MakeModifiersAndFlags(NamedTypeSymbol containingType, DestructorDeclarationSyntax syntax, bool isNullableAnalysisEnabled, Location location, BindingDiagnosticBag diagnostics, out bool modifierErrors)
	{
		DeclarationModifiers declarationModifiers = MakeModifiers(containingType, syntax.Modifiers, location, diagnostics, out modifierErrors);
		Flags item = SourceMemberMethodSymbol.MakeFlags(MethodKind.Destructor, RefKind.None, declarationModifiers, returnsVoid: true, returnsVoidIsSet: true, syntax.IsExpressionBodied(), isExtensionMethod: false, isNullableAnalysisEnabled, isVarArg: false, isExplicitInterfaceImplementation: false, hasThisInitializer: false);
		return (declarationModifiers, item);
	}

	private static Location GetSymbolLocation(DestructorDeclarationSyntax syntax, out Location location)
	{
		location = syntax.Identifier.GetLocation();
		return location;
	}

	protected override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		DestructorDeclarationSyntax syntax = GetSyntax();
		Binder binder = DeclaringCompilation.GetBinderFactory(syntaxReferenceOpt.SyntaxTree).GetBinder(syntax, syntax, this);
		_lazyReturnType = TypeWithAnnotations.Create(binder.GetSpecialType(SpecialType.System_Void, diagnostics, syntax));
	}

	internal DestructorDeclarationSyntax GetSyntax()
	{
		return (DestructorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return TryGetBodyBinderFromSyntax(binderFactoryOpt, ignoreAccessibility);
	}

	public override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	private static DeclarationModifiers MakeModifiers(NamedTypeSymbol containingType, SyntaxTokenList modifiers, Location location, BindingDiagnosticBag diagnostics, out bool modifierErrors)
	{
		bool hasExplicitAccessModifier;
		return (DeclarationModifiers)(((uint)ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: false, containingType.IsInterface, modifiers, DeclarationModifiers.None, DeclarationModifiers.Extern | DeclarationModifiers.Unsafe, location, diagnostics, out modifierErrors, out hasExplicitAccessModifier) & 0xFFFFFC0Fu) | 0x20);
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(GetSyntax().AttributeLists);
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetReturnTypeAttributeDeclarations()
	{
		return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
	}

	internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return true;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return (object)ContainingType.BaseTypeNoUseSiteDiagnostics == null;
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
