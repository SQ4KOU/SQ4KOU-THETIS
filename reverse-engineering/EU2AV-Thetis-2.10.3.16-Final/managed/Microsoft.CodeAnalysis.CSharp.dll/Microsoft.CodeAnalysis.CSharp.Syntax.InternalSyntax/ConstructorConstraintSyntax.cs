namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConstructorConstraintSyntax : TypeParameterConstraintSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken NewKeyword => newKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal ConstructorConstraintSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ConstructorConstraintSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ConstructorConstraintSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => newKeyword, 
			1 => openParenToken, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorConstraintSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConstructorConstraint(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstructorConstraint(this);
	}

	public ConstructorConstraintSyntax Update(SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
	{
		if (newKeyword != NewKeyword || openParenToken != OpenParenToken || closeParenToken != CloseParenToken)
		{
			ConstructorConstraintSyntax constructorConstraintSyntax = SyntaxFactory.ConstructorConstraint(newKeyword, openParenToken, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				constructorConstraintSyntax = constructorConstraintSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				constructorConstraintSyntax = constructorConstraintSyntax.WithAnnotationsGreen(annotations);
			}
			return constructorConstraintSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ConstructorConstraintSyntax(base.Kind, newKeyword, openParenToken, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ConstructorConstraintSyntax(base.Kind, newKeyword, openParenToken, closeParenToken, GetDiagnostics(), annotations);
	}
}
