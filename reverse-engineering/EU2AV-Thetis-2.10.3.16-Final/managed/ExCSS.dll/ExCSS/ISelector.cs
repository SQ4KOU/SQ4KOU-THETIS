namespace ExCSS;

public interface ISelector : IStylesheetNode, IStyleFormattable
{
	Priority Specificity { get; }

	string Text { get; }
}
