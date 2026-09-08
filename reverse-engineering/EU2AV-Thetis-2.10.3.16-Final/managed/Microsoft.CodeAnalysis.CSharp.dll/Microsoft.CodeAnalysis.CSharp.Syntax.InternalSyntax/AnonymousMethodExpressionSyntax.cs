using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AnonymousMethodExpressionSyntax : AnonymousFunctionExpressionSyntax
{
	internal readonly GreenNode? modifiers;

	internal readonly SyntaxToken delegateKeyword;

	internal readonly ParameterListSyntax? parameterList;

	internal readonly BlockSyntax block;

	internal readonly ExpressionSyntax? expressionBody;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public SyntaxToken DelegateKeyword => delegateKeyword;

	public ParameterListSyntax? ParameterList => parameterList;

	public override BlockSyntax Block => block;

	public override ExpressionSyntax? ExpressionBody => expressionBody;

	internal AnonymousMethodExpressionSyntax(SyntaxKind kind, GreenNode? modifiers, SyntaxToken delegateKeyword, ParameterListSyntax? parameterList, BlockSyntax block, ExpressionSyntax? expressionBody, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(delegateKeyword);
		this.delegateKeyword = delegateKeyword;
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
	}

	internal AnonymousMethodExpressionSyntax(SyntaxKind kind, GreenNode? modifiers, SyntaxToken delegateKeyword, ParameterListSyntax? parameterList, BlockSyntax block, ExpressionSyntax? expressionBody, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(delegateKeyword);
		this.delegateKeyword = delegateKeyword;
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
	}

	internal AnonymousMethodExpressionSyntax(SyntaxKind kind, GreenNode? modifiers, SyntaxToken delegateKeyword, ParameterListSyntax? parameterList, BlockSyntax block, ExpressionSyntax? expressionBody)
		: base(kind)
	{
		base.SlotCount = 5;
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(delegateKeyword);
		this.delegateKeyword = delegateKeyword;
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
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
			0 => modifiers, 
			1 => delegateKeyword, 
			2 => parameterList, 
			3 => block, 
			4 => expressionBody, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAnonymousMethodExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAnonymousMethodExpression(this);
	}

	public AnonymousMethodExpressionSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, BlockSyntax block, ExpressionSyntax expressionBody)
	{
		if (modifiers != Modifiers || delegateKeyword != DelegateKeyword || parameterList != ParameterList || block != Block || expressionBody != ExpressionBody)
		{
			AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax = SyntaxFactory.AnonymousMethodExpression(modifiers, delegateKeyword, parameterList, block, expressionBody);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				anonymousMethodExpressionSyntax = anonymousMethodExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				anonymousMethodExpressionSyntax = anonymousMethodExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return anonymousMethodExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new AnonymousMethodExpressionSyntax(base.Kind, modifiers, delegateKeyword, parameterList, block, expressionBody, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new AnonymousMethodExpressionSyntax(base.Kind, modifiers, delegateKeyword, parameterList, block, expressionBody, GetDiagnostics(), annotations);
	}
}
