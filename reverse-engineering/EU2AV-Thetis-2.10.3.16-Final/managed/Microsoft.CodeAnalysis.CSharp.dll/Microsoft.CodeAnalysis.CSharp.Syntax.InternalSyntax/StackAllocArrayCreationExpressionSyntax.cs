namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class StackAllocArrayCreationExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken stackAllocKeyword;

	internal readonly TypeSyntax type;

	internal readonly InitializerExpressionSyntax? initializer;

	public SyntaxToken StackAllocKeyword => stackAllocKeyword;

	public TypeSyntax Type => type;

	public InitializerExpressionSyntax? Initializer => initializer;

	internal StackAllocArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken stackAllocKeyword, TypeSyntax type, InitializerExpressionSyntax? initializer, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(stackAllocKeyword);
		this.stackAllocKeyword = stackAllocKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal StackAllocArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken stackAllocKeyword, TypeSyntax type, InitializerExpressionSyntax? initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(stackAllocKeyword);
		this.stackAllocKeyword = stackAllocKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal StackAllocArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken stackAllocKeyword, TypeSyntax type, InitializerExpressionSyntax? initializer)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(stackAllocKeyword);
		this.stackAllocKeyword = stackAllocKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => stackAllocKeyword, 
			1 => type, 
			2 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.StackAllocArrayCreationExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitStackAllocArrayCreationExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitStackAllocArrayCreationExpression(this);
	}

	public StackAllocArrayCreationExpressionSyntax Update(SyntaxToken stackAllocKeyword, TypeSyntax type, InitializerExpressionSyntax initializer)
	{
		if (stackAllocKeyword != StackAllocKeyword || type != Type || initializer != Initializer)
		{
			StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax = SyntaxFactory.StackAllocArrayCreationExpression(stackAllocKeyword, type, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				stackAllocArrayCreationExpressionSyntax = stackAllocArrayCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				stackAllocArrayCreationExpressionSyntax = stackAllocArrayCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return stackAllocArrayCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new StackAllocArrayCreationExpressionSyntax(base.Kind, stackAllocKeyword, type, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new StackAllocArrayCreationExpressionSyntax(base.Kind, stackAllocKeyword, type, initializer, GetDiagnostics(), annotations);
	}
}
