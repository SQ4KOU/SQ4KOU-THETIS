namespace Discord;

public class FileComponent : IMessageComponent
{
	public ComponentType Type => ComponentType.File;

	public int? Id { get; }

	public UnfurledMediaItem File { get; }

	public bool? IsSpoiler { get; }

	public FileComponentBuilder ToBuilder()
	{
		return new FileComponentBuilder(this);
	}

	internal FileComponent(UnfurledMediaItem file, bool? isSpoiler, int? id = null)
	{
		File = file;
		IsSpoiler = isSpoiler;
		Id = id;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
