using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ListPatternSyntax : PatternSyntax
{
	private SyntaxNode? patterns;

	private VariableDesignationSyntax? designation;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ListPatternSyntax)base.Green).openBracketToken, base.Position, 0);

	public SeparatedSyntaxList<PatternSyntax> Patterns
	{
		get
		{
			SyntaxNode red = GetRed(ref patterns, 1);
			if (red == null)
			{
				return default(SeparatedSyntaxList<PatternSyntax>);
			}
			return new SeparatedSyntaxList<PatternSyntax>(red, GetChildIndex(1));
		}
	}

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ListPatternSyntax)base.Green).closeBracketToken, GetChildPosition(2), GetChildIndex(2));

	public VariableDesignationSyntax? Designation => GetRed(ref designation, 3);

	internal ListPatternSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref patterns, 1), 
			3 => GetRed(ref designation, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			1 => patterns, 
			3 => designation, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitListPattern(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitListPattern(this);
	}

	public ListPatternSyntax Update(SyntaxToken openBracketToken, SeparatedSyntaxList<PatternSyntax> patterns, SyntaxToken closeBracketToken, VariableDesignationSyntax? designation)
	{
		if (openBracketToken != OpenBracketToken || patterns != Patterns || closeBracketToken != CloseBracketToken || designation != Designation)
		{
			ListPatternSyntax listPatternSyntax = SyntaxFactory.ListPattern(openBracketToken, patterns, closeBracketToken, designation);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return listPatternSyntax;
			}
			return listPatternSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public ListPatternSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Patterns, CloseBracketToken, Designation);
	}

	public ListPatternSyntax WithPatterns(SeparatedSyntaxList<PatternSyntax> patterns)
	{
		return Update(OpenBracketToken, patterns, CloseBracketToken, Designation);
	}

	public ListPatternSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Patterns, closeBracketToken, Designation);
	}

	public ListPatternSyntax WithDesignation(VariableDesignationSyntax? designation)
	{
		return Update(OpenBracketToken, Patterns, CloseBracketToken, designation);
	}

	public ListPatternSyntax AddPatterns(params PatternSyntax[] items)
	{
		return WithPatterns(Patterns.AddRange(items));
	}
}
