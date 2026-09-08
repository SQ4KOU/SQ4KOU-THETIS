using System;
using System.IO;

namespace Microsoft.CodeAnalysis;

internal class StrongNameFileSystem
{
	internal static readonly StrongNameFileSystem Instance = new StrongNameFileSystem();

	internal readonly string? _signingTempPath;

	internal StrongNameFileSystem(string? signingTempPath = null)
	{
		_signingTempPath = signingTempPath;
	}

	internal virtual FileStream CreateFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
	{
		return new FileStream(filePath, fileMode, fileAccess, fileShare);
	}

	internal virtual byte[] ReadAllBytes(string fullPath)
	{
		return File.ReadAllBytes(fullPath);
	}

	internal virtual bool FileExists(string? fullPath)
	{
		return File.Exists(fullPath);
	}

	internal string? GetSigningTempPath()
	{
		return _signingTempPath;
	}

	public override int GetHashCode()
	{
		if (_signingTempPath == null)
		{
			return 0;
		}
		return StringComparer.Ordinal.GetHashCode(_signingTempPath);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as StrongNameFileSystem);
	}

	private bool Equals(StrongNameFileSystem? other)
	{
		if (this == other)
		{
			return true;
		}
		if (GetType() == other?.GetType())
		{
			return StringComparer.Ordinal.Equals(_signingTempPath, other?._signingTempPath);
		}
		return false;
	}
}
