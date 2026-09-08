using System;

namespace Discord;

public class FileComponentBuilder : IMessageComponentBuilder
{
	public ComponentType Type => ComponentType.File;

	public int? Id { get; set; }

	public UnfurledMediaItemProperties File { get; set; }

	public bool? IsSpoiler { get; set; }

	public FileComponentBuilder()
	{
	}

	public FileComponentBuilder(UnfurledMediaItemProperties media, bool isSpoiler = false, int? id = null)
	{
		File = media;
		Id = id;
		IsSpoiler = isSpoiler;
	}

	public FileComponentBuilder(FileComponent file)
	{
		File = file.File.ToProperties();
		IsSpoiler = file.IsSpoiler;
		Id = file.Id;
	}

	public FileComponentBuilder WithFile(UnfurledMediaItemProperties file)
	{
		File = file;
		return this;
	}

	public FileComponentBuilder WithIsSpoiler(bool? isSpoiler)
	{
		IsSpoiler = isSpoiler;
		return this;
	}

	public FileComponent Build()
	{
		if (string.IsNullOrWhiteSpace(File.Url))
		{
			throw new InvalidOperationException("File URL must be set.");
		}
		if (!File.Url.StartsWith("attachment://"))
		{
			throw new InvalidOperationException("File URL must be an attachment URL.");
		}
		return new FileComponent(new UnfurledMediaItem(File.Url), IsSpoiler, Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
