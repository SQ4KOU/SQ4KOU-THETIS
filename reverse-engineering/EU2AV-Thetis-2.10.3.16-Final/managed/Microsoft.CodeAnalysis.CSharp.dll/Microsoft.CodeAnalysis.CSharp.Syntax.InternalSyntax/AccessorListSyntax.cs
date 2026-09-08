using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AccessorListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode? accessors;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax> Accessors => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax>(accessors);

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal AccessorListSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? accessors, SyntaxToken closeBraceToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (accessors != null)
		{
			AdjustFlagsAndWidth(accessors);
			this.accessors = accessors;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal AccessorListSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? accessors, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (accessors != null)
		{
			AdjustFlagsAndWidth(accessors);
			this.accessors = accessors;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal AccessorListSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? accessors, SyntaxToken closeBraceToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (accessors != null)
		{
			AdjustFlagsAndWidth(accessors);
			this.accessors = accessors;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBraceToken, 
			1 => accessors, 
			2 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAccessorList(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAccessorList(this);
	}

	public AccessorListSyntax Update(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax> accessors, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || accessors != Accessors || closeBraceToken != CloseBraceToken)
		{
			AccessorListSyntax accessorListSyntax = SyntaxFactory.AccessorList(openBraceToken, accessors, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				accessorListSyntax = accessorListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				accessorListSyntax = accessorListSyntax.WithAnnotationsGreen(annotations);
			}
			return accessorListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new AccessorListSyntax(base.Kind, openBraceToken, accessors, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new AccessorListSyntax(base.Kind, openBraceToken, accessors, closeBraceToken, GetDiagnostics(), annotations);
	}
}
