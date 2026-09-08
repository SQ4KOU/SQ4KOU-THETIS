using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class ImportRule : Rule, IImportRule, IRule, IStylesheetNode, IStyleFormattable
{
	private Stylesheet _stylesheet;

	public string Href { get; set; }

	public MediaList Media => base.Children.OfType<MediaList>().FirstOrDefault();

	public Stylesheet Sheet => _stylesheet;

	internal ImportRule(StylesheetParser parser)
		: base(RuleType.Import, parser)
	{
		AppendChild(new MediaList(parser));
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string mediaText = Media.MediaText;
		string text = (string.IsNullOrEmpty(mediaText) ? string.Empty : " ");
		string value = Href.StylesheetUrl() + text + mediaText;
		writer.Write(formatter.Rule("@import", value));
	}

	protected override void ReplaceWith(IRule rule)
	{
		Href = (rule as ImportRule)?.Href;
		_stylesheet = null;
		base.ReplaceWith(rule);
	}
}
