using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class NamespaceOrTypeSymbol : Symbol, INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	protected static readonly ObjectPool<PooledDictionary<ReadOnlyMemory<char>, object>> s_nameToObjectPool = PooledDictionary<ReadOnlyMemory<char>, object>.CreatePool(ReadOnlyMemoryOfCharComparer.Instance);

	public bool IsNamespace => Kind == SymbolKind.Namespace;

	public bool IsType => !IsNamespace;

	public sealed override bool IsVirtual => false;

	public sealed override bool IsOverride => false;

	public sealed override bool IsExtern => false;

	internal NamespaceOrTypeSymbol()
	{
	}

	public abstract ImmutableArray<Symbol> GetMembers();

	internal virtual ImmutableArray<Symbol> GetMembersUnordered()
	{
		return GetMembers().ConditionallyDeOrder();
	}

	public abstract ImmutableArray<Symbol> GetMembers(string name);

	internal virtual ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return GetTypeMembers().ConditionallyDeOrder();
	}

	public abstract ImmutableArray<NamedTypeSymbol> GetTypeMembers();

	public ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return GetTypeMembers(System.MemoryExtensions.AsMemory(name));
	}

	public ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return GetTypeMembers(System.MemoryExtensions.AsMemory(name), arity);
	}

	public abstract ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name);

	public virtual ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol t, int num) => t.Arity == num, arity);
	}

	internal SourceNamedTypeSymbol? GetSourceTypeMember(TypeDeclarationSyntax syntax)
	{
		return GetSourceTypeMember(syntax.Identifier.ValueText, syntax.Arity, syntax.Kind(), syntax);
	}

	internal SourceNamedTypeSymbol? GetSourceTypeMember(DelegateDeclarationSyntax syntax)
	{
		return GetSourceTypeMember(syntax.Identifier.ValueText, syntax.Arity, syntax.Kind(), syntax);
	}

	internal SourceNamedTypeSymbol? GetSourceTypeMember(string name, int arity, SyntaxKind kind, CSharpSyntaxNode syntax)
	{
		TypeKind typeKind = kind.ToDeclarationKind().ToTypeKind();
		foreach (NamedTypeSymbol typeMember in GetTypeMembers(name, arity))
		{
			if (!(typeMember is SourceNamedTypeSymbol sourceNamedTypeSymbol) || sourceNamedTypeSymbol.TypeKind != typeKind)
			{
				continue;
			}
			if (syntax != null)
			{
				foreach (SingleTypeDeclaration declaration in sourceNamedTypeSymbol.MergedDeclaration.Declarations)
				{
					SourceLocation nameLocation = declaration.NameLocation;
					if (nameLocation.IsInSource && nameLocation.SourceTree == syntax.SyntaxTree && syntax.Span.Contains(nameLocation.SourceSpan))
					{
						return sourceNamedTypeSymbol;
					}
				}
				continue;
			}
			return sourceNamedTypeSymbol;
		}
		return null;
	}

	internal virtual NamedTypeSymbol? LookupMetadataType(ref MetadataTypeName emittedTypeName)
	{
		if (Kind == SymbolKind.ErrorType)
		{
			return null;
		}
		NamedTypeSymbol namedTypeSymbol = null;
		bool isNamespace = IsNamespace;
		if (emittedTypeName.IsMangled && (emittedTypeName.ForcedArity == -1 || emittedTypeName.ForcedArity == emittedTypeName.InferredArity))
		{
			foreach (NamedTypeSymbol typeMember in GetTypeMembers(emittedTypeName.UnmangledTypeNameMemory))
			{
				if (emittedTypeName.InferredArity == typeMember.Arity && typeMember.MangleName && ReadOnlyMemoryOfCharComparer.Equals(System.MemoryExtensions.AsSpan(typeMember.MetadataName), emittedTypeName.TypeNameMemory))
				{
					if ((object)namedTypeSymbol != null)
					{
						namedTypeSymbol = null;
						break;
					}
					namedTypeSymbol = typeMember;
				}
			}
		}
		int num = emittedTypeName.ForcedArity;
		if (emittedTypeName.UseCLSCompliantNameArityEncoding)
		{
			if (emittedTypeName.InferredArity > 0)
			{
				goto IL_0127;
			}
			if (num == -1)
			{
				num = 0;
			}
			else if (num != 0)
			{
				goto IL_0127;
			}
		}
		foreach (NamedTypeSymbol typeMember2 in GetTypeMembers(emittedTypeName.TypeNameMemory))
		{
			if (!typeMember2.MangleName && (num == -1 || num == typeMember2.Arity) && ReadOnlyMemoryOfCharComparer.Equals(System.MemoryExtensions.AsSpan(typeMember2.MetadataName), emittedTypeName.TypeNameMemory))
			{
				if ((object)namedTypeSymbol != null)
				{
					namedTypeSymbol = null;
					break;
				}
				namedTypeSymbol = typeMember2;
			}
		}
		goto IL_0127;
		IL_0127:
		if (isNamespace && (emittedTypeName.ForcedArity == -1 || emittedTypeName.ForcedArity == emittedTypeName.InferredArity))
		{
			ReadOnlySpan<char> span = emittedTypeName.TypeNameMemory.Span;
			if (span.Length >= 1 && span[0] == '<' && GeneratedNameParser.TryParseFileTypeName(emittedTypeName.UnmangledTypeName, out string displayFileName, out byte[] checksum, out string originalTypeName))
			{
				foreach (NamedTypeSymbol typeMember3 in GetTypeMembers(originalTypeName))
				{
					FileIdentifier associatedFileIdentifier = typeMember3.AssociatedFileIdentifier;
					if (associatedFileIdentifier != null && associatedFileIdentifier.DisplayFilePath == displayFileName && !associatedFileIdentifier.FilePathChecksumOpt.IsDefault && associatedFileIdentifier.FilePathChecksumOpt.SequenceEqual(checksum) && typeMember3.Arity == emittedTypeName.InferredArity)
					{
						if ((object)namedTypeSymbol != null)
						{
							namedTypeSymbol = null;
							break;
						}
						namedTypeSymbol = typeMember3;
					}
				}
			}
		}
		return namedTypeSymbol;
	}

	internal IEnumerable<NamespaceOrTypeSymbol>? GetNamespaceOrTypeByQualifiedName(IEnumerable<string> qualifiedName)
	{
		NamespaceOrTypeSymbol namespaceOrTypeSymbol = this;
		IEnumerable<NamespaceOrTypeSymbol> enumerable = null;
		foreach (string item in qualifiedName)
		{
			if (enumerable != null)
			{
				namespaceOrTypeSymbol = enumerable.OfMinimalArity();
				if ((object)namespaceOrTypeSymbol == null)
				{
					return SpecializedCollections.EmptyEnumerable<NamespaceOrTypeSymbol>();
				}
			}
			enumerable = namespaceOrTypeSymbol.GetMembers(item).OfType<NamespaceOrTypeSymbol>();
		}
		return enumerable;
	}
}
