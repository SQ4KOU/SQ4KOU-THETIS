using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;

internal abstract class TypeSymbol : NamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol?>, ITypeSymbol, INamespaceOrTypeSymbol
{
	private ImmutableArray<INamedTypeSymbol> _allInterfaces;

	protected Microsoft.CodeAnalysis.NullableAnnotation NullableAnnotation { get; }

	internal abstract Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol UnderlyingTypeSymbol { get; }

	Microsoft.CodeAnalysis.NullableAnnotation ITypeSymbol.NullableAnnotation => NullableAnnotation;

	bool ISymbol.IsDefinition => this == ((ISymbol)this).OriginalDefinition;

	ITypeSymbol ITypeSymbol.OriginalDefinition => UnderlyingTypeSymbol.OriginalDefinition.GetPublicSymbol();

	INamedTypeSymbol ITypeSymbol.BaseType => UnderlyingTypeSymbol.BaseTypeNoUseSiteDiagnostics.GetPublicSymbol();

	ImmutableArray<INamedTypeSymbol> ITypeSymbol.Interfaces => UnderlyingTypeSymbol.InterfacesNoUseSiteDiagnostics().GetPublicSymbols();

	ImmutableArray<INamedTypeSymbol> ITypeSymbol.AllInterfaces
	{
		get
		{
			if (_allInterfaces.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _allInterfaces, UnderlyingTypeSymbol.AllInterfacesNoUseSiteDiagnostics.GetPublicSymbols());
			}
			return _allInterfaces;
		}
	}

	bool ITypeSymbol.IsUnmanagedType => !UnderlyingTypeSymbol.IsManagedTypeNoUseSiteDiagnostics;

	bool ITypeSymbol.IsReferenceType => UnderlyingTypeSymbol.IsReferenceType;

	bool ITypeSymbol.IsValueType => UnderlyingTypeSymbol.IsValueType;

	TypeKind ITypeSymbol.TypeKind => UnderlyingTypeSymbol.TypeKind;

	bool ITypeSymbol.IsTupleType => UnderlyingTypeSymbol.IsTupleType;

	bool ITypeSymbol.IsNativeIntegerType => UnderlyingTypeSymbol.IsNativeIntegerType;

	bool ITypeSymbol.IsExtension
	{
		get
		{
			if (UnderlyingTypeSymbol is Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol)
			{
				return namedTypeSymbol.IsExtension;
			}
			return false;
		}
	}

	IParameterSymbol? ITypeSymbol.ExtensionParameter
	{
		get
		{
			if (!(UnderlyingTypeSymbol is Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol))
			{
				return null;
			}
			return namedTypeSymbol.ExtensionParameter?.GetPublicSymbol();
		}
	}

	bool ITypeSymbol.IsAnonymousType => UnderlyingTypeSymbol.IsAnonymousType;

	SpecialType ITypeSymbol.SpecialType => UnderlyingTypeSymbol.SpecialType;

	bool ITypeSymbol.IsRefLikeType => UnderlyingTypeSymbol.IsRefLikeType;

	bool ITypeSymbol.IsReadOnly => UnderlyingTypeSymbol.IsReadOnly;

	bool ITypeSymbol.IsRecord
	{
		get
		{
			if (!UnderlyingTypeSymbol.IsRecord)
			{
				return UnderlyingTypeSymbol.IsRecordStruct;
			}
			return true;
		}
	}

	protected TypeSymbol(Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation)
	{
		NullableAnnotation = nullableAnnotation;
	}

	protected abstract ITypeSymbol WithNullableAnnotation(Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation);

	ITypeSymbol ITypeSymbol.WithNullableAnnotation(Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation)
	{
		if (UnderlyingTypeSymbol is Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol { IsExtension: not false })
		{
			throw new NotSupportedException();
		}
		if (NullableAnnotation == nullableAnnotation)
		{
			return this;
		}
		if (nullableAnnotation == UnderlyingTypeSymbol.DefaultNullableAnnotation)
		{
			return (ITypeSymbol)UnderlyingSymbol.ISymbol;
		}
		return WithNullableAnnotation(nullableAnnotation);
	}

	bool ISymbol.Equals(ISymbol other, Microsoft.CodeAnalysis.SymbolEqualityComparer equalityComparer)
	{
		return Equals(other as TypeSymbol, equalityComparer);
	}

	protected bool Equals(TypeSymbol otherType, Microsoft.CodeAnalysis.SymbolEqualityComparer equalityComparer)
	{
		if (otherType == null)
		{
			return false;
		}
		if (otherType == this)
		{
			return true;
		}
		TypeCompareKind compareKind = equalityComparer.CompareKind;
		if (NullableAnnotation != otherType.NullableAnnotation && (compareKind & TypeCompareKind.IgnoreNullableModifiersForReferenceTypes) == 0 && ((compareKind & TypeCompareKind.ObliviousNullableModifierMatchesAny) == 0 || (NullableAnnotation != Microsoft.CodeAnalysis.NullableAnnotation.None && otherType.NullableAnnotation != Microsoft.CodeAnalysis.NullableAnnotation.None)) && (!UnderlyingTypeSymbol.IsValueType || UnderlyingTypeSymbol.IsNullableType()))
		{
			return false;
		}
		return UnderlyingTypeSymbol.Equals(otherType.UnderlyingTypeSymbol, compareKind);
	}

	ISymbol ITypeSymbol.FindImplementationForInterfaceMember(ISymbol interfaceMember)
	{
		if (!(interfaceMember is Symbol symbol))
		{
			return null;
		}
		return UnderlyingTypeSymbol.FindImplementationForInterfaceMember(symbol.UnderlyingSymbol).GetPublicSymbol();
	}

	string ITypeSymbol.ToDisplayString(Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, SymbolDisplayFormat format)
	{
		return SymbolDisplay.ToDisplayString(this, topLevelNullability, format);
	}

	ImmutableArray<SymbolDisplayPart> ITypeSymbol.ToDisplayParts(Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, SymbolDisplayFormat format)
	{
		return SymbolDisplay.ToDisplayParts(this, topLevelNullability, format);
	}

	string ITypeSymbol.ToMinimalDisplayString(SemanticModel semanticModel, Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format)
	{
		return SymbolDisplay.ToMinimalDisplayString(this, topLevelNullability, semanticModel, position, format);
	}

	ImmutableArray<SymbolDisplayPart> ITypeSymbol.ToMinimalDisplayParts(SemanticModel semanticModel, Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format)
	{
		return SymbolDisplay.ToMinimalDisplayParts(this, topLevelNullability, semanticModel, position, format);
	}
}
