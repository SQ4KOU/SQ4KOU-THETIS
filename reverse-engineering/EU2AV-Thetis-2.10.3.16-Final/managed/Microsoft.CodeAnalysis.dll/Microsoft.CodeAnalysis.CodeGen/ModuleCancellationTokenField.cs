using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class ModuleCancellationTokenField : SynthesizedStaticField
{
	public override ImmutableArray<byte> MappedData => default(ImmutableArray<byte>);

	public override bool IsReadOnly => false;

	public ModuleCancellationTokenField(INamedTypeDefinition containingType, ITypeReference type)
		: base("ModuleCancellationToken", containingType, type)
	{
	}
}
