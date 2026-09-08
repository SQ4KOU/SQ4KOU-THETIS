using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ConversionOperatorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	private SyntaxNode? attributeLists;

	private ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier;

	private TypeSyntax? type;

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

	public SyntaxToken ImplicitOrExplicitKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)base.Green).implicitOrExplicitKeyword, GetChildPosition(2), GetChildIndex(2));

	public ExplicitInterfaceSpecifierSyntax? ExplicitInterfaceSpecifier => GetRed(ref explicitInterfaceSpecifier, 3);

	public SyntaxToken OperatorKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)base.Green).operatorKeyword, GetChildPosition(4), GetChildIndex(4));

	public SyntaxToken CheckedKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken checkedKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)base.Green).checkedKeyword;
			if (checkedKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, checkedKeyword, GetChildPosition(5), GetChildIndex(5));
		}
	}

	public TypeSyntax Type => GetRed(ref type, 6);

	public override ParameterListSyntax ParameterList => GetRed(ref parameterList, 7);

	public override BlockSyntax? Body => GetRed(ref body, 8);

	public override ArrowExpressionClauseSyntax? ExpressionBody => GetRed(ref expressionBody, 9);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)base.Green).semicolonToken;
			if (semicolonToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, semicolonToken, GetChildPosition(10), GetChildIndex(10));
		}
	}

	public ConversionOperatorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, implicitOrExplicitKeyword, ExplicitInterfaceSpecifier, operatorKeyword, type, parameterList, body, expressionBody, semicolonToken);
	}

	public ConversionOperatorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken implicitOrExplicitKeyword, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken operatorKeyword, TypeSyntax type, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, implicitOrExplicitKeyword, explicitInterfaceSpecifier, operatorKeyword, CheckedKeyword, type, parameterList, body, expressionBody, semicolonToken);
	}

	internal ConversionOperatorDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			3 => GetRed(ref explicitInterfaceSpecifier, 3), 
			6 => GetRed(ref type, 6), 
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
			3 => explicitInterfaceSpecifier, 
			6 => type, 
			7 => parameterList, 
			8 => body, 
			9 => expressionBody, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConversionOperatorDeclaration(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitConversionOperatorDeclaration(this);
	}

	public ConversionOperatorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken implicitOrExplicitKeyword, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken operatorKeyword, SyntaxToken checkedKeyword, TypeSyntax type, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || implicitOrExplicitKeyword != ImplicitOrExplicitKeyword || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || operatorKeyword != OperatorKeyword || checkedKeyword != CheckedKeyword || type != Type || parameterList != ParameterList || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax = SyntaxFactory.ConversionOperatorDeclaration(attributeLists, modifiers, implicitOrExplicitKeyword, explicitInterfaceSpecifier, operatorKeyword, checkedKeyword, type, parameterList, body, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return conversionOperatorDeclarationSyntax;
			}
			return conversionOperatorDeclarationSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override MemberDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new ConversionOperatorDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override MemberDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new ConversionOperatorDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithImplicitOrExplicitKeyword(SyntaxToken implicitOrExplicitKeyword)
	{
		return Update(AttributeLists, Modifiers, implicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, explicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithOperatorKeyword(SyntaxToken operatorKeyword)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, operatorKeyword, CheckedKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithCheckedKeyword(SyntaxToken checkedKeyword)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, checkedKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithType(TypeSyntax type)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithParameterListCore(ParameterListSyntax parameterList)
	{
		return WithParameterList(parameterList);
	}

	public new ConversionOperatorDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, parameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithBodyCore(BlockSyntax? body)
	{
		return WithBody(body);
	}

	public new ConversionOperatorDeclarationSyntax WithBody(BlockSyntax? body)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, ParameterList, body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithExpressionBodyCore(ArrowExpressionClauseSyntax? expressionBody)
	{
		return WithExpressionBody(expressionBody);
	}

	public new ConversionOperatorDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax? expressionBody)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, ParameterList, Body, expressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new ConversionOperatorDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, ExplicitInterfaceSpecifier, OperatorKeyword, CheckedKeyword, Type, ParameterList, Body, ExpressionBody, semicolonToken);
	}

	internal override MemberDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new ConversionOperatorDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override MemberDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new ConversionOperatorDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items)
	{
		return AddParameterListParameters(items);
	}

	public new ConversionOperatorDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	internal override BaseMethodDeclarationSyntax AddBodyAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddBodyAttributeLists(items);
	}

	public new ConversionOperatorDeclarationSyntax AddBodyAttributeLists(params AttributeListSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithAttributeLists(blockSyntax.AttributeLists.AddRange(items)));
	}

	internal override BaseMethodDeclarationSyntax AddBodyStatementsCore(params StatementSyntax[] items)
	{
		return AddBodyStatements(items);
	}

	public new ConversionOperatorDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}
}
