using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class MediaRule : ConditionRule, IMediaRule, IConditionRule, IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
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

	public MediaList Media => base.Children.OfType<MediaList>().FirstOrDefault();

	internal MediaRule(StylesheetParser parser)
		: base(RuleType.Media, parser)
	{
		AppendChild(new MediaList(parser));
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string rules = formatter.Block(base.Rules);
		writer.Write(formatter.Rule("@media", Media.MediaText, rules));
	}
}
