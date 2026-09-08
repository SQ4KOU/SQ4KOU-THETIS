using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class NamespaceSymbol : NamespaceOrTypeSymbol, INamespace, INamedEntity, INamespaceSymbolInternal, INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	private ImmutableArray<NamedTypeSymbol> _lazyTypesMightContainExtensionMethods;

	private string _lazyQualifiedName;

	INamespace INamespace.ContainingNamespace => AdaptedNamespaceSymbol.ContainingNamespace?.GetCciAdapter();

	string INamedEntity.Name => AdaptedNamespaceSymbol.MetadataName;

	internal NamespaceSymbol AdaptedNamespaceSymbol => this;

	public virtual bool IsGlobalNamespace => (object)ContainingNamespace == null;

	internal abstract NamespaceExtent Extent { get; }

	public NamespaceKind NamespaceKind => Extent.Kind;

	public CSharpCompilation ContainingCompilation
	{
		get
		{
			if (NamespaceKind != NamespaceKind.Compilation)
			{
				return null;
			}
			return Extent.Compilation;
		}
	}

	public virtual ImmutableArray<NamespaceSymbol> ConstituentNamespaces => ImmutableArray.Create(this);

	public sealed override NamedTypeSymbol ContainingType => null;

	public abstract override AssemblySymbol ContainingAssembly { get; }

	internal override ModuleSymbol ContainingModule
	{
		get
		{
			NamespaceExtent extent = Extent;
			if (extent.Kind == NamespaceKind.Module)
			{
				return extent.Module;
			}
			return null;
		}
	}

	public sealed override SymbolKind Kind => SymbolKind.Namespace;

	public sealed override bool IsImplicitlyDeclared => IsGlobalNamespace;

	public sealed override Accessibility DeclaredAccessibility => Accessibility.Public;

	public sealed override bool IsStatic => true;

	public sealed override bool IsAbstract => false;

	public sealed override bool IsSealed => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal NamedTypeSymbol ImplicitType
	{
		get
		{
			ImmutableArray<NamedTypeSymbol> typeMembers = GetTypeMembers("<invalid-global-code>");
			if (typeMembers.Length == 0)
			{
				return null;
			}
			return typeMembers[0];
		}
	}

	private ImmutableArray<NamedTypeSymbol> TypesMightContainExtensionMethods
	{
		get
		{
			ImmutableArray<NamedTypeSymbol> lazyTypesMightContainExtensionMethods = _lazyTypesMightContainExtensionMethods;
			if (lazyTypesMightContainExtensionMethods.IsDefault)
			{
				_lazyTypesMightContainExtensionMethods = GetTypeMembersUnordered().WhereAsArray((NamedTypeSymbol t) => t.MightContainExtensionMethods);
				lazyTypesMightContainExtensionMethods = _lazyTypesMightContainExtensionMethods;
			}
			return lazyTypesMightContainExtensionMethods;
		}
	}

	internal string QualifiedName => _lazyQualifiedName ?? (_lazyQualifiedName = ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat));

	bool INamespaceSymbolInternal.IsGlobalNamespace => IsGlobalNamespace;

	INamespaceSymbolInternal INamespace.GetInternalSymbol()
	{
		return AdaptedNamespaceSymbol;
	}

	internal new NamespaceSymbol GetCciAdapter()
	{
		return this;
	}

	public IEnumerable<NamespaceSymbol> GetNamespaceMembers()
	{
		return GetMembers().OfType<NamespaceSymbol>();
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNamespace(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitNamespace(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitNamespace(this);
	}

	internal NamespaceSymbol()
	{
	}

	internal NamespaceSymbol? LookupNestedNamespace(ImmutableArray<ReadOnlyMemory<char>> names)
	{
		NamespaceSymbol namespaceSymbol = this;
		foreach (ReadOnlyMemory<char> item in names)
		{
			namespaceSymbol = namespaceSymbol.GetNestedNamespace(item);
			if ((object)namespaceSymbol == null)
			{
				return null;
			}
		}
		return namespaceSymbol;
	}

	internal NamespaceSymbol? GetNestedNamespace(string name)
	{
		return GetNestedNamespace(System.MemoryExtensions.AsMemory(name));
	}

	internal virtual NamespaceSymbol? GetNestedNamespace(ReadOnlyMemory<char> name)
	{
		foreach (Symbol member in GetMembers(name))
		{
			if (member.Kind == SymbolKind.Namespace)
			{
				return (NamespaceSymbol)member;
			}
		}
		return null;
	}

	public abstract ImmutableArray<Symbol> GetMembers(ReadOnlyMemory<char> name);

	public sealed override ImmutableArray<Symbol> GetMembers(string name)
	{
		return GetMembers(System.MemoryExtensions.AsMemory(name));
	}

	internal NamespaceSymbol GetNestedNamespace(NameSyntax name)
	{
		switch (name.Kind())
		{
		case SyntaxKind.IdentifierName:
		case SyntaxKind.GenericName:
			return GetNestedNamespace(((SimpleNameSyntax)name).Identifier.ValueText);
		case SyntaxKind.QualifiedName:
		{
			QualifiedNameSyntax qualifiedNameSyntax = (QualifiedNameSyntax)name;
			NamespaceSymbol nestedNamespace = GetNestedNamespace(qualifiedNameSyntax.Left);
			if ((object)nestedNamespace != null)
			{
				return nestedNamespace.GetNestedNamespace(qualifiedNameSyntax.Right);
			}
			break;
		}
		case SyntaxKind.AliasQualifiedName:
			return GetNestedNamespace(name.GetUnqualifiedName().Identifier.ValueText);
		}
		return null;
	}

	internal virtual void GetExtensionMethods(ArrayBuilder<MethodSymbol> methods, string nameOpt, int arity, LookupOptions options)
	{
		if (ContainingAssembly.MightContainExtensionMethods)
		{
			foreach (NamedTypeSymbol typesMightContainExtensionMethod in TypesMightContainExtensionMethods)
			{
				typesMightContainExtensionMethod.DoGetExtensionMethods(methods, nameOpt, arity, options);
			}
		}
	}

	internal virtual void GetExtensionMembers(ArrayBuilder<Symbol> members, string? name, string? alternativeName, int arity, LookupOptions options, ConsList<FieldSymbol> fieldsBeingBound)
	{
		foreach (NamedTypeSymbol item in GetTypeMembersUnordered())
		{
			item.GetExtensionMembers(members, name, alternativeName, arity, options, fieldsBeingBound);
		}
	}

	protected sealed override ISymbol CreateISymbol()
	{
		return new Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.NamespaceSymbol(this);
	}
}
