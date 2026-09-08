using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Emit;

public readonly struct MethodInstrumentation
{
	internal static readonly MethodInstrumentation Empty = new MethodInstrumentation
	{
		Kinds = ImmutableArray<InstrumentationKind>.Empty
	};

	public ImmutableArray<InstrumentationKind> Kinds { get; init; }

	internal bool IsDefault => Kinds.IsDefault;

	internal bool IsEmpty => Kinds.IsEmpty;
}
