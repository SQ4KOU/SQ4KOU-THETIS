using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class OperatorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly TypeSyntax returnType;

	internal readonly ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier;

	internal readonly SyntaxToken operatorKeyword;

	internal readonly SyntaxToken? checkedKeyword;

	internal readonly SyntaxToken operatorToken;

	internal readonly ParameterListSyntax parameterList;

	internal readonly BlockSyntax? body;

	internal readonly ArrowExpressionClauseSyntax? expressionBody;

	internal readonly SyntaxToken? semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public TypeSyntax ReturnType => returnType;

	public ExplicitInterfaceSpecifierSyntax? ExplicitInterfaceSpecifier => explicitInterfaceSpecifier;

	public SyntaxToken OperatorKeyword => operatorKeyword;

	public SyntaxToken? CheckedKeyword => checkedKeyword;

	public SyntaxToken OperatorToken => operatorToken;

	public override ParameterListSyntax ParameterList => parameterList;

	public override BlockSyntax? Body => body;

	public override ArrowExpressionClauseSyntax? ExpressionBody => expressionBody;

	public override SyntaxToken? SemicolonToken => semicolonToken;

	internal OperatorDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 11;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(returnType);
		this.returnType = returnType;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			this.body = body;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal OperatorDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 11;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(returnType);
		this.returnType = returnType;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			this.body = body;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal OperatorDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken)
		: base(kind)
	{
		base.SlotCount = 11;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(returnType);
		this.returnType = returnType;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			this.body = body;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => returnType, 
			3 => explicitInterfaceSpecifier, 
			4 => operatorKeyword, 
			5 => checkedKeyword, 
			6 => operatorToken, 
			7 => parameterList, 
			8 => body, 
			9 => expressionBody, 
			10 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOperatorDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOperatorDeclaration(this);
	}

	public OperatorDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken operatorKeyword, SyntaxToken checkedKeyword, SyntaxToken operatorToken, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || returnType != ReturnType || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || operatorKeyword != OperatorKeyword || checkedKeyword != CheckedKeyword || operatorToken != OperatorToken || parameterList != ParameterList || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			OperatorDeclarationSyntax operatorDeclarationSyntax = SyntaxFactory.OperatorDeclaration(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, operatorKeyword, checkedKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				operatorDeclarationSyntax = operatorDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				operatorDeclarationSyntax = operatorDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return operatorDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new OperatorDeclarationSyntax(base.Kind, attributeLists, modifiers, returnType, explicitInterfaceSpecifier, operatorKeyword, checkedKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new OperatorDeclarationSyntax(base.Kind, attributeLists, modifiers, returnType, explicitInterfaceSpecifier, operatorKeyword, checkedKeyword, operatorToken, parameterList, body, expressionBody, semicolonToken, GetDiagnostics(), annotations);
	}
}
