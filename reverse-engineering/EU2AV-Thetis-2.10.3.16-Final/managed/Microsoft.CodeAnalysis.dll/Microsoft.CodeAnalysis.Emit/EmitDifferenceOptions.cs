namespace Microsoft.CodeAnalysis.Emit;

public readonly struct EmitDifferenceOptions
{
	public static readonly EmitDifferenceOptions Default = new EmitDifferenceOptions();

	public bool EmitFieldRva { get; init; } = false;

	public bool MethodImplEntriesSupported { get; init; } = true;

	public EmitDifferenceOptions()
	{
	}
}
