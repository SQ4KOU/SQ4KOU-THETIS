using System;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

public sealed class FileShadowCopy
{
	private readonly IDisposable _stream;

	public string OriginalPath { get; }

	public string FullPath { get; }

	internal FileShadowCopy(IDisposable stream, string originalPath, string fullPath)
	{
		_stream = stream;
		OriginalPath = originalPath;
		FullPath = fullPath;
	}

	internal void DisposeFileStream()
	{
		_stream.Dispose();
	}
}
