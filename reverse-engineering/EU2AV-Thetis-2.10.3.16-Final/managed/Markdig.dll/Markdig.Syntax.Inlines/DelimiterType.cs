using System;

namespace Markdig.Syntax.Inlines;

[Flags]
public enum DelimiterType : byte
{
	Undefined = 0,
	Open = 1,
	Close = 2
}
