namespace ExCSS;

internal abstract class GroupingRule : Rule, IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	public RuleList Rules { get; }

	IRuleList IGroupingRule.Rules => Rules;

	internal GroupingRule(RuleType type, StylesheetParser parser)
		: base(type, parser)
	{
		Rules = new RuleList(this);
	}

	public IRule AddNewRule(RuleType ruleType)
	{
		Rule rule = base.Parser.CreateRule(ruleType);
		Rules.Add(rule);
		return rule;
	}

	public int Insert(string ruleText, int index)
	{
		Rule rule = base.Parser.ParseRule(ruleText);
		Rules.Insert(index, rule);
		return index;
	}

	public void RemoveAt(int index)
	{
		Rules.RemoveAt(index);
	}
}
