using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDagSliceEvaluation : BoundDagEvaluation
{
	public TypeSymbol SliceType { get; }

	public BoundDagTemp LengthTemp { get; }

	public int StartIndex { get; }

	public int EndIndex { get; }

	public BoundExpression IndexerAccess { get; }

	public BoundSlicePatternReceiverPlaceholder ReceiverPlaceholder { get; }

	public BoundSlicePatternRangePlaceholder ArgumentPlaceholder { get; }

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ StartIndex ^ EndIndex;
	}

	public override bool IsEquivalentTo(BoundDagEvaluation obj)
	{
		if (base.IsEquivalentTo(obj))
		{
			BoundDagSliceEvaluation boundDagSliceEvaluation = (BoundDagSliceEvaluation)obj;
			if (StartIndex == boundDagSliceEvaluation.StartIndex)
			{
				return EndIndex == boundDagSliceEvaluation.EndIndex;
			}
		}
		return false;
	}

	public BoundDagSliceEvaluation(SyntaxNode syntax, TypeSymbol sliceType, BoundDagTemp lengthTemp, int startIndex, int endIndex, BoundExpression indexerAccess, BoundSlicePatternReceiverPlaceholder receiverPlaceholder, BoundSlicePatternRangePlaceholder argumentPlaceholder, BoundDagTemp input, bool hasErrors = false)
		: base(BoundKind.DagSliceEvaluation, syntax, input, hasErrors || lengthTemp.HasErrors() || indexerAccess.HasErrors() || receiverPlaceholder.HasErrors() || argumentPlaceholder.HasErrors() || input.HasErrors())
	{
		SliceType = sliceType;
		LengthTemp = lengthTemp;
		StartIndex = startIndex;
		EndIndex = endIndex;
		IndexerAccess = indexerAccess;
		ReceiverPlaceholder = receiverPlaceholder;
		ArgumentPlaceholder = argumentPlaceholder;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDagSliceEvaluation(this);
	}

	public BoundDagSliceEvaluation Update(TypeSymbol sliceType, BoundDagTemp lengthTemp, int startIndex, int endIndex, BoundExpression indexerAccess, BoundSlicePatternReceiverPlaceholder receiverPlaceholder, BoundSlicePatternRangePlaceholder argumentPlaceholder, BoundDagTemp input)
	{
		if (!TypeSymbol.Equals(sliceType, SliceType, TypeCompareKind.ConsiderEverything) || lengthTemp != LengthTemp || startIndex != StartIndex || endIndex != EndIndex || indexerAccess != IndexerAccess || receiverPlaceholder != ReceiverPlaceholder || argumentPlaceholder != ArgumentPlaceholder || input != base.Input)
		{
			BoundDagSliceEvaluation boundDagSliceEvaluation = new BoundDagSliceEvaluation(Syntax, sliceType, lengthTemp, startIndex, endIndex, indexerAccess, receiverPlaceholder, argumentPlaceholder, input, base.HasErrors);
			boundDagSliceEvaluation.CopyAttributes(this);
			return boundDagSliceEvaluation;
		}
		return this;
	}
}
