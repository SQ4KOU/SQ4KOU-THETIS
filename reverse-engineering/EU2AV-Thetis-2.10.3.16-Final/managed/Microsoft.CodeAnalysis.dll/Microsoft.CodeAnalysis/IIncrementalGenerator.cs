namespace Microsoft.CodeAnalysis;

public interface IIncrementalGenerator
{
	void Initialize(IncrementalGeneratorInitializationContext context);
}
