using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class ContainerRule : ConditionRule, IContainerRule, IConditionRule, IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	public MediaList Media => base.Children.OfType<MediaList>().FirstOrDefault();

	public string Name { get; set; }

	public string ConditionText
	{
		get
		{
			return Media.MediaText;
		}
		set
		{
			Media.MediaText = value;
		}
	}

	internal ContainerRule(StylesheetParser parser)
		: base(RuleType.Container, parser)
	{
		AppendChild(new MediaList(parser));
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string rules = formatter.Block(base.Rules);
		string text = "@container";
		if (!string.IsNullOrEmpty(Name))
		{
			text = text + " " + Name;
		}
		writer.Write(formatter.Rule(text, ConditionText, rules));
	}
}
