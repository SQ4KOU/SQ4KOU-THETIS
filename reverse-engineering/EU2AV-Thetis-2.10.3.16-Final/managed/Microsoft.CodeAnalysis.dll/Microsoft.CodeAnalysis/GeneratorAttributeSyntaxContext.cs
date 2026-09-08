using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorAttributeSyntaxContext
{
	public SyntaxNode TargetNode { get; }

	public ISymbol TargetSymbol { get; }

	public SemanticModel SemanticModel { get; }

	public ImmutableArray<AttributeData> Attributes { get; }

	internal GeneratorAttributeSyntaxContext(SyntaxNode targetNode, ISymbol targetSymbol, SemanticModel semanticModel, ImmutableArray<AttributeData> attributes)
	{
		TargetNode = targetNode;
		TargetSymbol = targetSymbol;
		SemanticModel = semanticModel;
		Attributes = attributes;
	}
}
