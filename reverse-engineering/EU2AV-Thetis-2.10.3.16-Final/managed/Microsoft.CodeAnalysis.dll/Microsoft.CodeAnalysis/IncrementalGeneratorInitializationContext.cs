using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

public readonly struct IncrementalGeneratorInitializationContext
{
	private readonly ArrayBuilder<SyntaxInputNode> _syntaxInputBuilder;

	private readonly ArrayBuilder<IIncrementalGeneratorOutputNode> _outputNodes;

	private readonly string _sourceExtension;

	private readonly string _embeddedAttributeDefinition;

	internal readonly ISyntaxHelper SyntaxHelper;

	internal readonly bool CatchAnalyzerExceptions;

	public SyntaxValueProvider SyntaxProvider => new SyntaxValueProvider(this, _syntaxInputBuilder, RegisterOutput, SyntaxHelper);

	public IncrementalValueProvider<Compilation> CompilationProvider => new IncrementalValueProvider<Compilation>(SharedInputNodes.Compilation.WithRegisterOutput(RegisterOutput).WithTrackingName("Compilation"), CatchAnalyzerExceptions);

	internal IncrementalValueProvider<CompilationOptions> CompilationOptionsProvider => new IncrementalValueProvider<CompilationOptions>(SharedInputNodes.CompilationOptions.WithRegisterOutput(RegisterOutput).WithComparer(ReferenceEqualityComparer.Instance).WithTrackingName("CompilationOptions"), CatchAnalyzerExceptions);

	public IncrementalValueProvider<ParseOptions> ParseOptionsProvider => new IncrementalValueProvider<ParseOptions>(SharedInputNodes.ParseOptions.WithRegisterOutput(RegisterOutput).WithTrackingName("ParseOptions"), CatchAnalyzerExceptions);

	public IncrementalValuesProvider<AdditionalText> AdditionalTextsProvider => new IncrementalValuesProvider<AdditionalText>(SharedInputNodes.AdditionalTexts.WithRegisterOutput(RegisterOutput).WithTrackingName("AdditionalTexts"), CatchAnalyzerExceptions);

	public IncrementalValueProvider<AnalyzerConfigOptionsProvider> AnalyzerConfigOptionsProvider => new IncrementalValueProvider<AnalyzerConfigOptionsProvider>(SharedInputNodes.AnalyzerConfigOptions.WithRegisterOutput(RegisterOutput).WithTrackingName("AnalyzerConfigOptions"), CatchAnalyzerExceptions);

	public IncrementalValuesProvider<MetadataReference> MetadataReferencesProvider => new IncrementalValuesProvider<MetadataReference>(SharedInputNodes.MetadataReferences.WithRegisterOutput(RegisterOutput).WithTrackingName("MetadataReferences"), CatchAnalyzerExceptions);

	internal IncrementalGeneratorInitializationContext(ArrayBuilder<SyntaxInputNode> syntaxInputBuilder, ArrayBuilder<IIncrementalGeneratorOutputNode> outputNodes, ISyntaxHelper syntaxHelper, string sourceExtension, string embeddedAttributeDefinition, bool catchAnalyzerExceptions)
	{
		_syntaxInputBuilder = syntaxInputBuilder;
		_outputNodes = outputNodes;
		SyntaxHelper = syntaxHelper;
		_sourceExtension = sourceExtension;
		_embeddedAttributeDefinition = embeddedAttributeDefinition;
		CatchAnalyzerExceptions = catchAnalyzerExceptions;
	}

	public void RegisterSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SourceProductionContext, TSource> action)
	{
		RegisterSourceOutput(source.Node, action, IncrementalGeneratorOutputKind.Source, _sourceExtension);
	}

	public void RegisterSourceOutput<TSource>(IncrementalValuesProvider<TSource> source, Action<SourceProductionContext, TSource> action)
	{
		RegisterSourceOutput(source.Node, action, IncrementalGeneratorOutputKind.Source, _sourceExtension);
	}

	public void RegisterImplementationSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SourceProductionContext, TSource> action)
	{
		RegisterSourceOutput(source.Node, action, IncrementalGeneratorOutputKind.Implementation, _sourceExtension);
	}

	public void RegisterImplementationSourceOutput<TSource>(IncrementalValuesProvider<TSource> source, Action<SourceProductionContext, TSource> action)
	{
		RegisterSourceOutput(source.Node, action, IncrementalGeneratorOutputKind.Implementation, _sourceExtension);
	}

	public void RegisterPostInitializationOutput(Action<IncrementalGeneratorPostInitializationContext> callback)
	{
		_outputNodes.Add(new PostInitOutputNode(callback.WrapUserAction(CatchAnalyzerExceptions), _embeddedAttributeDefinition));
	}

	[Experimental("RSEXPERIMENTAL004", UrlFormat = "https://github.com/dotnet/roslyn/issues/74753")]
	public void RegisterHostOutput<TSource>(IncrementalValueProvider<TSource> source, Action<HostOutputProductionContext, TSource> action)
	{
		source.Node.RegisterOutput(new HostOutputNode<TSource>(source.Node, action.WrapUserAction(CatchAnalyzerExceptions)));
	}

	[Experimental("RSEXPERIMENTAL004", UrlFormat = "https://github.com/dotnet/roslyn/issues/74753")]
	public void RegisterHostOutput<TSource>(IncrementalValuesProvider<TSource> source, Action<HostOutputProductionContext, TSource> action)
	{
		source.Node.RegisterOutput(new HostOutputNode<TSource>(source.Node, action.WrapUserAction(CatchAnalyzerExceptions)));
	}

	private void RegisterOutput(IIncrementalGeneratorOutputNode outputNode)
	{
		if (!_outputNodes.Contains(outputNode))
		{
			_outputNodes.Add(outputNode);
		}
	}

	private void RegisterSourceOutput<TSource>(IIncrementalGeneratorNode<TSource> node, Action<SourceProductionContext, TSource> action, IncrementalGeneratorOutputKind kind, string sourceExt)
	{
		node.RegisterOutput(new SourceOutputNode<TSource>(node, action.WrapUserAction(CatchAnalyzerExceptions), kind, sourceExt));
	}
}
