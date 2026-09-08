using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum NullableContext
{
	Disabled = 0,
	WarningsEnabled = 1,
	AnnotationsEnabled = 2,
	Enabled = WarningsEnabled | AnnotationsEnabled,
	WarningsContextInherited = 4,
	AnnotationsContextInherited = 8,
	ContextInherited = WarningsContextInherited | AnnotationsContextInherited
}
