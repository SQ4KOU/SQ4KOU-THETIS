namespace ExCSS;

public sealed class Counter
{
	public string CounterIdentifier { get; }

	public string ListStyle { get; }

	public string DefinedSeparator { get; }

	public Counter(string identifier, string listStyle, string separator)
	{
		CounterIdentifier = identifier;
		ListStyle = listStyle;
		DefinedSeparator = separator;
	}
}
