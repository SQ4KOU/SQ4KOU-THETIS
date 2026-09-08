using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SingleTypeDeclaration : SingleNamespaceOrTypeDeclaration
{
	[Flags]
	internal enum TypeDeclarationFlags : ushort
	{
		None = 0,
		AnyMemberHasExtensionMethodSyntax = 2,
		HasAnyAttributes = 4,
		HasBaseDeclarations = 8,
		AnyMemberHasAttributes = 0x10,
		HasAnyNontypeMembers = 0x20,
		HasAwaitExpressions = 0x40,
		IsIterator = 0x80,
		HasReturnWithExpression = 0x100,
		IsSimpleProgram = 0x200,
		HasRequiredMembers = 0x400,
		HasPrimaryConstructor = 0x800,
		AnyExtensionDeclarationSyntax = 0x1000
	}

	internal readonly struct TypeDeclarationIdentity : IEquatable<TypeDeclarationIdentity>
	{
		private readonly SingleTypeDeclaration _decl;

		internal TypeDeclarationIdentity(SingleTypeDeclaration decl)
		{
			_decl = decl;
		}

		public override bool Equals(object obj)
		{
			if (obj is TypeDeclarationIdentity)
			{
				return Equals((TypeDeclarationIdentity)obj);
			}
			return false;
		}

		public bool Equals(TypeDeclarationIdentity other)
		{
			SingleTypeDeclaration decl = _decl;
			SingleTypeDeclaration decl2 = other._decl;
			if (decl == decl2)
			{
				return true;
			}
			if (decl._arity != decl2._arity || decl._kind != decl2._kind || decl.name != decl2.name)
			{
				return false;
			}
			if (decl.SyntaxReference.SyntaxTree != decl2.SyntaxReference.SyntaxTree && ((decl.Modifiers & DeclarationModifiers.File) != DeclarationModifiers.None || (decl2.Modifiers & DeclarationModifiers.File) != DeclarationModifiers.None))
			{
				return false;
			}
			DeclarationKind kind = decl._kind;
			if ((kind - 4 <= DeclarationKind.Class || kind == DeclarationKind.Extension) ? true : false)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			SingleTypeDeclaration decl = _decl;
			return Hash.Combine(decl.Name.GetHashCode(), Hash.Combine(decl.Arity.GetHashCode(), (int)decl.Kind));
		}
	}

	private readonly DeclarationKind _kind;

	private readonly TypeDeclarationFlags _flags;

	private readonly ushort _arity;

	private readonly DeclarationModifiers _modifiers;

	private readonly ImmutableArray<SingleTypeDeclaration> _children;

	private readonly StrongBox<ImmutableSegmentedHashSet<string>> _memberNames;

	public QuickAttributes QuickAttributes { get; }

	public override DeclarationKind Kind => _kind;

	public new ImmutableArray<SingleTypeDeclaration> Children => _children;

	public int Arity => _arity;

	public DeclarationModifiers Modifiers => _modifiers;

	public StrongBox<ImmutableSegmentedHashSet<string>> MemberNames => _memberNames;

	public bool AnyMemberHasExtensionMethodSyntax => (_flags & TypeDeclarationFlags.AnyMemberHasExtensionMethodSyntax) != 0;

	public bool AnyExtensionDeclarationSyntax => (_flags & TypeDeclarationFlags.AnyExtensionDeclarationSyntax) != 0;

	public bool HasAnyAttributes => (_flags & TypeDeclarationFlags.HasAnyAttributes) != 0;

	public bool HasBaseDeclarations => (_flags & TypeDeclarationFlags.HasBaseDeclarations) != 0;

	public bool AnyMemberHasAttributes => (_flags & TypeDeclarationFlags.AnyMemberHasAttributes) != 0;

	public bool HasAnyNontypeMembers => (_flags & TypeDeclarationFlags.HasAnyNontypeMembers) != 0;

	public bool HasAwaitExpressions => (_flags & TypeDeclarationFlags.HasAwaitExpressions) != 0;

	public bool HasReturnWithExpression => (_flags & TypeDeclarationFlags.HasReturnWithExpression) != 0;

	public bool IsIterator => (_flags & TypeDeclarationFlags.IsIterator) != 0;

	public bool IsSimpleProgram => (_flags & TypeDeclarationFlags.IsSimpleProgram) != 0;

	public bool HasRequiredMembers => (_flags & TypeDeclarationFlags.HasRequiredMembers) != 0;

	public bool HasPrimaryConstructor => (_flags & TypeDeclarationFlags.HasPrimaryConstructor) != 0;

	internal TypeDeclarationIdentity Identity => new TypeDeclarationIdentity(this);

	internal SingleTypeDeclaration(DeclarationKind kind, string name, int arity, DeclarationModifiers modifiers, TypeDeclarationFlags declFlags, SyntaxReference syntaxReference, SourceLocation nameLocation, StrongBox<ImmutableSegmentedHashSet<string>> memberNames, ImmutableArray<SingleTypeDeclaration> children, ImmutableArray<Diagnostic> diagnostics, QuickAttributes quickAttributes)
		: base(name, syntaxReference, nameLocation, diagnostics)
	{
		_kind = kind;
		_arity = (ushort)arity;
		_modifiers = modifiers;
		_memberNames = memberNames;
		_children = children;
		_flags = declFlags;
		QuickAttributes = quickAttributes;
	}

	protected override ImmutableArray<SingleNamespaceOrTypeDeclaration> GetNamespaceOrTypeDeclarationChildren()
	{
		return StaticCast<SingleNamespaceOrTypeDeclaration>.From(_children);
	}
}
