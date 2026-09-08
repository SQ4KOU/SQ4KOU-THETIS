namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseInterpolatedStringContentOperation : Operation, IInterpolatedStringContentOperation, IOperation
{
	protected BaseInterpolatedStringContentOperation(SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
	}
}
