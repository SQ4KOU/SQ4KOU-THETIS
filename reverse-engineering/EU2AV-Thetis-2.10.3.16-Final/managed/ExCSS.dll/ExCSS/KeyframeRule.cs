using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class KeyframeRule : Rule, IKeyframeRule, IRule, IStylesheetNode, IStyleFormattable
{
	public string KeyText
	{
		get
		{
			return Key.Text;
		}
		set
		{
			KeyframeSelector keyframeSelector = base.Parser.ParseKeyframeSelector(value);
			Key = keyframeSelector ?? throw new ParseException("Unable to parse keyframe selector");
		}
	}

	public KeyframeSelector Key
	{
		get
		{
			return base.Children.OfType<KeyframeSelector>().FirstOrDefault();
		}
		set
		{
			ReplaceSingle(Key, value);
		}
	}

	public StyleDeclaration Style => base.Children.OfType<StyleDeclaration>().FirstOrDefault();

	internal KeyframeRule(StylesheetParser parser)
		: base(RuleType.Keyframe, parser)
	{
		AppendChild(new StyleDeclaration(this));
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(formatter.Style(KeyText, Style));
	}
}
