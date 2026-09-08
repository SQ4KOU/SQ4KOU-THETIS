using System;

namespace Discord;

public class ThumbnailBuilder : IMessageComponentBuilder
{
	public const int MaxDescriptionLength = 1024;

	public ComponentType Type => ComponentType.Thumbnail;

	public int? Id { get; set; }

	public UnfurledMediaItemProperties Media { get; set; }

	public string Description { get; set; }

	public bool IsSpoiler { get; set; }

	public ThumbnailBuilder()
	{
	}

	public ThumbnailBuilder(UnfurledMediaItemProperties media, string description = null, bool isSpoiler = false)
	{
		Media = media;
		Description = description;
		IsSpoiler = isSpoiler;
	}

	public ThumbnailBuilder(ThumbnailComponent component)
	{
		Media = component.Media.ToProperties();
		Description = component.Description;
		IsSpoiler = component.IsSpoiler;
		Id = component.Id;
	}

	public ThumbnailBuilder WithMedia(UnfurledMediaItemProperties media)
	{
		Media = media;
		return this;
	}

	public ThumbnailBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	public ThumbnailBuilder WithSpoiler(bool isSpoiler)
	{
		IsSpoiler = isSpoiler;
		return this;
	}

	public ThumbnailComponent Build()
	{
		if (Description != null && Description.Length > 1024)
		{
			throw new ArgumentException($"Description length must be less than or equal to {1024}.", "Description");
		}
		return new ThumbnailComponent(Id, new UnfurledMediaItem(Media.Url), Description, IsSpoiler);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
