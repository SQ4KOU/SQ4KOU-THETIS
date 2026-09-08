namespace Microsoft.CodeAnalysis;

public enum IncrementalStepRunReason
{
	New,
	Modified,
	Unchanged,
	Cached,
	Removed
}
