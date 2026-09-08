using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord;

public class MediaGalleryBuilder : IMessageComponentBuilder
{
	public const int MaxItems = 10;

	private List<MediaGalleryItemProperties> _items = new List<MediaGalleryItemProperties>();

	public ComponentType Type => ComponentType.MediaGallery;

	public int? Id { get; set; }

	public List<MediaGalleryItemProperties> Items
	{
		get
		{
			return _items;
		}
		set
		{
			_items = value;
		}
	}

	public MediaGalleryBuilder()
	{
	}

	public MediaGalleryBuilder(params IEnumerable<MediaGalleryItemProperties> items)
	{
		Items = items?.ToList();
	}

	public MediaGalleryBuilder(MediaGalleryComponent mediaGallery)
	{
		Items = mediaGallery.Items.Select((MediaGalleryItem x) => x.ToProperties()).ToList();
		Id = mediaGallery.Id;
	}

	public MediaGalleryBuilder AddItem(MediaGalleryItemProperties item)
	{
		_items.Add(item);
		return this;
	}

	public MediaGalleryBuilder AddItem(string url, string description = null, bool isSpoiler = false)
	{
		_items.Add(new MediaGalleryItemProperties(new UnfurledMediaItemProperties(url), description, isSpoiler));
		return this;
	}

	public MediaGalleryBuilder AddItems(params IEnumerable<MediaGalleryItemProperties> items)
	{
		foreach (MediaGalleryItemProperties item in items)
		{
			_items.Add(item);
		}
		return this;
	}

	public MediaGalleryBuilder WithItems(IEnumerable<MediaGalleryItemProperties> items)
	{
		_items = items.ToList();
		return this;
	}

	public MediaGalleryComponent Build()
	{
		if (_items.Any((MediaGalleryItemProperties x) => (x.Description?.Length ?? 0) > 256))
		{
			throw new ArgumentException(string.Format("{0} description length cannot exceed {1} characters.", "MediaGalleryItemProperties", 256));
		}
		if (_items.Any(delegate(MediaGalleryItemProperties x)
		{
			string url = x.Media.Url;
			if (url == null || !url.StartsWith("http://"))
			{
				string url2 = x.Media.Url;
				if (url2 == null || !url2.StartsWith("https://"))
				{
					return !(x.Media.Url?.StartsWith("attachment://") ?? false);
				}
			}
			return false;
		}))
		{
			throw new ArgumentException("MediaGalleryItemProperties description must be a valid URL or attachment.");
		}
		int count = _items.Count;
		if ((count > 10 || count == 0) ? true : false)
		{
			throw new ArgumentOutOfRangeException("Items", $"Media gallery items count must be in range [1, {10}]");
		}
		return new MediaGalleryComponent(_items.Select((MediaGalleryItemProperties x) => new MediaGalleryItem(new UnfurledMediaItem(x.Media.Url), x.Description, x.IsSpoiler)).ToImmutableArray(), Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
