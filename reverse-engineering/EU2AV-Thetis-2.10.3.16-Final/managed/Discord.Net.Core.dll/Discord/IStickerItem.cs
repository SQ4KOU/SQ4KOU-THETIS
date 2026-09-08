namespace Discord;

public interface IStickerItem
{
	ulong Id { get; }

	string Name { get; }

	StickerFormatType Format { get; }
}
