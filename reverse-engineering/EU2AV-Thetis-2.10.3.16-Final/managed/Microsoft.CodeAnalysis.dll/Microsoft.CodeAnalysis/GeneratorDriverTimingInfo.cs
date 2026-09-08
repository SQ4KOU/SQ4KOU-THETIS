using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorDriverTimingInfo
{
	public TimeSpan ElapsedTime { get; }

	public ImmutableArray<GeneratorTimingInfo> GeneratorTimes { get; }

	internal GeneratorDriverTimingInfo(TimeSpan elapsedTime, ImmutableArray<GeneratorTimingInfo> generatorTimes)
	{
		ElapsedTime = elapsedTime;
		GeneratorTimes = generatorTimes;
	}
}
