namespace Microsoft.CodeAnalysis.Emit;

internal static class InstrumentationKindExtensions
{
	internal const InstrumentationKind LocalStateTracing = (InstrumentationKind)(-1);

	internal static bool IsValid(this InstrumentationKind value)
	{
		if (value >= InstrumentationKind.None)
		{
			return value <= InstrumentationKind.ModuleCancellation;
		}
		return false;
	}
}
