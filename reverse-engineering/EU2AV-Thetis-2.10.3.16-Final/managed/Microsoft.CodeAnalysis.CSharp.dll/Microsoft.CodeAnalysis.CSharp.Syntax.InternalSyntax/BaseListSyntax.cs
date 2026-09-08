using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BaseListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken colonToken;

	internal readonly GreenNode? types;

	public SyntaxToken ColonToken => colonToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax> Types => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(types));

	internal BaseListSyntax(SyntaxKind kind, SyntaxToken colonToken, GreenNode? types, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (types != null)
		{
			AdjustFlagsAndWidth(types);
			this.types = types;
		}
	}

	internal BaseListSyntax(SyntaxKind kind, SyntaxToken colonToken, GreenNode? types, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (types != null)
		{
			AdjustFlagsAndWidth(types);
			this.types = types;
		}
	}

	internal BaseListSyntax(SyntaxKind kind, SyntaxToken colonToken, GreenNode? types)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (types != null)
		{
			AdjustFlagsAndWidth(types);
			this.types = types;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => colonToken, 
			1 => types, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBaseList(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBaseList(this);
	}

	public BaseListSyntax Update(SyntaxToken colonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax> types)
	{
		if (colonToken != ColonToken || types != Types)
		{
			BaseListSyntax baseListSyntax = SyntaxFactory.BaseList(colonToken, types);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				baseListSyntax = baseListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				baseListSyntax = baseListSyntax.WithAnnotationsGreen(annotations);
			}
			return baseListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new BaseListSyntax(base.Kind, colonToken, types, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new BaseListSyntax(base.Kind, colonToken, types, GetDiagnostics(), annotations);
	}
}
