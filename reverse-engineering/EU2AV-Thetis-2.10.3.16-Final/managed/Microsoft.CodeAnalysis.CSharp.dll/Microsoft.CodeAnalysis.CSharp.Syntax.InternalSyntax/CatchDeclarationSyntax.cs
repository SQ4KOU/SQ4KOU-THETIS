namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CatchDeclarationSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openParenToken;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken? identifier;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public TypeSyntax Type => type;

	public SyntaxToken? Identifier => identifier;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal CatchDeclarationSyntax(SyntaxKind kind, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken? identifier, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CatchDeclarationSyntax(SyntaxKind kind, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken? identifier, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CatchDeclarationSyntax(SyntaxKind kind, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken? identifier, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => type, 
			2 => identifier, 
			3 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCatchDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCatchDeclaration(this);
	}

	public CatchDeclarationSyntax Update(SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || type != Type || identifier != Identifier || closeParenToken != CloseParenToken)
		{
			CatchDeclarationSyntax catchDeclarationSyntax = SyntaxFactory.CatchDeclaration(openParenToken, type, identifier, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				catchDeclarationSyntax = catchDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				catchDeclarationSyntax = catchDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return catchDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new CatchDeclarationSyntax(base.Kind, openParenToken, type, identifier, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new CatchDeclarationSyntax(base.Kind, openParenToken, type, identifier, closeParenToken, GetDiagnostics(), annotations);
	}
}
