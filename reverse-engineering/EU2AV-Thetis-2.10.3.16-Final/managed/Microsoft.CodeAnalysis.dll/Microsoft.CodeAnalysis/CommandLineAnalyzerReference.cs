using System;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{FilePath,nq}")]
public readonly struct CommandLineAnalyzerReference(string path) : IEquatable<CommandLineAnalyzerReference>
{
	private readonly string _path = path;

	public string FilePath => _path;

	public override bool Equals(object? obj)
	{
		if (obj is CommandLineAnalyzerReference)
		{
			return base.Equals((object?)(CommandLineAnalyzerReference)obj);
		}
		return false;
	}

	public bool Equals(CommandLineAnalyzerReference other)
	{
		return _path == other._path;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_path, 0);
	}
}
