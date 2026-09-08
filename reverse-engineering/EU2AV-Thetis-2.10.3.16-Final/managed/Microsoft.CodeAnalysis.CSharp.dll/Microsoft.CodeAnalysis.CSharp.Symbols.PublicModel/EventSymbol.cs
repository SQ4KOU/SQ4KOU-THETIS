using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;

internal sealed class EventSymbol : Symbol, IEventSymbol, ISymbol, IEquatable<ISymbol?>
{
	private readonly Microsoft.CodeAnalysis.CSharp.Symbols.EventSymbol _underlying;

	private ITypeSymbol? _lazyType;

	internal override Microsoft.CodeAnalysis.CSharp.Symbol UnderlyingSymbol => _underlying;

	internal Microsoft.CodeAnalysis.CSharp.Symbols.EventSymbol UnderlyingEventSymbol => _underlying;

	ITypeSymbol IEventSymbol.Type
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

	Microsoft.CodeAnalysis.NullableAnnotation IEventSymbol.NullableAnnotation => _underlying.TypeWithAnnotations.ToPublicAnnotation();

	IMethodSymbol? IEventSymbol.AddMethod => _underlying.AddMethod.GetPublicSymbol();

	IMethodSymbol? IEventSymbol.RemoveMethod => _underlying.RemoveMethod.GetPublicSymbol();

	IMethodSymbol? IEventSymbol.RaiseMethod => null;

	IEventSymbol IEventSymbol.OriginalDefinition => _underlying.OriginalDefinition.GetPublicSymbol();

	IEventSymbol? IEventSymbol.OverriddenEvent => _underlying.OverriddenEvent.GetPublicSymbol();

	ImmutableArray<IEventSymbol> IEventSymbol.ExplicitInterfaceImplementations => _underlying.ExplicitInterfaceImplementations.GetPublicSymbols();

	bool IEventSymbol.IsWindowsRuntimeEvent => _underlying.IsWindowsRuntimeEvent;

	IEventSymbol? IEventSymbol.PartialDefinitionPart => _underlying.PartialDefinitionPart.GetPublicSymbol();

	IEventSymbol? IEventSymbol.PartialImplementationPart => _underlying.PartialImplementationPart.GetPublicSymbol();

	bool IEventSymbol.IsPartialDefinition => _underlying.IsPartialDefinition;

	public EventSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.EventSymbol underlying)
	{
		_underlying = underlying;
	}

	protected override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitEvent(this);
	}

	protected override TResult? Accept<TResult>(SymbolVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitEvent(this);
	}

	protected override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEvent(this, argument);
	}
}
