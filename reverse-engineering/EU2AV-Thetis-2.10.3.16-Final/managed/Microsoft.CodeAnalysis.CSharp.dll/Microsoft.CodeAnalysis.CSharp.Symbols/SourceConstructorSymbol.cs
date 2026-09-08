using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceConstructorSymbol : SourceConstructorSymbolBase
{
	private SourceConstructorSymbol? _otherPartOfPartial;

	private SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (ContainingType is SourceMemberContainerTypeSymbol { AnyMemberHasAttributes: not false })
			{
				return GetSyntax().AttributeLists;
			}
			return default(SyntaxList<AttributeListSyntax>);
		}
	}

	protected override SourceMemberMethodSymbol? BoundAttributesSource => SourcePartialDefinitionPart;

	protected override bool AllowRefOrOut => true;

	public sealed override bool IsExtern => PartialImplementationPart?.IsExtern ?? base.HasExternModifier;

	private bool HasAnyBody => flags.HasAnyBody;

	private bool HasExplicitAccessModifier => flags.HasExplicitAccessModifier;

	internal bool IsPartialDefinition
	{
		get
		{
			if (base.IsPartial && !HasAnyBody)
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
				if (!HasAnyBody)
				{
					return base.HasExternModifier;
				}
				return true;
			}
			return false;
		}
	}

	internal SourceConstructorSymbol? OtherPartOfPartial => _otherPartOfPartial;

	internal SourceConstructorSymbol? SourcePartialDefinitionPart
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

	internal SourceConstructorSymbol? SourcePartialImplementationPart
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

	public sealed override MethodSymbol? PartialDefinitionPart => SourcePartialDefinitionPart;

	public sealed override MethodSymbol? PartialImplementationPart => SourcePartialImplementationPart;

	public static SourceConstructorSymbol CreateConstructorSymbol(SourceMemberContainerTypeSymbol containingType, ConstructorDeclarationSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
	{
		MethodKind methodKind = ((!syntax.Modifiers.Any(SyntaxKind.StaticKeyword)) ? MethodKind.Constructor : MethodKind.StaticConstructor);
		return new SourceConstructorSymbol(containingType, syntax.Identifier.GetLocation(), syntax, methodKind, isNullableAnalysisEnabled, diagnostics);
	}

	private SourceConstructorSymbol(SourceMemberContainerTypeSymbol containingType, Location location, ConstructorDeclarationSyntax syntax, MethodKind methodKind, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
	{
		bool isIterator = SyntaxFacts.HasYieldOperations(syntax);
		ConstructorInitializerSyntax? initializer = syntax.Initializer;
		base._002Ector(containingType, location, syntax, isIterator, MakeModifiersAndFlags(containingType, syntax, methodKind, isNullableAnalysisEnabled, initializer != null && initializer.Kind() == SyntaxKind.ThisConstructorInitializer, location, diagnostics, out var modifierErrors, out var report_ERR_StaticConstructorWithAccessModifiers));
		this.CheckUnsafeModifier(DeclarationModifiers, diagnostics);
		if (report_ERR_StaticConstructorWithAccessModifiers)
		{
			diagnostics.Add(ErrorCode.ERR_StaticConstructorWithAccessModifiers, location, this);
		}
		if (syntax.Identifier.ValueText != containingType.Name)
		{
			if (syntax.Identifier.Text == "extension")
			{
				MessageID.IDS_FeatureExtensions.CheckFeatureAvailability(diagnostics, syntax);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_MemberNeedsType, location);
			}
		}
		bool flag = syntax.HasAnyBody();
		if (IsExtern)
		{
			if (methodKind == MethodKind.Constructor && syntax.Initializer != null)
			{
				diagnostics.Add(ErrorCode.ERR_ExternHasConstructorInitializer, location, this);
			}
			if (flag)
			{
				diagnostics.Add(ErrorCode.ERR_ExternHasBody, location, this);
			}
		}
		if (IsPartialDefinition)
		{
			ConstructorInitializerSyntax initializer2 = syntax.Initializer;
			if (initializer2 != null)
			{
				diagnostics.Add(ErrorCode.ERR_PartialConstructorInitializer, initializer2, this);
			}
		}
		if (methodKind == MethodKind.StaticConstructor)
		{
			CheckFeatureAvailabilityAndRuntimeSupport(syntax, location, flag, diagnostics);
		}
		ModifierUtils.CheckAccessibility(DeclarationModifiers, this, isExplicitInterfaceImplementation: false, diagnostics, location);
		if (!modifierErrors)
		{
			CheckModifiers(methodKind, flag, location, diagnostics);
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	private static (DeclarationModifiers, Flags) MakeModifiersAndFlags(NamedTypeSymbol containingType, ConstructorDeclarationSyntax syntax, MethodKind methodKind, bool isNullableAnalysisEnabled, bool hasThisInitializer, Location location, BindingDiagnosticBag diagnostics, out bool modifierErrors, out bool report_ERR_StaticConstructorWithAccessModifiers)
	{
		bool flag = syntax.HasAnyBody();
		DeclarationModifiers declarationModifiers = MakeModifiers(containingType, syntax, methodKind, flag, location, diagnostics, out modifierErrors, out var hasExplicitAccessModifier, out report_ERR_StaticConstructorWithAccessModifiers);
		bool isExpressionBodied = syntax.IsExpressionBodied();
		bool isVararg = syntax.IsVarArg();
		Flags item = new Flags(methodKind, RefKind.None, declarationModifiers, returnsVoid: true, returnsVoidIsSet: true, flag, isExpressionBodied, isExtensionMethod: false, isNullableAnalysisEnabled, isVararg, isExplicitInterfaceImplementation: false, hasThisInitializer, hasExplicitAccessModifier);
		return (declarationModifiers, item);
	}

	internal ConstructorDeclarationSyntax GetSyntax()
	{
		return (ConstructorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return TryGetBodyBinderFromSyntax(binderFactoryOpt, ignoreAccessibility);
	}

	protected override ParameterListSyntax GetParameterList()
	{
		return GetSyntax().ParameterList;
	}

	protected override CSharpSyntaxNode GetInitializer()
	{
		return GetSyntax().Initializer;
	}

	private static DeclarationModifiers MakeModifiers(NamedTypeSymbol containingType, ConstructorDeclarationSyntax syntax, MethodKind methodKind, bool hasBody, Location location, BindingDiagnosticBag diagnostics, out bool modifierErrors, out bool hasExplicitAccessModifier, out bool report_ERR_StaticConstructorWithAccessModifiers)
	{
		DeclarationModifiers defaultAccess = ((methodKind != MethodKind.StaticConstructor) ? DeclarationModifiers.Private : DeclarationModifiers.None);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Static | DeclarationModifiers.Extern | DeclarationModifiers.Unsafe;
		if (methodKind == MethodKind.Constructor)
		{
			declarationModifiers |= DeclarationModifiers.Partial;
		}
		bool isInterface = containingType.IsInterface;
		DeclarationModifiers declarationModifiers2 = ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: false, isInterface, syntax.Modifiers, defaultAccess, declarationModifiers, location, diagnostics, out modifierErrors, out hasExplicitAccessModifier);
		report_ERR_StaticConstructorWithAccessModifiers = false;
		if (methodKind == MethodKind.StaticConstructor)
		{
			if ((declarationModifiers2 & DeclarationModifiers.AccessibilityMask) != DeclarationModifiers.None && containingType.Name == syntax.Identifier.ValueText)
			{
				declarationModifiers2 = (DeclarationModifiers)((uint)declarationModifiers2 & 0xFFFFFC0Fu);
				report_ERR_StaticConstructorWithAccessModifiers = true;
				modifierErrors = true;
			}
			declarationModifiers2 |= DeclarationModifiers.Private;
			if (isInterface)
			{
				ModifierUtils.ReportDefaultInterfaceImplementationModifiers(hasBody, declarationModifiers2, DeclarationModifiers.Extern, location, diagnostics);
			}
		}
		return declarationModifiers2;
	}

	private void CheckModifiers(MethodKind methodKind, bool hasBody, Location location, BindingDiagnosticBag diagnostics)
	{
		if (!hasBody && !IsExtern && !base.IsPartial)
		{
			diagnostics.Add(ErrorCode.ERR_ConcreteMissingBody, location, this);
		}
		else if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected() && !IsOverride)
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), location, this);
		}
		else if (ContainingType.IsStatic && methodKind == MethodKind.Constructor)
		{
			diagnostics.Add(ErrorCode.ERR_ConstructorInStaticClass, location);
		}
		else if (base.IsPartial && !ContainingType.IsPartial())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberOnlyInPartialClass, location);
		}
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		SourceConstructorSymbol sourcePartialImplementationPart = SourcePartialImplementationPart;
		if ((object)sourcePartialImplementationPart != null)
		{
			return OneOrMany.Create(AttributeDeclarationSyntaxList, sourcePartialImplementationPart.AttributeDeclarationSyntaxList);
		}
		return OneOrMany.Create(AttributeDeclarationSyntaxList);
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		if (!flags.HasThisInitializer)
		{
			return ((SourceMemberContainerTypeSymbol)ContainingType).IsNullableEnabledForConstructorsAndInitializers(IsStatic);
		}
		return flags.IsNullableAnalysisEnabled;
	}

	protected override bool IsWithinExpressionOrBlockBody(int position, out int offset)
	{
		ConstructorDeclarationSyntax syntax = GetSyntax();
		BlockSyntax? body = syntax.Body;
		if (body != null && body.Span.Contains(position))
		{
			offset = position - syntax.Body.Span.Start;
			return true;
		}
		ArrowExpressionClauseSyntax? expressionBody = syntax.ExpressionBody;
		if (expressionBody != null && expressionBody.Span.Contains(position))
		{
			offset = position - syntax.ExpressionBody.Span.Start;
			return true;
		}
		offset = -1;
		return false;
	}

	internal sealed override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		SourcePartialImplementationPart?.ForceComplete(locationOpt, filter, cancellationToken);
		base.ForceComplete(locationOpt, filter, cancellationToken);
	}

	protected override void PartialConstructorChecks(BindingDiagnosticBag diagnostics)
	{
		SourceConstructorSymbol sourcePartialImplementationPart = SourcePartialImplementationPart;
		if ((object)sourcePartialImplementationPart != null)
		{
			PartialConstructorChecks(sourcePartialImplementationPart, diagnostics);
		}
	}

	private void PartialConstructorChecks(SourceConstructorSymbol implementation, BindingDiagnosticBag diagnostics)
	{
		if (MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(this, implementation))
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberInconsistentTupleNames, implementation.GetFirstLocation(), this, implementation);
		}
		else if (!MemberSignatureComparer.PartialMethodsStrictComparer.Equals(this, implementation) || !Parameters.SequenceEqual(implementation.Parameters, (ParameterSymbol a, ParameterSymbol b) => a.Name == b.Name))
		{
			diagnostics.Add(ErrorCode.WRN_PartialMemberSignatureDifference, implementation.GetFirstLocation(), new FormattedSymbol(this, SymbolDisplayFormat.MinimallyQualifiedFormat), new FormattedSymbol(implementation, SymbolDisplayFormat.MinimallyQualifiedFormat));
		}
		if (base.IsUnsafe != implementation.IsUnsafe && this.CompilationAllowsUnsafe())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberUnsafeDifference, implementation.GetFirstLocation());
		}
		if (this.IsParams() != implementation.IsParams())
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberParamsDifference, implementation.GetFirstLocation());
		}
		if (DeclaredAccessibility != implementation.DeclaredAccessibility || HasExplicitAccessModifier != implementation.HasExplicitAccessModifier)
		{
			diagnostics.Add(ErrorCode.ERR_PartialMemberAccessibilityDifference, implementation.GetFirstLocation());
		}
		for (int num = 0; num < ParameterCount; num++)
		{
			SourceParameterSymbol obj = (SourceParameterSymbol)Parameters[num];
			SourceParameterSymbol sourceParameterSymbol = (SourceParameterSymbol)implementation.Parameters[num];
			if (obj.DeclaredScope != sourceParameterSymbol.DeclaredScope)
			{
				diagnostics.Add(ErrorCode.ERR_ScopedMismatchInParameterOfPartial, implementation.GetFirstLocation(), new FormattedSymbol(implementation.Parameters[num], SymbolDisplayFormat.ShortFormat));
			}
		}
	}

	internal static void InitializePartialConstructorParts(SourceConstructorSymbol definition, SourceConstructorSymbol implementation)
	{
		definition._otherPartOfPartial = implementation;
		implementation._otherPartOfPartial = definition;
	}
}
