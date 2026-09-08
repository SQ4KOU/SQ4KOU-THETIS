using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LineDirectivePositionSyntax : CSharpSyntaxNode
{
	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectivePositionSyntax)base.Green).openParenToken, base.Position, 0);

	public SyntaxToken Line => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectivePositionSyntax)base.Green).line, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken CommaToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectivePositionSyntax)base.Green).commaToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken Character => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectivePositionSyntax)base.Green).character, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectivePositionSyntax)base.Green).closeParenToken, GetChildPosition(4), GetChildIndex(4));

	internal LineDirectivePositionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return null;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLineDirectivePosition(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitLineDirectivePosition(this);
	}

	public LineDirectivePositionSyntax Update(SyntaxToken openParenToken, SyntaxToken line, SyntaxToken commaToken, SyntaxToken character, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || line != Line || commaToken != CommaToken || character != Character || closeParenToken != CloseParenToken)
		{
			LineDirectivePositionSyntax lineDirectivePositionSyntax = SyntaxFactory.LineDirectivePosition(openParenToken, line, commaToken, character, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return lineDirectivePositionSyntax;
			}
			return lineDirectivePositionSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public LineDirectivePositionSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(openParenToken, Line, CommaToken, Character, CloseParenToken);
	}

	public LineDirectivePositionSyntax WithLine(SyntaxToken line)
	{
		return Update(OpenParenToken, line, CommaToken, Character, CloseParenToken);
	}

	public LineDirectivePositionSyntax WithCommaToken(SyntaxToken commaToken)
	{
		return Update(OpenParenToken, Line, commaToken, Character, CloseParenToken);
	}

	public LineDirectivePositionSyntax WithCharacter(SyntaxToken character)
	{
		return Update(OpenParenToken, Line, CommaToken, character, CloseParenToken);
	}

	public LineDirectivePositionSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(OpenParenToken, Line, CommaToken, Character, closeParenToken);
	}
}
