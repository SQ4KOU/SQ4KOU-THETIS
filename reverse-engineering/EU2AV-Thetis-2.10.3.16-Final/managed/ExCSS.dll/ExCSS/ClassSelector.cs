namespace ExCSS;

public sealed class ClassSelector : SelectorBase
{
	public string Class { get; }

	private ClassSelector(string name)
		: base(Priority.OneClass, "." + name)
	{
		Class = name;
	}

	public static ClassSelector Create(string name)
	{
		return new ClassSelector(name);
	}
}
