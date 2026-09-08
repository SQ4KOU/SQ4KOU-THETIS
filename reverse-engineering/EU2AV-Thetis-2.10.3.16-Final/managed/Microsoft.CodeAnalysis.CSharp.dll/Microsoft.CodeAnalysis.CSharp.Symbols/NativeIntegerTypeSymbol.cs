using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class NativeIntegerTypeSymbol : WrappedNamedTypeSymbol, IReference
{
	private sealed class NativeIntegerTypeMap : AbstractTypeMap
	{
		private readonly NativeIntegerTypeSymbol _type;

		private readonly SpecialType _specialType;

		internal NativeIntegerTypeMap(NativeIntegerTypeSymbol type)
		{
			_type = type;
			_specialType = _type.UnderlyingNamedType.SpecialType;
		}

		internal override NamedTypeSymbol SubstituteTypeDeclaration(NamedTypeSymbol previous)
		{
			if (previous.SpecialType != _specialType)
			{
				return base.SubstituteTypeDeclaration(previous);
			}
			return _type;
		}

		internal override ImmutableArray<CustomModifier> SubstituteCustomModifiers(ImmutableArray<CustomModifier> customModifiers)
		{
			return customModifiers;
		}
	}

	private ImmutableArray<NamedTypeSymbol> _lazyInterfaces;

	private ImmutableArray<Symbol> _lazyMembers;

	private NativeIntegerTypeMap? _lazyTypeMap;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override Symbol ContainingSymbol => _underlyingType.ContainingSymbol;

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => ImmutableArray<TypeWithAnnotations>.Empty;

	internal override bool IsComImport => _underlyingType.IsComImport;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => _underlyingType.BaseTypeNoUseSiteDiagnostics;

	public override ExtendedSpecialType ExtendedSpecialType => _underlyingType.ExtendedSpecialType;

	public override IEnumerable<string> MemberNames => from m in GetMembers()
		select m.Name;

	internal override bool HasDeclaredRequiredMembers => false;

	public override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 173);
		}
	}

	internal override bool IsNativeIntegerWrapperType => true;

	internal sealed override NamedTypeSymbol NativeIntegerUnderlyingType => _underlyingType;

	internal sealed override bool IsFileLocal => false;

	internal sealed override FileIdentifier? AssociatedFileIdentifier => null;

	internal sealed override bool IsRecord => false;

	internal sealed override bool IsRecordStruct => false;

	internal sealed override ParameterSymbol? ExtensionParameter => null;

	internal override bool HasCompilerLoweringPreserveAttribute => false;

	internal sealed override string? ExtensionGroupingName => null;

	internal sealed override string? ExtensionMarkerName => null;

	internal NativeIntegerTypeSymbol(NamedTypeSymbol underlyingType)
		: base(underlyingType, null)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (_lazyMembers.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref _lazyMembers, makeMembers(_underlyingType.GetMembers()));
		}
		return _lazyMembers;
		ImmutableArray<Symbol> makeMembers(ImmutableArray<Symbol> underlyingMembers)
		{
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
			foreach (Symbol item in underlyingMembers)
			{
				if (item.DeclaredAccessibility == Accessibility.Public)
				{
					if (!(item is MethodSymbol methodSymbol))
					{
						if (item is PropertySymbol { ParameterCount: 0 } propertySymbol && propertySymbol.Name != "Size")
						{
							NativeIntegerPropertySymbol nativeIntegerPropertySymbol = new NativeIntegerPropertySymbol(this, propertySymbol, (NativeIntegerTypeSymbol container, NativeIntegerPropertySymbol property, MethodSymbol? underlyingAccessor) => ((object)underlyingAccessor != null) ? new NativeIntegerMethodSymbol(container, underlyingAccessor, property) : null);
							instance.Add(nativeIntegerPropertySymbol);
							instance.AddIfNotNull(nativeIntegerPropertySymbol.GetMethod);
							instance.AddIfNotNull(nativeIntegerPropertySymbol.SetMethod);
						}
					}
					else if (!methodSymbol.IsGenericMethod && !methodSymbol.IsAccessor())
					{
						switch (methodSymbol.MethodKind)
						{
						case MethodKind.Ordinary:
							switch (methodSymbol.Name)
							{
							default:
								instance.Add(new NativeIntegerMethodSymbol(this, methodSymbol, null));
								break;
							case "Subtract":
							case "ToUInt32":
							case "ToUInt64":
							case "ToInt32":
							case "ToInt64":
							case "Add":
							case "ToPointer":
								break;
							}
							break;
						case MethodKind.Constructor:
							if (methodSymbol.ParameterCount == 0)
							{
								instance.Add(new NativeIntegerMethodSymbol(this, methodSymbol, null));
							}
							break;
						}
					}
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return GetMembers().WhereAsArray((Symbol member, string text) => member.Name == text, name);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		return _underlyingType.GetDeclaredBaseType(basesBeingResolved);
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		return GetInterfaces(basesBeingResolved);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 154);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 156);
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 158);
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 160);
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol>? basesBeingResolved = null)
	{
		return GetInterfaces(basesBeingResolved);
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 164);
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		return _underlyingType.GetUseSiteInfo();
	}

	internal sealed override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 177);
	}

	internal sealed override bool HasPossibleWellKnownCloneMethod()
	{
		return false;
	}

	internal override bool Equals(TypeSymbol? other, TypeCompareKind comparison)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (!_underlyingType.Equals(other, comparison))
		{
			return false;
		}
		if ((comparison & TypeCompareKind.IgnoreNativeIntegers) == 0)
		{
			return other.IsNativeIntegerWrapperType;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return _underlyingType.GetHashCode();
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 218);
	}

	private ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeSymbol>? basesBeingResolved)
	{
		if (_lazyInterfaces.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> value = _underlyingType.InterfacesNoUseSiteDiagnostics(basesBeingResolved).SelectAsArray((NamedTypeSymbol type, NativeIntegerTypeMap map) => map.SubstituteNamedType(type), GetTypeMap());
			ImmutableInterlocked.InterlockedInitialize(ref _lazyInterfaces, value);
		}
		return _lazyInterfaces;
	}

	private NativeIntegerTypeMap GetTypeMap()
	{
		if (_lazyTypeMap == null)
		{
			Interlocked.CompareExchange(ref _lazyTypeMap, new NativeIntegerTypeMap(this), null);
		}
		return _lazyTypeMap;
	}

	internal TypeWithAnnotations SubstituteUnderlyingType(TypeWithAnnotations type)
	{
		return type.SubstituteType(GetTypeMap());
	}

	internal NamedTypeSymbol SubstituteUnderlyingType(NamedTypeSymbol type)
	{
		return GetTypeMap().SubstituteNamedType(type);
	}

	internal static bool EqualsHelper<TSymbol>(TSymbol symbol, Symbol? other, TypeCompareKind comparison, Func<TSymbol, Symbol> getUnderlyingSymbol) where TSymbol : Symbol
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)symbol == other)
		{
			return true;
		}
		if (!getUnderlyingSymbol(symbol).Equals(other, comparison))
		{
			return false;
		}
		if ((comparison & TypeCompareKind.IgnoreNativeIntegers) == 0)
		{
			return other is TSymbol;
		}
		return true;
	}

	[Conditional("DEBUG")]
	internal static void VerifyEquality(Symbol symbolA, Symbol symbolB)
	{
	}

	internal override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		return SpecializedCollections.EmptyEnumerable<(MethodSymbol, MethodSymbol)>();
	}

	internal override bool HasInlineArrayAttribute(out int length)
	{
		length = 0;
		return false;
	}

	internal sealed override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		builderType = null;
		methodName = null;
		return false;
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		builderArgument = null;
		return false;
	}
}
