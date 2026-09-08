namespace Microsoft.CodeAnalysis.Operations;

public interface ISlicePatternOperation : IPatternOperation, IOperation
{
	ISymbol? SliceSymbol { get; }

	IPatternOperation? Pattern { get; }
}
