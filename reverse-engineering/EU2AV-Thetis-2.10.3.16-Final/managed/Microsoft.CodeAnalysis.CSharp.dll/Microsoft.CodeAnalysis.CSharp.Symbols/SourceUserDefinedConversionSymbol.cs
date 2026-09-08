using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceUserDefinedConversionSymbol : SourceUserDefinedOperatorSymbolBase
{
	protected override Location ReturnTypeLocation => GetSyntax().Type.Location;

	internal override bool GenerateDebugInfo => true;

	public static SourceUserDefinedConversionSymbol CreateUserDefinedConversionSymbol(SourceMemberContainerTypeSymbol containingType, Binder bodyBinder, ConversionOperatorDeclarationSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
	{
		Location location = syntax.Type.Location;
		string text = OperatorFacts.OperatorNameFromDeclaration(syntax);
		if (text == "op_CheckedExplicit")
		{
			MessageID.IDS_FeatureCheckedUserDefinedOperators.CheckFeatureAvailability(diagnostics, syntax.CheckedKeyword);
		}
		else if (syntax.CheckedKeyword.IsKind(SyntaxKind.CheckedKeyword))
		{
			diagnostics.Add(ErrorCode.ERR_ImplicitConversionOperatorCantBeChecked, syntax.CheckedKeyword.GetLocation());
		}
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = syntax.ExplicitInterfaceSpecifier;
		text = ExplicitInterfaceHelpers.GetMemberNameAndInterfaceSymbol(bodyBinder, syntax.Modifiers, explicitInterfaceSpecifier, text, diagnostics, out var explicitInterfaceTypeOpt, out var _);
		return new SourceUserDefinedConversionSymbol((explicitInterfaceSpecifier == null) ? MethodKind.Conversion : MethodKind.ExplicitInterfaceImplementation, containingType, explicitInterfaceTypeOpt, text, location, syntax, isNullableAnalysisEnabled, diagnostics);
	}

	private SourceUserDefinedConversionSymbol(MethodKind methodKind, SourceMemberContainerTypeSymbol containingType, TypeSymbol explicitInterfaceType, string name, Location location, ConversionOperatorDeclarationSyntax syntax, bool isNullableAnalysisEnabled, BindingDiagnosticBag diagnostics)
		: base(methodKind, explicitInterfaceType, name, isCompoundAssignmentOrIncrementAssignment: false, containingType, location, syntax, SourceUserDefinedOperatorSymbolBase.MakeDeclarationModifiers(isCompoundAssignmentOrIncrementAssignment: false, methodKind, containingType, syntax, location, diagnostics), syntax.HasAnyBody(), syntax.IsExpressionBodied(), SyntaxFacts.HasYieldOperations(syntax.Body), isNullableAnalysisEnabled, diagnostics)
	{
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
		if (syntax.ParameterList.Parameters.Count != 1)
		{
			diagnostics.Add(ErrorCode.ERR_OvlUnaryOperatorExpected, syntax.ParameterList.GetLocation());
		}
		if (IsStatic && (IsAbstract || IsVirtual))
		{
			CheckFeatureAvailabilityAndRuntimeSupport(syntax, location, syntax.Body != null || syntax.ExpressionBody != null, diagnostics);
		}
		if (syntax.ExplicitInterfaceSpecifier != null)
		{
			MessageID.IDS_FeatureStaticAbstractMembersInInterfaces.CheckFeatureAvailability(diagnostics, syntax.ExplicitInterfaceSpecifier);
		}
	}

	internal ConversionOperatorDeclarationSyntax GetSyntax()
	{
		return (ConversionOperatorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
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
		ConversionOperatorDeclarationSyntax syntax = GetSyntax();
		return MakeParametersAndBindReturnType(syntax, syntax.Type, diagnostics);
	}
}
