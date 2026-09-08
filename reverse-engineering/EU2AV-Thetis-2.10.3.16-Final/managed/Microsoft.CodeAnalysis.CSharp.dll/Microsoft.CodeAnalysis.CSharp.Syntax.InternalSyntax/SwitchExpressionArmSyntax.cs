namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SwitchExpressionArmSyntax : CSharpSyntaxNode
{
	internal readonly PatternSyntax pattern;

	internal readonly WhenClauseSyntax? whenClause;

	internal readonly SyntaxToken equalsGreaterThanToken;

	internal readonly ExpressionSyntax expression;

	public PatternSyntax Pattern => pattern;

	public WhenClauseSyntax? WhenClause => whenClause;

	public SyntaxToken EqualsGreaterThanToken => equalsGreaterThanToken;

	public ExpressionSyntax Expression => expression;

	internal SwitchExpressionArmSyntax(SyntaxKind kind, PatternSyntax pattern, WhenClauseSyntax? whenClause, SyntaxToken equalsGreaterThanToken, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(equalsGreaterThanToken);
		this.equalsGreaterThanToken = equalsGreaterThanToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal SwitchExpressionArmSyntax(SyntaxKind kind, PatternSyntax pattern, WhenClauseSyntax? whenClause, SyntaxToken equalsGreaterThanToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(equalsGreaterThanToken);
		this.equalsGreaterThanToken = equalsGreaterThanToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal SwitchExpressionArmSyntax(SyntaxKind kind, PatternSyntax pattern, WhenClauseSyntax? whenClause, SyntaxToken equalsGreaterThanToken, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(equalsGreaterThanToken);
		this.equalsGreaterThanToken = equalsGreaterThanToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => pattern, 
			1 => whenClause, 
			2 => equalsGreaterThanToken, 
			3 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SwitchExpressionArmSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSwitchExpressionArm(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSwitchExpressionArm(this);
	}

	public SwitchExpressionArmSyntax Update(PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken equalsGreaterThanToken, ExpressionSyntax expression)
	{
		if (pattern != Pattern || whenClause != WhenClause || equalsGreaterThanToken != EqualsGreaterThanToken || expression != Expression)
		{
			SwitchExpressionArmSyntax switchExpressionArmSyntax = SyntaxFactory.SwitchExpressionArm(pattern, whenClause, equalsGreaterThanToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				switchExpressionArmSyntax = switchExpressionArmSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				switchExpressionArmSyntax = switchExpressionArmSyntax.WithAnnotationsGreen(annotations);
			}
			return switchExpressionArmSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SwitchExpressionArmSyntax(base.Kind, pattern, whenClause, equalsGreaterThanToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SwitchExpressionArmSyntax(base.Kind, pattern, whenClause, equalsGreaterThanToken, expression, GetDiagnostics(), annotations);
	}
}
