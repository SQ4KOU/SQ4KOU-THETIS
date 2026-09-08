using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class OperatorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	private SyntaxNode? attributeLists;

	private TypeSyntax? returnType;

	private ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier;

	private ParameterListSyntax? parameterList;

	private BlockSyntax? body;

	private ArrowExpressionClauseSyntax? expressionBody;

	public override SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public override SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = base.Green.GetSlot(1);
			if (slot == null)
			{
				return default(SyntaxTokenList);
			}
			return new SyntaxTokenList(this, slot, GetChildPosition(1), GetChildIndex(1));
		}
	}

	public TypeSyntax ReturnType => GetRed(ref returnType, 2);

	public ExplicitInterfaceSpecifierSyntax? ExplicitInterfaceSpecifier => GetRed(ref explicitInterfaceSpecifier, 3);

	public SyntaxToken OperatorKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)base.Green).operatorKeyword, GetChildPosition(4), GetChildIndex(4));

	public SyntaxToken CheckedKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken checkedKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)base.Green).checkedKeyword;
			if (checkedKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, checkedKeyword, GetChildPosition(5), GetChildIndex(5));
		}
	}

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)base.Green).operatorToken, GetChildPosition(6), GetChildIndex(6));

	public override ParameterListSyntax ParameterList => GetRed(ref parameterList, 7);

	public override BlockSyntax? Body => GetRed(ref body, 8);

	public override ArrowExpressionClauseSyntax? ExpressionBody => GetRed(ref expressionBody, 9);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)base.Green).semicolonToken;
			if (semicolonToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, semicolonToken, GetChildPosition(10), GetChildIndex(10));
		}
	}

	public OperatorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax returnType, SyntaxToken operatorKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, returnType, ExplicitInterfaceSpecifier, operatorKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken);
	}

	public OperatorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken operatorKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, operatorKeyword, CheckedKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken);
	}

	internal OperatorDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			2 => GetRed(ref returnType, 2), 
			3 => GetRed(ref explicitInterfaceSpecifier, 3), 
			7 => GetRed(ref parameterList, 7), 
			8 => GetRed(ref body, 8), 
			9 => GetRed(ref expressionBody, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			2 => returnType, 
			3 => explicitInterfaceSpecifier, 
			7 => parameterList, 
			8 => body, 
			9 => expressionBody, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOperatorDeclaration(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitOperatorDeclaration(this);
	}

	public OperatorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken operatorKeyword, SyntaxToken checkedKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || returnType != ReturnType || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || operatorKeyword != OperatorKeyword || checkedKeyword != CheckedKeyword || operatorToken != OperatorToken || parameterList != ParameterList || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			OperatorDeclarationSyntax operatorDeclarationSyntax = SyntaxFactory.OperatorDeclaration(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, operatorKeyword, checkedKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return operatorDeclarationSyntax;
			}
			return operatorDeclarationSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override MemberDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new OperatorDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override MemberDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new OperatorDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public OperatorDeclarationSyntax WithReturnType(TypeSyntax returnType)
	{
		return Update(AttributeLists, Modifiers, returnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public OperatorDeclarationSyntax WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier)
	{
		return Update(AttributeLists, Modifiers, ReturnType, explicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public OperatorDeclarationSyntax WithOperatorKeyword(SyntaxToken operatorKeyword)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, operatorKeyword, CheckedKeyword, OperatorToken, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public OperatorDeclarationSyntax WithCheckedKeyword(SyntaxToken checkedKeyword)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, checkedKeyword, OperatorToken, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public OperatorDeclarationSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, operatorToken, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithParameterListCore(ParameterListSyntax parameterList)
	{
		return WithParameterList(parameterList);
	}

	public new OperatorDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, parameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithBodyCore(BlockSyntax? body)
	{
		return WithBody(body);
	}

	public new OperatorDeclarationSyntax WithBody(BlockSyntax? body)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, ParameterList, body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithExpressionBodyCore(ArrowExpressionClauseSyntax? expressionBody)
	{
		return WithExpressionBody(expressionBody);
	}

	public new OperatorDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax? expressionBody)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, ParameterList, Body, expressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new OperatorDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, OperatorToken, ParameterList, Body, ExpressionBody, semicolonToken);
	}

	internal override MemberDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new OperatorDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override MemberDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new OperatorDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items)
	{
		return AddParameterListParameters(items);
	}

	public new OperatorDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	internal override BaseMethodDeclarationSyntax AddBodyAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddBodyAttributeLists(items);
	}

	public new OperatorDeclarationSyntax AddBodyAttributeLists(params AttributeListSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithAttributeLists(blockSyntax.AttributeLists.AddRange(items)));
	}

	internal override BaseMethodDeclarationSyntax AddBodyStatementsCore(params StatementSyntax[] items)
	{
		return AddBodyStatements(items);
	}

	public new OperatorDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}
}
