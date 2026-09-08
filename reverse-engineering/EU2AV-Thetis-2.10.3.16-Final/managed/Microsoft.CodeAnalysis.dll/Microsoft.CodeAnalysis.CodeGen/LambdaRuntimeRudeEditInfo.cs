using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal readonly struct LambdaRuntimeRudeEditInfo(DebugId lambdaId, RuntimeRudeEdit rudeEdit)
{
	public DebugId LambdaId { get; } = lambdaId;

	public RuntimeRudeEdit RudeEdit { get; } = rudeEdit;
}
