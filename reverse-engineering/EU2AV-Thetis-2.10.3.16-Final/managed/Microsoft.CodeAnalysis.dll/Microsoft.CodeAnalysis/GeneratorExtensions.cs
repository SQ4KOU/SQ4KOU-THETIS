using System;

namespace Microsoft.CodeAnalysis;

public static class GeneratorExtensions
{
	public static Type GetGeneratorType(this ISourceGenerator generator)
	{
		if (generator is IncrementalGeneratorWrapper incrementalGeneratorWrapper)
		{
			return incrementalGeneratorWrapper.Generator.GetType();
		}
		return generator.GetType();
	}

	public static Type GetGeneratorType(this IIncrementalGenerator generator)
	{
		if (generator is SourceGeneratorAdaptor sourceGeneratorAdaptor)
		{
			return sourceGeneratorAdaptor.SourceGenerator.GetType();
		}
		return generator.GetType();
	}

	public static ISourceGenerator AsSourceGenerator(this IIncrementalGenerator incrementalGenerator)
	{
		if (incrementalGenerator is SourceGeneratorAdaptor sourceGeneratorAdaptor)
		{
			return sourceGeneratorAdaptor.SourceGenerator;
		}
		return new IncrementalGeneratorWrapper(incrementalGenerator);
	}

	public static IIncrementalGenerator AsIncrementalGenerator(this ISourceGenerator sourceGenerator)
	{
		if (sourceGenerator is IncrementalGeneratorWrapper incrementalGeneratorWrapper)
		{
			return incrementalGeneratorWrapper.Generator;
		}
		return new SourceGeneratorAdaptor(sourceGenerator, ".dummy");
	}
}
