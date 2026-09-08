using System.Diagnostics;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal readonly struct SourceSpan(DebugSourceDocument document, int startLine, int startColumn, int endLine, int endColumn)
{
	public readonly int StartLine = startLine;

	public readonly int StartColumn = startColumn;

	public readonly int EndLine = endLine;

	public readonly int EndColumn = endColumn;

	public readonly DebugSourceDocument Document = document;

	public override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/SourceSpan.cs", 39);
	}

	public override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/SourceSpan.cs", 44);
	}

	private string GetDebuggerDisplay()
	{
		return $"({StartLine}, {StartColumn}) - ({EndLine}, {EndColumn})";
	}
}
