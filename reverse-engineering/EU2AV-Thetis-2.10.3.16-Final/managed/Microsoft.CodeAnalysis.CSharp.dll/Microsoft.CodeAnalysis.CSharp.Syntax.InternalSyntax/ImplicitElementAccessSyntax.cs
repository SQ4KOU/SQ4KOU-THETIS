namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ImplicitElementAccessSyntax : ExpressionSyntax
{
	internal readonly BracketedArgumentListSyntax argumentList;

	public BracketedArgumentListSyntax ArgumentList => argumentList;

	internal ImplicitElementAccessSyntax(SyntaxKind kind, BracketedArgumentListSyntax argumentList, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ImplicitElementAccessSyntax(SyntaxKind kind, BracketedArgumentListSyntax argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ImplicitElementAccessSyntax(SyntaxKind kind, BracketedArgumentListSyntax argumentList)
		: base(kind)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal override GreenNode? GetSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return argumentList;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitElementAccessSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitImplicitElementAccess(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitImplicitElementAccess(this);
	}

	public ImplicitElementAccessSyntax Update(BracketedArgumentListSyntax argumentList)
	{
		if (argumentList != ArgumentList)
		{
			ImplicitElementAccessSyntax implicitElementAccessSyntax = SyntaxFactory.ImplicitElementAccess(argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				implicitElementAccessSyntax = implicitElementAccessSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				implicitElementAccessSyntax = implicitElementAccessSyntax.WithAnnotationsGreen(annotations);
			}
			return implicitElementAccessSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ImplicitElementAccessSyntax(base.Kind, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ImplicitElementAccessSyntax(base.Kind, argumentList, GetDiagnostics(), annotations);
	}
}
