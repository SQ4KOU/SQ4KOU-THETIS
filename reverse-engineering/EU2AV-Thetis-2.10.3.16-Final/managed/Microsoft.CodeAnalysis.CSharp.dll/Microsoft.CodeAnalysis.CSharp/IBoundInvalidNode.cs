using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal interface IBoundInvalidNode
{
	ImmutableArray<BoundNode> InvalidNodeChildren { get; }
}
