using System;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis;

[ImplementationIsObsolete("https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md")]
public interface ISourceGenerator
{
	[Obsolete("ISourceGenerator is deprecated and should not be implemented. Please implement IIncrementalGenerator instead. See https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md.")]
	void Initialize(GeneratorInitializationContext context);

	[Obsolete("ISourceGenerator is deprecated and should not be implemented. Please implement IIncrementalGenerator instead. See https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md.")]
	void Execute(GeneratorExecutionContext context);
}
