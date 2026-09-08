using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundIsPatternExpression : BoundExpression
{
	public BoundExpression Expression { get; }

	public BoundPattern Pattern { get; }

	public bool IsNegated { get; }

	public BoundDecisionDag ReachabilityDecisionDag { get; }

	public LabelSymbol WhenTrueLabel { get; }

	public LabelSymbol WhenFalseLabel { get; }

	public BoundDecisionDag GetDecisionDagForLowering(CSharpCompilation compilation)
	{
		BoundDecisionDag boundDecisionDag = ReachabilityDecisionDag;
		if (boundDecisionDag.ContainsAnySynthesizedNodes())
		{
			Pattern.IsNegated(out BoundPattern innerPattern);
			boundDecisionDag = DecisionDagBuilder.CreateDecisionDagForIsPattern(compilation, Syntax, Expression, innerPattern, WhenTrueLabel, WhenFalseLabel, BindingDiagnosticBag.Discarded, forLowering: true);
		}
		return boundDecisionDag;
	}

	public BoundIsPatternExpression(SyntaxNode syntax, BoundExpression expression, BoundPattern pattern, bool isNegated, BoundDecisionDag reachabilityDecisionDag, LabelSymbol whenTrueLabel, LabelSymbol whenFalseLabel, TypeSymbol? type, bool hasErrors = false)
		: base(BoundKind.IsPatternExpression, syntax, type, hasErrors || expression.HasErrors() || pattern.HasErrors() || reachabilityDecisionDag.HasErrors())
	{
		Expression = expression;
		Pattern = pattern;
		IsNegated = isNegated;
		ReachabilityDecisionDag = reachabilityDecisionDag;
		WhenTrueLabel = whenTrueLabel;
		WhenFalseLabel = whenFalseLabel;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIsPatternExpression(this);
	}

	public BoundIsPatternExpression Update(BoundExpression expression, BoundPattern pattern, bool isNegated, BoundDecisionDag reachabilityDecisionDag, LabelSymbol whenTrueLabel, LabelSymbol whenFalseLabel, TypeSymbol? type)
	{
		if (expression != Expression || pattern != Pattern || isNegated != IsNegated || reachabilityDecisionDag != ReachabilityDecisionDag || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(whenTrueLabel, WhenTrueLabel) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(whenFalseLabel, WhenFalseLabel) || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundIsPatternExpression boundIsPatternExpression = new BoundIsPatternExpression(Syntax, expression, pattern, isNegated, reachabilityDecisionDag, whenTrueLabel, whenFalseLabel, type, base.HasErrors);
			boundIsPatternExpression.CopyAttributes(this);
			return boundIsPatternExpression;
		}
		return this;
	}
}
