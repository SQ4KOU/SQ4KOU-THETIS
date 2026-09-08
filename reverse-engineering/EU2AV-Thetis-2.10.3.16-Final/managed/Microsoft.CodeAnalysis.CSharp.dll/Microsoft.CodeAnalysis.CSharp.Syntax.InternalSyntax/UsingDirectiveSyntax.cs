namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class UsingDirectiveSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken? globalKeyword;

	internal readonly SyntaxToken usingKeyword;

	internal readonly SyntaxToken? staticKeyword;

	internal readonly SyntaxToken? unsafeKeyword;

	internal readonly NameEqualsSyntax? alias;

	internal readonly TypeSyntax namespaceOrType;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken? GlobalKeyword => globalKeyword;

	public SyntaxToken UsingKeyword => usingKeyword;

	public SyntaxToken? StaticKeyword => staticKeyword;

	public SyntaxToken? UnsafeKeyword => unsafeKeyword;

	public NameEqualsSyntax? Alias => alias;

	public TypeSyntax NamespaceOrType => namespaceOrType;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal UsingDirectiveSyntax(SyntaxKind kind, SyntaxToken? globalKeyword, SyntaxToken usingKeyword, SyntaxToken? staticKeyword, SyntaxToken? unsafeKeyword, NameEqualsSyntax? alias, TypeSyntax namespaceOrType, SyntaxToken semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 7;
		if (globalKeyword != null)
		{
			AdjustFlagsAndWidth(globalKeyword);
			this.globalKeyword = globalKeyword;
		}
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		if (staticKeyword != null)
		{
			AdjustFlagsAndWidth(staticKeyword);
			this.staticKeyword = staticKeyword;
		}
		if (unsafeKeyword != null)
		{
			AdjustFlagsAndWidth(unsafeKeyword);
			this.unsafeKeyword = unsafeKeyword;
		}
		if (alias != null)
		{
			AdjustFlagsAndWidth(alias);
			this.alias = alias;
		}
		AdjustFlagsAndWidth(namespaceOrType);
		this.namespaceOrType = namespaceOrType;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal UsingDirectiveSyntax(SyntaxKind kind, SyntaxToken? globalKeyword, SyntaxToken usingKeyword, SyntaxToken? staticKeyword, SyntaxToken? unsafeKeyword, NameEqualsSyntax? alias, TypeSyntax namespaceOrType, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 7;
		if (globalKeyword != null)
		{
			AdjustFlagsAndWidth(globalKeyword);
			this.globalKeyword = globalKeyword;
		}
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		if (staticKeyword != null)
		{
			AdjustFlagsAndWidth(staticKeyword);
			this.staticKeyword = staticKeyword;
		}
		if (unsafeKeyword != null)
		{
			AdjustFlagsAndWidth(unsafeKeyword);
			this.unsafeKeyword = unsafeKeyword;
		}
		if (alias != null)
		{
			AdjustFlagsAndWidth(alias);
			this.alias = alias;
		}
		AdjustFlagsAndWidth(namespaceOrType);
		this.namespaceOrType = namespaceOrType;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal UsingDirectiveSyntax(SyntaxKind kind, SyntaxToken? globalKeyword, SyntaxToken usingKeyword, SyntaxToken? staticKeyword, SyntaxToken? unsafeKeyword, NameEqualsSyntax? alias, TypeSyntax namespaceOrType, SyntaxToken semicolonToken)
		: base(kind)
	{
		base.SlotCount = 7;
		if (globalKeyword != null)
		{
			AdjustFlagsAndWidth(globalKeyword);
			this.globalKeyword = globalKeyword;
		}
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		if (staticKeyword != null)
		{
			AdjustFlagsAndWidth(staticKeyword);
			this.staticKeyword = staticKeyword;
		}
		if (unsafeKeyword != null)
		{
			AdjustFlagsAndWidth(unsafeKeyword);
			this.unsafeKeyword = unsafeKeyword;
		}
		if (alias != null)
		{
			AdjustFlagsAndWidth(alias);
			this.alias = alias;
		}
		AdjustFlagsAndWidth(namespaceOrType);
		this.namespaceOrType = namespaceOrType;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => globalKeyword, 
			1 => usingKeyword, 
			2 => staticKeyword, 
			3 => unsafeKeyword, 
			4 => alias, 
			5 => namespaceOrType, 
			6 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUsingDirective(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUsingDirective(this);
	}

	public UsingDirectiveSyntax Update(SyntaxToken globalKeyword, SyntaxToken usingKeyword, SyntaxToken staticKeyword, SyntaxToken unsafeKeyword, NameEqualsSyntax alias, TypeSyntax namespaceOrType, SyntaxToken semicolonToken)
	{
		if (globalKeyword != GlobalKeyword || usingKeyword != UsingKeyword || staticKeyword != StaticKeyword || unsafeKeyword != UnsafeKeyword || alias != Alias || namespaceOrType != NamespaceOrType || semicolonToken != SemicolonToken)
		{
			UsingDirectiveSyntax usingDirectiveSyntax = SyntaxFactory.UsingDirective(globalKeyword, usingKeyword, staticKeyword, unsafeKeyword, alias, namespaceOrType, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				usingDirectiveSyntax = usingDirectiveSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				usingDirectiveSyntax = usingDirectiveSyntax.WithAnnotationsGreen(annotations);
			}
			return usingDirectiveSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new UsingDirectiveSyntax(base.Kind, globalKeyword, usingKeyword, staticKeyword, unsafeKeyword, alias, namespaceOrType, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new UsingDirectiveSyntax(base.Kind, globalKeyword, usingKeyword, staticKeyword, unsafeKeyword, alias, namespaceOrType, semicolonToken, GetDiagnostics(), annotations);
	}
}
