using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class CachedArrayField : SynthesizedStaticField
{
	public override ImmutableArray<byte> MappedData => default(ImmutableArray<byte>);

	public override bool IsReadOnly => false;

	internal CachedArrayField(string name, INamedTypeDefinition containingType, ITypeReference type)
		: base(name, containingType, type)
	{
	}
}
