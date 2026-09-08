using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundForEachDeconstructStep : BoundNode
{
	public BoundDeconstructionAssignmentOperator DeconstructionAssignment { get; }

	public BoundDeconstructValuePlaceholder TargetPlaceholder { get; }

	public BoundForEachDeconstructStep(SyntaxNode syntax, BoundDeconstructionAssignmentOperator deconstructionAssignment, BoundDeconstructValuePlaceholder targetPlaceholder, bool hasErrors = false)
		: base(BoundKind.ForEachDeconstructStep, syntax, hasErrors || deconstructionAssignment.HasErrors() || targetPlaceholder.HasErrors())
	{
		DeconstructionAssignment = deconstructionAssignment;
		TargetPlaceholder = targetPlaceholder;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitForEachDeconstructStep(this);
	}

	public BoundForEachDeconstructStep Update(BoundDeconstructionAssignmentOperator deconstructionAssignment, BoundDeconstructValuePlaceholder targetPlaceholder)
	{
		if (deconstructionAssignment != DeconstructionAssignment || targetPlaceholder != TargetPlaceholder)
		{
			BoundForEachDeconstructStep boundForEachDeconstructStep = new BoundForEachDeconstructStep(Syntax, deconstructionAssignment, targetPlaceholder, base.HasErrors);
			boundForEachDeconstructStep.CopyAttributes(this);
			return boundForEachDeconstructStep;
		}
		return this;
	}
}
