using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDagAssignmentEvaluation : BoundDagEvaluation
{
	public BoundDagTemp Target { get; }

	public override int GetHashCode()
	{
		return Hash.Combine(base.GetHashCode(), Target.GetHashCode());
	}

	public override bool IsEquivalentTo(BoundDagEvaluation obj)
	{
		if (base.IsEquivalentTo(obj))
		{
			return Target.Equals(((BoundDagAssignmentEvaluation)obj).Target);
		}
		return false;
	}

	public BoundDagAssignmentEvaluation(SyntaxNode syntax, BoundDagTemp target, BoundDagTemp input, bool hasErrors = false)
		: base(BoundKind.DagAssignmentEvaluation, syntax, input, hasErrors || target.HasErrors() || input.HasErrors())
	{
		Target = target;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDagAssignmentEvaluation(this);
	}

	public BoundDagAssignmentEvaluation Update(BoundDagTemp target, BoundDagTemp input)
	{
		if (target != Target || input != base.Input)
		{
			BoundDagAssignmentEvaluation boundDagAssignmentEvaluation = new BoundDagAssignmentEvaluation(Syntax, target, input, base.HasErrors);
			boundDagAssignmentEvaluation.CopyAttributes(this);
			return boundDagAssignmentEvaluation;
		}
		return this;
	}
}
