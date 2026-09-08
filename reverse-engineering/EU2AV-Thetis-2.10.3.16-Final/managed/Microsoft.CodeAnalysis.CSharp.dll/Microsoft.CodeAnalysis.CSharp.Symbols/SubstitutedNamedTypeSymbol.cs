using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SubstitutedNamedTypeSymbol : WrappedNamedTypeSymbol
{
	private static readonly Func<Symbol, NamedTypeSymbol, Symbol> s_symbolAsMemberFunc = SymbolExtensions.SymbolAsMember;

	private readonly bool _unbound;

	private readonly TypeMap _inputMap;

	private readonly Symbol _newContainer;

	private TypeMap _lazyMap;

	private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

	private StrongBox<ParameterSymbol?> _lazyExtensionParameter;

	private NamedTypeSymbol _lazyBaseType = ErrorTypeSymbol.UnknownResultType;

	private int _hashCode;

	private ConcurrentCache<string, ImmutableArray<Symbol>> _lazyMembersByNameCache;

	private ImmutableArray<Symbol> _lazyMembers;

	public sealed override bool IsUnboundGenericType => _unbound;

	private TypeMap Map
	{
		get
		{
			EnsureMapAndTypeParameters();
			return _lazyMap;
		}
	}

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			EnsureMapAndTypeParameters();
			return _lazyTypeParameters;
		}
	}

	public sealed override Symbol ContainingSymbol => _newContainer;

	public override NamedTypeSymbol ContainingType => _newContainer as NamedTypeSymbol;

	public sealed override SymbolKind Kind => OriginalDefinition.Kind;

	public sealed override NamedTypeSymbol OriginalDefinition => _underlyingType;

	internal sealed override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if (_unbound)
			{
				return null;
			}
			if ((object)_lazyBaseType == ErrorTypeSymbol.UnknownResultType)
			{
				NamedTypeSymbol value = Map.SubstituteNamedType(OriginalDefinition.BaseTypeNoUseSiteDiagnostics);
				Interlocked.CompareExchange(ref _lazyBaseType, value, ErrorTypeSymbol.UnknownResultType);
			}
			return _lazyBaseType;
		}
	}

	public sealed override IEnumerable<string> MemberNames
	{
		get
		{
			if (_unbound)
			{
				return new List<string>((from s in GetTypeMembersUnordered()
					select s.Name).Distinct());
			}
			if (IsTupleType)
			{
				return (from s in GetMembers()
					select s.Name).Distinct();
			}
			return OriginalDefinition.MemberNames;
		}
	}

	internal sealed override bool HasDeclaredRequiredMembers
	{
		get
		{
			if (!_unbound)
			{
				return OriginalDefinition.HasDeclaredRequiredMembers;
			}
			return false;
		}
	}

	public sealed override NamedTypeSymbol EnumUnderlyingType => OriginalDefinition.EnumUnderlyingType;

	internal sealed override TypeMap TypeSubstitution => Map;

	internal sealed override bool IsComImport => OriginalDefinition.IsComImport;

	internal sealed override NamedTypeSymbol ComImportCoClass => OriginalDefinition.ComImportCoClass;

	internal sealed override bool IsFileLocal => _underlyingType.IsFileLocal;

	internal sealed override FileIdentifier AssociatedFileIdentifier => _underlyingType.AssociatedFileIdentifier;

	internal sealed override NamedTypeSymbol NativeIntegerUnderlyingType => null;

	internal sealed override bool IsRecord => _underlyingType.IsRecord;

	internal sealed override bool IsRecordStruct => _underlyingType.IsRecordStruct;

	internal sealed override bool HasCompilerLoweringPreserveAttribute => _underlyingType.HasCompilerLoweringPreserveAttribute;

	internal sealed override ParameterSymbol? ExtensionParameter
	{
		get
		{
			if (_lazyExtensionParameter == null)
			{
				Interlocked.CompareExchange(ref _lazyExtensionParameter, new StrongBox<ParameterSymbol>(substituteParameter()), null);
			}
			return _lazyExtensionParameter.Value;
			ParameterSymbol? substituteParameter()
			{
				if (!IsExtension)
				{
					return null;
				}
				ParameterSymbol extensionParameter = OriginalDefinition.ExtensionParameter;
				if ((object)extensionParameter == null)
				{
					return null;
				}
				return new SubstitutedParameterSymbol(this, Map, extensionParameter);
			}
		}
	}

	internal sealed override string? ExtensionGroupingName => _underlyingType.ExtensionGroupingName;

	internal sealed override string? ExtensionMarkerName => _underlyingType.ExtensionMarkerName;

	protected SubstitutedNamedTypeSymbol(Symbol newContainer, TypeMap map, NamedTypeSymbol originalDefinition, NamedTypeSymbol constructedFrom = null, bool unbound = false, TupleExtraData tupleData = null)
		: base(originalDefinition, tupleData)
	{
		_newContainer = newContainer;
		_inputMap = map;
		_unbound = unbound;
		if ((object)constructedFrom != null)
		{
			_lazyTypeParameters = constructedFrom.TypeParameters;
			_lazyMap = map;
		}
	}

	private void EnsureMapAndTypeParameters()
	{
		if (RoslynImmutableInterlocked.VolatileRead(in _lazyTypeParameters).IsDefault)
		{
			TypeMap value = _inputMap.WithAlphaRename(OriginalDefinition, this, out var newTypeParameters);
			TypeMap typeMap = Interlocked.CompareExchange(ref _lazyMap, value, null);
			if (typeMap != null)
			{
				newTypeParameters = typeMap.SubstituteTypeParameters(OriginalDefinition.TypeParameters);
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyTypeParameters, newTypeParameters, default(ImmutableArray<TypeParameterSymbol>));
		}
	}

	internal sealed override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (!_unbound)
		{
			return Map.SubstituteNamedType(OriginalDefinition.GetDeclaredBaseType(basesBeingResolved));
		}
		return null;
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (!_unbound)
		{
			return Map.SubstituteNamedTypes(OriginalDefinition.GetDeclaredInterfaces(basesBeingResolved));
		}
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (!_unbound)
		{
			return Map.SubstituteNamedTypes(OriginalDefinition.InterfacesNoUseSiteDiagnostics(basesBeingResolved));
		}
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedNamedTypeSymbol.cs", 186);
	}

	internal abstract override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes);

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return OriginalDefinition.GetAttributes();
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return OriginalDefinition.GetTypeMembersUnordered().SelectAsArray((NamedTypeSymbol t, SubstitutedNamedTypeSymbol self) => t.AsMember(self), this);
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return OriginalDefinition.GetTypeMembers().SelectAsArray((NamedTypeSymbol t, SubstitutedNamedTypeSymbol self) => t.AsMember(self), this);
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		return OriginalDefinition.GetTypeMembers(name).SelectAsArray((NamedTypeSymbol t, SubstitutedNamedTypeSymbol self) => t.AsMember(self), this);
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return OriginalDefinition.GetTypeMembers(name, arity).SelectAsArray((NamedTypeSymbol t, SubstitutedNamedTypeSymbol self) => t.AsMember(self), this);
	}

	public sealed override ImmutableArray<Symbol> GetMembers()
	{
		if (!_lazyMembers.IsDefault)
		{
			return _lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		if (_unbound)
		{
			foreach (Symbol member in OriginalDefinition.GetMembers())
			{
				if (member.Kind == SymbolKind.NamedType)
				{
					instance.Add(((NamedTypeSymbol)member).AsMember(this));
				}
			}
		}
		else
		{
			foreach (Symbol member2 in OriginalDefinition.GetMembers())
			{
				instance.Add(member2.SymbolAsMember(this));
			}
		}
		instance = AddOrWrapTupleMembersIfNecessary(instance);
		ImmutableArray<Symbol> value = instance.ToImmutableAndFree();
		ImmutableInterlocked.InterlockedInitialize(ref _lazyMembers, value);
		return _lazyMembers;
	}

	private ArrayBuilder<Symbol> AddOrWrapTupleMembersIfNecessary(ArrayBuilder<Symbol> builder)
	{
		if (IsTupleType)
		{
			ImmutableArray<Symbol> currentMembers = builder.ToImmutableAndFree();
			HashSet<Symbol> hashSet = new HashSet<Symbol>(ReferenceEqualityComparer.Instance);
			builder = MakeSynthesizedTupleMembers(currentMembers, hashSet);
			foreach (Symbol item in currentMembers)
			{
				if (!hashSet.Contains(item))
				{
					builder.Add(item);
				}
			}
		}
		return builder;
	}

	internal sealed override ImmutableArray<Symbol> GetMembersUnordered()
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		if (_unbound)
		{
			foreach (Symbol item in OriginalDefinition.GetMembersUnordered())
			{
				if (item.Kind == SymbolKind.NamedType)
				{
					instance.Add(((NamedTypeSymbol)item).AsMember(this));
				}
			}
		}
		else
		{
			foreach (Symbol item2 in OriginalDefinition.GetMembersUnordered())
			{
				instance.Add(item2.SymbolAsMember(this));
			}
		}
		instance = AddOrWrapTupleMembersIfNecessary(instance);
		return instance.ToImmutableAndFree();
	}

	public sealed override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (_unbound)
		{
			return StaticCast<Symbol>.From(GetTypeMembers(name));
		}
		ConcurrentCache<string, ImmutableArray<Symbol>> lazyMembersByNameCache = _lazyMembersByNameCache;
		if (lazyMembersByNameCache != null && lazyMembersByNameCache.TryGetValue(name, out var value))
		{
			return value;
		}
		return GetMembersWorker(name);
	}

	private ImmutableArray<Symbol> GetMembersWorker(string name)
	{
		if (IsTupleType)
		{
			ImmutableArray<Symbol> result = GetMembers().WhereAsArray((Symbol m, string text) => m.Name == text, name);
			cacheResult(result);
			return result;
		}
		ImmutableArray<Symbol> members = OriginalDefinition.GetMembers(name);
		if (members.IsDefaultOrEmpty)
		{
			return members;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(members.Length);
		foreach (Symbol item in members)
		{
			instance.Add(item.SymbolAsMember(this));
		}
		ImmutableArray<Symbol> result2 = instance.ToImmutableAndFree();
		cacheResult(result2);
		return result2;
		void cacheResult(ImmutableArray<Symbol> value)
		{
			(_lazyMembersByNameCache ?? (_lazyMembersByNameCache = new ConcurrentCache<string, ImmutableArray<Symbol>>(8))).TryAdd(name, value);
		}
	}

	internal sealed override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		if (_unbound)
		{
			yield break;
		}
		foreach (var item5 in OriginalDefinition.SynthesizedInterfaceMethodImpls())
		{
			MethodSymbol item = item5.Body;
			MethodSymbol item2 = item5.Implemented;
			MethodSymbol item3 = ExplicitInterfaceHelpers.SubstituteExplicitInterfaceImplementation(item, TypeSubstitution);
			MethodSymbol item4 = ExplicitInterfaceHelpers.SubstituteExplicitInterfaceImplementation(item2, TypeSubstitution);
			yield return (Body: item3, Implemented: item4);
		}
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedNamedTypeSymbol.cs", 389);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		if (!_unbound)
		{
			return OriginalDefinition.GetEarlyAttributeDecodingMembers().SelectAsArray(s_symbolAsMemberFunc, this);
		}
		return GetMembers();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		if (_unbound)
		{
			return GetMembers(name);
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (Symbol earlyAttributeDecodingMember in OriginalDefinition.GetEarlyAttributeDecodingMembers(name))
		{
			instance.Add(earlyAttributeDecodingMember.SymbolAsMember(this));
		}
		return instance.ToImmutableAndFree();
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode = this.ComputeHashCode();
		}
		return _hashCode;
	}

	internal sealed override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		return _underlyingType.HasCollectionBuilderAttribute(out builderType, out methodName);
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		return _underlyingType.HasAsyncMethodBuilderAttribute(out builderArgument);
	}

	internal override IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedNamedTypeSymbol.cs", 459);
	}

	internal override IEnumerable<EventSymbol> GetEventsToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedNamedTypeSymbol.cs", 464);
	}

	internal override IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedNamedTypeSymbol.cs", 469);
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedNamedTypeSymbol.cs", 474);
	}

	internal sealed override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SubstitutedNamedTypeSymbol.cs", 480);
	}

	internal sealed override bool HasPossibleWellKnownCloneMethod()
	{
		return _underlyingType.HasPossibleWellKnownCloneMethod();
	}

	internal sealed override bool HasInlineArrayAttribute(out int length)
	{
		return _underlyingType.HasInlineArrayAttribute(out length);
	}
}
