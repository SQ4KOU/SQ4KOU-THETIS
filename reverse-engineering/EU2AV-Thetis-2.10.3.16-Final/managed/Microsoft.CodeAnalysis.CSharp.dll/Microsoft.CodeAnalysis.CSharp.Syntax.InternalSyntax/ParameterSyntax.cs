using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ParameterSyntax : BaseParameterSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly TypeSyntax? type;

	internal readonly SyntaxToken? identifier;

	internal readonly EqualsValueClauseSyntax? @default;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override TypeSyntax? Type => type;

	public SyntaxToken? Identifier => identifier;

	public EqualsValueClauseSyntax? Default => @default;

	internal ParameterSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? type, SyntaxToken? identifier, EqualsValueClauseSyntax? @default, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
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
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
		if (@default != null)
		{
			AdjustFlagsAndWidth(@default);
			this.@default = @default;
		}
	}

	internal ParameterSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? type, SyntaxToken? identifier, EqualsValueClauseSyntax? @default, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
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
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
		if (@default != null)
		{
			AdjustFlagsAndWidth(@default);
			this.@default = @default;
		}
	}

	internal ParameterSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, TypeSyntax? type, SyntaxToken? identifier, EqualsValueClauseSyntax? @default)
		: base(kind)
	{
		base.SlotCount = 5;
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
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
		if (@default != null)
		{
			AdjustFlagsAndWidth(@default);
			this.@default = @default;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => type, 
			3 => identifier, 
			4 => @default, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParameter(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParameter(this);
	}

	public ParameterSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type || identifier != Identifier || @default != Default)
		{
			ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(attributeLists, modifiers, type, identifier, @default);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				parameterSyntax = parameterSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				parameterSyntax = parameterSyntax.WithAnnotationsGreen(annotations);
			}
			return parameterSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ParameterSyntax(base.Kind, attributeLists, modifiers, type, identifier, @default, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ParameterSyntax(base.Kind, attributeLists, modifiers, type, identifier, @default, GetDiagnostics(), annotations);
	}
}
