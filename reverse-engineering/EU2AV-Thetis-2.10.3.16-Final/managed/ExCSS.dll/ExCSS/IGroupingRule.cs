namespace ExCSS;

public interface IGroupingRule : IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	IRuleList Rules { get; }

	int Insert(string rule, int index);

	void RemoveAt(int index);
}
