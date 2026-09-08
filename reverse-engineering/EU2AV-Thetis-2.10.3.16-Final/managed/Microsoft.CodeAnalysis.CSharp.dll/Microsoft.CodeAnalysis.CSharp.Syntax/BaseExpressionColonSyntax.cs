using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseExpressionColonSyntax : CSharpSyntaxNode
{
	public abstract ExpressionSyntax Expression { get; }

	public abstract SyntaxToken ColonToken { get; }

	internal BaseExpressionColonSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseExpressionColonSyntax WithExpression(ExpressionSyntax expression)
	{
		return WithExpressionCore(expression);
	}

	internal abstract BaseExpressionColonSyntax WithExpressionCore(ExpressionSyntax expression);

	public BaseExpressionColonSyntax WithColonToken(SyntaxToken colonToken)
	{
		return WithColonTokenCore(colonToken);
	}

	internal abstract BaseExpressionColonSyntax WithColonTokenCore(SyntaxToken colonToken);
}
