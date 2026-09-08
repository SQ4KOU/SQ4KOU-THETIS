using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConvertedSwitchExpression : BoundSwitchExpression
{
	public new TypeSymbol Type => base.Type;

	public TypeSymbol? NaturalTypeOpt { get; }

	public bool WasTargetTyped { get; }

	public BoundConvertedSwitchExpression(SyntaxNode syntax, TypeSymbol? naturalTypeOpt, bool wasTargetTyped, BoundExpression expression, ImmutableArray<BoundSwitchExpressionArm> switchArms, BoundDecisionDag reachabilityDecisionDag, LabelSymbol? defaultLabel, bool reportedNotExhaustive, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ConvertedSwitchExpression, syntax, expression, switchArms, reachabilityDecisionDag, defaultLabel, reportedNotExhaustive, type, hasErrors || expression.HasErrors() || switchArms.HasErrors() || reachabilityDecisionDag.HasErrors())
	{
		NaturalTypeOpt = naturalTypeOpt;
		WasTargetTyped = wasTargetTyped;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConvertedSwitchExpression(this);
	}

	public BoundConvertedSwitchExpression Update(TypeSymbol? naturalTypeOpt, bool wasTargetTyped, BoundExpression expression, ImmutableArray<BoundSwitchExpressionArm> switchArms, BoundDecisionDag reachabilityDecisionDag, LabelSymbol? defaultLabel, bool reportedNotExhaustive, TypeSymbol type)
	{
		if (!TypeSymbol.Equals(naturalTypeOpt, NaturalTypeOpt, TypeCompareKind.ConsiderEverything) || wasTargetTyped != WasTargetTyped || expression != base.Expression || switchArms != base.SwitchArms || reachabilityDecisionDag != base.ReachabilityDecisionDag || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(defaultLabel, base.DefaultLabel) || reportedNotExhaustive != base.ReportedNotExhaustive || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundConvertedSwitchExpression boundConvertedSwitchExpression = new BoundConvertedSwitchExpression(Syntax, naturalTypeOpt, wasTargetTyped, expression, switchArms, reachabilityDecisionDag, defaultLabel, reportedNotExhaustive, type, base.HasErrors);
			boundConvertedSwitchExpression.CopyAttributes(this);
			return boundConvertedSwitchExpression;
		}
		return this;
	}
}
