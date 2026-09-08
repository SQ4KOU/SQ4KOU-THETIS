using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class OperatorMemberCrefSyntax : MemberCrefSyntax
{
	private CrefParameterListSyntax? parameters;

	public SyntaxToken OperatorKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorMemberCrefSyntax)base.Green).operatorKeyword, base.Position, 0);

	public SyntaxToken CheckedKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken checkedKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorMemberCrefSyntax)base.Green).checkedKeyword;
			if (checkedKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, checkedKeyword, GetChildPosition(1), GetChildIndex(1));
		}
	}

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorMemberCrefSyntax)base.Green).operatorToken, GetChildPosition(2), GetChildIndex(2));

	public CrefParameterListSyntax? Parameters => GetRed(ref parameters, 3);

	public OperatorMemberCrefSyntax Update(SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax? parameters)
	{
		return Update(operatorKeyword, CheckedKeyword, operatorToken, parameters);
	}

	internal OperatorMemberCrefSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 3)
		{
			return null;
		}
		return GetRed(ref parameters, 3);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 3)
		{
			return null;
		}
		return parameters;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOperatorMemberCref(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitOperatorMemberCref(this);
	}

	public OperatorMemberCrefSyntax Update(SyntaxToken operatorKeyword, SyntaxToken checkedKeyword, SyntaxToken operatorToken, CrefParameterListSyntax? parameters)
	{
		if (operatorKeyword != OperatorKeyword || checkedKeyword != CheckedKeyword || operatorToken != OperatorToken || parameters != Parameters)
		{
			OperatorMemberCrefSyntax operatorMemberCrefSyntax = SyntaxFactory.OperatorMemberCref(operatorKeyword, checkedKeyword, operatorToken, parameters);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return operatorMemberCrefSyntax;
			}
			return operatorMemberCrefSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public OperatorMemberCrefSyntax WithOperatorKeyword(SyntaxToken operatorKeyword)
	{
		return Update(operatorKeyword, CheckedKeyword, OperatorToken, Parameters);
	}

	public OperatorMemberCrefSyntax WithCheckedKeyword(SyntaxToken checkedKeyword)
	{
		return Update(OperatorKeyword, checkedKeyword, OperatorToken, Parameters);
	}

	public OperatorMemberCrefSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(OperatorKeyword, CheckedKeyword, operatorToken, Parameters);
	}

	public OperatorMemberCrefSyntax WithParameters(CrefParameterListSyntax? parameters)
	{
		return Update(OperatorKeyword, CheckedKeyword, OperatorToken, parameters);
	}

	public OperatorMemberCrefSyntax AddParametersParameters(params CrefParameterSyntax[] items)
	{
		CrefParameterListSyntax crefParameterListSyntax = Parameters ?? SyntaxFactory.CrefParameterList();
		return WithParameters(crefParameterListSyntax.WithParameters(crefParameterListSyntax.Parameters.AddRange(items)));
	}
}
