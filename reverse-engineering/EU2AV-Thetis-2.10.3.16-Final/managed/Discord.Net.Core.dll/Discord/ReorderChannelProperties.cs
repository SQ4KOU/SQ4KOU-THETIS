namespace Discord;

public class ReorderChannelProperties
{
	public ulong Id { get; }

	public int Position { get; }

	public ReorderChannelProperties(ulong id, int position)
	{
		Id = id;
		Position = position;
	}
}
