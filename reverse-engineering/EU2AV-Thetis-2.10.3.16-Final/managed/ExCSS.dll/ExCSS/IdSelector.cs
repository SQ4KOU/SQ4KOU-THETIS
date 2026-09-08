namespace ExCSS;

public sealed class IdSelector : SelectorBase
{
	public string Id { get; }

	private IdSelector(string name)
		: base(Priority.OneId, "#" + name)
	{
		Id = name;
	}

	public static IdSelector Create(string name)
	{
		return new IdSelector(name);
	}
}
