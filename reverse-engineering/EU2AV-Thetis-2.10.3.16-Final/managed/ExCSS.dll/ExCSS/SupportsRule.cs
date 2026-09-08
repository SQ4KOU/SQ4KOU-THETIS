using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class SupportsRule : ConditionRule, ISupportsRule, IConditionRule, IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	public string ConditionText
	{
		get
		{
			return Condition.ToCss();
		}
		set
		{
			IConditionFunction conditionFunction = base.Parser.ParseCondition(value);
			Condition = conditionFunction ?? throw new ParseException("Unable to parse condition");
		}
	}

	public IConditionFunction Condition
	{
		get
		{
			return base.Children.OfType<IConditionFunction>().FirstOrDefault() ?? new EmptyCondition();
		}
		set
		{
			if (value != null)
			{
				RemoveChild(Condition);
				AppendChild(value);
			}
		}
	}

	internal SupportsRule(StylesheetParser parser)
		: base(RuleType.Supports, parser)
	{
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string rules = formatter.Block(base.Rules);
		writer.Write(formatter.Rule("@supports", ConditionText, rules));
	}
}
