namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IndexerMemberCrefSyntax : MemberCrefSyntax
{
	internal readonly SyntaxToken thisKeyword;

	internal readonly CrefBracketedParameterListSyntax? parameters;

	public SyntaxToken ThisKeyword => thisKeyword;

	public CrefBracketedParameterListSyntax? Parameters => parameters;

	internal IndexerMemberCrefSyntax(SyntaxKind kind, SyntaxToken thisKeyword, CrefBracketedParameterListSyntax? parameters, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(thisKeyword);
		this.thisKeyword = thisKeyword;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal IndexerMemberCrefSyntax(SyntaxKind kind, SyntaxToken thisKeyword, CrefBracketedParameterListSyntax? parameters, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(thisKeyword);
		this.thisKeyword = thisKeyword;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal IndexerMemberCrefSyntax(SyntaxKind kind, SyntaxToken thisKeyword, CrefBracketedParameterListSyntax? parameters)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(thisKeyword);
		this.thisKeyword = thisKeyword;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => thisKeyword, 
			1 => parameters, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IndexerMemberCrefSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIndexerMemberCref(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIndexerMemberCref(this);
	}

	public IndexerMemberCrefSyntax Update(SyntaxToken thisKeyword, CrefBracketedParameterListSyntax parameters)
	{
		if (thisKeyword != ThisKeyword || parameters != Parameters)
		{
			IndexerMemberCrefSyntax indexerMemberCrefSyntax = SyntaxFactory.IndexerMemberCref(thisKeyword, parameters);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				indexerMemberCrefSyntax = indexerMemberCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				indexerMemberCrefSyntax = indexerMemberCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return indexerMemberCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new IndexerMemberCrefSyntax(base.Kind, thisKeyword, parameters, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new IndexerMemberCrefSyntax(base.Kind, thisKeyword, parameters, GetDiagnostics(), annotations);
	}
}
