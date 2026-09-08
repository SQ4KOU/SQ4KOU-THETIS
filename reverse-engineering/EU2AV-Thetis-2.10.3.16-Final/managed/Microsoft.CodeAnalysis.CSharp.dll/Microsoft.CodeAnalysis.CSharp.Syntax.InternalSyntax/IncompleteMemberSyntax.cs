using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IncompleteMemberSyntax : MemberDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly TypeSyntax? type;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public TypeSyntax? Type => type;

	internal IncompleteMemberSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? type, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
	}

	internal IncompleteMemberSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
	}

	internal IncompleteMemberSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? type)
		: base(kind)
	{
		base.SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => type, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IncompleteMemberSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIncompleteMember(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIncompleteMember(this);
	}

	public IncompleteMemberSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type)
		{
			IncompleteMemberSyntax incompleteMemberSyntax = SyntaxFactory.IncompleteMember(attributeLists, modifiers, type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				incompleteMemberSyntax = incompleteMemberSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				incompleteMemberSyntax = incompleteMemberSyntax.WithAnnotationsGreen(annotations);
			}
			return incompleteMemberSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new IncompleteMemberSyntax(base.Kind, attributeLists, modifiers, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new IncompleteMemberSyntax(base.Kind, attributeLists, modifiers, type, GetDiagnostics(), annotations);
	}
}
