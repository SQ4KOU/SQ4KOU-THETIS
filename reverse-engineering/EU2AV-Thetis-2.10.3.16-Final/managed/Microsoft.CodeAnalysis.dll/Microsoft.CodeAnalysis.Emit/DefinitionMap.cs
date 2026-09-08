using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class DefinitionMap
{
	private readonly struct MetadataLambdasAndClosures(ImmutableArray<(DebugId id, IMethodSymbolInternal symbol)> lambdaSymbols, IReadOnlyDictionary<DebugId, (DebugId? parentId, ImmutableArray<string> structCaptures)> closureTree)
	{
		public ImmutableArray<(DebugId id, IMethodSymbolInternal symbol)> LambdaSymbols { get; } = lambdaSymbols;

		public IReadOnlyDictionary<DebugId, (DebugId? parentId, ImmutableArray<string> structCaptures)> ClosureTree { get; } = closureTree;

		public IMethodSymbolInternal? GetLambdaSymbol(DebugId lambdaId)
		{
			int num = LambdaSymbols.BinarySearch<(DebugId, IMethodSymbolInternal), DebugId>(lambdaId, ((DebugId id, IMethodSymbolInternal symbol) info, DebugId id) => info.id.CompareTo(id));
			if (num < 0)
			{
				return null;
			}
			return LambdaSymbols[num].symbol;
		}

		public (DebugId? parentId, ImmutableArray<string>) TryGetClosureInfo(DebugId closureId)
		{
			if (!ClosureTree.TryGetValue(closureId, out (DebugId?, ImmutableArray<string>) value))
			{
				return default((DebugId?, ImmutableArray<string>));
			}
			return value;
		}
	}

	private readonly ImmutableDictionary<IMethodSymbolInternal, MethodInstrumentation> _methodInstrumentations;

	protected readonly IReadOnlyDictionary<IMethodSymbolInternal, EncMappedMethod> mappedMethods;

	private ImmutableDictionary<INamedTypeSymbolInternal, MetadataLambdasAndClosures> _metadataLambdasAndClosures = ImmutableDictionary<INamedTypeSymbolInternal, MetadataLambdasAndClosures>.Empty;

	public readonly EmitBaseline Baseline;

	public abstract SymbolMatcher SourceToMetadataSymbolMatcher { get; }

	public abstract SymbolMatcher SourceToPreviousSymbolMatcher { get; }

	public abstract SymbolMatcher PreviousSourceToMetadataSymbolMatcher { get; }

	internal abstract CommonMessageProvider MessageProvider { get; }

	protected DefinitionMap(IEnumerable<SemanticEdit> edits, EmitBaseline baseline)
	{
		mappedMethods = GetMappedMethods(edits);
		_methodInstrumentations = edits.Where((SemanticEdit edit) => !edit.Instrumentation.IsEmpty).ToImmutableDictionary((SemanticEdit edit) => (IMethodSymbolInternal)GetISymbolInternalOrNull(edit.NewSymbol), (SemanticEdit edit) => edit.Instrumentation);
		Baseline = baseline;
	}

	private IReadOnlyDictionary<IMethodSymbolInternal, EncMappedMethod> GetMappedMethods(IEnumerable<SemanticEdit> edits)
	{
		Dictionary<IMethodSymbolInternal, EncMappedMethod> dictionary = new Dictionary<IMethodSymbolInternal, EncMappedMethod>();
		foreach (SemanticEdit edit in edits)
		{
			if (edit.Kind == SemanticEditKind.Update && edit.SyntaxMap != null)
			{
				IMethodSymbolInternal previousMethod = (IMethodSymbolInternal)GetISymbolInternalOrNull(edit.OldSymbol);
				IMethodSymbolInternal key = (IMethodSymbolInternal)GetISymbolInternalOrNull(edit.NewSymbol);
				dictionary.Add(key, new EncMappedMethod(previousMethod, edit.SyntaxMap, edit.RuntimeRudeEdit));
			}
		}
		return dictionary;
	}

	protected abstract ISymbolInternal? GetISymbolInternalOrNull(ISymbol symbol);

	internal IDefinition? MapDefinition(IDefinition definition)
	{
		IDefinition? definition2 = SourceToPreviousSymbolMatcher.MapDefinition(definition);
		if (definition2 == null)
		{
			if (SourceToMetadataSymbolMatcher == SourceToPreviousSymbolMatcher)
			{
				return null;
			}
			definition2 = SourceToMetadataSymbolMatcher.MapDefinition(definition);
		}
		return definition2;
	}

	internal INamespace? MapNamespace(INamespace @namespace)
	{
		INamespace? obj = SourceToPreviousSymbolMatcher.MapNamespace(@namespace);
		if (obj == null)
		{
			if (SourceToMetadataSymbolMatcher == SourceToPreviousSymbolMatcher)
			{
				return null;
			}
			obj = SourceToMetadataSymbolMatcher.MapNamespace(@namespace);
		}
		return obj;
	}

	internal bool DefinitionExists(IDefinition definition)
	{
		return MapDefinition(definition) != null;
	}

	internal bool NamespaceExists(INamespace @namespace)
	{
		return MapNamespace(@namespace) != null;
	}

	internal EntityHandle GetInitialMetadataHandle(IDefinition def)
	{
		return MetadataTokens.EntityHandle((SourceToMetadataSymbolMatcher.MapDefinition(def)?.GetInternalSymbol()?.MetadataToken).GetValueOrDefault());
	}

	public bool TryGetMethodHandle(IMethodSymbolInternal method, out MethodDefinitionHandle handle)
	{
		IMethodDefinition methodDefinition = (IMethodDefinition)method.GetCciAdapter();
		EntityHandle initialMetadataHandle = GetInitialMetadataHandle(methodDefinition);
		if (!initialMetadataHandle.IsNil)
		{
			handle = (MethodDefinitionHandle)initialMetadataHandle;
			return true;
		}
		IMethodDefinition methodDefinition2 = (IMethodDefinition)SourceToPreviousSymbolMatcher.MapDefinition(methodDefinition);
		if (methodDefinition2 != null && Baseline.MethodsAdded.TryGetValue(methodDefinition2, out var value))
		{
			handle = MetadataTokens.MethodDefinitionHandle(value);
			return true;
		}
		handle = default(MethodDefinitionHandle);
		return false;
	}

	public MethodDefinitionHandle GetPreviousMethodHandle(IMethodSymbolInternal oldMethod)
	{
		IMethodSymbolInternal peMethod;
		return GetPreviousMethodHandle(oldMethod, out peMethod);
	}

	public PropertyDefinitionHandle GetPreviousPropertyHandle(IPropertySymbolInternal oldProperty)
	{
		IPropertySymbolInternal peProperty;
		return GetPreviousPropertyHandle(oldProperty, out peProperty);
	}

	public EventDefinitionHandle GetPreviousEventHandle(IEventSymbolInternal oldEvent)
	{
		IEventSymbolInternal peEvent;
		return GetPreviousEventHandle(oldEvent, out peEvent);
	}

	private MethodDefinitionHandle GetPreviousMethodHandle(IMethodSymbolInternal oldProperty, out IMethodSymbolInternal? peMethod)
	{
		IMethodDefinition methodDefinition = (IMethodDefinition)oldProperty.GetCciAdapter();
		if (Baseline.MethodsAdded.TryGetValue(methodDefinition, out var value))
		{
			peMethod = null;
			return MetadataTokens.MethodDefinitionHandle(value);
		}
		peMethod = (IMethodSymbolInternal)(PreviousSourceToMetadataSymbolMatcher.MapDefinition(methodDefinition)?.GetInternalSymbol());
		return (MethodDefinitionHandle)MetadataTokens.EntityHandle(peMethod.MetadataToken);
	}

	private PropertyDefinitionHandle GetPreviousPropertyHandle(IPropertySymbolInternal oldProperty, out IPropertySymbolInternal? peProperty)
	{
		IPropertyDefinition propertyDefinition = (IPropertyDefinition)oldProperty.GetCciAdapter();
		if (Baseline.PropertiesAdded.TryGetValue(propertyDefinition, out var value))
		{
			peProperty = null;
			return MetadataTokens.PropertyDefinitionHandle(value);
		}
		peProperty = (IPropertySymbolInternal)(PreviousSourceToMetadataSymbolMatcher.MapDefinition(propertyDefinition)?.GetInternalSymbol());
		return (PropertyDefinitionHandle)MetadataTokens.EntityHandle(peProperty.MetadataToken);
	}

	private EventDefinitionHandle GetPreviousEventHandle(IEventSymbolInternal oldEvent, out IEventSymbolInternal? peEvent)
	{
		IEventDefinition eventDefinition = (IEventDefinition)oldEvent.GetCciAdapter();
		if (Baseline.EventsAdded.TryGetValue(eventDefinition, out var value))
		{
			peEvent = null;
			return MetadataTokens.EventDefinitionHandle(value);
		}
		peEvent = (IEventSymbolInternal)(PreviousSourceToMetadataSymbolMatcher.MapDefinition(eventDefinition)?.GetInternalSymbol());
		return (EventDefinitionHandle)MetadataTokens.EntityHandle(peEvent.MetadataToken);
	}

	protected static IReadOnlyDictionary<SyntaxNode, int> CreateDeclaratorToSyntaxOrdinalMap(ImmutableArray<SyntaxNode> declarators)
	{
		Dictionary<SyntaxNode, int> dictionary = new Dictionary<SyntaxNode, int>();
		for (int i = 0; i < declarators.Length; i++)
		{
			dictionary.Add(declarators[i], i);
		}
		return dictionary;
	}

	protected abstract void GetStateMachineFieldMapFromMetadata(ITypeSymbolInternal stateMachineType, ImmutableArray<LocalSlotDebugInfo> localSlotDebugInfo, out IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap, out IReadOnlyDictionary<ITypeReference, int> awaiterMap, out int awaiterSlotCount);

	protected abstract ImmutableArray<EncLocalInfo> GetLocalSlotMapFromMetadata(StandaloneSignatureHandle handle, EditAndContinueMethodDebugInformation debugInfo);

	protected abstract ITypeSymbolInternal? TryGetStateMachineType(MethodDefinitionHandle methodHandle);

	protected abstract IMethodSymbolInternal GetMethodSymbol(MethodDefinitionHandle methodHandle);

	internal VariableSlotAllocator? TryCreateVariableSlotAllocator(Compilation compilation, IMethodSymbolInternal method, IMethodSymbolInternal topLevelMethod, DiagnosticBag diagnostics)
	{
		if (!mappedMethods.TryGetValue(topLevelMethod, out var value))
		{
			return null;
		}
		if (!TryGetMethodHandle(method, out var handle))
		{
			return null;
		}
		IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap = null;
		IReadOnlyDictionary<ITypeReference, int> awaiterMap = null;
		IReadOnlyDictionary<int, EncLambdaMapValue> lambdaMap = null;
		IReadOnlyDictionary<int, EncClosureMapValue> closureMap = null;
		IReadOnlyDictionary<(int, AwaitDebugId), StateMachineState> readOnlyDictionary = null;
		StateMachineState? firstUnusedIncreasingStateMachineState = null;
		StateMachineState? firstUnusedDecreasingStateMachineState = null;
		int hoistedLocalSlotCount = 0;
		int awaiterSlotCount = 0;
		string stateMachineTypeName = null;
		int rowNumber = MetadataTokens.GetRowNumber(handle);
		DebugId? methodId;
		ImmutableArray<EncLocalInfo> previousLocals;
		SymbolMatcher symbolMap;
		if (Baseline.AddedOrChangedMethods.TryGetValue(rowNumber, out var value2))
		{
			methodId = value2.MethodId;
			lambdaMap = MakeLambdaMap(value2.LambdaDebugInfo);
			closureMap = MakeClosureMap(value2.ClosureDebugInfo);
			readOnlyDictionary = MakeStateMachineStateMap(value2.StateMachineStates.States);
			firstUnusedIncreasingStateMachineState = value2.StateMachineStates.FirstUnusedIncreasingStateMachineState;
			firstUnusedDecreasingStateMachineState = value2.StateMachineStates.FirstUnusedDecreasingStateMachineState;
			if (value2.StateMachineTypeName != null)
			{
				GetStateMachineFieldMapFromPreviousCompilation(value2.StateMachineHoistedLocalSlotsOpt, value2.StateMachineAwaiterSlotsOpt, out hoistedLocalMap, out awaiterMap);
				hoistedLocalSlotCount = value2.StateMachineHoistedLocalSlotsOpt.Length;
				awaiterSlotCount = value2.StateMachineAwaiterSlotsOpt.Length;
				previousLocals = ImmutableArray<EncLocalInfo>.Empty;
				stateMachineTypeName = value2.StateMachineTypeName;
			}
			else
			{
				previousLocals = value2.Locals;
			}
			symbolMap = SourceToPreviousSymbolMatcher;
		}
		else
		{
			EditAndContinueMethodDebugInformation debugInfo;
			StandaloneSignatureHandle standaloneSignatureHandle;
			try
			{
				debugInfo = Baseline.DebugInformationProvider(handle);
				standaloneSignatureHandle = Baseline.LocalSignatureProvider(handle);
			}
			catch (Exception ex) when (((ex is InvalidDataException || ex is IOException || ex is BadImageFormatException) ? 1 : 0) != 0)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_InvalidDebugInfo, method.GetFirstLocation(), method, MetadataTokens.GetToken(handle), method.ContainingAssembly, ex.Message));
				return null;
			}
			if (debugInfo.Lambdas.IsDefaultOrEmpty)
			{
				methodId = null;
			}
			else
			{
				methodId = new DebugId(debugInfo.MethodOrdinal, 0);
				IMethodSymbolInternal methodSymbol = GetMethodSymbol(handle);
				MakeLambdaAndClosureMapFromMetadata(debugInfo, methodSymbol, methodId.Value, out lambdaMap, out closureMap);
			}
			readOnlyDictionary = MakeStateMachineStateMap(debugInfo.StateMachineStates);
			if (!debugInfo.StateMachineStates.IsDefaultOrEmpty)
			{
				firstUnusedIncreasingStateMachineState = debugInfo.StateMachineStates.Max((StateMachineStateDebugInfo s) => s.StateNumber) + 1;
				firstUnusedDecreasingStateMachineState = debugInfo.StateMachineStates.Min((StateMachineStateDebugInfo s) => s.StateNumber) - 1;
			}
			ITypeSymbolInternal typeSymbolInternal = TryGetStateMachineType(handle);
			if (typeSymbolInternal != null)
			{
				ImmutableArray<LocalSlotDebugInfo> localSlotDebugInfo = debugInfo.LocalSlots.NullToEmpty();
				GetStateMachineFieldMapFromMetadata(typeSymbolInternal, localSlotDebugInfo, out hoistedLocalMap, out awaiterMap, out awaiterSlotCount);
				hoistedLocalSlotCount = localSlotDebugInfo.Length;
				previousLocals = ImmutableArray<EncLocalInfo>.Empty;
				stateMachineTypeName = typeSymbolInternal.Name;
			}
			else
			{
				if (method.IsAsync)
				{
					if (compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_AsyncStateMachineAttribute__ctor) == null)
					{
						ReportMissingStateMachineAttribute(diagnostics, method, AttributeDescription.AsyncStateMachineAttribute.FullName);
						return null;
					}
				}
				else if (method.IsIterator && compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_IteratorStateMachineAttribute__ctor) == null)
				{
					ReportMissingStateMachineAttribute(diagnostics, method, AttributeDescription.IteratorStateMachineAttribute.FullName);
					return null;
				}
				try
				{
					previousLocals = (standaloneSignatureHandle.IsNil ? ImmutableArray<EncLocalInfo>.Empty : GetLocalSlotMapFromMetadata(standaloneSignatureHandle, debugInfo));
				}
				catch (Exception ex2) when (ex2 is UnsupportedSignatureContent || ex2 is BadImageFormatException || ex2 is IOException)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_InvalidDebugInfo, method.GetFirstLocation(), method, MetadataTokens.GetToken(standaloneSignatureHandle), method.ContainingAssembly, ex2.Message));
					return null;
				}
			}
			symbolMap = SourceToMetadataSymbolMatcher;
		}
		return new EncVariableSlotAllocator(symbolMap, value, methodId, previousLocals, lambdaMap, closureMap, stateMachineTypeName, hoistedLocalSlotCount, hoistedLocalMap, awaiterSlotCount, awaiterMap, readOnlyDictionary, firstUnusedIncreasingStateMachineState, firstUnusedDecreasingStateMachineState, GetLambdaSyntaxFacts());
	}

	internal MethodInstrumentation GetMethodBodyInstrumentations(IMethodSymbolInternal method)
	{
		if (!_methodInstrumentations.TryGetValue(method, out var value))
		{
			return MethodInstrumentation.Empty;
		}
		return value;
	}

	protected abstract LambdaSyntaxFacts GetLambdaSyntaxFacts();

	private void ReportMissingStateMachineAttribute(DiagnosticBag diagnostics, IMethodSymbolInternal method, string stateMachineAttributeFullName)
	{
		diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_EncUpdateFailedMissingSymbol, method.GetFirstLocation(), CodeAnalysisResources.Attribute, stateMachineAttributeFullName));
	}

	private static IReadOnlyDictionary<int, EncLambdaMapValue> MakeLambdaMap(ImmutableArray<EncLambdaInfo> lambdaDebugInfo)
	{
		return lambdaDebugInfo.ToImmutableSegmentedDictionary((EncLambdaInfo info) => info.DebugInfo.SyntaxOffset, (EncLambdaInfo info) => new EncLambdaMapValue(info.DebugInfo.LambdaId, info.DebugInfo.ClosureOrdinal, info.StructClosureIds));
	}

	private static IReadOnlyDictionary<int, EncClosureMapValue> MakeClosureMap(ImmutableArray<EncClosureInfo> closureDebugInfo)
	{
		return closureDebugInfo.ToImmutableSegmentedDictionary((EncClosureInfo info) => info.DebugInfo.SyntaxOffset, (EncClosureInfo info) => new EncClosureMapValue(info.DebugInfo.ClosureId, info.ParentDebugId, info.StructCaptures));
	}

	private void MakeLambdaAndClosureMapFromMetadata(EditAndContinueMethodDebugInformation debugInfo, IMethodSymbolInternal method, DebugId methodId, out IReadOnlyDictionary<int, EncLambdaMapValue> lambdaMap, out IReadOnlyDictionary<int, EncClosureMapValue> closureMap)
	{
		MetadataLambdasAndClosures map = GetMetadataLambdaAndClosureMap(method.ContainingType, methodId);
		lambdaMap = debugInfo.Lambdas.ToImmutableSegmentedDictionary((LambdaDebugInfo info) => info.SyntaxOffset, (LambdaDebugInfo info) => new EncLambdaMapValue(info.LambdaId, info.ClosureOrdinal, getLambdaStructClosureIdsFromMetadata(map.GetLambdaSymbol(info.LambdaId), methodId)));
		closureMap = debugInfo.Closures.ToImmutableSegmentedDictionary((ClosureDebugInfo info) => info.SyntaxOffset, delegate(ClosureDebugInfo info)
		{
			var (parentId, structCaptures) = map.TryGetClosureInfo(info.ClosureId);
			return new EncClosureMapValue(info.ClosureId, parentId, structCaptures);
		});
		ImmutableArray<DebugId> getLambdaStructClosureIdsFromMetadata(IMethodSymbolInternal? lambda, DebugId debugId)
		{
			if (lambda == null || lambda.Parameters.Length == 0)
			{
				return ImmutableArray<DebugId>.Empty;
			}
			ArrayBuilder<DebugId> instance = ArrayBuilder<DebugId>.GetInstance(lambda.Parameters.Length);
			int suffixIndex = default(int);
			char idSeparator = default(char);
			bool isDisplayClass = default(bool);
			bool hasDebugIds = default(bool);
			foreach (IParameterSymbolInternal parameter in lambda.Parameters)
			{
				string name = parameter.Type.Name;
				if (((parameter.RefKind == RefKind.Ref && TryParseDisplayClassOrLambdaName(name, out suffixIndex, out idSeparator, out isDisplayClass, out var _, out hasDebugIds)) & isDisplayClass & hasDebugIds) && CommonGeneratedNames.TryParseDebugIds(System.MemoryExtensions.AsSpan(name, suffixIndex), idSeparator, isMethodIdOptional: false, out var methodId2, out var entityId) && methodId2 == debugId)
				{
					instance.Add(entityId);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	private static IReadOnlyDictionary<(int syntaxOffset, AwaitDebugId debugId), StateMachineState>? MakeStateMachineStateMap(ImmutableArray<StateMachineStateDebugInfo> debugInfos)
	{
		if (!debugInfos.IsDefault)
		{
			return debugInfos.ToImmutableSegmentedDictionary((StateMachineStateDebugInfo entry) => (SyntaxOffset: entry.SyntaxOffset, AwaitId: entry.AwaitId), (StateMachineStateDebugInfo entry) => entry.StateNumber);
		}
		return null;
	}

	private static void GetStateMachineFieldMapFromPreviousCompilation(ImmutableArray<EncHoistedLocalInfo> hoistedLocalSlots, ImmutableArray<ITypeReference?> hoistedAwaiters, out IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap, out IReadOnlyDictionary<ITypeReference, int> awaiterMap)
	{
		Dictionary<EncHoistedLocalInfo, int> dictionary = new Dictionary<EncHoistedLocalInfo, int>();
		Dictionary<ITypeReference, int> dictionary2 = new Dictionary<ITypeReference, int>(SymbolEquivalentEqualityComparer.Instance);
		for (int i = 0; i < hoistedLocalSlots.Length; i++)
		{
			EncHoistedLocalInfo key = hoistedLocalSlots[i];
			if (!key.IsUnused)
			{
				dictionary.Add(key, i);
			}
		}
		for (int j = 0; j < hoistedAwaiters.Length; j++)
		{
			ITypeReference typeReference = hoistedAwaiters[j];
			if (typeReference != null)
			{
				dictionary2.Add(typeReference, j);
			}
		}
		hoistedLocalMap = dictionary;
		awaiterMap = dictionary2;
	}

	protected abstract bool TryParseDisplayClassOrLambdaName(string name, out int suffixIndex, out char idSeparator, out bool isDisplayClass, out bool isDisplayClassParentField, out bool hasDebugIds);

	private MetadataLambdasAndClosures GetMetadataLambdaAndClosureMap(INamedTypeSymbolInternal peType, DebugId methodId)
	{
		return ImmutableInterlocked.GetOrAdd(ref _metadataLambdasAndClosures, peType, (INamedTypeSymbolInternal type, (DefinitionMap self, DebugId methodId) arg) => arg.self.CreateLambdaAndClosureMap(type.GetMembers(), null, arg.methodId), (this, methodId));
	}

	private MetadataLambdasAndClosures CreateLambdaAndClosureMap(ImmutableArray<ISymbolInternal> members, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>? synthesizedMemberMap, DebugId methodId)
	{
		ArrayBuilder<(DebugId id, IMethodSymbolInternal symbol)> lambdasBuilder = ArrayBuilder<(DebugId, IMethodSymbolInternal)>.GetInstance();
		ImmutableSegmentedDictionary<DebugId, (DebugId? parentId, ImmutableArray<string> structCaptures)>.Builder closureTreeBuilder = ImmutableSegmentedDictionary.CreateBuilder<DebugId, (DebugId?, ImmutableArray<string>)>();
		recurse(members, null);
		lambdasBuilder.Sort(((DebugId id, IMethodSymbolInternal symbol) x, (DebugId id, IMethodSymbolInternal symbol) y) => x.id.CompareTo(y.id));
		return new MetadataLambdasAndClosures(lambdasBuilder.ToImmutableAndFree(), closureTreeBuilder.ToImmutable());
		static ImmutableArray<string> getHoistedVariableNames(ImmutableArray<ISymbolInternal> immutableArray)
		{
			ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
			foreach (ISymbolInternal item in immutableArray)
			{
				if (item != null && item.Kind == SymbolKind.Field && !item.IsStatic)
				{
					instance.Add(item.Name);
				}
			}
			return instance.ToImmutableAndFree();
		}
		void recurse(ImmutableArray<ISymbolInternal> immutableArray, DebugId? containingDisplayClassId)
		{
			foreach (ISymbolInternal item2 in immutableArray)
			{
				string name = item2.Name;
				if (TryParseDisplayClassOrLambdaName(name, out var suffixIndex, out var idSeparator, out var isDisplayClass, out var isDisplayClassParentField, out var hasDebugIds))
				{
					DebugId methodId2 = default(DebugId);
					DebugId entityId = default(DebugId);
					if (!hasDebugIds || (CommonGeneratedNames.TryParseDebugIds(System.MemoryExtensions.AsSpan(name, suffixIndex), idSeparator, containingDisplayClassId.HasValue, out methodId2, out entityId) && (containingDisplayClassId.HasValue || !(methodId2 != methodId))))
					{
						if (isDisplayClass)
						{
							INamedTypeSymbolInternal namedTypeSymbolInternal = (INamedTypeSymbolInternal)item2;
							ImmutableArray<ISymbolInternal> members2 = ((synthesizedMemberMap == null) ? namedTypeSymbolInternal.GetMembers() : (synthesizedMemberMap.TryGetValue(namedTypeSymbolInternal, out ImmutableArray<ISymbolInternal> value) ? value : ImmutableArray<ISymbolInternal>.Empty));
							if (namedTypeSymbolInternal.TypeKind == TypeKind.Struct)
							{
								ImmutableSegmentedDictionary<DebugId, (DebugId? parentId, ImmutableArray<string> structCaptures)>.Builder builder = closureTreeBuilder;
								DebugId key = entityId;
								(DebugId?, ImmutableArray<string>) valueOrDefault = closureTreeBuilder.GetValueOrDefault(entityId);
								valueOrDefault.Item2 = getHoistedVariableNames(members2);
								builder[key] = valueOrDefault;
							}
							recurse(members2, hasDebugIds ? new DebugId?(entityId) : ((DebugId?)null));
						}
						else if (isDisplayClassParentField)
						{
							if (item2 is IFieldSymbolInternal fieldSymbolInternal && tryParseDisplayClassDebugId(fieldSymbolInternal.Type.Name, out var id))
							{
								ImmutableSegmentedDictionary<DebugId, (DebugId? parentId, ImmutableArray<string> structCaptures)>.Builder builder2 = closureTreeBuilder;
								DebugId value2 = containingDisplayClassId.Value;
								(DebugId?, ImmutableArray<string>) valueOrDefault = closureTreeBuilder.GetValueOrDefault(containingDisplayClassId.Value);
								valueOrDefault.Item1 = id;
								builder2[value2] = valueOrDefault;
							}
						}
						else
						{
							lambdasBuilder.Add((entityId, (IMethodSymbolInternal)item2));
						}
					}
				}
			}
		}
		bool tryParseDisplayClassDebugId(string displayClassName, out DebugId id)
		{
			if ((TryParseDisplayClassOrLambdaName(displayClassName, out var suffixIndex, out var idSeparator, out var isDisplayClass, out var _, out var hasDebugIds) & isDisplayClass & hasDebugIds) && CommonGeneratedNames.TryParseDebugIds(System.MemoryExtensions.AsSpan(displayClassName, suffixIndex), idSeparator, isMethodIdOptional: false, out var methodId2, out var entityId) && methodId2 == methodId)
			{
				id = entityId;
				return true;
			}
			id = default(DebugId);
			return false;
		}
	}

	public IEnumerable<(DebugId id, IMethodSymbolInternal symbol)> GetDeletedSynthesizedMethods(IMethodSymbolInternal oldMethod, ImmutableArray<EncLambdaInfo> currentLambdas)
	{
		int rowNumber = MetadataTokens.GetRowNumber(GetPreviousMethodHandle(oldMethod, out IMethodSymbolInternal peMethod));
		if (Baseline.AddedOrChangedMethods.TryGetValue(rowNumber, out var value))
		{
			if (!value.LambdaDebugInfo.IsDefaultOrEmpty && Baseline.SynthesizedMembers.TryGetValue(oldMethod.ContainingType, out ImmutableArray<ISymbolInternal> value2))
			{
				return getDeletedLambdas(CreateLambdaAndClosureMap(value2, Baseline.SynthesizedMembers, value.MethodId), value.LambdaDebugInfo);
			}
			return Array.Empty<(DebugId, IMethodSymbolInternal)>();
		}
		EditAndContinueMethodDebugInformation editAndContinueMethodDebugInformation;
		try
		{
			editAndContinueMethodDebugInformation = Baseline.DebugInformationProvider(MetadataTokens.MethodDefinitionHandle(rowNumber));
		}
		catch (Exception ex) when (((ex is InvalidDataException || ex is IOException || ex is BadImageFormatException) ? 1 : 0) != 0)
		{
			return Array.Empty<(DebugId, IMethodSymbolInternal)>();
		}
		if (editAndContinueMethodDebugInformation.Lambdas.IsDefaultOrEmpty)
		{
			return Array.Empty<(DebugId, IMethodSymbolInternal)>();
		}
		MetadataLambdasAndClosures metadataLambdaAndClosureMap = GetMetadataLambdaAndClosureMap(peMethod.ContainingType, new DebugId(editAndContinueMethodDebugInformation.MethodOrdinal, 0));
		ImmutableArray<LambdaDebugInfo> lambdas = editAndContinueMethodDebugInformation.Lambdas;
		return getDeletedLambdas(metadataLambdaAndClosureMap, default(ImmutableArray<EncLambdaInfo>), lambdas);
		IEnumerable<(DebugId id, IMethodSymbolInternal symbol)> getDeletedLambdas(MetadataLambdasAndClosures map, ImmutableArray<EncLambdaInfo> lambdasToInclude = default(ImmutableArray<EncLambdaInfo>), ImmutableArray<LambdaDebugInfo> metadataLambdasToInclude = default(ImmutableArray<LambdaDebugInfo>))
		{
			PooledHashSet<DebugId> lambdaIdSet = PooledHashSet<DebugId>.GetInstance();
			foreach (EncLambdaInfo item in lambdasToInclude.NullToEmpty())
			{
				lambdaIdSet.Add(item.DebugInfo.LambdaId);
			}
			foreach (LambdaDebugInfo item2 in metadataLambdasToInclude.NullToEmpty())
			{
				lambdaIdSet.Add(item2.LambdaId);
			}
			foreach (EncLambdaInfo item3 in currentLambdas)
			{
				lambdaIdSet.Remove(item3.DebugInfo.LambdaId);
			}
			foreach (var lambdaSymbol in map.LambdaSymbols)
			{
				if (lambdaIdSet.Contains(lambdaSymbol.id))
				{
					yield return lambdaSymbol;
				}
			}
			lambdaIdSet.Free();
		}
	}
}
