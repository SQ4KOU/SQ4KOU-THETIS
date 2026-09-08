using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SubpatternSyntax : CSharpSyntaxNode
{
	private BaseExpressionColonSyntax? expressionColon;

	private PatternSyntax? pattern;

	public NameColonSyntax? NameColon => ExpressionColon as NameColonSyntax;

	public BaseExpressionColonSyntax? ExpressionColon => GetRedAtZero(ref expressionColon);

	public PatternSyntax Pattern => GetRed(ref pattern, 1);

	public SubpatternSyntax WithNameColon(NameColonSyntax? nameColon)
	{
		return WithExpressionColon(nameColon);
	}

	public SubpatternSyntax Update(NameColonSyntax? nameColon, PatternSyntax pattern)
	{
		return Update((BaseExpressionColonSyntax?)nameColon, pattern);
	}

	internal SubpatternSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref expressionColon), 
			1 => GetRed(ref pattern, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			0 => expressionColon, 
			1 => pattern, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSubpattern(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitSubpattern(this);
	}

	public SubpatternSyntax Update(BaseExpressionColonSyntax? expressionColon, PatternSyntax pattern)
	{
		if (expressionColon != ExpressionColon || pattern != Pattern)
		{
			SubpatternSyntax subpatternSyntax = SyntaxFactory.Subpattern(expressionColon, pattern);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return subpatternSyntax;
			}
			return subpatternSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public SubpatternSyntax WithExpressionColon(BaseExpressionColonSyntax? expressionColon)
	{
		return Update(expressionColon, Pattern);
	}

	public SubpatternSyntax WithPattern(PatternSyntax pattern)
	{
		return Update(ExpressionColon, pattern);
	}
}
