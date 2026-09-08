using System.IO;

namespace ExCSS;

internal sealed class UnknownRule : Rule
{
	public string Name { get; }

	public UnknownRule(string name, StylesheetParser parser)
		: base(RuleType.Unknown, parser)
	{
		Name = name;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(base.StylesheetText?.Text);
	}
}
