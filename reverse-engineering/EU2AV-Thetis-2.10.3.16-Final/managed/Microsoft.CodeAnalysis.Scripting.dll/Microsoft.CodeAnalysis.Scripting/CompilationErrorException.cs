using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Scripting;

public sealed class CompilationErrorException : Exception
{
	public ImmutableArray<Diagnostic> Diagnostics { get; }

	public CompilationErrorException(string message, ImmutableArray<Diagnostic> diagnostics)
		: base(message)
	{
		if (diagnostics.IsDefault)
		{
			throw new ArgumentNullException("diagnostics");
		}
		Diagnostics = diagnostics;
	}
}
