using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis;

internal sealed class RebuildData
{
	internal ImmutableArray<string> NonSourceFileDocumentNames { get; }

	internal BlobReader OptionsBlobReader { get; }

	internal RebuildData(BlobReader optionsBlobReader, ImmutableArray<string> nonSourceFileDocumentNames)
	{
		OptionsBlobReader = optionsBlobReader;
		NonSourceFileDocumentNames = nonSourceFileDocumentNames;
	}
}
