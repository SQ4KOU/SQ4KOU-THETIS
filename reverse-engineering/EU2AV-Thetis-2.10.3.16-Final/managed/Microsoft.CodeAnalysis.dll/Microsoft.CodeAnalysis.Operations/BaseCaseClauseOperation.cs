namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseCaseClauseOperation : Operation, ICaseClauseOperation, IOperation
{
	public abstract CaseKind CaseKind { get; }

	public ILabelSymbol? Label { get; }

	protected BaseCaseClauseOperation(ILabelSymbol? label, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Label = label;
	}
}
