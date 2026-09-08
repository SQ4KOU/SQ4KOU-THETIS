using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

internal readonly struct GeneratorDriverState
{
	internal readonly ImmutableArray<ISourceGenerator> Generators;

	internal readonly ImmutableArray<IIncrementalGenerator> IncrementalGenerators;

	internal readonly ImmutableArray<GeneratorState> GeneratorStates;

	internal readonly ImmutableArray<AdditionalText> AdditionalTexts;

	internal readonly AnalyzerConfigOptionsProvider OptionsProvider;

	internal readonly ParseOptions ParseOptions;

	internal readonly DriverStateTable StateTable;

	internal readonly SyntaxStore SyntaxStore;

	private readonly GeneratorDriverOptions _driverOptions;

	internal readonly IncrementalGeneratorOutputKind DisabledOutputs;

	internal readonly TimeSpan RunTime;

	internal readonly bool TrackIncrementalSteps;

	internal string? BaseDirectory => _driverOptions.BaseDirectory;

	internal SourceHashAlgorithm ChecksumAlgorithm => _driverOptions.ChecksumAlgorithm;

	internal GeneratorDriverState(ParseOptions parseOptions, AnalyzerConfigOptionsProvider optionsProvider, ImmutableArray<ISourceGenerator> sourceGenerators, ImmutableArray<IIncrementalGenerator> incrementalGenerators, ImmutableArray<AdditionalText> additionalTexts, ImmutableArray<GeneratorState> generatorStates, DriverStateTable stateTable, SyntaxStore syntaxStore, GeneratorDriverOptions driverOptions, TimeSpan runtime)
	{
		Generators = sourceGenerators;
		IncrementalGenerators = incrementalGenerators;
		GeneratorStates = generatorStates;
		AdditionalTexts = additionalTexts;
		ParseOptions = parseOptions;
		OptionsProvider = optionsProvider;
		StateTable = stateTable;
		SyntaxStore = syntaxStore;
		_driverOptions = driverOptions;
		DisabledOutputs = driverOptions.DisabledOutputs;
		TrackIncrementalSteps = driverOptions.TrackIncrementalGeneratorSteps;
		RunTime = runtime;
	}

	internal GeneratorDriverState With(ImmutableArray<ISourceGenerator>? sourceGenerators = null, ImmutableArray<IIncrementalGenerator>? incrementalGenerators = null, ImmutableArray<GeneratorState>? generatorStates = null, ImmutableArray<AdditionalText>? additionalTexts = null, DriverStateTable? stateTable = null, SyntaxStore? syntaxStore = null, ParseOptions? parseOptions = null, AnalyzerConfigOptionsProvider? optionsProvider = null, TimeSpan? runTime = null)
	{
		return new GeneratorDriverState(parseOptions ?? ParseOptions, optionsProvider ?? OptionsProvider, sourceGenerators ?? Generators, incrementalGenerators ?? IncrementalGenerators, additionalTexts ?? AdditionalTexts, generatorStates ?? GeneratorStates, stateTable ?? StateTable, syntaxStore ?? SyntaxStore, _driverOptions, runTime ?? RunTime);
	}
}
