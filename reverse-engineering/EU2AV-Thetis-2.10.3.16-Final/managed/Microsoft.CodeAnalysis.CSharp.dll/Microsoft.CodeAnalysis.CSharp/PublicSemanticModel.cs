using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class PublicSemanticModel : CSharpSemanticModel
{
	internal sealed override SemanticModel ContainingPublicModelOrSelf => this;

	protected AttributeSemanticModel CreateModelForAttribute(Binder enclosingBinder, AttributeSyntax attribute, MemberSemanticModel containingModel)
	{
		NamedTypeSymbol attributeType = (NamedTypeSymbol)enclosingBinder.BindType(attribute.Name, BindingDiagnosticBag.Discarded, out var alias).Type;
		Symbol attributeTarget = getAttributeTarget(attribute.Parent?.Parent);
		return AttributeSemanticModel.Create(this, attribute, attributeType, alias, attributeTarget, enclosingBinder, containingModel?.GetRemappedSymbols());
		Symbol? getAttributeTarget(SyntaxNode? targetSyntax)
		{
			if (targetSyntax is BaseMethodDeclarationSyntax || targetSyntax is LocalFunctionStatementSyntax || targetSyntax is ParameterSyntax || targetSyntax is TypeParameterSyntax || targetSyntax is IndexerDeclarationSyntax || targetSyntax is AccessorDeclarationSyntax || targetSyntax is DelegateDeclarationSyntax)
			{
				return GetDeclaredSymbolForNode(targetSyntax).GetSymbol();
			}
			if (targetSyntax is AnonymousFunctionExpressionSyntax expression)
			{
				return GetSymbolInfo(expression).Symbol.GetSymbol();
			}
			return null;
		}
	}
}
