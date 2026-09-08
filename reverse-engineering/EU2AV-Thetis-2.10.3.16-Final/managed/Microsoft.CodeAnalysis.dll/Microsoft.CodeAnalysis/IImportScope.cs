using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IImportScope
{
	ImmutableArray<IAliasSymbol> Aliases { get; }

	ImmutableArray<IAliasSymbol> ExternAliases { get; }

	ImmutableArray<ImportedNamespaceOrType> Imports { get; }

	ImmutableArray<ImportedXmlNamespace> XmlNamespaces { get; }
}
