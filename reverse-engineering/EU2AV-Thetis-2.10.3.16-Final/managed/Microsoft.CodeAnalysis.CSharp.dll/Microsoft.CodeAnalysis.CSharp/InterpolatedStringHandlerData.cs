using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct InterpolatedStringHandlerData
{
	public readonly TypeSymbol? BuilderType;

	public readonly BoundExpression Construction;

	public readonly bool UsesBoolReturns;

	public readonly ImmutableArray<BoundInterpolatedStringArgumentPlaceholder> ArgumentPlaceholders;

	public readonly ImmutableArray<ImmutableArray<(bool IsLiteral, bool HasAlignment, bool HasFormat)>> PositionInfo;

	public readonly BoundInterpolatedStringHandlerPlaceholder? ReceiverPlaceholder;

	public bool HasTrailingHandlerValidityParameter
	{
		get
		{
			if (ArgumentPlaceholders.Length > 0)
			{
				ImmutableArray<BoundInterpolatedStringArgumentPlaceholder> argumentPlaceholders = ArgumentPlaceholders;
				return argumentPlaceholders[argumentPlaceholders.Length - 1].ArgumentIndex == -3;
			}
			return false;
		}
	}

	public bool IsDefault => Construction == null;

	public InterpolatedStringHandlerData(TypeSymbol builderType, BoundExpression construction, bool usesBoolReturns, ImmutableArray<BoundInterpolatedStringArgumentPlaceholder> placeholders, ImmutableArray<ImmutableArray<(bool IsLiteral, bool HasAlignment, bool HasFormat)>> positionInfo, BoundInterpolatedStringHandlerPlaceholder receiverPlaceholder)
	{
		BuilderType = builderType;
		Construction = construction;
		UsesBoolReturns = usesBoolReturns;
		ArgumentPlaceholders = placeholders;
		PositionInfo = positionInfo;
		ReceiverPlaceholder = receiverPlaceholder;
	}

	public InterpolatedStringHandlerData(BoundExpression construction)
	{
		BuilderType = null;
		Construction = construction;
		UsesBoolReturns = false;
		ArgumentPlaceholders = default(ImmutableArray<BoundInterpolatedStringArgumentPlaceholder>);
		PositionInfo = default(ImmutableArray<ImmutableArray<(bool, bool, bool)>>);
		ReceiverPlaceholder = null;
	}
}
