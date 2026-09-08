namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CatchClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken catchKeyword;

	internal readonly CatchDeclarationSyntax? declaration;

	internal readonly CatchFilterClauseSyntax? filter;

	internal readonly BlockSyntax block;

	public SyntaxToken CatchKeyword => catchKeyword;

	public CatchDeclarationSyntax? Declaration => declaration;

	public CatchFilterClauseSyntax? Filter => filter;

	public BlockSyntax Block => block;

	internal CatchClauseSyntax(SyntaxKind kind, SyntaxToken catchKeyword, CatchDeclarationSyntax? declaration, CatchFilterClauseSyntax? filter, BlockSyntax block, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(catchKeyword);
		this.catchKeyword = catchKeyword;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (filter != null)
		{
			AdjustFlagsAndWidth(filter);
			this.filter = filter;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal CatchClauseSyntax(SyntaxKind kind, SyntaxToken catchKeyword, CatchDeclarationSyntax? declaration, CatchFilterClauseSyntax? filter, BlockSyntax block, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(catchKeyword);
		this.catchKeyword = catchKeyword;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (filter != null)
		{
			AdjustFlagsAndWidth(filter);
			this.filter = filter;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal CatchClauseSyntax(SyntaxKind kind, SyntaxToken catchKeyword, CatchDeclarationSyntax? declaration, CatchFilterClauseSyntax? filter, BlockSyntax block)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(catchKeyword);
		this.catchKeyword = catchKeyword;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (filter != null)
		{
			AdjustFlagsAndWidth(filter);
			this.filter = filter;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => catchKeyword, 
			1 => declaration, 
			2 => filter, 
			3 => block, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCatchClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCatchClause(this);
	}

	public CatchClauseSyntax Update(SyntaxToken catchKeyword, CatchDeclarationSyntax declaration, CatchFilterClauseSyntax filter, BlockSyntax block)
	{
		if (catchKeyword != CatchKeyword || declaration != Declaration || filter != Filter || block != Block)
		{
			CatchClauseSyntax catchClauseSyntax = SyntaxFactory.CatchClause(catchKeyword, declaration, filter, block);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				catchClauseSyntax = catchClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				catchClauseSyntax = catchClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return catchClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new CatchClauseSyntax(base.Kind, catchKeyword, declaration, filter, block, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new CatchClauseSyntax(base.Kind, catchKeyword, declaration, filter, block, GetDiagnostics(), annotations);
	}
}
