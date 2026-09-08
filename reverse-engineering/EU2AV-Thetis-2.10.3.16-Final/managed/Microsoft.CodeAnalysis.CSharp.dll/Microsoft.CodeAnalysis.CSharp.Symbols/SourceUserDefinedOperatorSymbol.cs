using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceUserDefinedOperatorSymbol : SourceUserDefinedOperatorSymbolBase
{
	protected override Location ReturnTypeLocation => GetSyntax().ReturnType.Location;

	internal override bool GenerateDebugInfo => true;

	public static SourceUserDefinedOperatorSymbol CreateUserDefinedOperatorSymbol(SourceMemberContainerTypeSymbol containingType, Binder bodyBinder, OperatorDeclarationSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
	{
		Location location = syntax.OperatorToken.GetLocation();
		string text = OperatorFacts.OperatorNameFromDeclaration(syntax);
		bool isCompoundAssignmentOrIncrementAssignment = OperatorFacts.IsCompoundAssignmentOperatorName(text);
		if (SyntaxFacts.IsCheckedOperator(text))
		{
			MessageID.IDS_FeatureCheckedUserDefinedOperators.CheckFeatureAvailability(diagnostics, syntax.CheckedKeyword);
		}
		else if (!syntax.OperatorToken.IsMissing && syntax.CheckedKeyword.IsKind(SyntaxKind.CheckedKeyword))
		{
			diagnostics.Add(ErrorCode.ERR_OperatorCantBeChecked, syntax.CheckedKeyword.GetLocation(), SyntaxFacts.GetText(SyntaxFacts.GetOperatorKind(text)));
		}
		if (text == "op_UnsignedRightShift")
		{
			MessageID.IDS_FeatureUnsignedRightShift.CheckFeatureAvailability(diagnostics, syntax.OperatorToken);
		}
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = syntax.ExplicitInterfaceSpecifier;
		text = ExplicitInterfaceHelpers.GetMemberNameAndInterfaceSymbol(bodyBinder, syntax.Modifiers, explicitInterfaceSpecifier, text, diagnostics, out var explicitInterfaceTypeOpt, out var _);
		return new SourceUserDefinedOperatorSymbol((explicitInterfaceSpecifier == null) ? MethodKind.UserDefinedOperator : MethodKind.ExplicitInterfaceImplementation, containingType, explicitInterfaceTypeOpt, text, isCompoundAssignmentOrIncrementAssignment, location, syntax, isNullableAnalysisEnabled, diagnostics);
	}

	private SourceUserDefinedOperatorSymbol(MethodKind methodKind, SourceMemberContainerTypeSymbol containingType, TypeSymbol explicitInterfaceType, string name, bool isCompoundAssignmentOrIncrementAssignment, Location location, OperatorDeclarationSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(methodKind, explicitInterfaceType, name, isCompoundAssignmentOrIncrementAssignment, containingType, location, syntax, SourceUserDefinedOperatorSymbolBase.MakeDeclarationModifiers(isCompoundAssignmentOrIncrementAssignment, methodKind, containingType, syntax, location, diagnostics), syntax.HasAnyBody(), syntax.IsExpressionBodied(), SyntaxFacts.HasYieldOperations(syntax.Body), isNullableAnalysisEnabled, diagnostics)
	{
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
		if (IsAbstract || IsVirtual || (name != "op_Equality" && name != "op_Inequality"))
		{
			CheckFeatureAvailabilityAndRuntimeSupport(syntax, location, syntax.Body != null || syntax.ExpressionBody != null, diagnostics);
		}
		if (syntax.ExplicitInterfaceSpecifier != null)
		{
			MessageID.IDS_FeatureStaticAbstractMembersInInterfaces.CheckFeatureAvailability(diagnostics, syntax.ExplicitInterfaceSpecifier);
		}
	}

	internal OperatorDeclarationSyntax GetSyntax()
	{
		return (OperatorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return TryGetBodyBinderFromSyntax(binderFactoryOpt, ignoreAccessibility);
	}

	protected override int GetParameterCountFromSyntax()
	{
		return GetSyntax().ParameterList.ParameterCount;
	}

	internal sealed override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(GetSyntax().AttributeLists);
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		OperatorDeclarationSyntax syntax = GetSyntax();
		return MakeParametersAndBindReturnType(syntax, syntax.ReturnType, diagnostics);
	}
}
