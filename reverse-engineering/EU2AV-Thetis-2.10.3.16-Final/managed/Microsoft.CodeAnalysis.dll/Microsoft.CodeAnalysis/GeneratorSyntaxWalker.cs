using System;

namespace Microsoft.CodeAnalysis;

internal sealed class GeneratorSyntaxWalker
{
	private readonly ISyntaxContextReceiver _syntaxReceiver;

	private readonly ISyntaxHelper _syntaxHelper;

	internal GeneratorSyntaxWalker(ISyntaxContextReceiver syntaxReceiver, ISyntaxHelper syntaxHelper)
	{
		_syntaxReceiver = syntaxReceiver;
		_syntaxHelper = syntaxHelper;
	}

	public void VisitWithModel(Lazy<SemanticModel>? model, SyntaxNode node)
	{
		foreach (SyntaxNode item in node.DescendantNodesAndSelf())
		{
			_syntaxReceiver.OnVisitSyntaxNode(new GeneratorSyntaxContext(item, model, _syntaxHelper));
		}
	}
}
