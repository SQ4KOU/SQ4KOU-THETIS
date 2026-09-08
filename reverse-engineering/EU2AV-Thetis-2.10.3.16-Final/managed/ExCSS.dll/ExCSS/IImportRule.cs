namespace ExCSS;

public interface IImportRule : IRule, IStylesheetNode, IStyleFormattable
{
	string Href { get; set; }

	MediaList Media { get; }
}
