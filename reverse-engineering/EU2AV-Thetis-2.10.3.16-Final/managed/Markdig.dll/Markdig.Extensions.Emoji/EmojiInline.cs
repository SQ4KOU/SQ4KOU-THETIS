using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Emoji;

public class EmojiInline : LiteralInline
{
	public string? Match { get; set; }

	public EmojiInline()
	{
	}

	public EmojiInline(string content)
	{
		Content = new StringSlice(content);
	}
}
