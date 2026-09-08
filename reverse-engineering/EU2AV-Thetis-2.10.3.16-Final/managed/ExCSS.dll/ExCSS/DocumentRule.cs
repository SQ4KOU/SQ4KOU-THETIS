using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class DocumentRule : GroupingRule
{
	public string ConditionText
	{
		get
		{
			IEnumerable<string> values = Conditions.Select((IDocumentFunction m) => m.ToCss());
			return string.Join(", ", values);
		}
		set
		{
			List<DocumentFunction> obj = base.Parser.ParseDocumentRules(value) ?? throw new ParseException("Unable to parse document rules");
			Clear();
			foreach (DocumentFunction item in obj)
			{
				AppendChild(item);
			}
		}
	}

	public IEnumerable<IDocumentFunction> Conditions => base.Children.OfType<IDocumentFunction>();

	internal DocumentRule(StylesheetParser parser)
		: base(RuleType.Document, parser)
	{
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string rules = formatter.Block(base.Rules);
		writer.Write(formatter.Rule("@document", ConditionText, rules));
	}
}
