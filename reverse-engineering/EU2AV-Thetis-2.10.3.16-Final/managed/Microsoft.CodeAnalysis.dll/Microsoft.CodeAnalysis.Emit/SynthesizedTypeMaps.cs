using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct SynthesizedTypeMaps(ImmutableSegmentedDictionary<AnonymousTypeKey, AnonymousTypeValue>? anonymousTypeMap, ImmutableSegmentedDictionary<SynthesizedDelegateKey, SynthesizedDelegateValue>? anonymousDelegates, ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>>? anonymousDelegatesWithIndexedNames)
{
	public static readonly SynthesizedTypeMaps Empty = new SynthesizedTypeMaps(null, null, null);

	public bool IsEmpty
	{
		get
		{
			if (AnonymousTypes.IsEmpty && AnonymousDelegates.IsEmpty)
			{
				return AnonymousDelegatesWithIndexedNames.IsEmpty;
			}
			return false;
		}
	}

	public ImmutableSegmentedDictionary<AnonymousTypeKey, AnonymousTypeValue> AnonymousTypes { get; } = anonymousTypeMap ?? ImmutableSegmentedDictionary<AnonymousTypeKey, AnonymousTypeValue>.Empty;

	public ImmutableSegmentedDictionary<SynthesizedDelegateKey, SynthesizedDelegateValue> AnonymousDelegates { get; } = anonymousDelegates ?? ImmutableSegmentedDictionary<SynthesizedDelegateKey, SynthesizedDelegateValue>.Empty;

	public ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>> AnonymousDelegatesWithIndexedNames { get; } = anonymousDelegatesWithIndexedNames ?? ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>>.Empty;
}
