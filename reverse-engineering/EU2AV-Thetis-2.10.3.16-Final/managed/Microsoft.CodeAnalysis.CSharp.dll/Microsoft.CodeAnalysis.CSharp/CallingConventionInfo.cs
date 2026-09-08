using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct CallingConventionInfo(CallingConvention callKind, ImmutableHashSet<CustomModifier> unmanagedCallingConventionTypes)
{
	internal readonly CallingConvention CallKind = callKind;

	internal readonly ImmutableHashSet<CustomModifier>? UnmanagedCallingConventionTypes = unmanagedCallingConventionTypes;
}
