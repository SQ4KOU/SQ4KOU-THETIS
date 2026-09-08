using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CrefParameterSyntax : CSharpSyntaxNode
{
	private TypeSyntax? type;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public SyntaxToken RefOrOutKeyword => RefKindKeyword;

	public SyntaxToken RefKindKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken refKindKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterSyntax)base.Green).refKindKeyword;
			if (refKindKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, refKindKeyword, base.Position, 0);
		}
	}

	public SyntaxToken ReadOnlyKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken readOnlyKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterSyntax)base.Green).readOnlyKeyword;
			if (readOnlyKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, readOnlyKeyword, GetChildPosition(1), GetChildIndex(1));
		}
	}

	public TypeSyntax Type => GetRed(ref type, 2);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public CrefParameterSyntax WithRefOrOutKeyword(SyntaxToken refOrOutKeyword)
	{
		return Update(refOrOutKeyword, Type);
	}

	public CrefParameterSyntax Update(SyntaxToken refKindKeyword, TypeSyntax type)
	{
		return Update(refKindKeyword, ReadOnlyKeyword, type);
	}

	internal CrefParameterSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 2)
		{
			return null;
		}
		return GetRed(ref type, 2);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 2)
		{
			return null;
		}
		return type;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefParameter(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitCrefParameter(this);
	}

	public CrefParameterSyntax Update(SyntaxToken refKindKeyword, SyntaxToken readOnlyKeyword, TypeSyntax type)
	{
		if (refKindKeyword != RefKindKeyword || readOnlyKeyword != ReadOnlyKeyword || type != Type)
		{
			CrefParameterSyntax crefParameterSyntax = SyntaxFactory.CrefParameter(refKindKeyword, readOnlyKeyword, type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return crefParameterSyntax;
			}
			return crefParameterSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public CrefParameterSyntax WithRefKindKeyword(SyntaxToken refKindKeyword)
	{
		return Update(refKindKeyword, ReadOnlyKeyword, Type);
	}

	public CrefParameterSyntax WithReadOnlyKeyword(SyntaxToken readOnlyKeyword)
	{
		return Update(RefKindKeyword, readOnlyKeyword, Type);
	}

	public CrefParameterSyntax WithType(TypeSyntax type)
	{
		return Update(RefKindKeyword, ReadOnlyKeyword, type);
	}
}
