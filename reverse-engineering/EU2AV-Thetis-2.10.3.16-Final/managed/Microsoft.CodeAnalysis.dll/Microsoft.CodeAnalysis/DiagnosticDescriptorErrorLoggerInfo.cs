using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal readonly record struct DiagnosticDescriptorErrorLoggerInfo(double ExecutionTime, int ExecutionPercentage, ImmutableHashSet<ReportDiagnostic>? EffectiveSeverities, bool HasAnyExternalSuppression);
