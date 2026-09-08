using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class SymbolMatcher
{
	public abstract ITypeReference? MapReference(ITypeReference reference);

	public abstract IDefinition? MapDefinition(IDefinition definition);

	public abstract INamespace? MapNamespace(INamespace @namespace);

	protected abstract bool TryGetMatchingDelegateWithIndexedName(INamedTypeSymbolInternal delegateTemplate, ImmutableArray<AnonymousTypeValue> values, out AnonymousTypeValue match);

	public ISymbolInternal? MapDefinitionOrNamespace(ISymbolInternal symbol)
	{
		IReference cciAdapter = symbol.GetCciAdapter();
		if (!(cciAdapter is IDefinition definition))
		{
			return MapNamespace((INamespace)cciAdapter)?.GetInternalSymbol();
		}
		return MapDefinition(definition)?.GetInternalSymbol();
	}

	public EmitBaseline MapBaselineToCompilation(EmitBaseline baseline, Compilation targetCompilation, CommonPEModuleBuilder targetModuleBuilder, SynthesizedTypeMaps mappedSynthesizedTypes, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> mappedSynthesizedMembers, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> mappedDeletedMembers)
	{
		IReadOnlyDictionary<ITypeDefinition, int> typesAdded = MapDefinitions(baseline.TypesAdded);
		IReadOnlyDictionary<IEventDefinition, int> eventsAdded = MapDefinitions(baseline.EventsAdded);
		IReadOnlyDictionary<IFieldDefinition, int> fieldsAdded = MapDefinitions(baseline.FieldsAdded);
		IReadOnlyDictionary<IMethodDefinition, int> methodsAdded = MapDefinitions(baseline.MethodsAdded);
		IReadOnlyDictionary<IPropertyDefinition, int> propertiesAdded = MapDefinitions(baseline.PropertiesAdded);
		IReadOnlyDictionary<IDefinition, int> generationOrdinals = MapDefinitions(baseline.GenerationOrdinals);
		return baseline.With(targetCompilation, targetModuleBuilder, baseline.Ordinal, baseline.EncId, generationOrdinals, typesAdded, eventsAdded, fieldsAdded, methodsAdded, baseline.FirstParamRowMap, propertiesAdded, baseline.EventMapAdded, baseline.PropertyMapAdded, baseline.MethodImplsAdded, baseline.CustomAttributesAdded, baseline.TableEntriesAdded, baseline.BlobStreamLengthAdded, baseline.StringStreamLengthAdded, baseline.UserStringStreamLengthAdded, baseline.GuidStreamLengthAdded, mappedSynthesizedTypes, mappedSynthesizedMembers, mappedDeletedMembers, MapAddedOrChangedMethods(baseline.AddedOrChangedMethods), baseline.DebugInformationProvider, baseline.LocalSignatureProvider);
	}

	private IReadOnlyDictionary<K, V> MapDefinitions<K, V>(IReadOnlyDictionary<K, V> items) where K : class, IDefinition
	{
		Dictionary<K, V> dictionary = new Dictionary<K, V>(SymbolEquivalentEqualityComparer.Instance);
		foreach (KeyValuePair<K, V> item in items)
		{
			K val = (K)MapDefinition(item.Key);
			if (val != null)
			{
				dictionary.Add(val, item.Value);
			}
		}
		return dictionary;
	}

	private IReadOnlyDictionary<int, AddedOrChangedMethodInfo> MapAddedOrChangedMethods(IReadOnlyDictionary<int, AddedOrChangedMethodInfo> addedOrChangedMethods)
	{
		Dictionary<int, AddedOrChangedMethodInfo> dictionary = new Dictionary<int, AddedOrChangedMethodInfo>();
		foreach (KeyValuePair<int, AddedOrChangedMethodInfo> addedOrChangedMethod in addedOrChangedMethods)
		{
			dictionary.Add(addedOrChangedMethod.Key, addedOrChangedMethod.Value.MapTypes(this));
		}
		return dictionary;
	}

	private static ImmutableSegmentedDictionary<TKey, TValue> MapAnonymousTypesAndDelegatesWithUniqueKey<TKey, TValue>(ImmutableSegmentedDictionary<TKey, TValue> previousTypes, ImmutableSegmentedDictionary<TKey, TValue> newTypes) where TKey : IEquatable<TKey>
	{
		if (previousTypes.Count == 0)
		{
			return newTypes;
		}
		ImmutableSegmentedDictionary<TKey, TValue>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<TKey, TValue>();
		builder.AddRange(newTypes);
		foreach (var (key, value) in previousTypes)
		{
			if (!newTypes.ContainsKey(key))
			{
				builder.Add(key, value);
			}
		}
		return builder.ToImmutable();
	}

	private ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>> MapAnonymousDelegatesWithIndexedNames(ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>> previousDelegates, ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>> newDelegates)
	{
		if (previousDelegates.Count == 0)
		{
			return newDelegates;
		}
		ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>>();
		builder.AddRange(newDelegates);
		foreach (var (key, value) in previousDelegates)
		{
			if (!newDelegates.TryGetValue(key, out var value2))
			{
				builder.Add(key, value);
				continue;
			}
			ArrayBuilder<AnonymousTypeValue> arrayBuilder = null;
			foreach (AnonymousTypeValue item in value)
			{
				INamedTypeSymbolInternal delegateTemplate = (INamedTypeSymbolInternal)item.Type.GetInternalSymbol();
				if (!TryGetMatchingDelegateWithIndexedName(delegateTemplate, value2, out var _))
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<AnonymousTypeValue>.GetInstance();
					}
					arrayBuilder.Add(item);
				}
			}
			if (arrayBuilder != null)
			{
				arrayBuilder.AddRange(value2);
				builder[key] = arrayBuilder.ToImmutableAndFree();
			}
		}
		return builder.ToImmutable();
	}

	internal SynthesizedTypeMaps MapSynthesizedTypes(SynthesizedTypeMaps previousTypes, SynthesizedTypeMaps newTypes)
	{
		return new SynthesizedTypeMaps(MapAnonymousTypesAndDelegatesWithUniqueKey(previousTypes.AnonymousTypes, newTypes.AnonymousTypes), MapAnonymousTypesAndDelegatesWithUniqueKey(previousTypes.AnonymousDelegates, newTypes.AnonymousDelegates), MapAnonymousDelegatesWithIndexedNames(previousTypes.AnonymousDelegatesWithIndexedNames, newTypes.AnonymousDelegatesWithIndexedNames));
	}

	internal IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> MapSynthesizedOrDeletedMembers(IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> previousMembers, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> newMembers, bool isDeletedMemberMapping)
	{
		if (previousMembers.Count == 0)
		{
			return newMembers;
		}
		ImmutableSegmentedDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<ISymbolInternal, ImmutableArray<ISymbolInternal>>();
		builder.AddRange(newMembers);
		foreach (var (symbolInternal2, value) in previousMembers)
		{
			ISymbolInternal symbolInternal3 = MapDefinitionOrNamespace(symbolInternal2);
			if (symbolInternal3 == null)
			{
				builder.Add(symbolInternal2, value);
				continue;
			}
			if (!newMembers.TryGetValue(symbolInternal3, out ImmutableArray<ISymbolInternal> value2))
			{
				builder.Add(symbolInternal3, value);
				continue;
			}
			ArrayBuilder<ISymbolInternal> instance = ArrayBuilder<ISymbolInternal>.GetInstance();
			instance.AddRange(value2);
			foreach (ISymbolInternal item in value)
			{
				if (MapDefinitionOrNamespace(item) == null)
				{
					instance.Add(item);
				}
			}
			builder[symbolInternal3] = instance.ToImmutableAndFree();
		}
		return builder.ToImmutable();
	}
}
