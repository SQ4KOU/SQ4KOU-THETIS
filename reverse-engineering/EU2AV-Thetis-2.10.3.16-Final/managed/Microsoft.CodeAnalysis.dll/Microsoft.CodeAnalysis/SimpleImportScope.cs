using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal sealed class SimpleImportScope : IImportScope
{
	public ImmutableArray<IAliasSymbol> Aliases { get; }

	public ImmutableArray<IAliasSymbol> ExternAliases { get; }

	public ImmutableArray<ImportedNamespaceOrType> Imports { get; }

	public ImmutableArray<ImportedXmlNamespace> XmlNamespaces { get; }

	public SimpleImportScope(ImmutableArray<IAliasSymbol> aliases, ImmutableArray<IAliasSymbol> externAliases, ImmutableArray<ImportedNamespaceOrType> imports, ImmutableArray<ImportedXmlNamespace> xmlNamespaces)
	{
		Aliases = aliases.ConditionallyDeOrder();
		ExternAliases = externAliases.ConditionallyDeOrder();
		Imports = imports.ConditionallyDeOrder();
		XmlNamespaces = xmlNamespaces.ConditionallyDeOrder();
	}
}
