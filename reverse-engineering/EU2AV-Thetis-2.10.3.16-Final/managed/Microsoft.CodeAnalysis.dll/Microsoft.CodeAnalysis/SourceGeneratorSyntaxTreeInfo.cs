using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum SourceGeneratorSyntaxTreeInfo
{
	NotComputedYet = 0,
	None = 1,
	ContainsGlobalAliases = 2,
	ContainsAttributeList = 4,
	ContainsGlobalAliasesOrAttributeList = ContainsGlobalAliases | ContainsAttributeList
}
