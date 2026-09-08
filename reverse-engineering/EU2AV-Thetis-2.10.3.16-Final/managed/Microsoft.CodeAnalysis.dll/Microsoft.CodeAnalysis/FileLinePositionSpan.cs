using System;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DataContract]
public readonly struct FileLinePositionSpan : IEquatable<FileLinePositionSpan>
{
	[DataMember(Order = 0)]
	public string Path { get; }

	[DataMember(Order = 1)]
	public LinePositionSpan Span { get; }

	[DataMember(Order = 2)]
	public bool HasMappedPath { get; }

	public LinePosition StartLinePosition => Span.Start;

	public LinePosition EndLinePosition => Span.End;

	public bool IsValid => Path != null;

	public FileLinePositionSpan(string path, LinePosition start, LinePosition end)
		: this(path, new LinePositionSpan(start, end))
	{
	}

	public FileLinePositionSpan(string path, LinePositionSpan span)
	{
		Path = path ?? throw new ArgumentNullException("path");
		Span = span;
		HasMappedPath = false;
	}

	internal FileLinePositionSpan(string path, LinePositionSpan span, bool hasMappedPath)
	{
		Path = path;
		Span = span;
		HasMappedPath = hasMappedPath;
	}

	public bool Equals(FileLinePositionSpan other)
	{
		if (Span.Equals(other.Span) && HasMappedPath == other.HasMappedPath)
		{
			return string.Equals(Path, other.Path, StringComparison.Ordinal);
		}
		return false;
	}

	public override bool Equals(object? other)
	{
		if (other is FileLinePositionSpan other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Path, Hash.Combine(HasMappedPath, Span.GetHashCode()));
	}

	public override string ToString()
	{
		return Path + ": " + Span;
	}

	public static bool operator ==(FileLinePositionSpan left, FileLinePositionSpan right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FileLinePositionSpan left, FileLinePositionSpan right)
	{
		return !(left == right);
	}
}
