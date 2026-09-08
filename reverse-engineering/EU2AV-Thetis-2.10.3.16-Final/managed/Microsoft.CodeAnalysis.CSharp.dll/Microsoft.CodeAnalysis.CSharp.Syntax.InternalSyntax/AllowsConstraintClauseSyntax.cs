using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AllowsConstraintClauseSyntax : TypeParameterConstraintSyntax
{
	internal readonly SyntaxToken allowsKeyword;

	internal readonly GreenNode? constraints;

	public SyntaxToken AllowsKeyword => allowsKeyword;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AllowsConstraintSyntax> Constraints => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AllowsConstraintSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(constraints));

	internal AllowsConstraintClauseSyntax(SyntaxKind kind, SyntaxToken allowsKeyword, GreenNode? constraints, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(allowsKeyword);
		this.allowsKeyword = allowsKeyword;
		if (constraints != null)
		{
			AdjustFlagsAndWidth(constraints);
			this.constraints = constraints;
		}
	}

	internal AllowsConstraintClauseSyntax(SyntaxKind kind, SyntaxToken allowsKeyword, GreenNode? constraints, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(allowsKeyword);
		this.allowsKeyword = allowsKeyword;
		if (constraints != null)
		{
			AdjustFlagsAndWidth(constraints);
			this.constraints = constraints;
		}
	}

	internal AllowsConstraintClauseSyntax(SyntaxKind kind, SyntaxToken allowsKeyword, GreenNode? constraints)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(allowsKeyword);
		this.allowsKeyword = allowsKeyword;
		if (constraints != null)
		{
			AdjustFlagsAndWidth(constraints);
			this.constraints = constraints;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => allowsKeyword, 
			1 => constraints, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AllowsConstraintClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAllowsConstraintClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAllowsConstraintClause(this);
	}

	public AllowsConstraintClauseSyntax Update(SyntaxToken allowsKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AllowsConstraintSyntax> constraints)
	{
		if (allowsKeyword != AllowsKeyword || constraints != Constraints)
		{
			AllowsConstraintClauseSyntax allowsConstraintClauseSyntax = SyntaxFactory.AllowsConstraintClause(allowsKeyword, constraints);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				allowsConstraintClauseSyntax = allowsConstraintClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				allowsConstraintClauseSyntax = allowsConstraintClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return allowsConstraintClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new AllowsConstraintClauseSyntax(base.Kind, allowsKeyword, constraints, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new AllowsConstraintClauseSyntax(base.Kind, allowsKeyword, constraints, GetDiagnostics(), annotations);
	}
}
