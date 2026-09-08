using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

public interface IFieldSymbol : ISymbol, IEquatable<ISymbol?>
{
	ISymbol? AssociatedSymbol { get; }

	bool IsConst { get; }

	bool IsReadOnly { get; }

	bool IsVolatile { get; }

	bool IsRequired { get; }

	bool IsFixedSizeBuffer { get; }

	int FixedSize { get; }

	RefKind RefKind { get; }

	ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	ITypeSymbol Type { get; }

	NullableAnnotation NullableAnnotation { get; }

	[MemberNotNullWhen(true, "ConstantValue")]
	bool HasConstantValue
	{
		[MemberNotNullWhen(true, "ConstantValue")]
		get;
	}

	object? ConstantValue { get; }

	ImmutableArray<CustomModifier> CustomModifiers { get; }

	new IFieldSymbol OriginalDefinition { get; }

	IFieldSymbol? CorrespondingTupleField { get; }

	bool IsExplicitlyNamedTupleElement { get; }
}
