using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeParameterSyntax : CSharpSyntaxNode
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken? varianceKeyword;

	internal readonly SyntaxToken identifier;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken? VarianceKeyword => varianceKeyword;

	public SyntaxToken Identifier => identifier;

	internal TypeParameterSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? varianceKeyword, SyntaxToken identifier, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (varianceKeyword != null)
		{
			AdjustFlagsAndWidth(varianceKeyword);
			this.varianceKeyword = varianceKeyword;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal TypeParameterSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? varianceKeyword, SyntaxToken identifier, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (varianceKeyword != null)
		{
			AdjustFlagsAndWidth(varianceKeyword);
			this.varianceKeyword = varianceKeyword;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal TypeParameterSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? varianceKeyword, SyntaxToken identifier)
		: base(kind)
	{
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (varianceKeyword != null)
		{
			AdjustFlagsAndWidth(varianceKeyword);
			this.varianceKeyword = varianceKeyword;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => varianceKeyword, 
			2 => identifier, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeParameter(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeParameter(this);
	}

	public TypeParameterSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken varianceKeyword, SyntaxToken identifier)
	{
		if (attributeLists != AttributeLists || varianceKeyword != VarianceKeyword || identifier != Identifier)
		{
			TypeParameterSyntax typeParameterSyntax = SyntaxFactory.TypeParameter(attributeLists, varianceKeyword, identifier);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeParameterSyntax = typeParameterSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeParameterSyntax = typeParameterSyntax.WithAnnotationsGreen(annotations);
			}
			return typeParameterSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new TypeParameterSyntax(base.Kind, attributeLists, varianceKeyword, identifier, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new TypeParameterSyntax(base.Kind, attributeLists, varianceKeyword, identifier, GetDiagnostics(), annotations);
	}
}
