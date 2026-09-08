using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDagIndexerEvaluation : BoundDagEvaluation
{
	public TypeSymbol IndexerType { get; }

	public BoundDagTemp LengthTemp { get; }

	public int Index { get; }

	public BoundExpression IndexerAccess { get; }

	public BoundListPatternReceiverPlaceholder ReceiverPlaceholder { get; }

	public BoundListPatternIndexPlaceholder ArgumentPlaceholder { get; }

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ Index;
	}

	public override bool IsEquivalentTo(BoundDagEvaluation obj)
	{
		if (base.IsEquivalentTo(obj))
		{
			return Index == ((BoundDagIndexerEvaluation)obj).Index;
		}
		return false;
	}

	public BoundDagIndexerEvaluation(SyntaxNode syntax, TypeSymbol indexerType, BoundDagTemp lengthTemp, int index, BoundExpression indexerAccess, BoundListPatternReceiverPlaceholder receiverPlaceholder, BoundListPatternIndexPlaceholder argumentPlaceholder, BoundDagTemp input, bool hasErrors = false)
		: base(BoundKind.DagIndexerEvaluation, syntax, input, hasErrors || lengthTemp.HasErrors() || indexerAccess.HasErrors() || receiverPlaceholder.HasErrors() || argumentPlaceholder.HasErrors() || input.HasErrors())
	{
		IndexerType = indexerType;
		LengthTemp = lengthTemp;
		Index = index;
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
		return visitor.VisitDagIndexerEvaluation(this);
	}

	public BoundDagIndexerEvaluation Update(TypeSymbol indexerType, BoundDagTemp lengthTemp, int index, BoundExpression indexerAccess, BoundListPatternReceiverPlaceholder receiverPlaceholder, BoundListPatternIndexPlaceholder argumentPlaceholder, BoundDagTemp input)
	{
		if (!TypeSymbol.Equals(indexerType, IndexerType, TypeCompareKind.ConsiderEverything) || lengthTemp != LengthTemp || index != Index || indexerAccess != IndexerAccess || receiverPlaceholder != ReceiverPlaceholder || argumentPlaceholder != ArgumentPlaceholder || input != base.Input)
		{
			BoundDagIndexerEvaluation boundDagIndexerEvaluation = new BoundDagIndexerEvaluation(Syntax, indexerType, lengthTemp, index, indexerAccess, receiverPlaceholder, argumentPlaceholder, input, base.HasErrors);
			boundDagIndexerEvaluation.CopyAttributes(this);
			return boundDagIndexerEvaluation;
		}
		return this;
	}
}
