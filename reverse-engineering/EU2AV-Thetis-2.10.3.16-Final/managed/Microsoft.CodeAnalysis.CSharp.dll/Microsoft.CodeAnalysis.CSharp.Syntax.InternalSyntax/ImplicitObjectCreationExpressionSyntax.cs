namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ImplicitObjectCreationExpressionSyntax : BaseObjectCreationExpressionSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly ArgumentListSyntax argumentList;

	internal readonly InitializerExpressionSyntax? initializer;

	public override SyntaxToken NewKeyword => newKeyword;

	public override ArgumentListSyntax ArgumentList => argumentList;

	public override InitializerExpressionSyntax? Initializer => initializer;

	internal ImplicitObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, ArgumentListSyntax argumentList, InitializerExpressionSyntax? initializer, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ImplicitObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, ArgumentListSyntax argumentList, InitializerExpressionSyntax? initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ImplicitObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, ArgumentListSyntax argumentList, InitializerExpressionSyntax? initializer)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
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
			0 => newKeyword, 
			1 => argumentList, 
			2 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitObjectCreationExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitImplicitObjectCreationExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitImplicitObjectCreationExpression(this);
	}

	public ImplicitObjectCreationExpressionSyntax Update(SyntaxToken newKeyword, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer)
	{
		if (newKeyword != NewKeyword || argumentList != ArgumentList || initializer != Initializer)
		{
			ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpressionSyntax = SyntaxFactory.ImplicitObjectCreationExpression(newKeyword, argumentList, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				implicitObjectCreationExpressionSyntax = implicitObjectCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				implicitObjectCreationExpressionSyntax = implicitObjectCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return implicitObjectCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ImplicitObjectCreationExpressionSyntax(base.Kind, newKeyword, argumentList, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ImplicitObjectCreationExpressionSyntax(base.Kind, newKeyword, argumentList, initializer, GetDiagnostics(), annotations);
	}
}
