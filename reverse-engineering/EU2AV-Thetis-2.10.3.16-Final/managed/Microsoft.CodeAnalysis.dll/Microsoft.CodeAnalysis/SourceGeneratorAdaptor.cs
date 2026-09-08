using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

internal sealed class SourceGeneratorAdaptor : IIncrementalGenerator
{
	internal record GeneratorContextBuilder(Compilation Compilation)
	{
		public ParseOptions? ParseOptions;

		public ImmutableArray<AdditionalText> AdditionalTexts;

		public AnalyzerConfigOptionsProvider? ConfigOptions;

		public ISyntaxContextReceiver? Receiver;

		public GeneratorExecutionContext ToExecutionContext(string sourceExtension, SourceHashAlgorithm checksumAlgorithm, CancellationToken cancellationToken)
		{
			return new GeneratorExecutionContext(Compilation, ParseOptions, AdditionalTexts, ConfigOptions, Receiver, sourceExtension, checksumAlgorithm, cancellationToken);
		}
	}

	public const string DummySourceExtension = ".dummy";

	private readonly string _sourceExtension;

	internal ISourceGenerator SourceGenerator { get; }

	public SourceGeneratorAdaptor(ISourceGenerator generator, string sourceExtension)
	{
		SourceGenerator = generator;
		_sourceExtension = sourceExtension;
	}

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		GeneratorInitializationContext context2 = new GeneratorInitializationContext(CancellationToken.None);
		SourceGenerator.Initialize(context2);
		if (context2.Callbacks.PostInitCallback != null)
		{
			context.RegisterPostInitializationOutput(context2.Callbacks.PostInitCallback);
		}
		IncrementalValueProvider<GeneratorContextBuilder> incrementalValueProvider = context.CompilationProvider.Select((Compilation c, CancellationToken _) => new GeneratorContextBuilder(c)).Combine(context.ParseOptionsProvider).Select(((GeneratorContextBuilder Left, ParseOptions Right) p, CancellationToken _) => p.Left with
		{
			ParseOptions = p.Right
		})
			.Combine(context.AnalyzerConfigOptionsProvider)
			.Select(((GeneratorContextBuilder Left, AnalyzerConfigOptionsProvider Right) p, CancellationToken _) => p.Left with
			{
				ConfigOptions = p.Right
			})
			.Combine(context.AdditionalTextsProvider.Collect())
			.Select(((GeneratorContextBuilder Left, ImmutableArray<AdditionalText> Right) p, CancellationToken _) => p.Left with
			{
				AdditionalTexts = p.Right
			});
		SyntaxContextReceiverCreator syntaxContextReceiverCreator = context2.Callbacks.SyntaxContextReceiverCreator;
		if (syntaxContextReceiverCreator != null)
		{
			incrementalValueProvider = incrementalValueProvider.Combine(context.SyntaxProvider.CreateSyntaxReceiverProvider(syntaxContextReceiverCreator)).Select<(GeneratorContextBuilder, ISyntaxContextReceiver), GeneratorContextBuilder>(((GeneratorContextBuilder Left, ISyntaxContextReceiver Right) p, CancellationToken _) => p.Left with
			{
				Receiver = p.Right
			});
		}
		context.RegisterSourceOutput(incrementalValueProvider, delegate(SourceProductionContext productionContext, GeneratorContextBuilder contextBuilder)
		{
			GeneratorExecutionContext context3 = contextBuilder.ToExecutionContext(_sourceExtension, productionContext.ChecksumAlgorithm, productionContext.CancellationToken);
			SourceGenerator.Execute(context3);
			context3.CopyToProductionContext(productionContext);
			context3.Free();
		});
	}
}
