using System;
using System.Threading;

namespace Microsoft.CodeAnalysis;

internal interface ISyntaxInputBuilder
{
	void VisitTree(Lazy<SyntaxNode> root, EntryState state, Lazy<SemanticModel>? model, CancellationToken cancellationToken);

	void SaveStateAndFree(StateTableStore.Builder tableStoreBuilder);
}
