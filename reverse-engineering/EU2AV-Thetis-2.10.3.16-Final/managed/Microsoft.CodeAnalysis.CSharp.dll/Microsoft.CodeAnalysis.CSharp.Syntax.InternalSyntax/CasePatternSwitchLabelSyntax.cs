namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CasePatternSwitchLabelSyntax : SwitchLabelSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly PatternSyntax pattern;

	internal readonly WhenClauseSyntax? whenClause;

	internal readonly SyntaxToken colonToken;

	public override SyntaxToken Keyword => keyword;

	public PatternSyntax Pattern => pattern;

	public WhenClauseSyntax? WhenClause => whenClause;

	public override SyntaxToken ColonToken => colonToken;

	internal CasePatternSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax? whenClause, SyntaxToken colonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal CasePatternSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax? whenClause, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal CasePatternSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax? whenClause, SyntaxToken colonToken)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => keyword, 
			1 => pattern, 
			2 => whenClause, 
			3 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCasePatternSwitchLabel(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCasePatternSwitchLabel(this);
	}

	public CasePatternSwitchLabelSyntax Update(SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken)
	{
		if (keyword != Keyword || pattern != Pattern || whenClause != WhenClause || colonToken != ColonToken)
		{
			CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = SyntaxFactory.CasePatternSwitchLabel(keyword, pattern, whenClause, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				casePatternSwitchLabelSyntax = casePatternSwitchLabelSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				casePatternSwitchLabelSyntax = casePatternSwitchLabelSyntax.WithAnnotationsGreen(annotations);
			}
			return casePatternSwitchLabelSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new CasePatternSwitchLabelSyntax(base.Kind, keyword, pattern, whenClause, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new CasePatternSwitchLabelSyntax(base.Kind, keyword, pattern, whenClause, colonToken, GetDiagnostics(), annotations);
	}
}
