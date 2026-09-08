namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DeclarationExpressionSyntax : ExpressionSyntax
{
	internal readonly TypeSyntax type;

	internal readonly VariableDesignationSyntax designation;

	public TypeSyntax Type => type;

	public VariableDesignationSyntax Designation => designation;

	internal DeclarationExpressionSyntax(SyntaxKind kind, TypeSyntax type, VariableDesignationSyntax designation, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal DeclarationExpressionSyntax(SyntaxKind kind, TypeSyntax type, VariableDesignationSyntax designation, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal DeclarationExpressionSyntax(SyntaxKind kind, TypeSyntax type, VariableDesignationSyntax designation)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => type, 
			1 => designation, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDeclarationExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDeclarationExpression(this);
	}

	public DeclarationExpressionSyntax Update(TypeSyntax type, VariableDesignationSyntax designation)
	{
		if (type != Type || designation != Designation)
		{
			DeclarationExpressionSyntax declarationExpressionSyntax = SyntaxFactory.DeclarationExpression(type, designation);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				declarationExpressionSyntax = declarationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				declarationExpressionSyntax = declarationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return declarationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new DeclarationExpressionSyntax(base.Kind, type, designation, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new DeclarationExpressionSyntax(base.Kind, type, designation, GetDiagnostics(), annotations);
	}
}
