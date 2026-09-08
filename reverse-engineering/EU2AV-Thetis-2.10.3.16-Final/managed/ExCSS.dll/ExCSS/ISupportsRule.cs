namespace ExCSS;

public interface ISupportsRule : IConditionRule, IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	IConditionFunction Condition { get; }
}
