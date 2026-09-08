using System;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorDriverOptions
{
	public readonly IncrementalGeneratorOutputKind DisabledOutputs;

	public readonly bool TrackIncrementalGeneratorSteps;

	public string? BaseDirectory { get; }

	internal SourceHashAlgorithm ChecksumAlgorithm { get; init; }

	public GeneratorDriverOptions(IncrementalGeneratorOutputKind disabledOutputs)
		: this(disabledOutputs, trackIncrementalGeneratorSteps: false)
	{
	}

	public GeneratorDriverOptions(IncrementalGeneratorOutputKind disabledOutputs, bool trackIncrementalGeneratorSteps)
	{
		BaseDirectory = null;
		ChecksumAlgorithm = SourceHashAlgorithm.None;
		DisabledOutputs = disabledOutputs;
		TrackIncrementalGeneratorSteps = trackIncrementalGeneratorSteps;
	}

	public GeneratorDriverOptions(IncrementalGeneratorOutputKind disabledOutputs = IncrementalGeneratorOutputKind.None, bool trackIncrementalGeneratorSteps = false, string? baseDirectory = null)
	{
		ChecksumAlgorithm = SourceHashAlgorithm.None;
		if (baseDirectory != null && !PathUtilities.IsAbsolute(baseDirectory))
		{
			throw new ArgumentException(CodeAnalysisResources.AbsolutePathExpected, "baseDirectory");
		}
		DisabledOutputs = disabledOutputs;
		TrackIncrementalGeneratorSteps = trackIncrementalGeneratorSteps;
		BaseDirectory = baseDirectory;
	}
}
