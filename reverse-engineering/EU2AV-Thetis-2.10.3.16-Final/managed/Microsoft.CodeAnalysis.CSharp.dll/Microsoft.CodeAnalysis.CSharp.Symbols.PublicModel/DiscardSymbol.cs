using System;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;

internal sealed class DiscardSymbol : Symbol, IDiscardSymbol, ISymbol, IEquatable<ISymbol?>
{
	private readonly Microsoft.CodeAnalysis.CSharp.Symbols.DiscardSymbol _underlying;

	private ITypeSymbol? _lazyType;

	internal override Microsoft.CodeAnalysis.CSharp.Symbol UnderlyingSymbol => _underlying;

	ITypeSymbol IDiscardSymbol.Type
	{
		get
		{
			if (_lazyType == null)
			{
				Interlocked.CompareExchange(ref _lazyType, _underlying.TypeWithAnnotations.GetPublicSymbol(), null);
			}
			return _lazyType;
		}
	}

	Microsoft.CodeAnalysis.NullableAnnotation IDiscardSymbol.NullableAnnotation => _underlying.TypeWithAnnotations.ToPublicAnnotation();

	public DiscardSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.DiscardSymbol underlying)
	{
		_underlying = underlying;
	}

	protected override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDiscard(this);
	}

	protected override TResult? Accept<TResult>(SymbolVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitDiscard(this);
	}

	protected override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDiscard(this, argument);
	}
}
