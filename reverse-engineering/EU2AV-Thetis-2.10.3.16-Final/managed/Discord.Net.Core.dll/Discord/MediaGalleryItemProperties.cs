namespace Discord;

public struct MediaGalleryItemProperties
{
	public const int MaxDescriptionLength = 256;

	public UnfurledMediaItemProperties Media { get; set; }

	public string Description { get; set; }

	public bool IsSpoiler { get; set; }

	public MediaGalleryItemProperties()
	{
		Media = default(UnfurledMediaItemProperties);
		Description = null;
		IsSpoiler = false;
	}

	public MediaGalleryItemProperties(UnfurledMediaItemProperties media, string description = null, bool isSpoiler = false)
	{
		Media = media;
		Description = description;
		IsSpoiler = isSpoiler;
	}
}
