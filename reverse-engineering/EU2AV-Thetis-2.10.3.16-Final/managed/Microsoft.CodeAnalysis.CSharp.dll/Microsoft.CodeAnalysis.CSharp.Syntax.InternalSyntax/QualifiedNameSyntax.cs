namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class QualifiedNameSyntax : NameSyntax
{
	internal readonly NameSyntax left;

	internal readonly SyntaxToken dotToken;

	internal readonly SimpleNameSyntax right;

	public NameSyntax Left => left;

	public SyntaxToken DotToken => dotToken;

	public SimpleNameSyntax Right => right;

	internal QualifiedNameSyntax(SyntaxKind kind, NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal QualifiedNameSyntax(SyntaxKind kind, NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal QualifiedNameSyntax(SyntaxKind kind, NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => left, 
			1 => dotToken, 
			2 => right, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitQualifiedName(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQualifiedName(this);
	}

	public QualifiedNameSyntax Update(NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right)
	{
		if (left != Left || dotToken != DotToken || right != Right)
		{
			QualifiedNameSyntax qualifiedNameSyntax = SyntaxFactory.QualifiedName(left, dotToken, right);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				qualifiedNameSyntax = qualifiedNameSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				qualifiedNameSyntax = qualifiedNameSyntax.WithAnnotationsGreen(annotations);
			}
			return qualifiedNameSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new QualifiedNameSyntax(base.Kind, left, dotToken, right, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new QualifiedNameSyntax(base.Kind, left, dotToken, right, GetDiagnostics(), annotations);
	}
}
