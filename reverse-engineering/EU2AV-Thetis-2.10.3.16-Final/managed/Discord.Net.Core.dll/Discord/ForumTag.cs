using System;

namespace Discord;

public readonly struct ForumTag : ISnowflakeEntity, IEntity<ulong>, IForumTag
{
	public ulong Id { get; }

	public string Name { get; }

	public IEmote? Emoji { get; }

	public bool IsModerated { get; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	ulong? IForumTag.Id => Id;

	internal ForumTag(ulong id, string name, ulong? emojiId = null, string? emojiName = null, bool moderated = false)
	{
		if (emojiId.HasValue && emojiId.Value != 0L)
		{
			Emoji = new Emote(emojiId.Value, null, false);
		}
		else if (emojiName != null)
		{
			Emoji = new Emoji(emojiName);
		}
		else
		{
			Emoji = null;
		}
		Id = id;
		Name = name;
		IsModerated = moderated;
	}

	public override int GetHashCode()
	{
		return ((object)(Id, Name, Emoji, IsModerated)/*cast due to constrained. prefix*/).GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		if (obj is ForumTag tag)
		{
			return Equals(tag);
		}
		return false;
	}

	public bool Equals(ForumTag tag)
	{
		if (Id == tag.Id && Name == tag.Name && ((Emoji is Emoji emoji && tag.Emoji is Emoji obj && emoji.Equals(obj)) || (Emoji is Emote emote && tag.Emoji is Emote obj2 && emote.Equals(obj2))))
		{
			return IsModerated == tag.IsModerated;
		}
		return false;
	}

	public static bool operator ==(ForumTag? left, ForumTag? right)
	{
		return left?.Equals(right) ?? (!right.HasValue);
	}

	public static bool operator !=(ForumTag? left, ForumTag? right)
	{
		return !(left == right);
	}
}
