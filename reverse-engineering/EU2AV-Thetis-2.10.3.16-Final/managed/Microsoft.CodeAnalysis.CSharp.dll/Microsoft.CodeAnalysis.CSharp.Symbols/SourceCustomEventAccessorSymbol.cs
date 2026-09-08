using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceCustomEventAccessorSymbol : SourceEventAccessorSymbol
{
	public override Accessibility DeclaredAccessibility => AssociatedSymbol.DeclaredAccessibility;

	protected override SourceMemberMethodSymbol? BoundAttributesSource => (SourceMemberMethodSymbol)PartialDefinitionPart;

	internal SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (base.AssociatedEvent.containingType.AnyMemberHasAttributes)
			{
				return GetSyntax().AttributeLists;
			}
			return default(SyntaxList<AttributeListSyntax>);
		}
	}

	public override bool IsImplicitlyDeclared => false;

	internal override bool GenerateDebugInfo => true;

	internal SourceCustomEventAccessorSymbol(SourceEventSymbol @event, AccessorDeclarationSyntax syntax, EventSymbol explicitlyImplementedEventOpt, string aliasQualifierOpt, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(@event, syntax.GetReference(), syntax.Keyword.GetLocation(), explicitlyImplementedEventOpt, aliasQualifierOpt, syntax.Kind() == SyntaxKind.AddAccessorDeclaration, SyntaxFacts.HasYieldOperations(syntax.Body), isNullableAnalysisEnabled, syntax != null && syntax.Body == null && syntax.ExpressionBody != null)
	{
		CheckFeatureAvailabilityAndRuntimeSupport(syntax, base.Location, hasBody: true, diagnostics);
		if ((syntax.Body != null || syntax.ExpressionBody != null) && IsExtern && !IsAbstract)
		{
			diagnostics.Add(ErrorCode.ERR_ExternHasBody, base.Location, this);
		}
		if (syntax.Modifiers.Count > 0)
		{
			diagnostics.Add(ErrorCode.ERR_NoModifiersOnAccessor, syntax.Modifiers[0].GetLocation());
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	internal AccessorDeclarationSyntax GetSyntax()
	{
		return (AccessorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return TryGetBodyBinderFromSyntax(binderFactoryOpt, ignoreAccessibility);
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(AttributeDeclarationSyntaxList);
	}
}
