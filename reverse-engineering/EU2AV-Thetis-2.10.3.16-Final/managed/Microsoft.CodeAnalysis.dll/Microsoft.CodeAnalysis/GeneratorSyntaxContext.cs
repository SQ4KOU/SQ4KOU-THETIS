using System;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorSyntaxContext
{
	internal readonly ISyntaxHelper SyntaxHelper;

	private readonly Lazy<SemanticModel>? _semanticModel;

	public SyntaxNode Node { get; }

	public SemanticModel SemanticModel => _semanticModel.Value;

	internal GeneratorSyntaxContext(SyntaxNode node, Lazy<SemanticModel>? semanticModel, ISyntaxHelper syntaxHelper)
	{
		Node = node;
		_semanticModel = semanticModel;
		SyntaxHelper = syntaxHelper;
	}
}
