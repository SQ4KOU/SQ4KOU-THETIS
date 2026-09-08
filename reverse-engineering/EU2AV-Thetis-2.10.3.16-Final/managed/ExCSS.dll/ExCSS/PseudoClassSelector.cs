namespace ExCSS;

public sealed class PseudoClassSelector : SelectorBase
{
	public string Class { get; }

	private PseudoClassSelector(string name)
		: base(Priority.OneClass, PseudoClassNames.Separator + name)
	{
		Class = name;
	}

	public static ISelector Create(string name)
	{
		return new PseudoClassSelector(name);
	}
}
