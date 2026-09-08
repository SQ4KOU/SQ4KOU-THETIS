namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ImplicitStackAllocArrayCreationExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken stackAllocKeyword;

	internal readonly SyntaxToken openBracketToken;

	internal readonly SyntaxToken closeBracketToken;

	internal readonly InitializerExpressionSyntax initializer;

	public SyntaxToken StackAllocKeyword => stackAllocKeyword;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public SyntaxToken CloseBracketToken => closeBracketToken;

	public InitializerExpressionSyntax Initializer => initializer;

	internal ImplicitStackAllocArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken stackAllocKeyword, SyntaxToken openBracketToken, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(stackAllocKeyword);
		this.stackAllocKeyword = stackAllocKeyword;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal ImplicitStackAllocArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken stackAllocKeyword, SyntaxToken openBracketToken, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(stackAllocKeyword);
		this.stackAllocKeyword = stackAllocKeyword;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal ImplicitStackAllocArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken stackAllocKeyword, SyntaxToken openBracketToken, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(stackAllocKeyword);
		this.stackAllocKeyword = stackAllocKeyword;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => stackAllocKeyword, 
			1 => openBracketToken, 
			2 => closeBracketToken, 
			3 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitStackAllocArrayCreationExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitImplicitStackAllocArrayCreationExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitImplicitStackAllocArrayCreationExpression(this);
	}

	public ImplicitStackAllocArrayCreationExpressionSyntax Update(SyntaxToken stackAllocKeyword, SyntaxToken openBracketToken, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
	{
		if (stackAllocKeyword != StackAllocKeyword || openBracketToken != OpenBracketToken || closeBracketToken != CloseBracketToken || initializer != Initializer)
		{
			ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpressionSyntax = SyntaxFactory.ImplicitStackAllocArrayCreationExpression(stackAllocKeyword, openBracketToken, closeBracketToken, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				implicitStackAllocArrayCreationExpressionSyntax = implicitStackAllocArrayCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				implicitStackAllocArrayCreationExpressionSyntax = implicitStackAllocArrayCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return implicitStackAllocArrayCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ImplicitStackAllocArrayCreationExpressionSyntax(base.Kind, stackAllocKeyword, openBracketToken, closeBracketToken, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ImplicitStackAllocArrayCreationExpressionSyntax(base.Kind, stackAllocKeyword, openBracketToken, closeBracketToken, initializer, GetDiagnostics(), annotations);
	}
}
