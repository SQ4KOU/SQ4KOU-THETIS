using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public sealed class SyntaxAnnotation : IEquatable<SyntaxAnnotation?>
{
	private readonly long _id;

	private static long s_nextId;

	public static SyntaxAnnotation ElasticAnnotation { get; } = new SyntaxAnnotation();

	public string? Kind { get; }

	public string? Data { get; }

	public SyntaxAnnotation()
	{
		_id = Interlocked.Increment(ref s_nextId);
	}

	public SyntaxAnnotation(string? kind)
		: this()
	{
		Kind = kind;
	}

	public SyntaxAnnotation(string? kind, string? data)
		: this(kind)
	{
		Data = data;
	}

	private string GetDebuggerDisplay()
	{
		return string.Format("Annotation: Kind='{0}' Data='{1}'", Kind ?? "", Data ?? "");
	}

	public bool Equals(SyntaxAnnotation? other)
	{
		if ((object)other != null)
		{
			return _id == other._id;
		}
		return false;
	}

	public static bool operator ==(SyntaxAnnotation? left, SyntaxAnnotation? right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(SyntaxAnnotation? left, SyntaxAnnotation? right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SyntaxAnnotation);
	}

	public override int GetHashCode()
	{
		long id = _id;
		return id.GetHashCode();
	}
}
