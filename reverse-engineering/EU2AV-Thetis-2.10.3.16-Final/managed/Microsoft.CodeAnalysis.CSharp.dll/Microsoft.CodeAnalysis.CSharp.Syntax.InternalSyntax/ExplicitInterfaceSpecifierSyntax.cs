namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExplicitInterfaceSpecifierSyntax : CSharpSyntaxNode
{
	internal readonly NameSyntax name;

	internal readonly SyntaxToken dotToken;

	public NameSyntax Name => name;

	public SyntaxToken DotToken => dotToken;

	internal ExplicitInterfaceSpecifierSyntax(SyntaxKind kind, NameSyntax name, SyntaxToken dotToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
	}

	internal ExplicitInterfaceSpecifierSyntax(SyntaxKind kind, NameSyntax name, SyntaxToken dotToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
	}

	internal ExplicitInterfaceSpecifierSyntax(SyntaxKind kind, NameSyntax name, SyntaxToken dotToken)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => name, 
			1 => dotToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExplicitInterfaceSpecifier(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExplicitInterfaceSpecifier(this);
	}

	public ExplicitInterfaceSpecifierSyntax Update(NameSyntax name, SyntaxToken dotToken)
	{
		if (name != Name || dotToken != DotToken)
		{
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = SyntaxFactory.ExplicitInterfaceSpecifier(name, dotToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				explicitInterfaceSpecifierSyntax = explicitInterfaceSpecifierSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				explicitInterfaceSpecifierSyntax = explicitInterfaceSpecifierSyntax.WithAnnotationsGreen(annotations);
			}
			return explicitInterfaceSpecifierSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ExplicitInterfaceSpecifierSyntax(base.Kind, name, dotToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ExplicitInterfaceSpecifierSyntax(base.Kind, name, dotToken, GetDiagnostics(), annotations);
	}
}
