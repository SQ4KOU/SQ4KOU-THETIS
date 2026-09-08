using System;

namespace Markdig.Extensions.AutoIdentifiers;

[Flags]
public enum AutoIdentifierOptions
{
	None = 0,
	Default = 3,
	AutoLink = 1,
	AllowOnlyAscii = 2,
	GitHub = 4
}
