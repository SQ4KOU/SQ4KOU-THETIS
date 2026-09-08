using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

public abstract class GeneratorDriver
{
	internal readonly GeneratorDriverState _state;

	internal abstract CommonMessageProvider MessageProvider { get; }

	internal abstract string SourceExtension { get; }

	internal abstract string EmbeddedAttributeDefinition { get; }

	internal abstract ISyntaxHelper SyntaxHelper { get; }

	internal GeneratorDriver(GeneratorDriverState state)
	{
		_state = state;
	}

	internal GeneratorDriver(ParseOptions parseOptions, ImmutableArray<ISourceGenerator> generators, AnalyzerConfigOptionsProvider optionsProvider, ImmutableArray<AdditionalText> additionalTexts, GeneratorDriverOptions driverOptions)
	{
		ImmutableArray<IIncrementalGenerator> incrementalGenerators = GetIncrementalGenerators(generators, SourceExtension);
		_state = new GeneratorDriverState(parseOptions, optionsProvider, generators, incrementalGenerators, additionalTexts, ImmutableArray.Create(new GeneratorState[generators.Length]), DriverStateTable.Empty, SyntaxStore.Empty, driverOptions, TimeSpan.Zero);
	}

	public GeneratorDriver RunGenerators(Compilation compilation)
	{
		return RunGenerators(compilation, null);
	}

	public GeneratorDriver RunGenerators(Compilation compilation, CancellationToken cancellationToken)
	{
		return RunGenerators(compilation, null, cancellationToken);
	}

	public GeneratorDriver RunGenerators(Compilation compilation, Func<GeneratorFilterContext, bool>? generatorFilter, CancellationToken cancellationToken = default(CancellationToken))
	{
		GeneratorDriverState state = RunGeneratorsCore(compilation, null, generatorFilter, cancellationToken);
		return FromState(state);
	}

	public GeneratorDriver RunGeneratorsAndUpdateCompilation(Compilation compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default(CancellationToken))
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		GeneratorDriverState state = RunGeneratorsCore(compilation, instance, null, cancellationToken);
		diagnostics = instance.ToReadOnlyAndFree();
		ArrayBuilder<SyntaxTree> instance2 = ArrayBuilder<SyntaxTree>.GetInstance();
		foreach (GeneratorState generatorState in state.GeneratorStates)
		{
			instance2.AddRange(generatorState.PostInitTrees.Select((GeneratedSyntaxTree t) => t.Tree));
			instance2.AddRange(generatorState.GeneratedTrees.Select((GeneratedSyntaxTree t) => t.Tree));
		}
		outputCompilation = compilation.AddSyntaxTrees(instance2);
		instance2.Free();
		return FromState(state);
	}

	public GeneratorDriver AddGenerators(ImmutableArray<ISourceGenerator> generators)
	{
		ImmutableArray<IIncrementalGenerator> incrementalGenerators = GetIncrementalGenerators(generators, SourceExtension);
		GeneratorDriverState state = _state.With(_state.Generators.AddRange(generators), _state.IncrementalGenerators.AddRange(incrementalGenerators), _state.GeneratorStates.AddRange(new GeneratorState[generators.Length]));
		return FromState(state);
	}

	public GeneratorDriver ReplaceGenerators(ImmutableArray<ISourceGenerator> generators)
	{
		ImmutableArray<IIncrementalGenerator> incrementalGenerators = GetIncrementalGenerators(generators, SourceExtension);
		ArrayBuilder<GeneratorState> instance = ArrayBuilder<GeneratorState>.GetInstance(generators.Length);
		foreach (ISourceGenerator item in generators)
		{
			int num = _state.Generators.IndexOf(item);
			if (num >= 0)
			{
				instance.Add(_state.GeneratorStates[num]);
			}
			else
			{
				instance.Add(default(GeneratorState));
			}
		}
		return FromState(_state.With(generators, incrementalGenerators, instance.ToImmutableAndFree()));
	}

	public GeneratorDriver RemoveGenerators(ImmutableArray<ISourceGenerator> generators)
	{
		ImmutableArray<ISourceGenerator> value = _state.Generators;
		ImmutableArray<GeneratorState> value2 = _state.GeneratorStates;
		ImmutableArray<IIncrementalGenerator> value3 = _state.IncrementalGenerators;
		for (int i = 0; i < value.Length; i++)
		{
			if (generators.Contains(value[i]))
			{
				value = value.RemoveAt(i);
				value2 = value2.RemoveAt(i);
				value3 = value3.RemoveAt(i);
				i--;
			}
		}
		return FromState(_state.With(value, value3, value2));
	}

	public GeneratorDriver AddAdditionalTexts(ImmutableArray<AdditionalText> additionalTexts)
	{
		ref readonly GeneratorDriverState state = ref _state;
		ImmutableArray<AdditionalText>? additionalTexts2 = _state.AdditionalTexts.AddRange(additionalTexts);
		GeneratorDriverState state2 = state.With(null, null, null, additionalTexts2);
		return FromState(state2);
	}

	public GeneratorDriver RemoveAdditionalTexts(ImmutableArray<AdditionalText> additionalTexts)
	{
		ref readonly GeneratorDriverState state = ref _state;
		ImmutableArray<AdditionalText>? additionalTexts2 = _state.AdditionalTexts.RemoveRange(additionalTexts);
		GeneratorDriverState state2 = state.With(null, null, null, additionalTexts2);
		return FromState(state2);
	}

	public GeneratorDriver ReplaceAdditionalText(AdditionalText oldText, AdditionalText newText)
	{
		if (oldText == null)
		{
			throw new ArgumentNullException("oldText");
		}
		if (newText == null)
		{
			throw new ArgumentNullException("newText");
		}
		ref readonly GeneratorDriverState state = ref _state;
		ImmutableArray<AdditionalText>? additionalTexts = _state.AdditionalTexts.Replace(oldText, newText);
		GeneratorDriverState state2 = state.With(null, null, null, additionalTexts);
		return FromState(state2);
	}

	public GeneratorDriver ReplaceAdditionalTexts(ImmutableArray<AdditionalText> newTexts)
	{
		ref readonly GeneratorDriverState state = ref _state;
		ImmutableArray<AdditionalText>? additionalTexts = newTexts;
		return FromState(state.With(null, null, null, additionalTexts));
	}

	public GeneratorDriver WithUpdatedParseOptions(ParseOptions newOptions)
	{
		if ((object)newOptions == null)
		{
			throw new ArgumentNullException("newOptions");
		}
		return FromState(_state.With(null, null, null, null, null, null, newOptions));
	}

	public GeneratorDriver WithUpdatedAnalyzerConfigOptions(AnalyzerConfigOptionsProvider newOptions)
	{
		if (newOptions == null)
		{
			throw new ArgumentNullException("newOptions");
		}
		return FromState(_state.With(null, null, null, null, null, null, null, newOptions));
	}

	public GeneratorDriverRunResult GetRunResult()
	{
		return new GeneratorDriverRunResult(_state.Generators.ZipAsArray(_state.GeneratorStates, delegate(ISourceGenerator generator, GeneratorState generatorState)
		{
			ImmutableArray<Diagnostic> diagnostics = generatorState.Diagnostics;
			return new GeneratorRunResult(exception: generatorState.Exception, generatedSources: getGeneratorSources(generatorState), diagnostics: diagnostics, elapsedTime: generatorState.ElapsedTime, generator: generator, namedSteps: generatorState.ExecutedSteps, outputSteps: generatorState.OutputSteps, hostOutputs: generatorState.HostOutputs);
		}), _state.RunTime);
		static ImmutableArray<GeneratedSourceResult> getGeneratorSources(GeneratorState generatorState)
		{
			if (!generatorState.Initialized)
			{
				return default(ImmutableArray<GeneratedSourceResult>);
			}
			ArrayBuilder<GeneratedSourceResult> instance = ArrayBuilder<GeneratedSourceResult>.GetInstance(generatorState.PostInitTrees.Length + generatorState.GeneratedTrees.Length);
			foreach (GeneratedSyntaxTree postInitTree in generatorState.PostInitTrees)
			{
				instance.Add(new GeneratedSourceResult(postInitTree.Tree, postInitTree.Text, postInitTree.HintName));
			}
			foreach (GeneratedSyntaxTree generatedTree in generatorState.GeneratedTrees)
			{
				instance.Add(new GeneratedSourceResult(generatedTree.Tree, generatedTree.Text, generatedTree.HintName));
			}
			return instance.ToImmutableAndFree();
		}
	}

	public GeneratorDriverTimingInfo GetTimingInfo()
	{
		ImmutableArray<GeneratorTimingInfo> generatorTimes = _state.Generators.ZipAsArray(_state.GeneratorStates, (ISourceGenerator generator, GeneratorState generatorState) => new GeneratorTimingInfo(generator, generatorState.ElapsedTime));
		return new GeneratorDriverTimingInfo(_state.RunTime, generatorTimes);
	}

	internal GeneratorDriverState RunGeneratorsCore(Compilation compilation, DiagnosticBag? diagnosticsBag, Func<GeneratorFilterContext, bool>? generatorFilter = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_state.Generators.IsEmpty)
		{
			ref readonly GeneratorDriverState state = ref _state;
			DriverStateTable empty = DriverStateTable.Empty;
			TimeSpan? runTime = TimeSpan.Zero;
			return state.With(null, null, null, null, empty, null, null, null, runTime);
		}
		using (GeneratorTimerExtensions.RunTimer runTimer = CodeAnalysisEventSource.Log.CreateGeneratorDriverRunTimer())
		{
			GeneratorDriverState state2 = _state;
			ArrayBuilder<GeneratorState> stateBuilder = ArrayBuilder<GeneratorState>.GetInstance(state2.Generators.Length);
			ArrayBuilder<SyntaxTree> instance = ArrayBuilder<SyntaxTree>.GetInstance();
			ArrayBuilder<SyntaxInputNode> instance2 = ArrayBuilder<SyntaxInputNode>.GetInstance();
			for (int i = 0; i < state2.IncrementalGenerators.Length; i++)
			{
				IIncrementalGenerator incrementalGenerator = state2.IncrementalGenerators[i];
				GeneratorState item = state2.GeneratorStates[i];
				ISourceGenerator sourceGenerator = state2.Generators[i];
				if (shouldSkipGenerator(sourceGenerator))
				{
					stateBuilder.Add(item);
					continue;
				}
				if (!item.Initialized)
				{
					ArrayBuilder<IIncrementalGeneratorOutputNode> instance3 = ArrayBuilder<IIncrementalGeneratorOutputNode>.GetInstance();
					ArrayBuilder<SyntaxInputNode> instance4 = ArrayBuilder<SyntaxInputNode>.GetInstance();
					ImmutableArray<GeneratedSyntaxTree> postInitTrees = ImmutableArray<GeneratedSyntaxTree>.Empty;
					IncrementalGeneratorInitializationContext context = new IncrementalGeneratorInitializationContext(instance4, instance3, SyntaxHelper, SourceExtension, EmbeddedAttributeDefinition, compilation.CatchAnalyzerExceptions);
					Exception ex = null;
					try
					{
						incrementalGenerator.Initialize(context);
					}
					catch (Exception ex2) when (handleGeneratorException(compilation, MessageProvider, sourceGenerator, ex2, isInit: true))
					{
						ex = ex2;
					}
					ImmutableArray<IIncrementalGeneratorOutputNode> outputNodes = instance3.ToImmutableAndFree();
					ImmutableArray<SyntaxInputNode> inputNodes = instance4.ToImmutableAndFree();
					if (ex == null)
					{
						try
						{
							postInitTrees = ParseAdditionalSources(sourceGenerator, UpdateOutputs(outputNodes, IncrementalGeneratorOutputKind.PostInit, new GeneratorRunStateTable.Builder(recordingExecutedSteps: false), cancellationToken).ToImmutableAndFree().sources, cancellationToken);
						}
						catch (UserFunctionException ex3) when (handleGeneratorException(compilation, MessageProvider, sourceGenerator, ex3, isInit: true))
						{
							ex = ex3.InnerException;
						}
					}
					item = ((ex == null) ? new GeneratorState(postInitTrees, inputNodes, outputNodes) : SetGeneratorException(compilation, MessageProvider, GeneratorState.Empty, sourceGenerator, ex, diagnosticsBag, cancellationToken, null, isInit: true));
				}
				else if (item.PostInitTrees.Length > 0 && item.RequiresPostInitReparse(state2.ParseOptions))
				{
					ImmutableArray<GeneratedSyntaxTree> postInitTrees2 = ParseAdditionalSources(sourceGenerator, item.PostInitTrees.SelectAsArray((GeneratedSyntaxTree t) => new GeneratedSourceText(t.HintName, t.Text)), cancellationToken);
					item = new GeneratorState(postInitTrees2, item.InputNodes, item.OutputNodes);
				}
				if (!item.InputNodes.IsEmpty)
				{
					instance2.AddRange(item.InputNodes);
				}
				if (item.PostInitTrees.Length > 0)
				{
					instance.AddRange(item.PostInitTrees.Select((GeneratedSyntaxTree t) => t.Tree));
				}
				stateBuilder.Add(item);
			}
			if (instance.Count > 0)
			{
				compilation = compilation.AddSyntaxTrees(instance);
			}
			instance.Free();
			SyntaxStore.Builder syntaxStoreBuilder = _state.SyntaxStore.ToBuilder(compilation, instance2.ToImmutableAndFree(), _state.TrackIncrementalSteps, cancellationToken);
			DriverStateTable.Builder builder = new DriverStateTable.Builder(compilation, _state, syntaxStoreBuilder, cancellationToken);
			int i2;
			for (i2 = 0; i2 < state2.IncrementalGenerators.Length; i2++)
			{
				GeneratorState generatorState = stateBuilder[i2];
				if (shouldSkipGenerator(state2.Generators[i2]) || generatorState.OutputNodes.Length == 0)
				{
					continue;
				}
				using GeneratorTimerExtensions.RunTimer runTimer2 = CodeAnalysisEventSource.Log.CreateSingleGeneratorRunTimer(state2.Generators[i2], (TimeSpan t) => t.Add(syntaxStoreBuilder.GetRuntimeAdjustment(stateBuilder[i2].InputNodes)));
				try
				{
					(ImmutableArray<GeneratedSourceText> sources, ImmutableArray<Diagnostic> diagnostics, GeneratorRunStateTable executedSteps, ImmutableDictionary<string, object> hostOutputs) tuple = UpdateOutputs(generatorState.OutputNodes, IncrementalGeneratorOutputKind.Source | IncrementalGeneratorOutputKind.Implementation | IncrementalGeneratorOutputKind.Host, new GeneratorRunStateTable.Builder(state2.TrackIncrementalSteps), cancellationToken, builder).ToImmutableAndFree();
					ImmutableArray<GeneratedSourceText> item2 = tuple.sources;
					ImmutableArray<Diagnostic> item3 = tuple.diagnostics;
					GeneratorRunStateTable item4 = tuple.executedSteps;
					ImmutableDictionary<string, object> item5 = tuple.hostOutputs;
					item3 = FilterDiagnostics(compilation, item3, diagnosticsBag, cancellationToken);
					stateBuilder[i2] = generatorState.WithResults(ParseAdditionalSources(state2.Generators[i2], item2, cancellationToken), item3, item4.ExecutedSteps, item4.OutputSteps, item5, runTimer2.Elapsed);
				}
				catch (UserFunctionException ex4) when (handleGeneratorException(compilation, MessageProvider, state2.Generators[i2], ex4.InnerException, isInit: false))
				{
					stateBuilder[i2] = SetGeneratorException(compilation, MessageProvider, generatorState, state2.Generators[i2], ex4.InnerException, diagnosticsBag, cancellationToken, runTimer2.Elapsed);
				}
			}
			DriverStateTable empty = builder.ToImmutable();
			SyntaxStore syntaxStore = syntaxStoreBuilder.ToImmutable();
			ImmutableArray<GeneratorState>? generatorStates = stateBuilder.ToImmutableAndFree();
			TimeSpan? runTime = runTimer.Elapsed;
			return state2.With(null, null, generatorStates, null, empty, syntaxStore, null, null, runTime);
		}
		static bool handleGeneratorException(Compilation compilation2, CommonMessageProvider messageProvider, ISourceGenerator generator, Exception e, bool isInit)
		{
			if (!compilation2.CatchAnalyzerExceptions)
			{
				Environment.FailFast(CreateGeneratorExceptionDiagnostic(messageProvider, generator, e, isInit).ToString());
				return false;
			}
			return true;
		}
		bool shouldSkipGenerator(ISourceGenerator generator)
		{
			Func<GeneratorFilterContext, bool>? func = generatorFilter;
			if (func == null)
			{
				return false;
			}
			return !func(new GeneratorFilterContext(generator, cancellationToken));
		}
	}

	private IncrementalExecutionContext UpdateOutputs(ImmutableArray<IIncrementalGeneratorOutputNode> outputNodes, IncrementalGeneratorOutputKind outputKind, GeneratorRunStateTable.Builder generatorRunStateBuilder, CancellationToken cancellationToken, DriverStateTable.Builder? driverStateBuilder = null)
	{
		IncrementalExecutionContext incrementalExecutionContext = new IncrementalExecutionContext(driverStateBuilder, generatorRunStateBuilder, new AdditionalSourcesCollection(SourceExtension));
		foreach (IIncrementalGeneratorOutputNode item in outputNodes)
		{
			if (outputKind.HasFlag(item.Kind) && !_state.DisabledOutputs.HasFlag(item.Kind))
			{
				item.AppendOutputs(incrementalExecutionContext, cancellationToken);
			}
		}
		return incrementalExecutionContext;
	}

	private ImmutableArray<GeneratedSyntaxTree> ParseAdditionalSources(ISourceGenerator generator, ImmutableArray<GeneratedSourceText> generatedSources, CancellationToken cancellationToken)
	{
		ArrayBuilder<GeneratedSyntaxTree> instance = ArrayBuilder<GeneratedSyntaxTree>.GetInstance(generatedSources.Length);
		string filePathPrefixForGenerator = GetFilePathPrefixForGenerator(_state.BaseDirectory, generator);
		foreach (GeneratedSourceText item in generatedSources)
		{
			SyntaxTree tree = ParseGeneratedSourceText(item, Path.Combine(filePathPrefixForGenerator, item.HintName), cancellationToken);
			instance.Add(new GeneratedSyntaxTree(item.HintName, item.Text, tree));
		}
		return instance.ToImmutableAndFree();
	}

	private static GeneratorState SetGeneratorException(Compilation compilation, CommonMessageProvider provider, GeneratorState generatorState, ISourceGenerator generator, Exception e, DiagnosticBag? diagnosticBag, CancellationToken cancellationToken, TimeSpan? runTime = null, bool isInit = false)
	{
		if (CodeAnalysisEventSource.Log.IsEnabled())
		{
			CodeAnalysisEventSource.Log.GeneratorException(generator.GetGeneratorType().Name, e.ToString());
		}
		Diagnostic diagnostic = CreateGeneratorExceptionDiagnostic(provider, generator, e, isInit);
		Diagnostic diagnostic2 = compilation.Options.FilterDiagnostic(diagnostic, cancellationToken);
		if (diagnostic2 != null)
		{
			diagnosticBag?.Add(diagnostic2);
			return generatorState.WithError(e, diagnostic2, runTime ?? TimeSpan.Zero);
		}
		return generatorState;
	}

	private static Diagnostic CreateGeneratorExceptionDiagnostic(CommonMessageProvider provider, ISourceGenerator generator, Exception e, bool isInit)
	{
		int num = (isInit ? provider.WRN_GeneratorFailedDuringInitialization : provider.WRN_GeneratorFailedDuringGeneration);
		return Diagnostic.Create(new DiagnosticDescriptor(provider.GetIdForErrorCode(num), provider.GetTitle(num), provider.GetMessageFormat(num), "Compiler", DiagnosticSeverity.Warning, true, null, null, "AnalyzerException"), Location.None, generator.GetGeneratorType().Name, e.GetType().Name, e.Message, e.CreateDiagnosticDescription());
	}

	private static ImmutableArray<Diagnostic> FilterDiagnostics(Compilation compilation, ImmutableArray<Diagnostic> generatorDiagnostics, DiagnosticBag? driverDiagnostics, CancellationToken cancellationToken)
	{
		if (generatorDiagnostics.IsEmpty)
		{
			return generatorDiagnostics;
		}
		SuppressMessageAttributeState suppressMessageAttributeState = new SuppressMessageAttributeState(compilation);
		ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
		foreach (Diagnostic item in generatorDiagnostics)
		{
			Diagnostic diagnostic = compilation.Options.FilterDiagnostic(item, cancellationToken);
			if (diagnostic != null)
			{
				Diagnostic diagnostic2 = suppressMessageAttributeState.ApplySourceSuppressions(diagnostic);
				if (diagnostic2 != null)
				{
					instance.Add(diagnostic2);
					driverDiagnostics?.Add(diagnostic2);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal static string GetFilePathPrefixForGenerator(string? baseDirectory, ISourceGenerator generator)
	{
		Type generatorType = generator.GetGeneratorType();
		return Path.Combine(baseDirectory ?? "", generatorType.Assembly.GetName().Name ?? string.Empty, generatorType.FullName);
	}

	private static ImmutableArray<IIncrementalGenerator> GetIncrementalGenerators(ImmutableArray<ISourceGenerator> generators, string sourceExtension)
	{
		return generators.SelectAsArray(delegate(ISourceGenerator g)
		{
			if (g is IncrementalGeneratorWrapper incrementalGeneratorWrapper)
			{
				return incrementalGeneratorWrapper.Generator;
			}
			return (g is IIncrementalGenerator incrementalGenerator) ? incrementalGenerator : new SourceGeneratorAdaptor(g, sourceExtension);
		});
	}

	internal abstract GeneratorDriver FromState(GeneratorDriverState state);

	internal abstract SyntaxTree ParseGeneratedSourceText(GeneratedSourceText input, string fileName, CancellationToken cancellationToken);
}
