namespace Microsoft.CodeAnalysis;

internal sealed class IncrementalGeneratorWrapper : ISourceGenerator
{
	internal IIncrementalGenerator Generator { get; }

	public IncrementalGeneratorWrapper(IIncrementalGenerator generator)
	{
		Generator = generator;
	}

	void ISourceGenerator.Execute(GeneratorExecutionContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/IncrementalWrapper.cs", 29);
	}

	void ISourceGenerator.Initialize(GeneratorInitializationContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/SourceGeneration/IncrementalWrapper.cs", 31);
	}
}
