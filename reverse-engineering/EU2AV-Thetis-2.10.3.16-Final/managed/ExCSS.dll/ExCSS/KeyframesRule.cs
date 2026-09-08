using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class KeyframesRule : Rule, IKeyframesRule, IRule, IStylesheetNode, IStyleFormattable
{
	public string Name { get; set; }

	public RuleList Rules { get; }

	IRuleList IKeyframesRule.Rules => Rules;

	internal KeyframesRule(StylesheetParser parser)
		: base(RuleType.Keyframes, parser)
	{
		Rules = new RuleList(this);
	}

	protected override void ReplaceWith(IRule rule)
	{
		Name = (rule as KeyframesRule)?.Name;
		base.ReplaceWith(rule);
	}

	public void Add(string ruleText)
	{
		KeyframeRule rule = base.Parser.ParseKeyframeRule(ruleText);
		Rules.Add(rule);
	}

	public void Remove(string key)
	{
		KeyframeRule rule = Find(key);
		Rules.Remove(rule);
	}

	public KeyframeRule Find(string key)
	{
		return Rules.OfType<KeyframeRule>().FirstOrDefault((KeyframeRule m) => key.Isi(m.KeyText));
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string rules = formatter.Block(Rules);
		writer.Write(formatter.Rule("@keyframes", Name, rules));
	}

	IKeyframeRule IKeyframesRule.Find(string key)
	{
		return Find(key);
	}
}
