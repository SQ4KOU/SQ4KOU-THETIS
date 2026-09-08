using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class InstrumentationPayloadRootField : SynthesizedStaticField
{
	public override ImmutableArray<byte> MappedData => default(ImmutableArray<byte>);

	public override bool IsReadOnly => true;

	internal InstrumentationPayloadRootField(INamedTypeDefinition containingType, int analysisIndex, ITypeReference payloadType)
		: base("PayloadRoot" + analysisIndex.ToString(CultureInfo.InvariantCulture), containingType, payloadType)
	{
	}
}
