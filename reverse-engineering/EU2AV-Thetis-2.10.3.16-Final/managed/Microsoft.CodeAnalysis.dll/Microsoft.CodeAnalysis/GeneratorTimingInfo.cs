using System;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorTimingInfo
{
	public ISourceGenerator Generator { get; }

	public TimeSpan ElapsedTime { get; }

	internal GeneratorTimingInfo(ISourceGenerator generator, TimeSpan elapsedTime)
	{
		Generator = generator;
		ElapsedTime = elapsedTime;
	}
}
