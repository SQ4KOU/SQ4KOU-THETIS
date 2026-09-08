namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ElifDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken elifKeyword;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	internal readonly bool branchTaken;

	internal readonly bool conditionValue;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken ElifKeyword => elifKeyword;

	public override ExpressionSyntax Condition => condition;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	public override bool BranchTaken => branchTaken;

	public override bool ConditionValue => conditionValue;

	internal ElifDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(elifKeyword);
		this.elifKeyword = elifKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
		this.conditionValue = conditionValue;
	}

	internal ElifDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(elifKeyword);
		this.elifKeyword = elifKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
		this.conditionValue = conditionValue;
	}

	internal ElifDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(elifKeyword);
		this.elifKeyword = elifKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
		this.conditionValue = conditionValue;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => elifKeyword, 
			2 => condition, 
			3 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ElifDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElifDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElifDirectiveTrivia(this);
	}

	public ElifDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		if (hashToken != HashToken || elifKeyword != ElifKeyword || condition != Condition || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ElifDirectiveTriviaSyntax elifDirectiveTriviaSyntax = SyntaxFactory.ElifDirectiveTrivia(hashToken, elifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				elifDirectiveTriviaSyntax = elifDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				elifDirectiveTriviaSyntax = elifDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return elifDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ElifDirectiveTriviaSyntax(base.Kind, hashToken, elifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ElifDirectiveTriviaSyntax(base.Kind, hashToken, elifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue, GetDiagnostics(), annotations);
	}
}
