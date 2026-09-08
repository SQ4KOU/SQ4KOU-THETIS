namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class UnaryPatternSyntax : PatternSyntax
{
	internal readonly SyntaxToken operatorToken;

	internal readonly PatternSyntax pattern;

	public SyntaxToken OperatorToken => operatorToken;

	public PatternSyntax Pattern => pattern;

	internal UnaryPatternSyntax(SyntaxKind kind, SyntaxToken operatorToken, PatternSyntax pattern, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal UnaryPatternSyntax(SyntaxKind kind, SyntaxToken operatorToken, PatternSyntax pattern, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal UnaryPatternSyntax(SyntaxKind kind, SyntaxToken operatorToken, PatternSyntax pattern)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => operatorToken, 
			1 => pattern, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.UnaryPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUnaryPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUnaryPattern(this);
	}

	public UnaryPatternSyntax Update(SyntaxToken operatorToken, PatternSyntax pattern)
	{
		if (operatorToken != OperatorToken || pattern != Pattern)
		{
			UnaryPatternSyntax unaryPatternSyntax = SyntaxFactory.UnaryPattern(operatorToken, pattern);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				unaryPatternSyntax = unaryPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				unaryPatternSyntax = unaryPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return unaryPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new UnaryPatternSyntax(base.Kind, operatorToken, pattern, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new UnaryPatternSyntax(base.Kind, operatorToken, pattern, GetDiagnostics(), annotations);
	}
}
