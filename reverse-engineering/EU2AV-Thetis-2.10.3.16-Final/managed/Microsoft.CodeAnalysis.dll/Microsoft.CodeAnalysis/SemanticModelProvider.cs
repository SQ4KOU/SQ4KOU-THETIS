namespace Microsoft.CodeAnalysis;

internal abstract class SemanticModelProvider
{
	public abstract SemanticModel GetSemanticModel(SyntaxTree tree, Compilation compilation, SemanticModelOptions options = SemanticModelOptions.None);
}
