namespace ExCSS;

public interface IConditionRule : IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	string ConditionText { get; set; }
}
