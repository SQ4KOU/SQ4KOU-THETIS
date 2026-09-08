using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum CompilerFeatureRequiredFeatures
{
	None = 0,
	RefStructs = 1,
	RequiredMembers = 2,
	UserDefinedCompoundAssignmentOperators = 4
}
