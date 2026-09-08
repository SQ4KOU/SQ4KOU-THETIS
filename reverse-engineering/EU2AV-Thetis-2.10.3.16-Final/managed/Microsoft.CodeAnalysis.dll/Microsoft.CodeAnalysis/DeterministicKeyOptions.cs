using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum DeterministicKeyOptions
{
	Default = 0,
	IgnorePaths = 1,
	IgnoreToolVersions = 2
}
