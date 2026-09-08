using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Scripting;

public sealed class ScriptSourceResolver : SourceFileResolver, IEquatable<ScriptSourceResolver>
{
	public new static ScriptSourceResolver Default { get; } = new ScriptSourceResolver(ImmutableArray<string>.Empty, null);

	private ScriptSourceResolver(ImmutableArray<string> sourcePaths, string baseDirectory)
		: base(sourcePaths, baseDirectory)
	{
	}

	public ScriptSourceResolver WithSearchPaths(params string[] searchPaths)
	{
		return WithSearchPaths(searchPaths.AsImmutableOrEmpty());
	}

	public ScriptSourceResolver WithSearchPaths(IEnumerable<string> searchPaths)
	{
		return WithSearchPaths(searchPaths.AsImmutableOrEmpty());
	}

	public ScriptSourceResolver WithSearchPaths(ImmutableArray<string> searchPaths)
	{
		if (base.SearchPaths == searchPaths)
		{
			return this;
		}
		return new ScriptSourceResolver(ParameterValidationHelpers.ToImmutableArrayChecked(searchPaths, "searchPaths"), base.BaseDirectory);
	}

	public ScriptSourceResolver WithBaseDirectory(string baseDirectory)
	{
		if (base.BaseDirectory == baseDirectory)
		{
			return this;
		}
		return new ScriptSourceResolver(base.SearchPaths, baseDirectory);
	}

	public bool Equals(ScriptSourceResolver other)
	{
		return Equals((SourceFileResolver?)other);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}
}
