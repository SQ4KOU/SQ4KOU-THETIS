using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TryStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken tryKeyword;

	internal readonly BlockSyntax block;

	internal readonly GreenNode? catches;

	internal readonly FinallyClauseSyntax? @finally;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken TryKeyword => tryKeyword;

	public BlockSyntax Block => block;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax> Catches => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax>(catches);

	public FinallyClauseSyntax? Finally => @finally;

	internal TryStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken tryKeyword, BlockSyntax block, GreenNode? catches, FinallyClauseSyntax? @finally, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(tryKeyword);
		this.tryKeyword = tryKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (catches != null)
		{
			AdjustFlagsAndWidth(catches);
			this.catches = catches;
		}
		if (@finally != null)
		{
			AdjustFlagsAndWidth(@finally);
			this.@finally = @finally;
		}
	}

	internal TryStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken tryKeyword, BlockSyntax block, GreenNode? catches, FinallyClauseSyntax? @finally, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(tryKeyword);
		this.tryKeyword = tryKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (catches != null)
		{
			AdjustFlagsAndWidth(catches);
			this.catches = catches;
		}
		if (@finally != null)
		{
			AdjustFlagsAndWidth(@finally);
			this.@finally = @finally;
		}
	}

	internal TryStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken tryKeyword, BlockSyntax block, GreenNode? catches, FinallyClauseSyntax? @finally)
		: base(kind)
	{
		base.SlotCount = 5;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(tryKeyword);
		this.tryKeyword = tryKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (catches != null)
		{
			AdjustFlagsAndWidth(catches);
			this.catches = catches;
		}
		if (@finally != null)
		{
			AdjustFlagsAndWidth(@finally);
			this.@finally = @finally;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => tryKeyword, 
			2 => block, 
			3 => catches, 
			4 => @finally, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTryStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTryStatement(this);
	}

	public TryStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken tryKeyword, BlockSyntax block, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax> catches, FinallyClauseSyntax @finally)
	{
		if (attributeLists != AttributeLists || tryKeyword != TryKeyword || block != Block || catches != Catches || @finally != Finally)
		{
			TryStatementSyntax tryStatementSyntax = SyntaxFactory.TryStatement(attributeLists, tryKeyword, block, catches, @finally);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				tryStatementSyntax = tryStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				tryStatementSyntax = tryStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return tryStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new TryStatementSyntax(base.Kind, attributeLists, tryKeyword, block, catches, @finally, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new TryStatementSyntax(base.Kind, attributeLists, tryKeyword, block, catches, @finally, GetDiagnostics(), annotations);
	}
}
