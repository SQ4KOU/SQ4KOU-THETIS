using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

public sealed class PayloadOptions
{
	public TypeNameParseOptions? TypeNameParseOptions { get; set; }

	public bool UndoTruncatedTypeNames { get; set; }
}
