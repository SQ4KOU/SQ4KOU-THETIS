namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ParenthesizedPatternSyntax : PatternSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly PatternSyntax pattern;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public PatternSyntax Pattern => pattern;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal ParenthesizedPatternSyntax(SyntaxKind kind, SyntaxToken openParenToken, PatternSyntax pattern, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ParenthesizedPatternSyntax(SyntaxKind kind, SyntaxToken openParenToken, PatternSyntax pattern, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ParenthesizedPatternSyntax(SyntaxKind kind, SyntaxToken openParenToken, PatternSyntax pattern, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => pattern, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedPattern(this);
	}

	public ParenthesizedPatternSyntax Update(SyntaxToken openParenToken, PatternSyntax pattern, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || pattern != Pattern || closeParenToken != CloseParenToken)
		{
			ParenthesizedPatternSyntax parenthesizedPatternSyntax = SyntaxFactory.ParenthesizedPattern(openParenToken, pattern, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				parenthesizedPatternSyntax = parenthesizedPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				parenthesizedPatternSyntax = parenthesizedPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return parenthesizedPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ParenthesizedPatternSyntax(base.Kind, openParenToken, pattern, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ParenthesizedPatternSyntax(base.Kind, openParenToken, pattern, closeParenToken, GetDiagnostics(), annotations);
	}
}
