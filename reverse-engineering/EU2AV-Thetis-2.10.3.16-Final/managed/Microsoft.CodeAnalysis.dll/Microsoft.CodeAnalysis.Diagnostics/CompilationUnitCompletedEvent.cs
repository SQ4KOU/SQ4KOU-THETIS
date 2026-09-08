using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CompilationUnitCompletedEvent : CompilationEvent
{
	public SyntaxTree CompilationUnit { get; }

	public TextSpan? FilterSpan { get; }

	public CompilationUnitCompletedEvent(Compilation compilation, SyntaxTree compilationUnit, TextSpan? filterSpan = null)
		: base(compilation)
	{
		CompilationUnit = compilationUnit;
		FilterSpan = filterSpan;
	}

	public override string ToString()
	{
		return $"CompilationUnitCompletedEvent({CompilationUnit.FilePath}){FilterSpan}";
	}
}
