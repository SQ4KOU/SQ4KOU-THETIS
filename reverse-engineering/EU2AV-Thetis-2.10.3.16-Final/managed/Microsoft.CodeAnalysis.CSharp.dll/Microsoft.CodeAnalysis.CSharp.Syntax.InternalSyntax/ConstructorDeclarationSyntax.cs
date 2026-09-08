using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConstructorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly SyntaxToken identifier;

	internal readonly ParameterListSyntax parameterList;

	internal readonly ConstructorInitializerSyntax? initializer;

	internal readonly BlockSyntax? body;

	internal readonly ArrowExpressionClauseSyntax? expressionBody;

	internal readonly SyntaxToken? semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public SyntaxToken Identifier => identifier;

	public override ParameterListSyntax ParameterList => parameterList;

	public ConstructorInitializerSyntax? Initializer => initializer;

	public override BlockSyntax? Body => body;

	public override ArrowExpressionClauseSyntax? ExpressionBody => expressionBody;

	public override SyntaxToken? SemicolonToken => semicolonToken;

	internal ConstructorDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, ConstructorInitializerSyntax? initializer, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 8;
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
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
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

	internal ConstructorDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, ConstructorInitializerSyntax? initializer, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 8;
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
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
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

	internal ConstructorDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, ConstructorInitializerSyntax? initializer, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken)
		: base(kind)
	{
		base.SlotCount = 8;
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
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
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
			2 => identifier, 
			3 => parameterList, 
			4 => initializer, 
			5 => body, 
			6 => expressionBody, 
			7 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConstructorDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstructorDeclaration(this);
	}

	public ConstructorDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, ConstructorInitializerSyntax initializer, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || identifier != Identifier || parameterList != ParameterList || initializer != Initializer || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			ConstructorDeclarationSyntax constructorDeclarationSyntax = SyntaxFactory.ConstructorDeclaration(attributeLists, modifiers, identifier, parameterList, initializer, body, expressionBody, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				constructorDeclarationSyntax = constructorDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				constructorDeclarationSyntax = constructorDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return constructorDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ConstructorDeclarationSyntax(base.Kind, attributeLists, modifiers, identifier, parameterList, initializer, body, expressionBody, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ConstructorDeclarationSyntax(base.Kind, attributeLists, modifiers, identifier, parameterList, initializer, body, expressionBody, semicolonToken, GetDiagnostics(), annotations);
	}
}
