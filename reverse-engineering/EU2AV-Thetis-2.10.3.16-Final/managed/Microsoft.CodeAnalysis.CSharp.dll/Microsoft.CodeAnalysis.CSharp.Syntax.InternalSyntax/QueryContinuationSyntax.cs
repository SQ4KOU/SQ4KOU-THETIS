namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class QueryContinuationSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken intoKeyword;

	internal readonly SyntaxToken identifier;

	internal readonly QueryBodySyntax body;

	public SyntaxToken IntoKeyword => intoKeyword;

	public SyntaxToken Identifier => identifier;

	public QueryBodySyntax Body => body;

	internal QueryContinuationSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal QueryContinuationSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal QueryContinuationSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => intoKeyword, 
			1 => identifier, 
			2 => body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitQueryContinuation(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryContinuation(this);
	}

	public QueryContinuationSyntax Update(SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body)
	{
		if (intoKeyword != IntoKeyword || identifier != Identifier || body != Body)
		{
			QueryContinuationSyntax queryContinuationSyntax = SyntaxFactory.QueryContinuation(intoKeyword, identifier, body);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				queryContinuationSyntax = queryContinuationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				queryContinuationSyntax = queryContinuationSyntax.WithAnnotationsGreen(annotations);
			}
			return queryContinuationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new QueryContinuationSyntax(base.Kind, intoKeyword, identifier, body, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new QueryContinuationSyntax(base.Kind, intoKeyword, identifier, body, GetDiagnostics(), annotations);
	}
}
