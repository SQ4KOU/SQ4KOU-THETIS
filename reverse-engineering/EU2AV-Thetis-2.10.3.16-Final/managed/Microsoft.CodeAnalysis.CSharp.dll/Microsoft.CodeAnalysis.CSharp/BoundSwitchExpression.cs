using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundSwitchExpression : BoundExpression
{
	public BoundExpression Expression { get; }

	public ImmutableArray<BoundSwitchExpressionArm> SwitchArms { get; }

	public BoundDecisionDag ReachabilityDecisionDag { get; }

	public LabelSymbol? DefaultLabel { get; }

	public bool ReportedNotExhaustive { get; }

	public BoundDecisionDag GetDecisionDagForLowering(CSharpCompilation compilation, out LabelSymbol? defaultLabel)
	{
		defaultLabel = DefaultLabel;
		BoundDecisionDag boundDecisionDag = ReachabilityDecisionDag;
		if (boundDecisionDag.ContainsAnySynthesizedNodes())
		{
			boundDecisionDag = DecisionDagBuilder.CreateDecisionDagForSwitchExpression(compilation, Syntax, Expression, SwitchArms, defaultLabel ?? (defaultLabel = new GeneratedLabelSymbol("default")), BindingDiagnosticBag.Discarded, forLowering: true);
		}
		return boundDecisionDag;
	}

	protected BoundSwitchExpression(BoundKind kind, SyntaxNode syntax, BoundExpression expression, ImmutableArray<BoundSwitchExpressionArm> switchArms, BoundDecisionDag reachabilityDecisionDag, LabelSymbol? defaultLabel, bool reportedNotExhaustive, TypeSymbol? type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		Expression = expression;
		SwitchArms = switchArms;
		ReachabilityDecisionDag = reachabilityDecisionDag;
		DefaultLabel = defaultLabel;
		ReportedNotExhaustive = reportedNotExhaustive;
	}
}
