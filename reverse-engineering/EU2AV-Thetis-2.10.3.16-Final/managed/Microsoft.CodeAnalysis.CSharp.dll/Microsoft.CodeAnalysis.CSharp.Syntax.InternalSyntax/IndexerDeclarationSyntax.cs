using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IndexerDeclarationSyntax : BasePropertyDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly TypeSyntax type;

	internal readonly ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier;

	internal readonly SyntaxToken thisKeyword;

	internal readonly BracketedParameterListSyntax parameterList;

	internal readonly AccessorListSyntax? accessorList;

	internal readonly ArrowExpressionClauseSyntax? expressionBody;

	internal readonly SyntaxToken? semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override TypeSyntax Type => type;

	public override ExplicitInterfaceSpecifierSyntax? ExplicitInterfaceSpecifier => explicitInterfaceSpecifier;

	public SyntaxToken ThisKeyword => thisKeyword;

	public BracketedParameterListSyntax ParameterList => parameterList;

	public override AccessorListSyntax? AccessorList => accessorList;

	public ArrowExpressionClauseSyntax? ExpressionBody => expressionBody;

	public SyntaxToken? SemicolonToken => semicolonToken;

	internal IndexerDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken thisKeyword, BracketedParameterListSyntax parameterList, AccessorListSyntax? accessorList, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 9;
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
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(thisKeyword);
		this.thisKeyword = thisKeyword;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (accessorList != null)
		{
			AdjustFlagsAndWidth(accessorList);
			this.accessorList = accessorList;
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

	internal IndexerDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken thisKeyword, BracketedParameterListSyntax parameterList, AccessorListSyntax? accessorList, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 9;
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
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(thisKeyword);
		this.thisKeyword = thisKeyword;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (accessorList != null)
		{
			AdjustFlagsAndWidth(accessorList);
			this.accessorList = accessorList;
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

	internal IndexerDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier, SyntaxToken thisKeyword, BracketedParameterListSyntax parameterList, AccessorListSyntax? accessorList, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken)
		: base(kind)
	{
		base.SlotCount = 9;
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
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(thisKeyword);
		this.thisKeyword = thisKeyword;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (accessorList != null)
		{
			AdjustFlagsAndWidth(accessorList);
			this.accessorList = accessorList;
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
			2 => type, 
			3 => explicitInterfaceSpecifier, 
			4 => thisKeyword, 
			5 => parameterList, 
			6 => accessorList, 
			7 => expressionBody, 
			8 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIndexerDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIndexerDeclaration(this);
	}

	public IndexerDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken thisKeyword, BracketedParameterListSyntax parameterList, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || thisKeyword != ThisKeyword || parameterList != ParameterList || accessorList != AccessorList || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			IndexerDeclarationSyntax indexerDeclarationSyntax = SyntaxFactory.IndexerDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				indexerDeclarationSyntax = indexerDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				indexerDeclarationSyntax = indexerDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return indexerDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new IndexerDeclarationSyntax(base.Kind, attributeLists, modifiers, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new IndexerDeclarationSyntax(base.Kind, attributeLists, modifiers, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken, GetDiagnostics(), annotations);
	}
}
