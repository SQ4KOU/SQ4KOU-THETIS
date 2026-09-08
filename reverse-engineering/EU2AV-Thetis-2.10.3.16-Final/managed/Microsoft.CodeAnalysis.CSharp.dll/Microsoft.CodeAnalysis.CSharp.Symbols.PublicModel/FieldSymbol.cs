using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;

internal sealed class FieldSymbol : Symbol, IFieldSymbol, ISymbol, IEquatable<ISymbol?>
{
	private readonly Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol _underlying;

	private ITypeSymbol _lazyType;

	internal override Microsoft.CodeAnalysis.CSharp.Symbol UnderlyingSymbol => _underlying;

	ISymbol IFieldSymbol.AssociatedSymbol => _underlying.AssociatedSymbol.GetPublicSymbol();

	RefKind IFieldSymbol.RefKind => _underlying.RefKind;

	ImmutableArray<CustomModifier> IFieldSymbol.RefCustomModifiers => _underlying.RefCustomModifiers;

	ITypeSymbol IFieldSymbol.Type
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

	Microsoft.CodeAnalysis.NullableAnnotation IFieldSymbol.NullableAnnotation => _underlying.TypeWithAnnotations.ToPublicAnnotation();

	ImmutableArray<CustomModifier> IFieldSymbol.CustomModifiers => _underlying.TypeWithAnnotations.CustomModifiers;

	IFieldSymbol IFieldSymbol.OriginalDefinition => _underlying.OriginalDefinition.GetPublicSymbol();

	IFieldSymbol IFieldSymbol.CorrespondingTupleField => _underlying.CorrespondingTupleField.GetPublicSymbol();

	bool IFieldSymbol.IsExplicitlyNamedTupleElement => _underlying.IsExplicitlyNamedTupleElement;

	bool IFieldSymbol.IsConst => _underlying.IsConst;

	bool IFieldSymbol.IsReadOnly => _underlying.IsReadOnly;

	bool IFieldSymbol.IsVolatile => _underlying.IsVolatile;

	bool IFieldSymbol.IsRequired => _underlying.IsRequired;

	bool IFieldSymbol.IsFixedSizeBuffer => _underlying.IsFixedSizeBuffer;

	int IFieldSymbol.FixedSize => _underlying.FixedSize;

	bool IFieldSymbol.HasConstantValue => _underlying.HasConstantValue;

	object IFieldSymbol.ConstantValue => _underlying.ConstantValue;

	public FieldSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol underlying)
	{
		_underlying = underlying;
	}

	protected override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitField(this);
	}

	protected override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitField(this);
	}

	protected override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitField(this, argument);
	}
}
