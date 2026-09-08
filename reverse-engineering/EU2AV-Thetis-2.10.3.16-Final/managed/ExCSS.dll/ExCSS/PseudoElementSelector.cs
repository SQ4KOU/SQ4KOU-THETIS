namespace ExCSS;

public sealed class PseudoElementSelector : SelectorBase
{
	public string Name { get; }

	private PseudoElementSelector(string name)
		: base(Priority.OneTag, PseudoElementNames.Separator + name)
	{
		Name = name;
	}

	public static ISelector Create(string name)
	{
		return new PseudoElementSelector(name);
	}
}
