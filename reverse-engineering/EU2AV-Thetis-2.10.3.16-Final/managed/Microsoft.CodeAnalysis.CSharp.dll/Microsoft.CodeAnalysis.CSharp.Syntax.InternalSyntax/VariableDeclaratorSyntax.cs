namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class VariableDeclaratorSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken identifier;

	internal readonly BracketedArgumentListSyntax? argumentList;

	internal readonly EqualsValueClauseSyntax? initializer;

	public SyntaxToken Identifier => identifier;

	public BracketedArgumentListSyntax? ArgumentList => argumentList;

	public EqualsValueClauseSyntax? Initializer => initializer;

	internal VariableDeclaratorSyntax(SyntaxKind kind, SyntaxToken identifier, BracketedArgumentListSyntax? argumentList, EqualsValueClauseSyntax? initializer, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal VariableDeclaratorSyntax(SyntaxKind kind, SyntaxToken identifier, BracketedArgumentListSyntax? argumentList, EqualsValueClauseSyntax? initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal VariableDeclaratorSyntax(SyntaxKind kind, SyntaxToken identifier, BracketedArgumentListSyntax? argumentList, EqualsValueClauseSyntax? initializer)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
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
			0 => identifier, 
			1 => argumentList, 
			2 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitVariableDeclarator(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVariableDeclarator(this);
	}

	public VariableDeclaratorSyntax Update(SyntaxToken identifier, BracketedArgumentListSyntax argumentList, EqualsValueClauseSyntax initializer)
	{
		if (identifier != Identifier || argumentList != ArgumentList || initializer != Initializer)
		{
			VariableDeclaratorSyntax variableDeclaratorSyntax = SyntaxFactory.VariableDeclarator(identifier, argumentList, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				variableDeclaratorSyntax = variableDeclaratorSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				variableDeclaratorSyntax = variableDeclaratorSyntax.WithAnnotationsGreen(annotations);
			}
			return variableDeclaratorSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new VariableDeclaratorSyntax(base.Kind, identifier, argumentList, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new VariableDeclaratorSyntax(base.Kind, identifier, argumentList, initializer, GetDiagnostics(), annotations);
	}
}
