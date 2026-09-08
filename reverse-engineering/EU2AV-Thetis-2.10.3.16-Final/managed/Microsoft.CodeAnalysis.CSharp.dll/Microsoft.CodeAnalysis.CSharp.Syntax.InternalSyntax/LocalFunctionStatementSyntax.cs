using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LocalFunctionStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly TypeSyntax returnType;

	internal readonly SyntaxToken identifier;

	internal readonly TypeParameterListSyntax? typeParameterList;

	internal readonly ParameterListSyntax parameterList;

	internal readonly GreenNode? constraintClauses;

	internal readonly BlockSyntax? body;

	internal readonly ArrowExpressionClauseSyntax? expressionBody;

	internal readonly SyntaxToken? semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public TypeSyntax ReturnType => returnType;

	public SyntaxToken Identifier => identifier;

	public TypeParameterListSyntax? TypeParameterList => typeParameterList;

	public ParameterListSyntax ParameterList => parameterList;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax>(constraintClauses);

	public BlockSyntax? Body => body;

	public ArrowExpressionClauseSyntax? ExpressionBody => expressionBody;

	public SyntaxToken? SemicolonToken => semicolonToken;

	internal LocalFunctionStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax parameterList, GreenNode? constraintClauses, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 10;
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
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (typeParameterList != null)
		{
			AdjustFlagsAndWidth(typeParameterList);
			this.typeParameterList = typeParameterList;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (constraintClauses != null)
		{
			AdjustFlagsAndWidth(constraintClauses);
			this.constraintClauses = constraintClauses;
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

	internal LocalFunctionStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax parameterList, GreenNode? constraintClauses, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 10;
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
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (typeParameterList != null)
		{
			AdjustFlagsAndWidth(typeParameterList);
			this.typeParameterList = typeParameterList;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (constraintClauses != null)
		{
			AdjustFlagsAndWidth(constraintClauses);
			this.constraintClauses = constraintClauses;
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

	internal LocalFunctionStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax parameterList, GreenNode? constraintClauses, BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, SyntaxToken? semicolonToken)
		: base(kind)
	{
		base.SlotCount = 10;
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
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (typeParameterList != null)
		{
			AdjustFlagsAndWidth(typeParameterList);
			this.typeParameterList = typeParameterList;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		if (constraintClauses != null)
		{
			AdjustFlagsAndWidth(constraintClauses);
			this.constraintClauses = constraintClauses;
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
			2 => returnType, 
			3 => identifier, 
			4 => typeParameterList, 
			5 => parameterList, 
			6 => constraintClauses, 
			7 => body, 
			8 => expressionBody, 
			9 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLocalFunctionStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLocalFunctionStatement(this);
	}

	public LocalFunctionStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || returnType != ReturnType || identifier != Identifier || typeParameterList != TypeParameterList || parameterList != ParameterList || constraintClauses != ConstraintClauses || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			LocalFunctionStatementSyntax localFunctionStatementSyntax = SyntaxFactory.LocalFunctionStatement(attributeLists, modifiers, returnType, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				localFunctionStatementSyntax = localFunctionStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				localFunctionStatementSyntax = localFunctionStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return localFunctionStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LocalFunctionStatementSyntax(base.Kind, attributeLists, modifiers, returnType, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LocalFunctionStatementSyntax(base.Kind, attributeLists, modifiers, returnType, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken, GetDiagnostics(), annotations);
	}
}
