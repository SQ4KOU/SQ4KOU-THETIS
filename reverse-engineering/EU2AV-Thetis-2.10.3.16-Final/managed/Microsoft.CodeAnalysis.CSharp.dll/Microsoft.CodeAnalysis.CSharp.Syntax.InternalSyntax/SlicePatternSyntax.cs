namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SlicePatternSyntax : PatternSyntax
{
	internal readonly SyntaxToken dotDotToken;

	internal readonly PatternSyntax? pattern;

	public SyntaxToken DotDotToken => dotDotToken;

	public PatternSyntax? Pattern => pattern;

	internal SlicePatternSyntax(SyntaxKind kind, SyntaxToken dotDotToken, PatternSyntax? pattern, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(dotDotToken);
		this.dotDotToken = dotDotToken;
		if (pattern != null)
		{
			AdjustFlagsAndWidth(pattern);
			this.pattern = pattern;
		}
	}

	internal SlicePatternSyntax(SyntaxKind kind, SyntaxToken dotDotToken, PatternSyntax? pattern, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(dotDotToken);
		this.dotDotToken = dotDotToken;
		if (pattern != null)
		{
			AdjustFlagsAndWidth(pattern);
			this.pattern = pattern;
		}
	}

	internal SlicePatternSyntax(SyntaxKind kind, SyntaxToken dotDotToken, PatternSyntax? pattern)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(dotDotToken);
		this.dotDotToken = dotDotToken;
		if (pattern != null)
		{
			AdjustFlagsAndWidth(pattern);
			this.pattern = pattern;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => dotDotToken, 
			1 => pattern, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SlicePatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSlicePattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSlicePattern(this);
	}

	public SlicePatternSyntax Update(SyntaxToken dotDotToken, PatternSyntax pattern)
	{
		if (dotDotToken != DotDotToken || pattern != Pattern)
		{
			SlicePatternSyntax slicePatternSyntax = SyntaxFactory.SlicePattern(dotDotToken, pattern);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				slicePatternSyntax = slicePatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				slicePatternSyntax = slicePatternSyntax.WithAnnotationsGreen(annotations);
			}
			return slicePatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SlicePatternSyntax(base.Kind, dotDotToken, pattern, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SlicePatternSyntax(base.Kind, dotDotToken, pattern, GetDiagnostics(), annotations);
	}
}
