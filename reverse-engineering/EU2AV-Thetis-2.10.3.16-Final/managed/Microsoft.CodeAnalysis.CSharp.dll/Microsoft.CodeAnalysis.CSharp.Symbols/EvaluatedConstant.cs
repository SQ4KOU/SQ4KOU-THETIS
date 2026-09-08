namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class EvaluatedConstant
{
	public readonly ConstantValue Value;

	public readonly ReadOnlyBindingDiagnostic<AssemblySymbol> Diagnostics;

	public EvaluatedConstant(ConstantValue value, ReadOnlyBindingDiagnostic<AssemblySymbol> diagnostics)
	{
		Value = value;
		Diagnostics = diagnostics.NullToEmpty();
	}
}
