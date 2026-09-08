using System;

namespace Discord;

public class ForumTagBuilder
{
	private string? _name;

	private IEmote? _emoji;

	private bool _moderated;

	private ulong? _id;

	public const int MaxNameLength = 20;

	public ulong? Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public string? Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != null && value.Length > 20)
			{
				throw new ArgumentException($"Name length must be less than or equal to {20}.", "Name");
			}
			_name = value;
		}
	}

	public IEmote? Emoji
	{
		get
		{
			return _emoji;
		}
		set
		{
			_emoji = value;
		}
	}

	public bool IsModerated
	{
		get
		{
			return _moderated;
		}
		set
		{
			_moderated = value;
		}
	}

	public ForumTagBuilder()
	{
	}

	public ForumTagBuilder(string name, ulong? id = null, bool isModerated = false)
	{
		Name = name;
		IsModerated = isModerated;
		Id = id;
	}

	public ForumTagBuilder(string name, ulong? id = null, bool isModerated = false, IEmote? emoji = null)
	{
		Name = name;
		Emoji = emoji;
		IsModerated = isModerated;
		Id = id;
	}

	public ForumTagBuilder(string name, ulong? id = null, bool isModerated = false, ulong? emoteId = null)
	{
		Name = name;
		if (emoteId.HasValue)
		{
			Emoji = new Emote(emoteId.Value, null, false);
		}
		IsModerated = isModerated;
		Id = id;
	}

	public ForumTagProperties Build()
	{
		if (_name == null)
		{
			throw new ArgumentNullException("Name", "Name must be set to build the tag.");
		}
		return new ForumTagProperties(_id, _name, _emoji, _moderated);
	}

	public ForumTagBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public ForumTagBuilder WithId(ulong? id)
	{
		Id = id;
		return this;
	}

	public ForumTagBuilder WithEmoji(IEmote? emoji)
	{
		Emoji = emoji;
		return this;
	}

	public ForumTagBuilder WithModerated(bool moderated)
	{
		IsModerated = moderated;
		return this;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		if (obj is ForumTagBuilder builder)
		{
			return Equals(builder);
		}
		return false;
	}

	public bool Equals(ForumTagBuilder? builder)
	{
		if ((object)builder != null && Id == builder.Id && Name == builder.Name && ((Emoji is Emoji emoji && builder.Emoji is Emoji obj && emoji.Equals(obj)) || (Emoji is Emote emote && builder.Emoji is Emote obj2 && emote.Equals(obj2))))
		{
			return IsModerated == builder.IsModerated;
		}
		return false;
	}

	public static bool operator ==(ForumTagBuilder? left, ForumTagBuilder? right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(ForumTagBuilder? left, ForumTagBuilder? right)
	{
		return !(left == right);
	}
}
