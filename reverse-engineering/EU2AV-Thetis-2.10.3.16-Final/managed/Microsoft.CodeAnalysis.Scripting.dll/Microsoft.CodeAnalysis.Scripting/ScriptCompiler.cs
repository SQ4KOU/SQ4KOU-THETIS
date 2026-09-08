using System;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Scripting;

internal abstract class ScriptCompiler
{
	public abstract DiagnosticFormatter DiagnosticFormatter { get; }

	public abstract StringComparer IdentifierComparer { get; }

	public abstract Compilation CreateSubmission(Script script);

	public abstract SyntaxTree ParseSubmission(SourceText text, ParseOptions parseOptions, CancellationToken cancellationToken);

	public abstract bool IsCompleteSubmission(SyntaxTree tree);
}
