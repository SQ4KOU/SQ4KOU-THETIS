using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSwitchStatement : BoundStatement
{
	public BoundExpression Expression { get; }

	public ImmutableArray<LocalSymbol> InnerLocals { get; }

	public ImmutableArray<MethodSymbol> InnerLocalFunctions { get; }

	public ImmutableArray<BoundSwitchSection> SwitchSections { get; }

	public BoundDecisionDag ReachabilityDecisionDag { get; }

	public BoundSwitchLabel? DefaultLabel { get; }

	public LabelSymbol BreakLabel { get; }

	public BoundDecisionDag GetDecisionDagForLowering(CSharpCompilation compilation)
	{
		BoundDecisionDag boundDecisionDag = ReachabilityDecisionDag;
		if (boundDecisionDag.ContainsAnySynthesizedNodes())
		{
			boundDecisionDag = DecisionDagBuilder.CreateDecisionDagForSwitchStatement(compilation, Syntax, Expression, SwitchSections, DefaultLabel?.Label ?? BreakLabel, BindingDiagnosticBag.Discarded, forLowering: true);
		}
		return boundDecisionDag;
	}

	public BoundSwitchStatement(SyntaxNode syntax, BoundExpression expression, ImmutableArray<LocalSymbol> innerLocals, ImmutableArray<MethodSymbol> innerLocalFunctions, ImmutableArray<BoundSwitchSection> switchSections, BoundDecisionDag reachabilityDecisionDag, BoundSwitchLabel? defaultLabel, LabelSymbol breakLabel, bool hasErrors = false)
		: base(BoundKind.SwitchStatement, syntax, hasErrors || expression.HasErrors() || switchSections.HasErrors() || reachabilityDecisionDag.HasErrors() || defaultLabel.HasErrors())
	{
		Expression = expression;
		InnerLocals = innerLocals;
		InnerLocalFunctions = innerLocalFunctions;
		SwitchSections = switchSections;
		ReachabilityDecisionDag = reachabilityDecisionDag;
		DefaultLabel = defaultLabel;
		BreakLabel = breakLabel;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSwitchStatement(this);
	}

	public BoundSwitchStatement Update(BoundExpression expression, ImmutableArray<LocalSymbol> innerLocals, ImmutableArray<MethodSymbol> innerLocalFunctions, ImmutableArray<BoundSwitchSection> switchSections, BoundDecisionDag reachabilityDecisionDag, BoundSwitchLabel? defaultLabel, LabelSymbol breakLabel)
	{
		if (expression != Expression || innerLocals != InnerLocals || innerLocalFunctions != InnerLocalFunctions || switchSections != SwitchSections || reachabilityDecisionDag != ReachabilityDecisionDag || defaultLabel != DefaultLabel || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(breakLabel, BreakLabel))
		{
			BoundSwitchStatement boundSwitchStatement = new BoundSwitchStatement(Syntax, expression, innerLocals, innerLocalFunctions, switchSections, reachabilityDecisionDag, defaultLabel, breakLabel, base.HasErrors);
			boundSwitchStatement.CopyAttributes(this);
			return boundSwitchStatement;
		}
		return this;
	}
}
