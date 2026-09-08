namespace ExCSS;

public sealed class TypeSelector : SelectorBase
{
	public string Name { get; }

	private TypeSelector(string name)
		: base(Priority.OneTag, name)
	{
		Name = name;
	}

	public static TypeSelector Create(string name)
	{
		return new TypeSelector(name);
	}
}
