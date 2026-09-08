using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ConversionOperatorMemberCrefSyntax : MemberCrefSyntax
{
	private TypeSyntax? type;

	private CrefParameterListSyntax? parameters;

	public SyntaxToken ImplicitOrExplicitKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorMemberCrefSyntax)base.Green).implicitOrExplicitKeyword, base.Position, 0);

	public SyntaxToken OperatorKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorMemberCrefSyntax)base.Green).operatorKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken CheckedKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken checkedKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorMemberCrefSyntax)base.Green).checkedKeyword;
			if (checkedKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, checkedKeyword, GetChildPosition(2), GetChildIndex(2));
		}
	}

	public TypeSyntax Type => GetRed(ref type, 3);

	public CrefParameterListSyntax? Parameters => GetRed(ref parameters, 4);

	public ConversionOperatorMemberCrefSyntax Update(SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, CrefParameterListSyntax? parameters)
	{
		return Update(implicitOrExplicitKeyword, operatorKeyword, CheckedKeyword, type, parameters);
	}

	internal ConversionOperatorMemberCrefSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			3 => GetRed(ref type, 3), 
			4 => GetRed(ref parameters, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			3 => type, 
			4 => parameters, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConversionOperatorMemberCref(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitConversionOperatorMemberCref(this);
	}

	public ConversionOperatorMemberCrefSyntax Update(SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, SyntaxToken checkedKeyword, TypeSyntax type, CrefParameterListSyntax? parameters)
	{
		if (implicitOrExplicitKeyword != ImplicitOrExplicitKeyword || operatorKeyword != OperatorKeyword || checkedKeyword != CheckedKeyword || type != Type || parameters != Parameters)
		{
			ConversionOperatorMemberCrefSyntax conversionOperatorMemberCrefSyntax = SyntaxFactory.ConversionOperatorMemberCref(implicitOrExplicitKeyword, operatorKeyword, checkedKeyword, type, parameters);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return conversionOperatorMemberCrefSyntax;
			}
			return conversionOperatorMemberCrefSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public ConversionOperatorMemberCrefSyntax WithImplicitOrExplicitKeyword(SyntaxToken implicitOrExplicitKeyword)
	{
		return Update(implicitOrExplicitKeyword, OperatorKeyword, CheckedKeyword, Type, Parameters);
	}

	public ConversionOperatorMemberCrefSyntax WithOperatorKeyword(SyntaxToken operatorKeyword)
	{
		return Update(ImplicitOrExplicitKeyword, operatorKeyword, CheckedKeyword, Type, Parameters);
	}

	public ConversionOperatorMemberCrefSyntax WithCheckedKeyword(SyntaxToken checkedKeyword)
	{
		return Update(ImplicitOrExplicitKeyword, OperatorKeyword, checkedKeyword, Type, Parameters);
	}

	public ConversionOperatorMemberCrefSyntax WithType(TypeSyntax type)
	{
		return Update(ImplicitOrExplicitKeyword, OperatorKeyword, CheckedKeyword, type, Parameters);
	}

	public ConversionOperatorMemberCrefSyntax WithParameters(CrefParameterListSyntax? parameters)
	{
		return Update(ImplicitOrExplicitKeyword, OperatorKeyword, CheckedKeyword, Type, parameters);
	}

	public ConversionOperatorMemberCrefSyntax AddParametersParameters(params CrefParameterSyntax[] items)
	{
		CrefParameterListSyntax crefParameterListSyntax = Parameters ?? SyntaxFactory.CrefParameterList();
		return WithParameters(crefParameterListSyntax.WithParameters(crefParameterListSyntax.Parameters.AddRange(items)));
	}
}
