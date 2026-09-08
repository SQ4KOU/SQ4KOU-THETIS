using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ExtensionMemberCrefSyntax : MemberCrefSyntax
{
	private TypeArgumentListSyntax? typeArgumentList;

	private CrefParameterListSyntax? parameters;

	private MemberCrefSyntax? member;

	public SyntaxToken ExtensionKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExtensionMemberCrefSyntax)base.Green).extensionKeyword, base.Position, 0);

	public TypeArgumentListSyntax? TypeArgumentList => GetRed(ref typeArgumentList, 1);

	public CrefParameterListSyntax Parameters => GetRed(ref parameters, 2);

	public SyntaxToken DotToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExtensionMemberCrefSyntax)base.Green).dotToken, GetChildPosition(3), GetChildIndex(3));

	public MemberCrefSyntax Member => GetRed(ref member, 4);

	internal ExtensionMemberCrefSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref typeArgumentList, 1), 
			2 => GetRed(ref parameters, 2), 
			4 => GetRed(ref member, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			1 => typeArgumentList, 
			2 => parameters, 
			4 => member, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExtensionMemberCref(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitExtensionMemberCref(this);
	}

	public ExtensionMemberCrefSyntax Update(SyntaxToken extensionKeyword, TypeArgumentListSyntax? typeArgumentList, CrefParameterListSyntax parameters, SyntaxToken dotToken, MemberCrefSyntax member)
	{
		if (extensionKeyword != ExtensionKeyword || typeArgumentList != TypeArgumentList || parameters != Parameters || dotToken != DotToken || member != Member)
		{
			ExtensionMemberCrefSyntax extensionMemberCrefSyntax = SyntaxFactory.ExtensionMemberCref(extensionKeyword, typeArgumentList, parameters, dotToken, member);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return extensionMemberCrefSyntax;
			}
			return extensionMemberCrefSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public ExtensionMemberCrefSyntax WithExtensionKeyword(SyntaxToken extensionKeyword)
	{
		return Update(extensionKeyword, TypeArgumentList, Parameters, DotToken, Member);
	}

	public ExtensionMemberCrefSyntax WithTypeArgumentList(TypeArgumentListSyntax? typeArgumentList)
	{
		return Update(ExtensionKeyword, typeArgumentList, Parameters, DotToken, Member);
	}

	public ExtensionMemberCrefSyntax WithParameters(CrefParameterListSyntax parameters)
	{
		return Update(ExtensionKeyword, TypeArgumentList, parameters, DotToken, Member);
	}

	public ExtensionMemberCrefSyntax WithDotToken(SyntaxToken dotToken)
	{
		return Update(ExtensionKeyword, TypeArgumentList, Parameters, dotToken, Member);
	}

	public ExtensionMemberCrefSyntax WithMember(MemberCrefSyntax member)
	{
		return Update(ExtensionKeyword, TypeArgumentList, Parameters, DotToken, member);
	}

	public ExtensionMemberCrefSyntax AddTypeArgumentListArguments(params TypeSyntax[] items)
	{
		TypeArgumentListSyntax typeArgumentListSyntax = TypeArgumentList ?? SyntaxFactory.TypeArgumentList();
		return WithTypeArgumentList(typeArgumentListSyntax.WithArguments(typeArgumentListSyntax.Arguments.AddRange(items)));
	}

	public ExtensionMemberCrefSyntax AddParametersParameters(params CrefParameterSyntax[] items)
	{
		return WithParameters(Parameters.WithParameters(Parameters.Parameters.AddRange(items)));
	}
}
