namespace Microsoft.CodeAnalysis.CodeGen;

internal readonly record struct LambdaDebugInfo
{
	public readonly int SyntaxOffset;

	public readonly int ClosureOrdinal;

	public readonly DebugId LambdaId;

	public const int StaticClosureOrdinal = -1;

	public const int ThisOnlyClosureOrdinal = -2;

	public const int MinClosureOrdinal = -2;

	public LambdaDebugInfo(int syntaxOffset, DebugId lambdaId, int closureOrdinal)
	{
		SyntaxOffset = syntaxOffset;
		ClosureOrdinal = closureOrdinal;
		LambdaId = lambdaId;
	}

	public override string ToString()
	{
		if (ClosureOrdinal != -1)
		{
			if (ClosureOrdinal != -2)
			{
				return $"({LambdaId} @{SyntaxOffset} in {ClosureOrdinal})";
			}
			return $"(#{LambdaId} @{SyntaxOffset}, this)";
		}
		return $"({LambdaId} @{SyntaxOffset}, static)";
	}
}
