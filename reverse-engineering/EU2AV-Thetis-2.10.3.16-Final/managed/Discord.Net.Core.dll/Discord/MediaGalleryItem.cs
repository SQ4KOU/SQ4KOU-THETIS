namespace Discord;

public readonly struct MediaGalleryItem
{
	public UnfurledMediaItem Media { get; }

	public string Description { get; }

	public bool IsSpoiler { get; }

	public MediaGalleryItemProperties ToProperties()
	{
		return new MediaGalleryItemProperties(Media.ToProperties(), Description, IsSpoiler);
	}

	internal MediaGalleryItem(UnfurledMediaItem media, string description, bool? isSpoiler)
	{
		Media = media;
		Description = description;
		IsSpoiler = isSpoiler == true;
	}
}
