using System;
using System.Threading;

namespace Microsoft.CodeAnalysis;

internal sealed class PostInitOutputNode : IIncrementalGeneratorOutputNode
{
	private readonly Action<IncrementalGeneratorPostInitializationContext, CancellationToken> _callback;

	private readonly string _embeddedAttributeDefinition;

	public IncrementalGeneratorOutputKind Kind => IncrementalGeneratorOutputKind.PostInit;

	public PostInitOutputNode(Action<IncrementalGeneratorPostInitializationContext, CancellationToken> callback, string embeddedAttributeDefinition)
	{
		_callback = callback;
		_embeddedAttributeDefinition = embeddedAttributeDefinition;
	}

	public void AppendOutputs(IncrementalExecutionContext context, CancellationToken cancellationToken)
	{
		_callback(new IncrementalGeneratorPostInitializationContext(context.Sources, _embeddedAttributeDefinition, cancellationToken), cancellationToken);
	}
}
