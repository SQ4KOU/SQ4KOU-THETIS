using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class SymbolChanges
{
	private readonly DefinitionMap _definitionMap;

	private readonly IReadOnlyDictionary<ISymbolInternal, SymbolChange> _changes;

	private readonly ISet<ISymbolInternal> _replacedSymbols;

	public readonly IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> DeletedMembers;

	public readonly IReadOnlyDictionary<INamedTypeSymbolInternal, ImmutableArray<(IMethodSymbolInternal oldMethod, IMethodSymbolInternal newMethod)>> UpdatedMethods;

	private readonly Func<ISymbol, bool> _isAddedSymbol;

	public DefinitionMap DefinitionMap => _definitionMap;

	protected SymbolChanges(DefinitionMap definitionMap, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol)
	{
		_definitionMap = definitionMap;
		_isAddedSymbol = isAddedSymbol;
		CalculateChanges(edits, out _changes, out _replacedSymbols, out DeletedMembers, out UpdatedMethods);
	}

	public bool IsReplacedDef(IDefinition definition, bool checkEnclosingTypes = false)
	{
		ISymbolInternal internalSymbol = definition.GetInternalSymbol();
		if (internalSymbol != null)
		{
			return IsReplaced(internalSymbol, checkEnclosingTypes);
		}
		return false;
	}

	public bool IsReplaced(ISymbolInternal symbol, bool checkEnclosingTypes = false)
	{
		for (ISymbolInternal symbolInternal = symbol; symbolInternal != null; symbolInternal = symbolInternal.ContainingType)
		{
			if (_replacedSymbols.Contains(symbolInternal))
			{
				return true;
			}
			if (!checkEnclosingTypes)
			{
				return false;
			}
		}
		return false;
	}

	public bool IsAdded(ISymbol symbol)
	{
		return _isAddedSymbol(symbol);
	}

	public bool RequiresCompilation(ISymbolInternal symbol)
	{
		return GetChange(symbol) != SymbolChange.None;
	}

	private bool DefinitionExistsInPreviousGeneration(ISymbolInternal symbol)
	{
		IDefinition definition = (IDefinition)symbol.GetCciAdapter();
		if (!_definitionMap.DefinitionExists(definition))
		{
			return false;
		}
		ISymbolInternal symbolInternal = symbol;
		do
		{
			if (_replacedSymbols.Contains(symbolInternal))
			{
				return false;
			}
			symbolInternal = symbolInternal.ContainingType;
		}
		while (symbolInternal != null);
		return true;
	}

	public SymbolChange GetChange(IDefinition def)
	{
		ISymbolInternal internalSymbol = def.GetInternalSymbol();
		if (internalSymbol is ISynthesizedGlobalMethodSymbol)
		{
			return SymbolChange.Added;
		}
		if (internalSymbol is ISynthesizedMethodBodyImplementationSymbol synthesizedMethodBodyImplementationSymbol)
		{
			SymbolChange change = GetChange((IDefinition)synthesizedMethodBodyImplementationSymbol.Method.GetCciAdapter());
			switch (change)
			{
			case SymbolChange.Updated:
				if (!DefinitionExistsInPreviousGeneration(synthesizedMethodBodyImplementationSymbol.ContainingType))
				{
					return SymbolChange.Added;
				}
				if (!DefinitionExistsInPreviousGeneration(synthesizedMethodBodyImplementationSymbol))
				{
					return SymbolChange.Added;
				}
				if (!synthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency)
				{
					return SymbolChange.None;
				}
				if (synthesizedMethodBodyImplementationSymbol.Kind == SymbolKind.NamedType)
				{
					return SymbolChange.ContainsChanges;
				}
				if (synthesizedMethodBodyImplementationSymbol.Kind == SymbolKind.Method)
				{
					return SymbolChange.Updated;
				}
				return SymbolChange.None;
			case SymbolChange.Added:
				if (!DefinitionExistsInPreviousGeneration(synthesizedMethodBodyImplementationSymbol))
				{
					return SymbolChange.Added;
				}
				if (synthesizedMethodBodyImplementationSymbol.Kind == SymbolKind.NamedType)
				{
					return SymbolChange.ContainsChanges;
				}
				if (synthesizedMethodBodyImplementationSymbol.Kind == SymbolKind.Method)
				{
					return SymbolChange.Updated;
				}
				return SymbolChange.None;
			default:
				throw ExceptionUtilities.UnexpectedValue(change);
			}
		}
		if (internalSymbol != null)
		{
			return GetChange(internalSymbol);
		}
		if (_definitionMap.DefinitionExists(def))
		{
			if (!(def is ITypeDefinition))
			{
				return SymbolChange.None;
			}
			return SymbolChange.ContainsChanges;
		}
		return SymbolChange.Added;
	}

	private SymbolChange GetChange(ISymbolInternal symbol)
	{
		if (symbol is IMethodSymbolInternal methodSymbolInternal)
		{
			ISymbolInternal partialDefinitionPart = methodSymbolInternal.PartialDefinitionPart;
			symbol = partialDefinitionPart ?? symbol;
		}
		if (_changes.TryGetValue(symbol, out var value))
		{
			return value;
		}
		ISymbolInternal containingSymbol = GetContainingSymbol(symbol);
		if (containingSymbol == null)
		{
			return SymbolChange.None;
		}
		SymbolChange change = GetChange(containingSymbol);
		switch (change)
		{
		case SymbolChange.Added:
			return SymbolChange.Added;
		case SymbolChange.None:
			return SymbolChange.None;
		case SymbolChange.ContainsChanges:
		case SymbolChange.Updated:
			if (symbol.Kind == SymbolKind.Namespace)
			{
				if (!_definitionMap.NamespaceExists((INamespace)symbol.GetCciAdapter()))
				{
					return SymbolChange.Added;
				}
				return SymbolChange.ContainsChanges;
			}
			if (!DefinitionExistsInPreviousGeneration(symbol))
			{
				return SymbolChange.Added;
			}
			return SymbolChange.None;
		default:
			throw ExceptionUtilities.UnexpectedValue(change);
		}
	}

	public SymbolChange GetChangeForPossibleReAddedMember(ITypeDefinitionMember item, Func<ITypeDefinitionMember, bool> definitionExistsInAnyPreviousGeneration)
	{
		SymbolChange change = GetChange(item);
		return fixChangeIfMemberIsReAdded(item, change, definitionExistsInAnyPreviousGeneration);
		SymbolChange fixChangeIfMemberIsReAdded(ITypeDefinitionMember typeDefinitionMember, SymbolChange symbolChange, Func<ITypeDefinitionMember, bool> func)
		{
			if (typeDefinitionMember is IFieldDefinition fieldDefinition && GetContainingDefinitionForBackingField(fieldDefinition) is ITypeDefinitionMember typeDefinitionMember2 && GetChange(typeDefinitionMember2) == SymbolChange.Added && func(typeDefinitionMember) && fixChangeIfMemberIsReAdded(typeDefinitionMember2, SymbolChange.Added, func) == SymbolChange.Updated)
			{
				return SymbolChange.None;
			}
			if (symbolChange == SymbolChange.Added && !IsReplacedDef(typeDefinitionMember.ContainingTypeDefinition, checkEnclosingTypes: true) && func(typeDefinitionMember))
			{
				return SymbolChange.Updated;
			}
			return symbolChange;
		}
	}

	protected abstract ISymbolInternal? GetISymbolInternalOrNull(ISymbol symbol);

	public ISymbolInternal GetRequiredInternalSymbol(ISymbol? symbol)
	{
		return GetISymbolInternalOrNull(symbol);
	}

	public IEnumerable<INamespaceTypeDefinition> GetTopLevelSourceTypeDefinitions(EmitContext context)
	{
		foreach (KeyValuePair<ISymbolInternal, SymbolChange> change in _changes)
		{
			RoslynKeyValuePairExtensions.Deconstruct(change, out var key, out var _);
			INamespaceTypeDefinition namespaceTypeDefinition = (key.GetCciAdapter() as ITypeDefinition)?.AsNamespaceTypeDefinition(context);
			if (namespaceTypeDefinition != null)
			{
				yield return namespaceTypeDefinition;
			}
		}
	}

	private void CalculateChanges(IEnumerable<SemanticEdit> edits, out IReadOnlyDictionary<ISymbolInternal, SymbolChange> changes, out ISet<ISymbolInternal> replacedSymbols, out IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> deletedMembers, out IReadOnlyDictionary<INamedTypeSymbolInternal, ImmutableArray<(IMethodSymbolInternal oldMethod, IMethodSymbolInternal newMethod)>> updatedMethods)
	{
		Dictionary<ISymbolInternal, SymbolChange> dictionary = new Dictionary<ISymbolInternal, SymbolChange>();
		Dictionary<INamedTypeSymbolInternal, ArrayBuilder<(IMethodSymbolInternal, IMethodSymbolInternal)>> dictionary2 = new Dictionary<INamedTypeSymbolInternal, ArrayBuilder<(IMethodSymbolInternal, IMethodSymbolInternal)>>();
		HashSet<ISymbolInternal> hashSet = null;
		Dictionary<ISymbolInternal, ArrayBuilder<ISymbolInternal>> dictionary3 = null;
		foreach (SemanticEdit edit in edits)
		{
			SymbolChange value2;
			switch (edit.Kind)
			{
			case SemanticEditKind.Update:
				value2 = SymbolChange.Updated;
				break;
			case SemanticEditKind.Insert:
				value2 = SymbolChange.Added;
				break;
			case SemanticEditKind.Replace:
				(hashSet ?? (hashSet = new HashSet<ISymbolInternal>())).Add(GetRequiredInternalSymbol(edit.NewSymbol));
				value2 = SymbolChange.Added;
				break;
			case SemanticEditKind.Delete:
			{
				INamedTypeSymbolInternal namedTypeSymbolInternal = (INamedTypeSymbolInternal)GetRequiredInternalSymbol(edit.NewSymbol);
				if (dictionary3 == null)
				{
					dictionary3 = new Dictionary<ISymbolInternal, ArrayBuilder<ISymbolInternal>>();
				}
				if (!dictionary3.TryGetValue(namedTypeSymbolInternal, out var value))
				{
					value = ArrayBuilder<ISymbolInternal>.GetInstance();
					dictionary3.Add(namedTypeSymbolInternal, value);
				}
				ISymbolInternal requiredInternalSymbol = GetRequiredInternalSymbol(edit.OldSymbol);
				value.Add(requiredInternalSymbol);
				if (!dictionary.ContainsKey(namedTypeSymbolInternal))
				{
					dictionary.Add(namedTypeSymbolInternal, SymbolChange.ContainsChanges);
					AddContainingSymbolChanges(dictionary, namedTypeSymbolInternal);
				}
				continue;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(edit.Kind);
			}
			ISymbolInternal symbolInternal = GetRequiredInternalSymbol(edit.NewSymbol);
			if (symbolInternal.Kind == SymbolKind.Method)
			{
				ISymbolInternal partialDefinitionPart = ((IMethodSymbolInternal)symbolInternal).PartialDefinitionPart;
				symbolInternal = partialDefinitionPart ?? symbolInternal;
				if (edit.Kind == SemanticEditKind.Update)
				{
					IMethodSymbolInternal methodSymbolInternal = (IMethodSymbolInternal)GetRequiredInternalSymbol(edit.OldSymbol);
					if (!dictionary2.TryGetValue(symbolInternal.ContainingType, out var value3))
					{
						value3 = ArrayBuilder<(IMethodSymbolInternal, IMethodSymbolInternal)>.GetInstance();
						dictionary2.Add(symbolInternal.ContainingType, value3);
					}
					value3.Add((methodSymbolInternal.PartialDefinitionPart ?? methodSymbolInternal, (IMethodSymbolInternal)symbolInternal));
				}
			}
			else if (symbolInternal.Kind == SymbolKind.Property)
			{
				ISymbolInternal partialDefinitionPart = ((IPropertySymbolInternal)symbolInternal).PartialDefinitionPart;
				symbolInternal = partialDefinitionPart ?? symbolInternal;
			}
			AddContainingSymbolChanges(dictionary, symbolInternal);
			if (dictionary.TryGetValue(symbolInternal, out var value4) && value4 == SymbolChange.ContainsChanges)
			{
				dictionary[symbolInternal] = value2;
			}
			else
			{
				dictionary.Add(symbolInternal, value2);
			}
		}
		changes = dictionary;
		ISet<ISymbolInternal> set = hashSet;
		replacedSymbols = set ?? SpecializedCollections.EmptySet<ISymbolInternal>();
		deletedMembers = dictionary3?.ToImmutableSegmentedDictionary((KeyValuePair<ISymbolInternal, ArrayBuilder<ISymbolInternal>> e) => e.Key, (KeyValuePair<ISymbolInternal, ArrayBuilder<ISymbolInternal>> e) => e.Value.ToImmutableAndFree()) ?? ImmutableSegmentedDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>.Empty;
		updatedMethods = dictionary2.ToImmutableSegmentedDictionary<KeyValuePair<INamedTypeSymbolInternal, ArrayBuilder<(IMethodSymbolInternal, IMethodSymbolInternal)>>, INamedTypeSymbolInternal, ImmutableArray<(IMethodSymbolInternal, IMethodSymbolInternal)>>((KeyValuePair<INamedTypeSymbolInternal, ArrayBuilder<(IMethodSymbolInternal oldMethod, IMethodSymbolInternal newMethod)>> e) => e.Key, (KeyValuePair<INamedTypeSymbolInternal, ArrayBuilder<(IMethodSymbolInternal oldMethod, IMethodSymbolInternal newMethod)>> e) => e.Value.ToImmutableAndFree());
	}

	private static void AddContainingSymbolChanges(Dictionary<ISymbolInternal, SymbolChange> changes, ISymbolInternal symbol)
	{
		while (true)
		{
			ISymbolInternal containingSymbol = GetContainingSymbol(symbol);
			if (containingSymbol == null || changes.ContainsKey(containingSymbol))
			{
				break;
			}
			changes.Add(containingSymbol, SymbolChange.ContainsChanges);
			symbol = containingSymbol;
		}
	}

	private static ISymbolInternal? GetContainingSymbol(ISymbolInternal symbol)
	{
		ISymbolInternal associatedSymbol = GetAssociatedSymbol(symbol);
		if (associatedSymbol != null)
		{
			return associatedSymbol;
		}
		symbol = symbol.ContainingSymbol;
		if (symbol != null)
		{
			SymbolKind kind = symbol.Kind;
			if (kind == SymbolKind.Assembly || kind == SymbolKind.NetModule)
			{
				return null;
			}
		}
		return symbol;
	}

	private static ISymbolInternal? GetAssociatedSymbol(ISymbolInternal symbol)
	{
		if (!(symbol is IFieldSymbolInternal fieldSymbolInternal))
		{
			if (symbol is IMethodSymbolInternal methodSymbolInternal)
			{
				return methodSymbolInternal.AssociatedSymbol;
			}
			return null;
		}
		return fieldSymbolInternal.AssociatedSymbol;
	}

	internal IDefinition? GetContainingDefinitionForBackingField(IFieldDefinition fieldDefinition)
	{
		ISymbolInternal internalSymbol = fieldDefinition.GetInternalSymbol();
		if (internalSymbol == null)
		{
			return null;
		}
		return GetAssociatedSymbol(internalSymbol)?.GetCciAdapter() as IDefinition;
	}
}
