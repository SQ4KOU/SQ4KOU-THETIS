using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class UsingDirectiveSyntax : CSharpSyntaxNode
{
	private NameEqualsSyntax? alias;

	private TypeSyntax? namespaceOrType;

	public NameSyntax? Name => NamespaceOrType as NameSyntax;

	public SyntaxToken GlobalKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken globalKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)base.Green).globalKeyword;
			if (globalKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, globalKeyword, base.Position, 0);
		}
	}

	public SyntaxToken UsingKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)base.Green).usingKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken StaticKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken staticKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)base.Green).staticKeyword;
			if (staticKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, staticKeyword, GetChildPosition(2), GetChildIndex(2));
		}
	}

	public SyntaxToken UnsafeKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken unsafeKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)base.Green).unsafeKeyword;
			if (unsafeKeyword == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, unsafeKeyword, GetChildPosition(3), GetChildIndex(3));
		}
	}

	public NameEqualsSyntax? Alias => GetRed(ref alias, 4);

	public TypeSyntax NamespaceOrType => GetRed(ref namespaceOrType, 5);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)base.Green).semicolonToken, GetChildPosition(6), GetChildIndex(6));

	public UsingDirectiveSyntax Update(SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax? alias, NameSyntax name, SyntaxToken semicolonToken)
	{
		return Update(GlobalKeyword, usingKeyword, staticKeyword, UnsafeKeyword, alias, name, semicolonToken);
	}

	public UsingDirectiveSyntax Update(SyntaxToken globalKeyword, SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax? alias, NameSyntax name, SyntaxToken semicolonToken)
	{
		return Update(globalKeyword, usingKeyword, staticKeyword, UnsafeKeyword, alias, name, semicolonToken);
	}

	public UsingDirectiveSyntax WithName(NameSyntax name)
	{
		return WithNamespaceOrType(name);
	}

	internal UsingDirectiveSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			4 => GetRed(ref alias, 4), 
			5 => GetRed(ref namespaceOrType, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			4 => alias, 
			5 => namespaceOrType, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUsingDirective(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitUsingDirective(this);
	}

	public UsingDirectiveSyntax Update(SyntaxToken globalKeyword, SyntaxToken usingKeyword, SyntaxToken staticKeyword, SyntaxToken unsafeKeyword, NameEqualsSyntax? alias, TypeSyntax namespaceOrType, SyntaxToken semicolonToken)
	{
		if (globalKeyword != GlobalKeyword || usingKeyword != UsingKeyword || staticKeyword != StaticKeyword || unsafeKeyword != UnsafeKeyword || alias != Alias || namespaceOrType != NamespaceOrType || semicolonToken != SemicolonToken)
		{
			UsingDirectiveSyntax usingDirectiveSyntax = SyntaxFactory.UsingDirective(globalKeyword, usingKeyword, staticKeyword, unsafeKeyword, alias, namespaceOrType, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return usingDirectiveSyntax;
			}
			return usingDirectiveSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public UsingDirectiveSyntax WithGlobalKeyword(SyntaxToken globalKeyword)
	{
		return Update(globalKeyword, UsingKeyword, StaticKeyword, UnsafeKeyword, Alias, NamespaceOrType, SemicolonToken);
	}

	public UsingDirectiveSyntax WithUsingKeyword(SyntaxToken usingKeyword)
	{
		return Update(GlobalKeyword, usingKeyword, StaticKeyword, UnsafeKeyword, Alias, NamespaceOrType, SemicolonToken);
	}

	public UsingDirectiveSyntax WithStaticKeyword(SyntaxToken staticKeyword)
	{
		return Update(GlobalKeyword, UsingKeyword, staticKeyword, UnsafeKeyword, Alias, NamespaceOrType, SemicolonToken);
	}

	public UsingDirectiveSyntax WithUnsafeKeyword(SyntaxToken unsafeKeyword)
	{
		return Update(GlobalKeyword, UsingKeyword, StaticKeyword, unsafeKeyword, Alias, NamespaceOrType, SemicolonToken);
	}

	public UsingDirectiveSyntax WithAlias(NameEqualsSyntax? alias)
	{
		return Update(GlobalKeyword, UsingKeyword, StaticKeyword, UnsafeKeyword, alias, NamespaceOrType, SemicolonToken);
	}

	public UsingDirectiveSyntax WithNamespaceOrType(TypeSyntax namespaceOrType)
	{
		return Update(GlobalKeyword, UsingKeyword, StaticKeyword, UnsafeKeyword, Alias, namespaceOrType, SemicolonToken);
	}

	public UsingDirectiveSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(GlobalKeyword, UsingKeyword, StaticKeyword, UnsafeKeyword, Alias, NamespaceOrType, semicolonToken);
	}
}
