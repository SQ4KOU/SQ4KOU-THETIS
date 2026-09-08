using System;

namespace Markdig.Extensions.EmphasisExtras;

[Flags]
public enum EmphasisExtraOptions
{
	Default = 0x1F,
	Strikethrough = 1,
	Subscript = 2,
	Superscript = 4,
	Inserted = 8,
	Marked = 0x10
}
