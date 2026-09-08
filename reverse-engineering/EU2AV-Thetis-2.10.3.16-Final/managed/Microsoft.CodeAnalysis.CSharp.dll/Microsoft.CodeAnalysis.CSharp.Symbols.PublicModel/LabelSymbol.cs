using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;

internal sealed class LabelSymbol : Symbol, ILabelSymbol, ISymbol, IEquatable<ISymbol?>
{
	private readonly Microsoft.CodeAnalysis.CSharp.Symbols.LabelSymbol _underlying;

	internal override Microsoft.CodeAnalysis.CSharp.Symbol UnderlyingSymbol => _underlying;

	IMethodSymbol ILabelSymbol.ContainingMethod => _underlying.ContainingMethod.GetPublicSymbol();

	public LabelSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.LabelSymbol underlying)
	{
		_underlying = underlying;
	}

	protected override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitLabel(this);
	}

	protected override TResult? Accept<TResult>(SymbolVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitLabel(this);
	}

	protected override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLabel(this, argument);
	}
}
