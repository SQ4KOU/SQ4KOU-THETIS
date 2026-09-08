using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ParenthesizedLambdaExpressionSyntax : LambdaExpressionSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly TypeSyntax? returnType;

	internal readonly ParameterListSyntax parameterList;

	internal readonly SyntaxToken arrowToken;

	internal readonly BlockSyntax? block;

	internal readonly ExpressionSyntax? expressionBody;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public TypeSyntax? ReturnType => returnType;

	public ParameterListSyntax ParameterList => parameterList;

	public override SyntaxToken ArrowToken => arrowToken;

	public override BlockSyntax? Block => block;

	public override ExpressionSyntax? ExpressionBody => expressionBody;

	internal ParenthesizedLambdaExpressionSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? returnType, ParameterListSyntax parameterList, SyntaxToken arrowToken, BlockSyntax? block, ExpressionSyntax? expressionBody, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 7;
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
		if (returnType != null)
		{
			AdjustFlagsAndWidth(returnType);
			this.returnType = returnType;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		if (block != null)
		{
			AdjustFlagsAndWidth(block);
			this.block = block;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
	}

	internal ParenthesizedLambdaExpressionSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? returnType, ParameterListSyntax parameterList, SyntaxToken arrowToken, BlockSyntax? block, ExpressionSyntax? expressionBody, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 7;
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
		if (returnType != null)
		{
			AdjustFlagsAndWidth(returnType);
			this.returnType = returnType;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		if (block != null)
		{
			AdjustFlagsAndWidth(block);
			this.block = block;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
	}

	internal ParenthesizedLambdaExpressionSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? returnType, ParameterListSyntax parameterList, SyntaxToken arrowToken, BlockSyntax? block, ExpressionSyntax? expressionBody)
		: base(kind)
	{
		base.SlotCount = 7;
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
		if (returnType != null)
		{
			AdjustFlagsAndWidth(returnType);
			this.returnType = returnType;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		if (block != null)
		{
			AdjustFlagsAndWidth(block);
			this.block = block;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => returnType, 
			3 => parameterList, 
			4 => arrowToken, 
			5 => block, 
			6 => expressionBody, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedLambdaExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedLambdaExpression(this);
	}

	public ParenthesizedLambdaExpressionSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, ParameterListSyntax parameterList, SyntaxToken arrowToken, BlockSyntax block, ExpressionSyntax expressionBody)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || returnType != ReturnType || parameterList != ParameterList || arrowToken != ArrowToken || block != Block || expressionBody != ExpressionBody)
		{
			ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax = SyntaxFactory.ParenthesizedLambdaExpression(attributeLists, modifiers, returnType, parameterList, arrowToken, block, expressionBody);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				parenthesizedLambdaExpressionSyntax = parenthesizedLambdaExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				parenthesizedLambdaExpressionSyntax = parenthesizedLambdaExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return parenthesizedLambdaExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ParenthesizedLambdaExpressionSyntax(base.Kind, attributeLists, modifiers, returnType, parameterList, arrowToken, block, expressionBody, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ParenthesizedLambdaExpressionSyntax(base.Kind, attributeLists, modifiers, returnType, parameterList, arrowToken, block, expressionBody, GetDiagnostics(), annotations);
	}
}
