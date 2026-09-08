namespace Discord;

public class ForumTagProperties : IForumTag
{
	public ulong? Id { get; }

	public string Name { get; }

	public IEmote? Emoji { get; }

	public bool IsModerated { get; }

	internal ForumTagProperties(ulong? id, string name, IEmote? emoji = null, bool isModerated = false)
	{
		Id = id;
		Name = name;
		Emoji = emoji;
		IsModerated = isModerated;
	}

	public override int GetHashCode()
	{
		return ((object)(Id, Name, Emoji, IsModerated)/*cast due to constrained. prefix*/).GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		if (obj is ForumTagProperties tag)
		{
			return Equals(tag);
		}
		return false;
	}

	public bool Equals(ForumTagProperties? tag)
	{
		if ((object)tag != null && Id == tag.Id && Name == tag.Name && ((Emoji is Emoji emoji && tag.Emoji is Emoji obj && emoji.Equals(obj)) || (Emoji is Emote emote && tag.Emoji is Emote obj2 && emote.Equals(obj2))))
		{
			return IsModerated == tag.IsModerated;
		}
		return false;
	}

	public static bool operator ==(ForumTagProperties? left, ForumTagProperties? right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(ForumTagProperties? left, ForumTagProperties? right)
	{
		return !(left == right);
	}
}
