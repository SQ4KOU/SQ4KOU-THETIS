namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PrimaryConstructorBaseTypeSyntax : BaseTypeSyntax
{
	internal readonly TypeSyntax type;

	internal readonly ArgumentListSyntax argumentList;

	public override TypeSyntax Type => type;

	public ArgumentListSyntax ArgumentList => argumentList;

	internal PrimaryConstructorBaseTypeSyntax(SyntaxKind kind, TypeSyntax type, ArgumentListSyntax argumentList, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal PrimaryConstructorBaseTypeSyntax(SyntaxKind kind, TypeSyntax type, ArgumentListSyntax argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal PrimaryConstructorBaseTypeSyntax(SyntaxKind kind, TypeSyntax type, ArgumentListSyntax argumentList)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => type, 
			1 => argumentList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PrimaryConstructorBaseTypeSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPrimaryConstructorBaseType(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPrimaryConstructorBaseType(this);
	}

	public PrimaryConstructorBaseTypeSyntax Update(TypeSyntax type, ArgumentListSyntax argumentList)
	{
		if (type != Type || argumentList != ArgumentList)
		{
			PrimaryConstructorBaseTypeSyntax primaryConstructorBaseTypeSyntax = SyntaxFactory.PrimaryConstructorBaseType(type, argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				primaryConstructorBaseTypeSyntax = primaryConstructorBaseTypeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				primaryConstructorBaseTypeSyntax = primaryConstructorBaseTypeSyntax.WithAnnotationsGreen(annotations);
			}
			return primaryConstructorBaseTypeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new PrimaryConstructorBaseTypeSyntax(base.Kind, type, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new PrimaryConstructorBaseTypeSyntax(base.Kind, type, argumentList, GetDiagnostics(), annotations);
	}
}
