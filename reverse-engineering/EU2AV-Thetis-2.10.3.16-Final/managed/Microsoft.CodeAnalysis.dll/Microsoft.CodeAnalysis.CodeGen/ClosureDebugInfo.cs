using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal readonly record struct ClosureDebugInfo(int SyntaxOffset, DebugId ClosureId)
{
	internal string GetDebuggerDisplay()
	{
		return $"({ClosureId} @{SyntaxOffset})";
	}
}
