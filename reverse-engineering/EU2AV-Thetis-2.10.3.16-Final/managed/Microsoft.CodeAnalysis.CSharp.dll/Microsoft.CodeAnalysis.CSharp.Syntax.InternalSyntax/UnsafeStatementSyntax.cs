using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class UnsafeStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken unsafeKeyword;

	internal readonly BlockSyntax block;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken UnsafeKeyword => unsafeKeyword;

	public BlockSyntax Block => block;

	internal UnsafeStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken unsafeKeyword, BlockSyntax block, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(unsafeKeyword);
		this.unsafeKeyword = unsafeKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal UnsafeStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken unsafeKeyword, BlockSyntax block, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(unsafeKeyword);
		this.unsafeKeyword = unsafeKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal UnsafeStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken unsafeKeyword, BlockSyntax block)
		: base(kind)
	{
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(unsafeKeyword);
		this.unsafeKeyword = unsafeKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => unsafeKeyword, 
			2 => block, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUnsafeStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUnsafeStatement(this);
	}

	public UnsafeStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken unsafeKeyword, BlockSyntax block)
	{
		if (attributeLists != AttributeLists || unsafeKeyword != UnsafeKeyword || block != Block)
		{
			UnsafeStatementSyntax unsafeStatementSyntax = SyntaxFactory.UnsafeStatement(attributeLists, unsafeKeyword, block);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				unsafeStatementSyntax = unsafeStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				unsafeStatementSyntax = unsafeStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return unsafeStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new UnsafeStatementSyntax(base.Kind, attributeLists, unsafeKeyword, block, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new UnsafeStatementSyntax(base.Kind, attributeLists, unsafeKeyword, block, GetDiagnostics(), annotations);
	}
}
