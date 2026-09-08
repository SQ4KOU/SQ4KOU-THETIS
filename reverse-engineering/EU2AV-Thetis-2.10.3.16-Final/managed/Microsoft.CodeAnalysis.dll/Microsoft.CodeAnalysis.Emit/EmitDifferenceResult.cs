using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis.Emit;

public sealed class EmitDifferenceResult : EmitResult
{
	public EmitBaseline? Baseline { get; }

	public ImmutableArray<MethodDefinitionHandle> UpdatedMethods { get; }

	public ImmutableArray<TypeDefinitionHandle> ChangedTypes { get; }

	internal EmitDifferenceResult(bool success, ImmutableArray<Diagnostic> diagnostics, EmitBaseline? baseline, ImmutableArray<MethodDefinitionHandle> updatedMethods, ImmutableArray<TypeDefinitionHandle> changedTypes)
		: base(success, diagnostics)
	{
		Baseline = baseline;
		UpdatedMethods = updatedMethods;
		ChangedTypes = changedTypes;
	}
}
