using System;
using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class TupleElementFieldSymbol : WrappedFieldSymbol
{
	private readonly int _tupleElementIndex;

	protected readonly NamedTypeSymbol _containingTuple;

	private readonly ImmutableArray<Location> _locations;

	protected readonly FieldSymbol _correspondingDefaultField;

	private readonly bool _isImplicitlyDeclared;

	public sealed override int TupleElementIndex => _tupleElementIndex >> 1;

	public sealed override bool IsDefaultTupleElement => (_tupleElementIndex & 1) == 0;

	public sealed override bool IsExplicitlyNamedTupleElement => !_isImplicitlyDeclared;

	public sealed override FieldSymbol TupleUnderlyingField => _underlyingField;

	public sealed override Symbol? AssociatedSymbol => null;

	public override FieldSymbol OriginalDefinition
	{
		get
		{
			NamedTypeSymbol originalDefinition = ContainingType.OriginalDefinition;
			if (!originalDefinition.IsTupleType)
			{
				return this;
			}
			return originalDefinition.GetTupleMemberSymbolForUnderlyingMember(_underlyingField.OriginalDefinition);
		}
	}

	public sealed override Symbol ContainingSymbol => _containingTuple;

	public sealed override RefKind RefKind => _underlyingField.RefKind;

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => _underlyingField.RefCustomModifiers;

	internal override bool RequiresCompletion => _underlyingField.RequiresCompletion;

	public sealed override ImmutableArray<Location> Locations => _locations;

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			if (!_isImplicitlyDeclared)
			{
				return Symbol.GetDeclaringSyntaxReferenceHelper<CSharpSyntaxNode>(_locations);
			}
			return ImmutableArray<SyntaxReference>.Empty;
		}
	}

	public sealed override bool IsImplicitlyDeclared => _isImplicitlyDeclared;

	public sealed override FieldSymbol CorrespondingTupleField => _correspondingDefaultField;

	public TupleElementFieldSymbol(NamedTypeSymbol container, FieldSymbol underlyingField, int tupleElementIndex, ImmutableArray<Location> locations, bool isImplicitlyDeclared, FieldSymbol? correspondingDefaultFieldOpt = null)
		: base(underlyingField)
	{
		_containingTuple = container;
		_tupleElementIndex = (((object)correspondingDefaultFieldOpt == null) ? (tupleElementIndex << 1) : ((tupleElementIndex << 1) + 1));
		_locations = locations;
		_isImplicitlyDeclared = isImplicitlyDeclared;
		_correspondingDefaultField = correspondingDefaultFieldOpt ?? this;
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _underlyingField.GetFieldType(fieldsBeingBound);
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingField.GetAttributes();
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		return _underlyingField.GetUseSiteInfo();
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return _underlyingField.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		_underlyingField.ForceComplete(locationOpt, filter, cancellationToken);
	}

	public sealed override int GetHashCode()
	{
		int hashCode = _containingTuple.GetHashCode();
		int tupleElementIndex = _tupleElementIndex;
		return Hash.Combine(hashCode, tupleElementIndex.GetHashCode());
	}

	public sealed override bool Equals(Symbol obj, TypeCompareKind compareKind)
	{
		TupleElementFieldSymbol tupleElementFieldSymbol = obj as TupleElementFieldSymbol;
		if ((object)tupleElementFieldSymbol == this)
		{
			return true;
		}
		if ((object)tupleElementFieldSymbol != null && _tupleElementIndex == tupleElementFieldSymbol._tupleElementIndex)
		{
			return TypeSymbol.Equals(_containingTuple, tupleElementFieldSymbol._containingTuple, compareKind);
		}
		return false;
	}

	internal override FieldSymbol AsMember(NamedTypeSymbol newOwner)
	{
		NamedTypeSymbol newUnderlyingOwner = GetNewUnderlyingOwner(newOwner);
		return new TupleElementFieldSymbol(newOwner, _underlyingField.OriginalDefinition.AsMember(newUnderlyingOwner), TupleElementIndex, Locations, IsImplicitlyDeclared);
	}

	protected NamedTypeSymbol GetNewUnderlyingOwner(NamedTypeSymbol newOwner)
	{
		int num = TupleElementIndex;
		NamedTypeSymbol namedTypeSymbol = newOwner;
		while (num >= 7)
		{
			namedTypeSymbol = (NamedTypeSymbol)namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type;
			num -= 7;
		}
		return namedTypeSymbol;
	}
}
