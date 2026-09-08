using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct EncClosureMapValue(DebugId id, DebugId? parentId, ImmutableArray<string> structCaptures)
{
	public DebugId Id { get; } = id;

	public DebugId? ParentId { get; } = parentId;

	public ImmutableArray<string> StructCaptures { get; } = structCaptures;

	public bool IsStructClosure => !StructCaptures.IsDefault;

	public bool IsCompatibleWith(DebugId? parentClosureId, ImmutableArray<string> structCaptures)
	{
		DebugId? parentId = ParentId;
		DebugId? debugId = parentClosureId;
		if (parentId.HasValue == debugId.HasValue && (!parentId.HasValue || parentId.GetValueOrDefault() == debugId.GetValueOrDefault()) && StructCaptures.IsDefault == structCaptures.IsDefault)
		{
			if (!structCaptures.IsDefault)
			{
				return structCaptures.IsSubsetOf(StructCaptures);
			}
			return true;
		}
		return false;
	}
}
