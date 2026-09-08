using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum IncrementalGeneratorOutputKind
{
	None = 0,
	Source = 1,
	PostInit = 2,
	Implementation = 4,
	Host = 8
}
