using System;
using System.Diagnostics.CodeAnalysis;

namespace Markdig.Extensions.MediaLinks;

public interface IHostProvider
{
	string? Class { get; }

	bool AllowFullScreen { get; }

	bool TryHandle(Uri mediaUri, bool isSchemaRelative, [NotNullWhen(true)] out string? iframeUrl);
}
