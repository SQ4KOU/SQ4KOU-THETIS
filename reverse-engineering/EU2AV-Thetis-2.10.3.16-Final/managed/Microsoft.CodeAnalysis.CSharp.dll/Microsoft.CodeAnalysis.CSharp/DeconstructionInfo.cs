using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

public readonly struct DeconstructionInfo
{
	private readonly Conversion _conversion;

	public IMethodSymbol? Method
	{
		get
		{
			if (_conversion.Kind != ConversionKind.Deconstruction)
			{
				return null;
			}
			return _conversion.MethodSymbol;
		}
	}

	public Conversion? Conversion
	{
		get
		{
			if (_conversion.Kind != ConversionKind.Deconstruction)
			{
				return _conversion;
			}
			return null;
		}
	}

	public ImmutableArray<DeconstructionInfo> Nested
	{
		get
		{
			if (_conversion.Kind != ConversionKind.Deconstruction)
			{
				return ImmutableArray<DeconstructionInfo>.Empty;
			}
			ImmutableArray<(BoundValuePlaceholder, BoundExpression)> deconstructConversionInfo = _conversion.DeconstructConversionInfo;
			if (!deconstructConversionInfo.IsDefault)
			{
				return deconstructConversionInfo.SelectAsArray<(BoundValuePlaceholder, BoundExpression), DeconstructionInfo>(((BoundValuePlaceholder placeholder, BoundExpression conversion) c) => new DeconstructionInfo(BoundNode.GetConversion(c.conversion, c.placeholder)));
			}
			return ImmutableArray<DeconstructionInfo>.Empty;
		}
	}

	internal DeconstructionInfo(Conversion conversion)
	{
		_conversion = conversion;
	}
}
