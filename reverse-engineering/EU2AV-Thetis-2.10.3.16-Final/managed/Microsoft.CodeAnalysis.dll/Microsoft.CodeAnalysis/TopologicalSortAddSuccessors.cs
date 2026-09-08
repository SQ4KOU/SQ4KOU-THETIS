using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis;

internal delegate void TopologicalSortAddSuccessors<TNode>(ref TemporaryArray<TNode> builder, TNode node);
