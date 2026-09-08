using System;
using System.IO;

namespace Discord;

public struct Image : IDisposable
{
	private bool _isDisposed;

	public Stream Stream { get; }

	public Image(Stream stream)
	{
		_isDisposed = false;
		Stream = stream;
	}

	public Image(string path)
	{
		_isDisposed = false;
		Stream = File.OpenRead(path);
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			Stream?.Dispose();
			_isDisposed = true;
		}
	}
}
