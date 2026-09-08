using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct EncHoistedLocalMetadata(string name, ITypeReference type, SynthesizedLocalKind synthesizedKind)
{
	public readonly string Name = name;

	public readonly ITypeReference Type = type;

	public readonly SynthesizedLocalKind SynthesizedKind = synthesizedKind;
}
