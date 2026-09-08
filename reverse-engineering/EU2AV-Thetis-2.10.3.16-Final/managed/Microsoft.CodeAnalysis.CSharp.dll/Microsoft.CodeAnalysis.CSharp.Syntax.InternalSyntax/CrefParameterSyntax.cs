namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CrefParameterSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken? refKindKeyword;

	internal readonly SyntaxToken? readOnlyKeyword;

	internal readonly TypeSyntax type;

	public SyntaxToken? RefKindKeyword => refKindKeyword;

	public SyntaxToken? ReadOnlyKeyword => readOnlyKeyword;

	public TypeSyntax Type => type;

	internal CrefParameterSyntax(SyntaxKind kind, SyntaxToken? refKindKeyword, SyntaxToken? readOnlyKeyword, TypeSyntax type, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		if (readOnlyKeyword != null)
		{
			AdjustFlagsAndWidth(readOnlyKeyword);
			this.readOnlyKeyword = readOnlyKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal CrefParameterSyntax(SyntaxKind kind, SyntaxToken? refKindKeyword, SyntaxToken? readOnlyKeyword, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		if (readOnlyKeyword != null)
		{
			AdjustFlagsAndWidth(readOnlyKeyword);
			this.readOnlyKeyword = readOnlyKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal CrefParameterSyntax(SyntaxKind kind, SyntaxToken? refKindKeyword, SyntaxToken? readOnlyKeyword, TypeSyntax type)
		: base(kind)
	{
		base.SlotCount = 3;
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		if (readOnlyKeyword != null)
		{
			AdjustFlagsAndWidth(readOnlyKeyword);
			this.readOnlyKeyword = readOnlyKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => refKindKeyword, 
			1 => readOnlyKeyword, 
			2 => type, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefParameter(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCrefParameter(this);
	}

	public CrefParameterSyntax Update(SyntaxToken refKindKeyword, SyntaxToken readOnlyKeyword, TypeSyntax type)
	{
		if (refKindKeyword != RefKindKeyword || readOnlyKeyword != ReadOnlyKeyword || type != Type)
		{
			CrefParameterSyntax crefParameterSyntax = SyntaxFactory.CrefParameter(refKindKeyword, readOnlyKeyword, type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				crefParameterSyntax = crefParameterSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				crefParameterSyntax = crefParameterSyntax.WithAnnotationsGreen(annotations);
			}
			return crefParameterSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new CrefParameterSyntax(base.Kind, refKindKeyword, readOnlyKeyword, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new CrefParameterSyntax(base.Kind, refKindKeyword, readOnlyKeyword, type, GetDiagnostics(), annotations);
	}
}
