namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class OperatorMemberCrefSyntax : MemberCrefSyntax
{
	internal readonly SyntaxToken operatorKeyword;

	internal readonly SyntaxToken? checkedKeyword;

	internal readonly SyntaxToken operatorToken;

	internal readonly CrefParameterListSyntax? parameters;

	public SyntaxToken OperatorKeyword => operatorKeyword;

	public SyntaxToken? CheckedKeyword => checkedKeyword;

	public SyntaxToken OperatorToken => operatorToken;

	public CrefParameterListSyntax? Parameters => parameters;

	internal OperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, SyntaxToken operatorToken, CrefParameterListSyntax? parameters, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal OperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, SyntaxToken operatorToken, CrefParameterListSyntax? parameters, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal OperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, SyntaxToken operatorToken, CrefParameterListSyntax? parameters)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => operatorKeyword, 
			1 => checkedKeyword, 
			2 => operatorToken, 
			3 => parameters, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.OperatorMemberCrefSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOperatorMemberCref(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOperatorMemberCref(this);
	}

	public OperatorMemberCrefSyntax Update(SyntaxToken operatorKeyword, SyntaxToken checkedKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters)
	{
		if (operatorKeyword != OperatorKeyword || checkedKeyword != CheckedKeyword || operatorToken != OperatorToken || parameters != Parameters)
		{
			OperatorMemberCrefSyntax operatorMemberCrefSyntax = SyntaxFactory.OperatorMemberCref(operatorKeyword, checkedKeyword, operatorToken, parameters);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				operatorMemberCrefSyntax = operatorMemberCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				operatorMemberCrefSyntax = operatorMemberCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return operatorMemberCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new OperatorMemberCrefSyntax(base.Kind, operatorKeyword, checkedKeyword, operatorToken, parameters, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new OperatorMemberCrefSyntax(base.Kind, operatorKeyword, checkedKeyword, operatorToken, parameters, GetDiagnostics(), annotations);
	}
}
