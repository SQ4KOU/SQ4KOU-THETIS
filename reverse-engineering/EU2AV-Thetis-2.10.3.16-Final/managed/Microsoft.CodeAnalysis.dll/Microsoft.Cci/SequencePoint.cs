using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Microsoft.Cci;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal readonly struct SequencePoint(DebugSourceDocument document, int offset, int startLine, ushort startColumn, int endLine, ushort endColumn)
{
	public const int HiddenLine = 16707566;

	public readonly int Offset = offset;

	public readonly int StartLine = startLine;

	public readonly int EndLine = endLine;

	public readonly ushort StartColumn = startColumn;

	public readonly ushort EndColumn = endColumn;

	public readonly DebugSourceDocument Document = document;

	public bool IsHidden => StartLine == 16707566;

	public override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/SequencePoint.cs", 47);
	}

	public override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/SequencePoint.cs", 52);
	}

	private string GetDebuggerDisplay()
	{
		if (!IsHidden)
		{
			return $"{Offset}: ({StartLine}, {StartColumn}) - ({EndLine}, {EndColumn})";
		}
		return "<hidden>";
	}
}
