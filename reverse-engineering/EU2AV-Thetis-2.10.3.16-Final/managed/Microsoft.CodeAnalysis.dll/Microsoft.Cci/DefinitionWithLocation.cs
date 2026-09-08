using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace Microsoft.Cci;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal readonly struct DefinitionWithLocation(IDefinition definition, int startLine, int startColumn, int endLine, int endColumn) : IEquatable<DefinitionWithLocation>
{
	public readonly IDefinition Definition = definition;

	public readonly int StartLine = startLine;

	public readonly int StartColumn = startColumn;

	public readonly int EndLine = endLine;

	public readonly int EndColumn = endColumn;

	private string GetDebuggerDisplay()
	{
		return $"{Definition} => ({StartLine},{StartColumn}) - ({EndLine}, {EndColumn})";
	}

	public override bool Equals(object? obj)
	{
		if (obj is DefinitionWithLocation other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(DefinitionWithLocation other)
	{
		if (Definition == other.Definition && StartLine == other.StartLine && StartColumn == other.StartColumn && EndLine == other.EndLine)
		{
			return EndColumn == other.EndColumn;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = RuntimeHelpers.GetHashCode(Definition);
		int startLine = StartLine;
		return Hash.Combine(hashCode, startLine.GetHashCode());
	}
}
