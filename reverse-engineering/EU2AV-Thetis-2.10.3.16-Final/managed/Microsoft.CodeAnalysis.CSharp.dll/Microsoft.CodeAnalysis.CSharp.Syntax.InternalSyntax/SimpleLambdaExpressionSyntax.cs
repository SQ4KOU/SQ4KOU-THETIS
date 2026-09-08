using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SimpleLambdaExpressionSyntax : LambdaExpressionSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly ParameterSyntax parameter;

	internal readonly SyntaxToken arrowToken;

	internal readonly BlockSyntax? block;

	internal readonly ExpressionSyntax? expressionBody;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public ParameterSyntax Parameter => parameter;

	public override SyntaxToken ArrowToken => arrowToken;

	public override BlockSyntax? Block => block;

	public override ExpressionSyntax? ExpressionBody => expressionBody;

	internal SimpleLambdaExpressionSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, ParameterSyntax parameter, SyntaxToken arrowToken, BlockSyntax? block, ExpressionSyntax? expressionBody, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 6;
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
		AdjustFlagsAndWidth(parameter);
		this.parameter = parameter;
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

	internal SimpleLambdaExpressionSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, ParameterSyntax parameter, SyntaxToken arrowToken, BlockSyntax? block, ExpressionSyntax? expressionBody, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 6;
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
		AdjustFlagsAndWidth(parameter);
		this.parameter = parameter;
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

	internal SimpleLambdaExpressionSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, ParameterSyntax parameter, SyntaxToken arrowToken, BlockSyntax? block, ExpressionSyntax? expressionBody)
		: base(kind)
	{
		base.SlotCount = 6;
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
		AdjustFlagsAndWidth(parameter);
		this.parameter = parameter;
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
			2 => parameter, 
			3 => arrowToken, 
			4 => block, 
			5 => expressionBody, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSimpleLambdaExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleLambdaExpression(this);
	}

	public SimpleLambdaExpressionSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, ParameterSyntax parameter, SyntaxToken arrowToken, BlockSyntax block, ExpressionSyntax expressionBody)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || parameter != Parameter || arrowToken != ArrowToken || block != Block || expressionBody != ExpressionBody)
		{
			SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax = SyntaxFactory.SimpleLambdaExpression(attributeLists, modifiers, parameter, arrowToken, block, expressionBody);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				simpleLambdaExpressionSyntax = simpleLambdaExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				simpleLambdaExpressionSyntax = simpleLambdaExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return simpleLambdaExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SimpleLambdaExpressionSyntax(base.Kind, attributeLists, modifiers, parameter, arrowToken, block, expressionBody, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SimpleLambdaExpressionSyntax(base.Kind, attributeLists, modifiers, parameter, arrowToken, block, expressionBody, GetDiagnostics(), annotations);
	}
}
