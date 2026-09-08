using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MethodArgumentInfo
{
	public readonly MethodSymbol Method;

	public readonly ImmutableArray<BoundExpression> Arguments;

	public readonly BitVector DefaultArguments;

	public readonly bool Expanded;

	public MethodArgumentInfo(MethodSymbol method, ImmutableArray<BoundExpression> arguments, BitVector defaultArguments, bool expanded)
	{
		Method = method;
		Arguments = arguments;
		DefaultArguments = defaultArguments;
		Expanded = expanded;
	}

	public static MethodArgumentInfo CreateParameterlessMethod(MethodSymbol method)
	{
		return new MethodArgumentInfo(method, ImmutableArray<BoundExpression>.Empty, default(BitVector), expanded: false);
	}
}
