using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SlicePatternSyntax : PatternSyntax
{
	private PatternSyntax? pattern;

	public SyntaxToken DotDotToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SlicePatternSyntax)base.Green).dotDotToken, base.Position, 0);

	public PatternSyntax? Pattern => GetRed(ref pattern, 1);

	internal SlicePatternSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return GetRed(ref pattern, 1);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return pattern;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSlicePattern(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitSlicePattern(this);
	}

	public SlicePatternSyntax Update(SyntaxToken dotDotToken, PatternSyntax? pattern)
	{
		if (dotDotToken != DotDotToken || pattern != Pattern)
		{
			SlicePatternSyntax slicePatternSyntax = SyntaxFactory.SlicePattern(dotDotToken, pattern);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return slicePatternSyntax;
			}
			return slicePatternSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public SlicePatternSyntax WithDotDotToken(SyntaxToken dotDotToken)
	{
		return Update(dotDotToken, Pattern);
	}

	public SlicePatternSyntax WithPattern(PatternSyntax? pattern)
	{
		return Update(DotDotToken, pattern);
	}
}
