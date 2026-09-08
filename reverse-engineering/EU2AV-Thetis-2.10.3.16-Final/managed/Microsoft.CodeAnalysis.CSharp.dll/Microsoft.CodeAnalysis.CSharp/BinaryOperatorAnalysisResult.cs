using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal readonly struct BinaryOperatorAnalysisResult : IMemberResolutionResultWithPriority<MethodSymbol>
{
	public readonly Conversion LeftConversion;

	public readonly Conversion RightConversion;

	public readonly BinaryOperatorSignature Signature;

	public readonly OperatorAnalysisResultKind Kind;

	public bool IsValid => Kind == OperatorAnalysisResultKind.Applicable;

	public bool HasValue => Kind != OperatorAnalysisResultKind.Undefined;

	bool IMemberResolutionResultWithPriority<MethodSymbol>.IsApplicable => IsValid;

	MethodSymbol IMemberResolutionResultWithPriority<MethodSymbol>.MemberWithPriority => Signature.Method;

	private BinaryOperatorAnalysisResult(OperatorAnalysisResultKind kind, BinaryOperatorSignature signature, Conversion leftConversion, Conversion rightConversion)
	{
		Kind = kind;
		Signature = signature;
		LeftConversion = leftConversion;
		RightConversion = rightConversion;
	}

	public override bool Equals(object obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/Operators/BinaryOperatorAnalysisResult.cs", 46);
	}

	public override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/Operators/BinaryOperatorAnalysisResult.cs", 52);
	}

	public static BinaryOperatorAnalysisResult Applicable(BinaryOperatorSignature signature, Conversion leftConversion, Conversion rightConversion)
	{
		return new BinaryOperatorAnalysisResult(OperatorAnalysisResultKind.Applicable, signature, leftConversion, rightConversion);
	}

	public static BinaryOperatorAnalysisResult Inapplicable(BinaryOperatorSignature signature, Conversion leftConversion, Conversion rightConversion)
	{
		return new BinaryOperatorAnalysisResult(OperatorAnalysisResultKind.Inapplicable, signature, leftConversion, rightConversion);
	}

	public BinaryOperatorAnalysisResult Worse()
	{
		return new BinaryOperatorAnalysisResult(OperatorAnalysisResultKind.Worse, Signature, LeftConversion, RightConversion);
	}

	private string GetDebuggerDisplay()
	{
		return $"{Signature.Kind} {Kind} {Signature.Method?.ToDisplayString()}";
	}
}
