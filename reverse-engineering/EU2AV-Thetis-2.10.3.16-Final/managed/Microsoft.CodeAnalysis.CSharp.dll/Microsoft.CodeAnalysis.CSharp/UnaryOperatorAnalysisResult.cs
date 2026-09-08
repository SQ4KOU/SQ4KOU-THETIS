using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal readonly struct UnaryOperatorAnalysisResult : IMemberResolutionResultWithPriority<MethodSymbol>
{
	public readonly UnaryOperatorSignature Signature;

	public readonly Conversion Conversion;

	public readonly OperatorAnalysisResultKind Kind;

	public bool IsValid => Kind == OperatorAnalysisResultKind.Applicable;

	public bool HasValue => Kind != OperatorAnalysisResultKind.Undefined;

	bool IMemberResolutionResultWithPriority<MethodSymbol>.IsApplicable => IsValid;

	MethodSymbol IMemberResolutionResultWithPriority<MethodSymbol>.MemberWithPriority => Signature.Method;

	private UnaryOperatorAnalysisResult(OperatorAnalysisResultKind kind, UnaryOperatorSignature signature, Conversion conversion)
	{
		Kind = kind;
		Signature = signature;
		Conversion = conversion;
	}

	public static UnaryOperatorAnalysisResult Applicable(UnaryOperatorSignature signature, Conversion conversion)
	{
		return new UnaryOperatorAnalysisResult(OperatorAnalysisResultKind.Applicable, signature, conversion);
	}

	public static UnaryOperatorAnalysisResult Inapplicable(UnaryOperatorSignature signature, Conversion conversion)
	{
		return new UnaryOperatorAnalysisResult(OperatorAnalysisResultKind.Inapplicable, signature, conversion);
	}

	public UnaryOperatorAnalysisResult Worse()
	{
		return new UnaryOperatorAnalysisResult(OperatorAnalysisResultKind.Worse, Signature, Conversion);
	}

	private string GetDebuggerDisplay()
	{
		return $"{Signature.Kind} {Kind} {Signature.Method?.ToDisplayString()}";
	}
}
