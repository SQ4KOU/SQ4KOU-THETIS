using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis;

internal abstract class ErrorLogger
{
	public abstract void LogDiagnostic(Diagnostic diagnostic, SuppressionInfo? suppressionInfo);

	public abstract void AddAnalyzerDescriptorsAndExecutionTime(ImmutableArray<(DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info)> descriptors, double totalAnalyzerExecutionTime);
}
