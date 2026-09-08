using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct SynthesizedDelegateValue(ITypeDefinition @delegate)
{
	public readonly ITypeDefinition Delegate = @delegate;
}
