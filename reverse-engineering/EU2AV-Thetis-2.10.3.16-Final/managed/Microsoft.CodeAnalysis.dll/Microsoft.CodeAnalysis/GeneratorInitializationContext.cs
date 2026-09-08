using System;
using System.Threading;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorInitializationContext
{
	internal sealed class CallbackHolder
	{
		internal SyntaxContextReceiverCreator? SyntaxContextReceiverCreator { get; set; }

		internal Action<IncrementalGeneratorPostInitializationContext>? PostInitCallback { get; set; }
	}

	public CancellationToken CancellationToken { get; }

	internal CallbackHolder Callbacks { get; }

	internal GeneratorInitializationContext(CancellationToken cancellationToken = default(CancellationToken))
	{
		CancellationToken = cancellationToken;
		Callbacks = new CallbackHolder();
	}

	public void RegisterForSyntaxNotifications(SyntaxReceiverCreator receiverCreator)
	{
		CheckIsEmpty(Callbacks.SyntaxContextReceiverCreator, "SyntaxReceiverCreator / SyntaxContextReceiverCreator");
		Callbacks.SyntaxContextReceiverCreator = SyntaxContextReceiverAdaptor.Create(receiverCreator);
	}

	public void RegisterForSyntaxNotifications(SyntaxContextReceiverCreator receiverCreator)
	{
		CheckIsEmpty(Callbacks.SyntaxContextReceiverCreator, "SyntaxReceiverCreator / SyntaxContextReceiverCreator");
		Callbacks.SyntaxContextReceiverCreator = receiverCreator;
	}

	public void RegisterForPostInitialization(Action<GeneratorPostInitializationContext> callback)
	{
		CheckIsEmpty(Callbacks.PostInitCallback);
		Callbacks.PostInitCallback = delegate(IncrementalGeneratorPostInitializationContext context)
		{
			callback(new GeneratorPostInitializationContext(context.AdditionalSources, context.CancellationToken));
		};
	}

	private static void CheckIsEmpty<T>(T x, string? typeName = null) where T : class?
	{
		if (x != null)
		{
			throw new InvalidOperationException(string.Format(CodeAnalysisResources.Single_type_per_generator_0, typeName ?? typeof(T).Name));
		}
	}
}
