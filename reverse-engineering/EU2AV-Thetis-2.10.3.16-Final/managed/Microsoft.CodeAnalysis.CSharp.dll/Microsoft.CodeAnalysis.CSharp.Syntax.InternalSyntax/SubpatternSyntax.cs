namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SubpatternSyntax : CSharpSyntaxNode
{
	internal readonly BaseExpressionColonSyntax? expressionColon;

	internal readonly PatternSyntax pattern;

	public BaseExpressionColonSyntax? ExpressionColon => expressionColon;

	public PatternSyntax Pattern => pattern;

	internal SubpatternSyntax(SyntaxKind kind, BaseExpressionColonSyntax? expressionColon, PatternSyntax pattern, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		if (expressionColon != null)
		{
			AdjustFlagsAndWidth(expressionColon);
			this.expressionColon = expressionColon;
		}
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal SubpatternSyntax(SyntaxKind kind, BaseExpressionColonSyntax? expressionColon, PatternSyntax pattern, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		if (expressionColon != null)
		{
			AdjustFlagsAndWidth(expressionColon);
			this.expressionColon = expressionColon;
		}
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal SubpatternSyntax(SyntaxKind kind, BaseExpressionColonSyntax? expressionColon, PatternSyntax pattern)
		: base(kind)
	{
		base.SlotCount = 2;
		if (expressionColon != null)
		{
			AdjustFlagsAndWidth(expressionColon);
			this.expressionColon = expressionColon;
		}
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => expressionColon, 
			1 => pattern, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SubpatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSubpattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSubpattern(this);
	}

	public SubpatternSyntax Update(BaseExpressionColonSyntax expressionColon, PatternSyntax pattern)
	{
		if (expressionColon != ExpressionColon || pattern != Pattern)
		{
			SubpatternSyntax subpatternSyntax = SyntaxFactory.Subpattern(expressionColon, pattern);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				subpatternSyntax = subpatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				subpatternSyntax = subpatternSyntax.WithAnnotationsGreen(annotations);
			}
			return subpatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SubpatternSyntax(base.Kind, expressionColon, pattern, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SubpatternSyntax(base.Kind, expressionColon, pattern, GetDiagnostics(), annotations);
	}
}
