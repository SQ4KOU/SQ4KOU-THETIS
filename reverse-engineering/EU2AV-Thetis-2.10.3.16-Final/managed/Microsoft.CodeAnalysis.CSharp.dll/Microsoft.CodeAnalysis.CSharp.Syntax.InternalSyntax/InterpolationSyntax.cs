namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InterpolationSyntax : InterpolatedStringContentSyntax
{
	internal readonly SyntaxToken openBraceToken;

	internal readonly ExpressionSyntax expression;

	internal readonly InterpolationAlignmentClauseSyntax? alignmentClause;

	internal readonly InterpolationFormatClauseSyntax? formatClause;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public ExpressionSyntax Expression => expression;

	public InterpolationAlignmentClauseSyntax? AlignmentClause => alignmentClause;

	public InterpolationFormatClauseSyntax? FormatClause => formatClause;

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal InterpolationSyntax(SyntaxKind kind, SyntaxToken openBraceToken, ExpressionSyntax expression, InterpolationAlignmentClauseSyntax? alignmentClause, InterpolationFormatClauseSyntax? formatClause, SyntaxToken closeBraceToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (alignmentClause != null)
		{
			AdjustFlagsAndWidth(alignmentClause);
			this.alignmentClause = alignmentClause;
		}
		if (formatClause != null)
		{
			AdjustFlagsAndWidth(formatClause);
			this.formatClause = formatClause;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal InterpolationSyntax(SyntaxKind kind, SyntaxToken openBraceToken, ExpressionSyntax expression, InterpolationAlignmentClauseSyntax? alignmentClause, InterpolationFormatClauseSyntax? formatClause, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (alignmentClause != null)
		{
			AdjustFlagsAndWidth(alignmentClause);
			this.alignmentClause = alignmentClause;
		}
		if (formatClause != null)
		{
			AdjustFlagsAndWidth(formatClause);
			this.formatClause = formatClause;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal InterpolationSyntax(SyntaxKind kind, SyntaxToken openBraceToken, ExpressionSyntax expression, InterpolationAlignmentClauseSyntax? alignmentClause, InterpolationFormatClauseSyntax? formatClause, SyntaxToken closeBraceToken)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (alignmentClause != null)
		{
			AdjustFlagsAndWidth(alignmentClause);
			this.alignmentClause = alignmentClause;
		}
		if (formatClause != null)
		{
			AdjustFlagsAndWidth(formatClause);
			this.formatClause = formatClause;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBraceToken, 
			1 => expression, 
			2 => alignmentClause, 
			3 => formatClause, 
			4 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInterpolation(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInterpolation(this);
	}

	public InterpolationSyntax Update(SyntaxToken openBraceToken, ExpressionSyntax expression, InterpolationAlignmentClauseSyntax alignmentClause, InterpolationFormatClauseSyntax formatClause, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || expression != Expression || alignmentClause != AlignmentClause || formatClause != FormatClause || closeBraceToken != CloseBraceToken)
		{
			InterpolationSyntax interpolationSyntax = SyntaxFactory.Interpolation(openBraceToken, expression, alignmentClause, formatClause, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				interpolationSyntax = interpolationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				interpolationSyntax = interpolationSyntax.WithAnnotationsGreen(annotations);
			}
			return interpolationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new InterpolationSyntax(base.Kind, openBraceToken, expression, alignmentClause, formatClause, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new InterpolationSyntax(base.Kind, openBraceToken, expression, alignmentClause, formatClause, closeBraceToken, GetDiagnostics(), annotations);
	}
}
