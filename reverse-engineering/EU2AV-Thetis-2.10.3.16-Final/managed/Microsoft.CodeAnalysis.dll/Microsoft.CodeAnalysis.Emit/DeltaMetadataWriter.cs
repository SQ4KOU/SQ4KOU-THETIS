using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit.EditAndContinue;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class DeltaMetadataWriter : MetadataWriter
{
	private abstract class DefinitionIndexBase<T> where T : notnull
	{
		protected readonly Dictionary<T, int> added;

		protected readonly List<T> rows;

		private readonly int _firstRowId;

		private bool _frozen;

		public int FirstRowId => _firstRowId;

		public int NextRowId => added.Count + _firstRowId;

		public bool IsFrozen => _frozen;

		public DefinitionIndexBase(int lastRowId, IEqualityComparer<T>? comparer = null)
		{
			added = new Dictionary<T, int>(comparer);
			rows = new List<T>();
			_firstRowId = lastRowId + 1;
		}

		public abstract bool TryGetRowId(T item, out int rowId);

		public int GetRowId(T item)
		{
			if (!TryGetRowId(item, out var rowId) || rowId == 0)
			{
				throw ExceptionUtilities.UnexpectedValue(item);
			}
			return rowId;
		}

		public bool Contains(T item)
		{
			int rowId;
			return TryGetRowId(item, out rowId);
		}

		public IReadOnlyDictionary<T, int> GetAdded()
		{
			Freeze();
			return added;
		}

		public IReadOnlyList<T> GetRows()
		{
			Freeze();
			return rows;
		}

		protected virtual void OnFrozen()
		{
		}

		private void Freeze()
		{
			if (!_frozen)
			{
				_frozen = true;
				OnFrozen();
			}
		}
	}

	private sealed class DefinitionIndex<T> : DefinitionIndexBase<T> where T : class, IDefinition
	{
		public delegate bool TryGetExistingIndex(T item, out int index);

		private readonly TryGetExistingIndex _tryGetExistingIndex;

		private readonly Dictionary<int, T> _map;

		public DefinitionIndex(TryGetExistingIndex tryGetExistingIndex, int lastRowId)
			: base(lastRowId, (IEqualityComparer<T>?)ReferenceEqualityComparer.Instance)
		{
			_tryGetExistingIndex = tryGetExistingIndex;
			_map = new Dictionary<int, T>();
		}

		public override bool TryGetRowId(T item, out int index)
		{
			if (added.TryGetValue(item, out index))
			{
				return true;
			}
			if (_tryGetExistingIndex(item, out index))
			{
				_map[index] = item;
				return true;
			}
			return false;
		}

		public T GetDefinition(int rowId)
		{
			return _map[rowId];
		}

		public void Add(T item)
		{
			int nextRowId = base.NextRowId;
			added.Add(item, nextRowId);
			_map[nextRowId] = item;
			rows.Add(item);
		}

		public void AddUpdated(T item)
		{
			rows.Add(item);
		}

		public bool IsAddedNotChanged(T item)
		{
			return added.ContainsKey(item);
		}

		protected override void OnFrozen()
		{
			rows.Sort((T x, T y) => GetRowId(x).CompareTo(GetRowId(y)));
		}
	}

	private sealed class GenericParameterIndex : DefinitionIndexBase<IGenericParameter>
	{
		public GenericParameterIndex(int lastRowId)
			: base(lastRowId, (IEqualityComparer<IGenericParameter>?)ReferenceEqualityComparer.Instance)
		{
		}

		public override bool TryGetRowId(IGenericParameter item, out int index)
		{
			return added.TryGetValue(item, out index);
		}

		public void Add(IGenericParameter item)
		{
			int nextRowId = base.NextRowId;
			added.Add(item, nextRowId);
			rows.Add(item);
		}
	}

	private sealed class EventOrPropertyMapIndex : DefinitionIndexBase<int>
	{
		public delegate bool TryGetExistingIndex(int item, out int index);

		private readonly TryGetExistingIndex _tryGetExistingIndex;

		public EventOrPropertyMapIndex(TryGetExistingIndex tryGetExistingIndex, int lastRowId)
			: base(lastRowId, (IEqualityComparer<int>?)null)
		{
			_tryGetExistingIndex = tryGetExistingIndex;
		}

		public override bool TryGetRowId(int item, out int index)
		{
			if (added.TryGetValue(item, out index))
			{
				return true;
			}
			if (_tryGetExistingIndex(item, out index))
			{
				return true;
			}
			index = 0;
			return false;
		}

		public void Add(int item)
		{
			int nextRowId = base.NextRowId;
			added.Add(item, nextRowId);
			rows.Add(item);
		}
	}

	private sealed class MethodImplIndex : DefinitionIndexBase<MethodImplKey>
	{
		private readonly DeltaMetadataWriter _writer;

		public MethodImplIndex(DeltaMetadataWriter writer, int lastRowId)
			: base(lastRowId, (IEqualityComparer<MethodImplKey>?)null)
		{
			_writer = writer;
		}

		public override bool TryGetRowId(MethodImplKey item, out int index)
		{
			if (added.TryGetValue(item, out index))
			{
				return true;
			}
			if (_writer.TryGetExistingMethodImplIndex(item, out index))
			{
				return true;
			}
			index = 0;
			return false;
		}

		public void Add(MethodImplKey item)
		{
			int nextRowId = base.NextRowId;
			added.Add(item, nextRowId);
			rows.Add(item);
		}
	}

	private sealed class DeltaReferenceIndexer : ReferenceIndexer
	{
		private readonly SymbolChanges _changes;

		private readonly IReadOnlyDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> _deletedTypeMembers;

		public DeltaReferenceIndexer(DeltaMetadataWriter writer)
			: base(writer)
		{
			_changes = writer.Changes;
			_deletedTypeMembers = writer._deletedTypeMembers;
		}

		public override void Visit(CommonPEModuleBuilder module)
		{
			Visit(module.GetTopLevelTypeDefinitions(metadataWriter.Context));
			INamedTypeSymbolInternal usedSynthesizedHotReloadExceptionType = module.GetUsedSynthesizedHotReloadExceptionType();
			if (usedSynthesizedHotReloadExceptionType != null)
			{
				Visit((INamedTypeDefinition)usedSynthesizedHotReloadExceptionType.GetCciAdapter());
			}
		}

		public override void Visit(IEventDefinition eventDefinition)
		{
			base.Visit(eventDefinition);
		}

		public override void Visit(IFieldDefinition fieldDefinition)
		{
			base.Visit(fieldDefinition);
		}

		public override void Visit(ILocalDefinition localDefinition)
		{
			if (localDefinition.Signature == null)
			{
				base.Visit(localDefinition);
			}
		}

		public override void Visit(IMethodDefinition method)
		{
			base.Visit(method);
		}

		public override void Visit(Microsoft.Cci.MethodImplementation methodImplementation)
		{
			IMethodDefinition def = (IMethodDefinition)methodImplementation.ImplementingMethod.AsDefinition(Context);
			if (_changes.GetChange(def) == SymbolChange.Added)
			{
				base.Visit(methodImplementation);
			}
		}

		public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition)
		{
			base.Visit(namespaceTypeDefinition);
		}

		public override void Visit(INestedTypeDefinition nestedTypeDefinition)
		{
			base.Visit(nestedTypeDefinition);
		}

		public override void Visit(IPropertyDefinition propertyDefinition)
		{
			base.Visit(propertyDefinition);
		}

		public override void Visit(ITypeDefinition typeDefinition)
		{
			if (ShouldVisit(typeDefinition))
			{
				base.Visit(typeDefinition);
				if (_deletedTypeMembers.TryGetValue(typeDefinition, out ImmutableArray<ITypeDefinitionMember> value))
				{
					Visit(value);
				}
			}
		}

		public override void Visit(ITypeDefinitionMember typeMember)
		{
			if (ShouldVisit(typeMember))
			{
				base.Visit(typeMember);
			}
		}

		private bool ShouldVisit(IDefinition def)
		{
			if (!def.IsEncDeleted)
			{
				return _changes.GetChange(def) != SymbolChange.None;
			}
			return true;
		}
	}

	private readonly EmitBaseline _previousGeneration;

	private readonly Guid _encId;

	private readonly DefinitionMap _definitionMap;

	private readonly List<ITypeDefinition> _changedTypeDefs;

	private readonly Dictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> _deletedTypeMembers;

	private readonly IReadOnlyDictionary<ITypeDefinition, ArrayBuilder<ITypeDefinitionMember>> _deletedMemberDefs;

	private readonly DefinitionIndex<ITypeDefinition> _typeDefs;

	private readonly DefinitionIndex<IEventDefinition> _eventDefs;

	private readonly DefinitionIndex<IFieldDefinition> _fieldDefs;

	private readonly DefinitionIndex<IMethodDefinition> _methodDefs;

	private readonly DefinitionIndex<IPropertyDefinition> _propertyDefs;

	private readonly DefinitionIndex<IParameterDefinition> _parameterDefs;

	private readonly Dictionary<IParameterDefinition, IMethodDefinition> _parameterDefList;

	private readonly GenericParameterIndex _genericParameters;

	private readonly EventOrPropertyMapIndex _eventMap;

	private readonly EventOrPropertyMapIndex _propertyMap;

	private readonly MethodImplIndex _methodImpls;

	private readonly Dictionary<EntityHandle, ImmutableArray<int>> _customAttributesAdded;

	private readonly ArrayBuilder<int> _customAttributeRowIds;

	private readonly List<(EntityHandle parentHandle, IEnumerator<ICustomAttribute> attributeEnumerator)> _deferredCustomAttributes = new List<(EntityHandle, IEnumerator<ICustomAttribute>)>();

	private readonly Dictionary<IParameterDefinition, int> _existingParameterDefs;

	private readonly Dictionary<MethodDefinitionHandle, int> _firstParamRowMap;

	private readonly HeapOrReferenceIndex<AssemblyIdentity> _assemblyRefIndex;

	private readonly HeapOrReferenceIndex<string> _moduleRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<ITypeMemberReference> _memberRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference> _methodSpecIndex;

	private readonly TypeReferenceIndex _typeRefIndex;

	private readonly InstanceAndStructuralReferenceIndex<ITypeReference> _typeSpecIndex;

	private readonly HeapOrReferenceIndex<BlobHandle> _standAloneSignatureIndex;

	private readonly Dictionary<IMethodDefinition, AddedOrChangedMethodInfo> _addedOrChangedMethods;

	public SymbolChanges Changes => Context.Module.EncSymbolChanges;

	protected override ushort Generation => (ushort)(_previousGeneration.Ordinal + 1);

	protected override Guid EncId => _encId;

	protected override Guid EncBaseId => _previousGeneration.EncId;

	protected override int GreatestMethodDefIndex => _methodDefs.NextRowId;

	public DeltaMetadataWriter(EmitContext context, CommonMessageProvider messageProvider, EmitBaseline previousGeneration, Guid encId, DefinitionMap definitionMap, CancellationToken cancellationToken)
		: base(MakeTablesBuilder(previousGeneration), (context.Module.DebugInformationFormat == DebugInformationFormat.PortablePdb) ? new MetadataBuilder() : null, null, context, messageProvider, metadataOnly: false, deterministic: false, emitTestCoverageData: false, cancellationToken)
	{
		_previousGeneration = previousGeneration;
		_encId = encId;
		_definitionMap = definitionMap;
		ImmutableArray<int> tableSizes = previousGeneration.TableSizes;
		_changedTypeDefs = new List<ITypeDefinition>();
		_deletedTypeMembers = new Dictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>>(ReferenceEqualityComparer.Instance);
		_deletedMemberDefs = context.Module.GetDeletedMemberDefinitions();
		_typeDefs = new DefinitionIndex<ITypeDefinition>(TryGetExistingTypeDefIndex, tableSizes[2]);
		_eventDefs = new DefinitionIndex<IEventDefinition>(TryGetExistingEventDefIndex, tableSizes[20]);
		_fieldDefs = new DefinitionIndex<IFieldDefinition>(TryGetExistingFieldDefIndex, tableSizes[4]);
		_methodDefs = new DefinitionIndex<IMethodDefinition>(TryGetExistingMethodDefIndex, tableSizes[6]);
		_propertyDefs = new DefinitionIndex<IPropertyDefinition>(TryGetExistingPropertyDefIndex, tableSizes[23]);
		_parameterDefs = new DefinitionIndex<IParameterDefinition>(TryGetExistingParameterDefIndex, tableSizes[8]);
		_parameterDefList = new Dictionary<IParameterDefinition, IMethodDefinition>(SymbolEquivalentEqualityComparer.Instance);
		_genericParameters = new GenericParameterIndex(tableSizes[42]);
		_eventMap = new EventOrPropertyMapIndex(TryGetExistingEventMapIndex, tableSizes[18]);
		_propertyMap = new EventOrPropertyMapIndex(TryGetExistingPropertyMapIndex, tableSizes[21]);
		_methodImpls = new MethodImplIndex(this, tableSizes[25]);
		_customAttributesAdded = new Dictionary<EntityHandle, ImmutableArray<int>>();
		_customAttributeRowIds = ArrayBuilder<int>.GetInstance();
		_firstParamRowMap = new Dictionary<MethodDefinitionHandle, int>();
		_existingParameterDefs = new Dictionary<IParameterDefinition, int>(ReferenceEqualityComparer.Instance);
		_assemblyRefIndex = new HeapOrReferenceIndex<AssemblyIdentity>(this, tableSizes[35]);
		_moduleRefIndex = new HeapOrReferenceIndex<string>(this, tableSizes[26]);
		_memberRefIndex = new InstanceAndStructuralReferenceIndex<ITypeMemberReference>(this, new MemberRefComparer(this), tableSizes[10]);
		_methodSpecIndex = new InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference>(this, new MethodSpecComparer(this), tableSizes[43]);
		_typeRefIndex = new TypeReferenceIndex(this, tableSizes[1]);
		_typeSpecIndex = new InstanceAndStructuralReferenceIndex<ITypeReference>(this, new TypeSpecComparer(this), tableSizes[27]);
		_standAloneSignatureIndex = new HeapOrReferenceIndex<BlobHandle>(this, tableSizes[17]);
		_addedOrChangedMethods = new Dictionary<IMethodDefinition, AddedOrChangedMethodInfo>(SymbolEquivalentEqualityComparer.Instance);
	}

	private static MetadataBuilder MakeTablesBuilder(EmitBaseline previousGeneration)
	{
		return new MetadataBuilder(Math.Min(16777214, previousGeneration.UserStringStreamLength), previousGeneration.StringStreamLength, previousGeneration.BlobStreamLength, previousGeneration.GuidStreamLength);
	}

	private ImmutableArray<int> GetDeltaTableSizes(ImmutableArray<int> rowCounts)
	{
		int[] array = new int[MetadataTokens.TableCount];
		rowCounts.CopyTo(array);
		array[1] = _typeRefIndex.Rows.Count;
		array[2] = _typeDefs.GetAdded().Count;
		array[4] = _fieldDefs.GetAdded().Count;
		array[6] = _methodDefs.GetAdded().Count;
		array[8] = _parameterDefs.GetAdded().Count;
		array[10] = _memberRefIndex.Rows.Count;
		array[17] = _standAloneSignatureIndex.Rows.Count;
		array[18] = _eventMap.GetAdded().Count;
		array[20] = _eventDefs.GetAdded().Count;
		array[21] = _propertyMap.GetAdded().Count;
		array[23] = _propertyDefs.GetAdded().Count;
		array[25] = _methodImpls.GetAdded().Count;
		array[26] = _moduleRefIndex.Rows.Count;
		array[27] = _typeSpecIndex.Rows.Count;
		array[35] = _assemblyRefIndex.Rows.Count;
		array[42] = _genericParameters.GetAdded().Count;
		array[43] = _methodSpecIndex.Rows.Count;
		return ImmutableArray.Create(array);
	}

	internal EmitBaseline GetDelta(Compilation compilation, Guid encId, MetadataSizes metadataSizes)
	{
		Dictionary<int, AddedOrChangedMethodInfo> dictionary = new Dictionary<int, AddedOrChangedMethodInfo>();
		foreach (KeyValuePair<IMethodDefinition, AddedOrChangedMethodInfo> addedOrChangedMethod in _addedOrChangedMethods)
		{
			dictionary.Add(MetadataTokens.GetRowNumber(GetMethodDefinitionHandle(addedOrChangedMethod.Key)), addedOrChangedMethod.Value);
		}
		ImmutableArray<int> tableEntriesAdded = _previousGeneration.TableEntriesAdded;
		ImmutableArray<int> deltaTableSizes = GetDeltaTableSizes(metadataSizes.RowCounts);
		int[] array = new int[MetadataTokens.TableCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = tableEntriesAdded[i] + deltaTableSizes[i];
		}
		IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> readOnlyDictionary;
		if (_previousGeneration.Ordinal != 0)
		{
			readOnlyDictionary = _previousGeneration.SynthesizedMembers;
		}
		else
		{
			IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> allSynthesizedMembers = module.GetAllSynthesizedMembers();
			readOnlyDictionary = allSynthesizedMembers;
		}
		IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> synthesizedMembers = readOnlyDictionary;
		SynthesizedTypeMaps synthesizedTypes = ((_previousGeneration.Ordinal == 0) ? module.GetAllSynthesizedTypes() : _previousGeneration.SynthesizedTypes);
		IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> deletedMembers = ((_previousGeneration.Ordinal == 0) ? module.EncSymbolChanges.DeletedMembers : _previousGeneration.DeletedMembers);
		int num = _previousGeneration.Ordinal + 1;
		IReadOnlyDictionary<ITypeDefinition, int> added = _typeDefs.GetAdded();
		Dictionary<IDefinition, int> dictionary2 = CreateDictionary(_previousGeneration.GenerationOrdinals, SymbolEquivalentEqualityComparer.Instance);
		foreach (var (typeDefinition2, _) in added)
		{
			if (Changes.IsReplacedDef(typeDefinition2))
			{
				dictionary2[typeDefinition2] = num;
			}
		}
		return _previousGeneration.With(compilation, module, num, encId, dictionary2, AddRange(_previousGeneration.TypesAdded, added, SymbolEquivalentEqualityComparer.Instance), AddRange(_previousGeneration.EventsAdded, _eventDefs.GetAdded(), SymbolEquivalentEqualityComparer.Instance), AddRange(_previousGeneration.FieldsAdded, _fieldDefs.GetAdded(), SymbolEquivalentEqualityComparer.Instance), AddRange(_previousGeneration.MethodsAdded, _methodDefs.GetAdded(), SymbolEquivalentEqualityComparer.Instance), AddRange(_previousGeneration.FirstParamRowMap, _firstParamRowMap), AddRange(_previousGeneration.PropertiesAdded, _propertyDefs.GetAdded(), SymbolEquivalentEqualityComparer.Instance), AddRange(_previousGeneration.EventMapAdded, _eventMap.GetAdded()), AddRange(_previousGeneration.PropertyMapAdded, _propertyMap.GetAdded()), AddRange(_previousGeneration.MethodImplsAdded, _methodImpls.GetAdded()), AddRange(_previousGeneration.CustomAttributesAdded, _customAttributesAdded), ImmutableArray.Create(array), metadataSizes.GetAlignedHeapSize(HeapIndex.Blob) + _previousGeneration.BlobStreamLengthAdded, metadataSizes.HeapSizes[1] + _previousGeneration.StringStreamLengthAdded, metadataSizes.GetAlignedHeapSize(HeapIndex.UserString) + _previousGeneration.UserStringStreamLengthAdded, metadataSizes.HeapSizes[3], synthesizedTypes, synthesizedMembers, deletedMembers, AddRange(_previousGeneration.AddedOrChangedMethods, dictionary), _previousGeneration.DebugInformationProvider, _previousGeneration.LocalSignatureProvider);
	}

	private static Dictionary<K, V> CreateDictionary<K, V>(IReadOnlyDictionary<K, V> dictionary, IEqualityComparer<K>? comparer) where K : notnull
	{
		Dictionary<K, V> dictionary2 = new Dictionary<K, V>(comparer);
		foreach (KeyValuePair<K, V> item in dictionary)
		{
			dictionary2.Add(item.Key, item.Value);
		}
		return dictionary2;
	}

	private static IReadOnlyDictionary<K, V> AddRange<K, V>(IReadOnlyDictionary<K, V> previous, IReadOnlyDictionary<K, V> current, IEqualityComparer<K>? comparer = null) where K : notnull
	{
		if (previous.Count == 0)
		{
			return current;
		}
		if (current.Count == 0)
		{
			return previous;
		}
		Dictionary<K, V> dictionary = CreateDictionary(previous, comparer);
		foreach (KeyValuePair<K, V> item in current)
		{
			dictionary[item.Key] = item.Value;
		}
		return dictionary;
	}

	public void GetUpdatedMethodTokens(ArrayBuilder<MethodDefinitionHandle> methods)
	{
		foreach (IMethodDefinition row in _methodDefs.GetRows())
		{
			if (!_methodDefs.IsAddedNotChanged(row))
			{
				IMethodBody? body = row.GetBody(Context);
				if (body != null && body.SequencePoints.Length > 0)
				{
					methods.Add(MetadataTokens.MethodDefinitionHandle(_methodDefs.GetRowId(row)));
				}
			}
		}
	}

	public void GetChangedTypeTokens(ArrayBuilder<TypeDefinitionHandle> types)
	{
		foreach (ITypeDefinition changedTypeDef in _changedTypeDefs)
		{
			types.Add(GetTypeDefinitionHandle(changedTypeDef));
		}
	}

	protected override EventDefinitionHandle GetEventDefinitionHandle(IEventDefinition def)
	{
		return MetadataTokens.EventDefinitionHandle(_eventDefs.GetRowId(def));
	}

	protected override IReadOnlyList<IEventDefinition> GetEventDefs()
	{
		return _eventDefs.GetRows();
	}

	protected override FieldDefinitionHandle GetFieldDefinitionHandle(IFieldDefinition def)
	{
		return MetadataTokens.FieldDefinitionHandle(_fieldDefs.GetRowId(def));
	}

	protected override IReadOnlyList<IFieldDefinition> GetFieldDefs()
	{
		return _fieldDefs.GetRows();
	}

	protected override bool TryGetTypeDefinitionHandle(ITypeDefinition def, out TypeDefinitionHandle handle)
	{
		bool result = _typeDefs.TryGetRowId(def, out var rowId);
		handle = MetadataTokens.TypeDefinitionHandle(rowId);
		return result;
	}

	protected override TypeDefinitionHandle GetTypeDefinitionHandle(ITypeDefinition def)
	{
		return MetadataTokens.TypeDefinitionHandle(_typeDefs.GetRowId(def));
	}

	protected override ITypeDefinition GetTypeDef(TypeDefinitionHandle handle)
	{
		return _typeDefs.GetDefinition(MetadataTokens.GetRowNumber(handle));
	}

	protected override IReadOnlyList<ITypeDefinition> GetTypeDefs()
	{
		return _typeDefs.GetRows();
	}

	protected override bool TryGetMethodDefinitionHandle(IMethodDefinition def, out MethodDefinitionHandle handle)
	{
		bool result = _methodDefs.TryGetRowId(def, out var rowId);
		handle = MetadataTokens.MethodDefinitionHandle(rowId);
		return result;
	}

	protected override MethodDefinitionHandle GetMethodDefinitionHandle(IMethodDefinition def)
	{
		return MetadataTokens.MethodDefinitionHandle(_methodDefs.GetRowId(def));
	}

	protected override IMethodDefinition GetMethodDef(MethodDefinitionHandle index)
	{
		return _methodDefs.GetDefinition(MetadataTokens.GetRowNumber(index));
	}

	protected override IReadOnlyList<IMethodDefinition> GetMethodDefs()
	{
		return _methodDefs.GetRows();
	}

	protected override PropertyDefinitionHandle GetPropertyDefIndex(IPropertyDefinition def)
	{
		return MetadataTokens.PropertyDefinitionHandle(_propertyDefs.GetRowId(def));
	}

	protected override IReadOnlyList<IPropertyDefinition> GetPropertyDefs()
	{
		return _propertyDefs.GetRows();
	}

	protected override ParameterHandle GetParameterHandle(IParameterDefinition def)
	{
		return MetadataTokens.ParameterHandle(_parameterDefs.GetRowId(def));
	}

	protected override IReadOnlyList<IParameterDefinition> GetParameterDefs()
	{
		return _parameterDefs.GetRows();
	}

	protected override IReadOnlyList<IGenericParameter> GetGenericParameters()
	{
		return _genericParameters.GetRows();
	}

	protected override FieldDefinitionHandle GetFirstFieldDefinitionHandle(INamedTypeDefinition typeDef)
	{
		return default(FieldDefinitionHandle);
	}

	protected override MethodDefinitionHandle GetFirstMethodDefinitionHandle(INamedTypeDefinition typeDef)
	{
		return default(MethodDefinitionHandle);
	}

	protected override ParameterHandle GetFirstParameterHandle(IMethodDefinition methodDef)
	{
		return default(ParameterHandle);
	}

	protected override AssemblyReferenceHandle GetOrAddAssemblyReferenceHandle(IAssemblyReference reference)
	{
		AssemblyIdentity assemblyIdentity = reference.Identity;
		Version assemblyVersionPattern = reference.AssemblyVersionPattern;
		if ((object)assemblyVersionPattern != null)
		{
			assemblyIdentity = _previousGeneration.InitialBaseline.LazyMetadataSymbols.AssemblyReferenceIdentityMap[assemblyIdentity.WithVersion(assemblyVersionPattern)];
		}
		return MetadataTokens.AssemblyReferenceHandle(_assemblyRefIndex.GetOrAdd(assemblyIdentity));
	}

	protected override IReadOnlyList<AssemblyIdentity> GetAssemblyRefs()
	{
		return _assemblyRefIndex.Rows;
	}

	protected override ModuleReferenceHandle GetOrAddModuleReferenceHandle(string reference)
	{
		return MetadataTokens.ModuleReferenceHandle(_moduleRefIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<string> GetModuleRefs()
	{
		return _moduleRefIndex.Rows;
	}

	protected override MemberReferenceHandle GetOrAddMemberReferenceHandle(ITypeMemberReference reference)
	{
		return MetadataTokens.MemberReferenceHandle(_memberRefIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<ITypeMemberReference> GetMemberRefs()
	{
		return _memberRefIndex.Rows;
	}

	protected override MethodSpecificationHandle GetOrAddMethodSpecificationHandle(IGenericMethodInstanceReference reference)
	{
		return MetadataTokens.MethodSpecificationHandle(_methodSpecIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<IGenericMethodInstanceReference> GetMethodSpecs()
	{
		return _methodSpecIndex.Rows;
	}

	protected override bool TryGetTypeReferenceHandle(ITypeReference reference, out TypeReferenceHandle handle)
	{
		bool result = _typeRefIndex.TryGetValue(reference, out var index);
		handle = MetadataTokens.TypeReferenceHandle(index);
		return result;
	}

	protected override TypeReferenceHandle GetOrAddTypeReferenceHandle(ITypeReference reference)
	{
		return MetadataTokens.TypeReferenceHandle(_typeRefIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<ITypeReference> GetTypeRefs()
	{
		return _typeRefIndex.Rows;
	}

	protected override TypeSpecificationHandle GetOrAddTypeSpecificationHandle(ITypeReference reference)
	{
		return MetadataTokens.TypeSpecificationHandle(_typeSpecIndex.GetOrAdd(reference));
	}

	protected override IReadOnlyList<ITypeReference> GetTypeSpecs()
	{
		return _typeSpecIndex.Rows;
	}

	protected override StandaloneSignatureHandle GetOrAddStandaloneSignatureHandle(BlobHandle blobIndex)
	{
		return MetadataTokens.StandaloneSignatureHandle(_standAloneSignatureIndex.GetOrAdd(blobIndex));
	}

	protected override IReadOnlyList<BlobHandle> GetStandaloneSignatureBlobHandles()
	{
		return _standAloneSignatureIndex.Rows;
	}

	protected override void OnIndicesCreated()
	{
		((IPEDeltaAssemblyBuilder)module).OnCreatedIndices(Context.Diagnostics);
	}

	internal static IReadOnlyDictionary<ITypeDefinition, ArrayBuilder<ITypeDefinitionMember>> CreateDeletedMemberDefs(EmitContext context, SymbolChanges changes)
	{
		Dictionary<ITypeDefinition, ArrayBuilder<ITypeDefinitionMember>> result = new Dictionary<ITypeDefinition, ArrayBuilder<ITypeDefinitionMember>>(ReferenceEqualityComparer.Instance);
		Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> typesUsedByDeletedMembers = new Dictionary<ITypeDefinition, DeletedSourceTypeDefinition>(ReferenceEqualityComparer.Instance);
		foreach (INamespaceTypeDefinition item4 in context.Module.GetTopLevelTypeDefinitionsExcludingNoPiaAndRootModule(context, includePrivateImplementationDetails: false))
		{
			recurse(item4);
		}
		return result;
		ArrayBuilder<ITypeDefinitionMember>? getDeletedMemberDefs(ITypeDefinition typeDef)
		{
			ArrayBuilder<ITypeDefinitionMember> newMemberDefs;
			ImmutableArray<byte>? lazyDeletedLambdaIL;
			if (typeDef.GetInternalSymbol() is INamedTypeSymbolInternal key && (changes.DeletedMembers.TryGetValue(key, out ImmutableArray<ISymbolInternal> value) | changes.UpdatedMethods.TryGetValue(key, out ImmutableArray<(IMethodSymbolInternal, IMethodSymbolInternal)> value2)))
			{
				newMemberDefs = ArrayBuilder<ITypeDefinitionMember>.GetInstance();
				ICustomAttribute deletedAttribute = (value.IsDefaultOrEmpty ? null : context.Module.SynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_MetadataUpdateDeletedAttribute__ctor));
				ImmutableArray<byte>? immutableArray = null;
				lazyDeletedLambdaIL = null;
				foreach (ISymbolInternal item5 in value.NullToEmpty())
				{
					if (item5 is IMethodSymbolInternal methodSymbolInternal)
					{
						MethodDefinitionHandle previousMethodHandle = changes.DefinitionMap.GetPreviousMethodHandle(methodSymbolInternal);
						IMethodDefinition oldMethod = (IMethodDefinition)methodSymbolInternal.GetCciAdapter();
						ImmutableArray<byte> valueOrDefault = immutableArray.GetValueOrDefault();
						if (!immutableArray.HasValue)
						{
							valueOrDefault = DeletedMethodBody.GetIL(context, null, isLambdaOrLocalFunction: false);
							immutableArray = valueOrDefault;
						}
						newMemberDefs.Add(new DeletedSourceMethodDefinition(oldMethod, previousMethodHandle, immutableArray.Value, typesUsedByDeletedMembers, deletedAttribute));
						addDeletedClosureMethods(methodSymbolInternal, ImmutableArray<EncLambdaInfo>.Empty, ImmutableArray<LambdaRuntimeRudeEditInfo>.Empty);
					}
					else if (item5 is IPropertySymbolInternal propertySymbolInternal)
					{
						PropertyDefinitionHandle previousPropertyHandle = changes.DefinitionMap.GetPreviousPropertyHandle(propertySymbolInternal);
						IPropertyDefinition oldProperty = (IPropertyDefinition)propertySymbolInternal.GetCciAdapter();
						newMemberDefs.Add(new DeletedSourcePropertyDefinition(oldProperty, previousPropertyHandle, typesUsedByDeletedMembers, deletedAttribute));
					}
					else
					{
						if (!(item5 is IEventSymbolInternal eventSymbolInternal))
						{
							throw ExceptionUtilities.UnexpectedValue(item5);
						}
						EventDefinitionHandle previousEventHandle = changes.DefinitionMap.GetPreviousEventHandle(eventSymbolInternal);
						IEventDefinition oldEvent = (IEventDefinition)eventSymbolInternal.GetCciAdapter();
						newMemberDefs.Add(new DeletedSourceEventDefinition(oldEvent, previousEventHandle, typesUsedByDeletedMembers, deletedAttribute));
					}
				}
				IMethodSymbolInternal item;
				ImmutableArray<EncLambdaInfo> currentLambdas;
				ImmutableArray<LambdaRuntimeRudeEditInfo> orderedLambdaRuntimeRudeEdits2;
				for (ImmutableArray<(IMethodSymbolInternal, IMethodSymbolInternal)>.Enumerator enumerator3 = value2.NullToEmpty().GetEnumerator(); enumerator3.MoveNext(); addDeletedClosureMethods(item, currentLambdas, orderedLambdaRuntimeRudeEdits2))
				{
					(IMethodSymbolInternal, IMethodSymbolInternal) current2 = enumerator3.Current;
					item = current2.Item1;
					IMethodDefinition methodDefinition = (IMethodDefinition)current2.Item2.GetCciAdapter();
					ImmutableArray<LambdaRuntimeRudeEditInfo> orderedLambdaRuntimeRudeEdits;
					if (methodDefinition.HasBody)
					{
						IMethodBody body = methodDefinition.GetBody(context);
						if (body != null)
						{
							ImmutableArray<EncLambdaInfo> lambdaDebugInfo = body.LambdaDebugInfo;
							orderedLambdaRuntimeRudeEdits = body.OrderedLambdaRuntimeRudeEdits;
							orderedLambdaRuntimeRudeEdits2 = orderedLambdaRuntimeRudeEdits;
							currentLambdas = lambdaDebugInfo;
							continue;
						}
					}
					ImmutableArray<EncLambdaInfo> empty = ImmutableArray<EncLambdaInfo>.Empty;
					orderedLambdaRuntimeRudeEdits = ImmutableArray<LambdaRuntimeRudeEditInfo>.Empty;
					orderedLambdaRuntimeRudeEdits2 = orderedLambdaRuntimeRudeEdits;
					currentLambdas = empty;
				}
				return newMemberDefs;
			}
			return null;
			void addDeletedClosureMethods(IMethodSymbolInternal oldMethod2, ImmutableArray<EncLambdaInfo> currentLambdas2, ImmutableArray<LambdaRuntimeRudeEditInfo> array)
			{
				foreach (var deletedSynthesizedMethod in changes.DefinitionMap.GetDeletedSynthesizedMethods(oldMethod2, currentLambdas2))
				{
					DebugId item2 = deletedSynthesizedMethod.id;
					IMethodSymbolInternal item3 = deletedSynthesizedMethod.symbol;
					int num = array.BinarySearch(item2, (LambdaRuntimeRudeEditInfo rudeEdit, DebugId lambdaId) => rudeEdit.LambdaId.CompareTo(lambdaId));
					ImmutableArray<byte> immutableArray2;
					if (num < 0)
					{
						ImmutableArray<byte> valueOrDefault2 = lazyDeletedLambdaIL.GetValueOrDefault();
						if (!lazyDeletedLambdaIL.HasValue)
						{
							valueOrDefault2 = DeletedMethodBody.GetIL(context, null, isLambdaOrLocalFunction: true);
							lazyDeletedLambdaIL = valueOrDefault2;
							immutableArray2 = valueOrDefault2;
						}
						else
						{
							immutableArray2 = valueOrDefault2;
						}
					}
					else
					{
						immutableArray2 = DeletedMethodBody.GetIL(context, array[num].RudeEdit, isLambdaOrLocalFunction: true);
					}
					ImmutableArray<byte> bodyIL = immutableArray2;
					if (item3.MetadataToken != 0)
					{
						newMemberDefs.Add(new DeletedPEMethodDefinition(item3, bodyIL));
					}
					else
					{
						IMethodDefinition oldMethod3 = (IMethodDefinition)item3.GetCciAdapter();
						MethodDefinitionHandle previousMethodHandle2 = changes.DefinitionMap.GetPreviousMethodHandle(item3);
						newMemberDefs.Add(new DeletedSourceMethodDefinition(oldMethod3, previousMethodHandle2, bodyIL, typesUsedByDeletedMembers, null));
					}
				}
			}
		}
		void recurse(ITypeDefinition typeDef)
		{
			ArrayBuilder<ITypeDefinitionMember> arrayBuilder = getDeletedMemberDefs(typeDef);
			if (arrayBuilder != null && arrayBuilder.Count > 0)
			{
				result.Add(typeDef, arrayBuilder);
			}
			foreach (INestedTypeDefinition nestedType in typeDef.GetNestedTypes(context))
			{
				recurse(nestedType);
			}
		}
	}

	protected override void CreateIndicesForNonTypeMembers(ITypeDefinition typeDef)
	{
		SymbolChange change = Changes.GetChange(typeDef);
		switch (change)
		{
		case SymbolChange.Added:
		{
			_typeDefs.Add(typeDef);
			_changedTypeDefs.Add(typeDef);
			IEnumerable<IGenericTypeParameter> consolidatedTypeParameters = GetConsolidatedTypeParameters(typeDef);
			if (consolidatedTypeParameters == null)
			{
				break;
			}
			foreach (IGenericTypeParameter item7 in consolidatedTypeParameters)
			{
				_genericParameters.Add(item7);
			}
			break;
		}
		case SymbolChange.Updated:
			_typeDefs.AddUpdated(typeDef);
			_changedTypeDefs.Add(typeDef);
			break;
		case SymbolChange.ContainsChanges:
			_changedTypeDefs.Add(typeDef);
			break;
		case SymbolChange.None:
			return;
		default:
			throw ExceptionUtilities.UnexpectedValue(change);
		}
		int rowId = _typeDefs.GetRowId(typeDef);
		foreach (IEventDefinition @event in typeDef.GetEvents(Context))
		{
			if (!_eventMap.Contains(rowId))
			{
				_eventMap.Add(rowId);
			}
			SymbolChange changeForPossibleReAddedMember = Changes.GetChangeForPossibleReAddedMember(@event, DefinitionExistsInAnyPreviousGeneration);
			AddDefIfNecessary(_eventDefs, @event, changeForPossibleReAddedMember);
		}
		foreach (IFieldDefinition field in typeDef.GetFields(Context))
		{
			SymbolChange changeForPossibleReAddedMember2 = Changes.GetChangeForPossibleReAddedMember(field, DefinitionExistsInAnyPreviousGeneration);
			AddDefIfNecessary(_fieldDefs, field, changeForPossibleReAddedMember2);
		}
		foreach (IMethodDefinition method in typeDef.GetMethods(Context))
		{
			SymbolChange changeForPossibleReAddedMember3 = Changes.GetChangeForPossibleReAddedMember(method, DefinitionExistsInAnyPreviousGeneration);
			AddDefIfNecessary(_methodDefs, method, changeForPossibleReAddedMember3);
			CreateIndicesForMethod(method, changeForPossibleReAddedMember3);
		}
		if (_deletedMemberDefs.TryGetValue(typeDef, out ArrayBuilder<ITypeDefinitionMember> value))
		{
			foreach (ITypeDefinitionMember item8 in value)
			{
				if (item8 is IMethodDefinition item)
				{
					_methodDefs.AddUpdated(item);
					continue;
				}
				if (item8 is IPropertyDefinition item2)
				{
					_propertyDefs.AddUpdated(item2);
					continue;
				}
				if (item8 is IEventDefinition item3)
				{
					_eventDefs.AddUpdated(item3);
					continue;
				}
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeltaMetadataWriter.cs", 711);
			}
			_deletedTypeMembers.Add(typeDef, value.ToImmutable());
		}
		foreach (IPropertyDefinition property in typeDef.GetProperties(Context))
		{
			if (!_propertyMap.Contains(rowId))
			{
				_propertyMap.Add(rowId);
			}
			SymbolChange changeForPossibleReAddedMember4 = Changes.GetChangeForPossibleReAddedMember(property, DefinitionExistsInAnyPreviousGeneration);
			AddDefIfNecessary(_propertyDefs, property, changeForPossibleReAddedMember4);
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		foreach (Microsoft.Cci.MethodImplementation explicitImplementationOverride in typeDef.GetExplicitImplementationOverrides(Context))
		{
			IMethodDefinition item4 = (IMethodDefinition)explicitImplementationOverride.ImplementingMethod.AsDefinition(Context);
			int rowId2 = _methodDefs.GetRowId(item4);
			MethodImplKey item5 = new MethodImplKey(rowId2, 1);
			if (!_methodImpls.Contains(item5))
			{
				instance.Add(rowId2);
				methodImplList.Add(explicitImplementationOverride);
			}
		}
		foreach (int item9 in instance)
		{
			int num = 1;
			MethodImplKey item6;
			while (true)
			{
				item6 = new MethodImplKey(item9, num);
				if (!_methodImpls.Contains(item6))
				{
					break;
				}
				num++;
			}
			_methodImpls.Add(item6);
		}
		instance.Free();
	}

	private bool DefinitionExistsInAnyPreviousGeneration(ITypeDefinitionMember item)
	{
		int index;
		if (!(item is IMethodDefinition item2))
		{
			if (!(item is IPropertyDefinition item3))
			{
				if (!(item is IFieldDefinition item4))
				{
					if (item is IEventDefinition item5)
					{
						return TryGetExistingEventDefIndex(item5, out index);
					}
					return false;
				}
				return TryGetExistingFieldDefIndex(item4, out index);
			}
			return TryGetExistingPropertyDefIndex(item3, out index);
		}
		return TryGetExistingMethodDefIndex(item2, out index);
	}

	private void CreateIndicesForMethod(IMethodDefinition methodDef, SymbolChange methodChange)
	{
		switch (methodChange)
		{
		case SymbolChange.Added:
			_firstParamRowMap.Add(GetMethodDefinitionHandle(methodDef), _parameterDefs.NextRowId);
			foreach (IParameterDefinition item in GetParametersToEmit(methodDef))
			{
				_parameterDefs.Add(item);
				_parameterDefList.Add(item, methodDef);
			}
			break;
		case SymbolChange.Updated:
		{
			MethodDefinitionHandle methodDefinitionHandle = GetMethodDefinitionHandle(methodDef);
			if (_previousGeneration.OriginalMetadata.MetadataReader.GetTableRowCount(TableIndex.MethodDef) >= MetadataTokens.GetRowNumber(methodDefinitionHandle))
			{
				EmitParametersFromOriginalMetadata(methodDef, methodDefinitionHandle);
			}
			else
			{
				EmitParametersFromDelta(methodDef, methodDefinitionHandle);
			}
			break;
		}
		}
		if (methodChange != SymbolChange.Added || methodDef.GenericParameterCount <= 0)
		{
			return;
		}
		foreach (IGenericMethodParameter genericParameter in methodDef.GenericParameters)
		{
			_genericParameters.Add(genericParameter);
		}
	}

	private void EmitParametersFromOriginalMetadata(IMethodDefinition methodDef, MethodDefinitionHandle handle)
	{
		ParameterHandleCollection parameters = _previousGeneration.OriginalMetadata.MetadataReader.GetMethodDefinition(handle).GetParameters();
		ImmutableArray<IParameterDefinition> parametersToEmit = GetParametersToEmit(methodDef);
		int num = 0;
		foreach (ParameterHandle item in parameters)
		{
			IParameterDefinition parameterDefinition = parametersToEmit[num];
			_parameterDefs.AddUpdated(parameterDefinition);
			_existingParameterDefs.Add(parameterDefinition, MetadataTokens.GetRowNumber(item));
			_parameterDefList.Add(parameterDefinition, methodDef);
			num++;
		}
	}

	private void EmitParametersFromDelta(IMethodDefinition methodDef, MethodDefinitionHandle handle)
	{
		_previousGeneration.FirstParamRowMap.TryGetValue(handle, out var value);
		foreach (IParameterDefinition item in GetParametersToEmit(methodDef))
		{
			_parameterDefs.AddUpdated(item);
			_existingParameterDefs.Add(item, value++);
			_parameterDefList.Add(item, methodDef);
		}
	}

	private bool AddDefIfNecessary<T>(DefinitionIndex<T> defIndex, T def, SymbolChange change) where T : class, IDefinition
	{
		switch (change)
		{
		case SymbolChange.Added:
			defIndex.Add(def);
			return true;
		case SymbolChange.Updated:
			defIndex.AddUpdated(def);
			return false;
		case SymbolChange.ContainsChanges:
			return false;
		default:
			return false;
		}
	}

	protected override ReferenceIndexer CreateReferenceVisitor()
	{
		return new DeltaReferenceIndexer(this);
	}

	protected override void ReportReferencesToAddedSymbols()
	{
		foreach (ITypeReference typeRef in GetTypeRefs())
		{
			ReportReferencesToAddedSymbol(typeRef.GetInternalSymbol());
		}
		foreach (ITypeMemberReference memberRef in GetMemberRefs())
		{
			ReportReferencesToAddedSymbol(memberRef.GetInternalSymbol());
		}
	}

	private void ReportReferencesToAddedSymbol(ISymbolInternal? symbol)
	{
		if (symbol != null && Changes.IsAdded(symbol.GetISymbol()))
		{
			Context.Diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_EncReferenceToAddedMember, MetadataWriter.GetSymbolLocation(symbol), symbol.Name, symbol.ContainingAssembly.Name));
		}
	}

	protected override StandaloneSignatureHandle SerializeLocalVariablesSignature(IMethodBody body)
	{
		ImmutableArray<ILocalDefinition> localVariables = body.LocalVariables;
		ArrayBuilder<EncLocalInfo> instance = ArrayBuilder<EncLocalInfo>.GetInstance();
		StandaloneSignatureHandle result;
		if (localVariables.Length > 0)
		{
			PooledBlobBuilder instance2 = PooledBlobBuilder.GetInstance();
			LocalVariablesEncoder localVariablesEncoder = new BlobEncoder(instance2).LocalVariableSignature(localVariables.Length);
			foreach (ILocalDefinition item in localVariables)
			{
				byte[] array = item.Signature;
				if (array == null)
				{
					int count = instance2.Count;
					SerializeLocalVariableType(localVariablesEncoder.AddVariable(), item);
					array = instance2.ToArray(count, instance2.Count - count);
				}
				else
				{
					instance2.WriteBytes(array);
				}
				instance.Add(CreateEncLocalInfo(item, array));
			}
			BlobHandle orAddBlob = metadata.GetOrAddBlob(instance2);
			result = GetOrAddStandaloneSignatureHandle(orAddBlob);
			instance2.Free();
		}
		else
		{
			result = default(StandaloneSignatureHandle);
		}
		AddedOrChangedMethodInfo value = new AddedOrChangedMethodInfo(body.MethodId, instance.ToImmutable(), body.LambdaDebugInfo, body.ClosureDebugInfo, body.StateMachineTypeName, body.StateMachineHoistedLocalSlots, body.StateMachineAwaiterSlots, body.StateMachineStatesDebugInfo);
		_addedOrChangedMethods.Add(body.MethodDefinition, value);
		instance.Free();
		return result;
	}

	private EncLocalInfo CreateEncLocalInfo(ILocalDefinition localDef, byte[] signature)
	{
		if (localDef.SlotInfo.Id.IsNone)
		{
			return new EncLocalInfo(signature);
		}
		ITypeReference typeReference = localDef.Type;
		if (typeReference.GetInternalSymbol() is ITypeSymbolInternal type)
		{
			typeReference = Context.Module.EncTranslateType(type, Context.Diagnostics);
		}
		return new EncLocalInfo(localDef.SlotInfo, typeReference, localDef.Constraints, signature);
	}

	protected override void AddCustomAttributesToTable(EntityHandle parentHandle, IEnumerable<ICustomAttribute> attributes)
	{
		_deferredCustomAttributes.Add((parentHandle, attributes.GetEnumerator()));
	}

	protected override void FinalizeCustomAttributeTableRows()
	{
		base.FinalizeCustomAttributeTableRows();
		if (_deferredCustomAttributes.Count == 0)
		{
			return;
		}
		int num = ((_previousGeneration.CustomAttributesAdded.Count > 0) ? _previousGeneration.CustomAttributesAdded.Max(delegate(KeyValuePair<EntityHandle, ImmutableArray<int>> entry)
		{
			ImmutableArray<int> value3 = entry.Value;
			return value3[value3.Length - 1];
		}) : _previousGeneration.OriginalMetadata.MetadataReader.GetTableRowCount(TableIndex.CustomAttribute));
		_deferredCustomAttributes.Sort(delegate((EntityHandle parentHandle, IEnumerator<ICustomAttribute> attributeEnumerator) x, (EntityHandle parentHandle, IEnumerator<ICustomAttribute> attributeEnumerator) y)
		{
			if (x.parentHandle == y.parentHandle)
			{
				return 0;
			}
			int num2 = MetadataTokens.GetRowNumber(_previousGeneration.OriginalMetadata.MetadataReader.GetCustomAttributes(x.parentHandle).FirstOrDefault());
			int num3 = MetadataTokens.GetRowNumber(_previousGeneration.OriginalMetadata.MetadataReader.GetCustomAttributes(y.parentHandle).FirstOrDefault());
			if (num2 == 0)
			{
				num2 = int.MaxValue;
			}
			if (num3 == 0)
			{
				num3 = int.MaxValue;
			}
			int num4 = num2.CompareTo(num3);
			if (num4 != 0)
			{
				return num4;
			}
			num2 = (_previousGeneration.CustomAttributesAdded.TryGetValue(x.parentHandle, out var value3) ? value3[0] : int.MaxValue);
			num3 = (_previousGeneration.CustomAttributesAdded.TryGetValue(y.parentHandle, out value3) ? value3[0] : int.MaxValue);
			num4 = num2.CompareTo(num3);
			return (num4 != 0) ? num4 : HandleComparer.Default.Compare(x.parentHandle, y.parentHandle);
		});
		foreach (var deferredCustomAttribute in _deferredCustomAttributes)
		{
			EntityHandle item = deferredCustomAttribute.parentHandle;
			IEnumerator<ICustomAttribute> item2 = deferredCustomAttribute.attributeEnumerator;
			CustomAttributeHandleCollection customAttributes = _previousGeneration.OriginalMetadata.MetadataReader.GetCustomAttributes(item);
			foreach (CustomAttributeHandle item7 in customAttributes)
			{
				_customAttributeRowIds.Add(MetadataTokens.GetRowNumber(item7));
			}
			addWithCap(item, item2, customAttributes.Count);
		}
		foreach (var deferredCustomAttribute2 in _deferredCustomAttributes)
		{
			EntityHandle item3 = deferredCustomAttribute2.parentHandle;
			IEnumerator<ICustomAttribute> item4 = deferredCustomAttribute2.attributeEnumerator;
			ImmutableArray<int> items = (_previousGeneration.CustomAttributesAdded.TryGetValue(item3, out var value) ? value : ImmutableArray<int>.Empty);
			_customAttributeRowIds.AddRange(items);
			addWithCap(item3, item4, items.Length);
		}
		foreach (var deferredCustomAttribute3 in _deferredCustomAttributes)
		{
			EntityHandle item5 = deferredCustomAttribute3.parentHandle;
			IEnumerator<ICustomAttribute> item6 = deferredCustomAttribute3.attributeEnumerator;
			int count = _customAttributeRowIds.Count;
			while (item6.MoveNext())
			{
				if (AddCustomAttributeToTable(item5, item6.Current))
				{
					num++;
					_customAttributeRowIds.Add(num);
				}
			}
			if (_customAttributeRowIds.Count > count)
			{
				ImmutableArray<int> immutableArray = (_previousGeneration.CustomAttributesAdded.TryGetValue(item5, out var value2) ? value2 : ImmutableArray<int>.Empty);
				_customAttributesAdded.Add(item5, immutableArray.AddRange(_customAttributeRowIds.Skip(count)));
			}
		}
		void addWithCap(EntityHandle parentHandle, IEnumerator<ICustomAttribute> attributeEnumerator, int limit)
		{
			int num2 = 0;
			while (num2 < limit && attributeEnumerator.MoveNext())
			{
				if (AddCustomAttributeToTable(parentHandle, attributeEnumerator.Current))
				{
					num2++;
				}
			}
			int num3 = limit - num2;
			if (num3 > 0)
			{
				MetadataTokens.TryGetTableIndex(parentHandle.Kind, out var index);
				EntityHandle parent = MetadataTokens.EntityHandle(index, 0);
				EntityHandle constructor = MetadataTokens.EntityHandle(TableIndex.MemberRef, 0);
				for (int i = 0; i < num3; i++)
				{
					metadata.AddCustomAttribute(parent, constructor, default(BlobHandle));
				}
			}
		}
	}

	public override void PopulateEncTables(ImmutableArray<int> typeSystemRowCounts)
	{
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		PopulateEncLogTableRows(typeSystemRowCounts, instance);
		PopulateEncMapTableRows(typeSystemRowCounts, instance);
		_customAttributeRowIds.Free();
		instance.Free();
	}

	private void PopulateEncLogTableRows(ImmutableArray<int> rowCounts, ArrayBuilder<int> paramEncMapRows)
	{
		ImmutableArray<int> tableSizes = _previousGeneration.TableSizes;
		ImmutableArray<int> deltaTableSizes = GetDeltaTableSizes(rowCounts);
		PopulateEncLogTableRows(TableIndex.AssemblyRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.ModuleRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MemberRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MethodSpec, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.TypeRef, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.TypeSpec, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.StandAloneSig, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(_typeDefs, TableIndex.TypeDef);
		PopulateEncLogTableRows(TableIndex.EventMap, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.PropertyMap, tableSizes, deltaTableSizes);
		PopulateEncLogTableEventsOrProperties(_eventDefs, TableIndex.Event, EditAndContinueOperation.AddEvent, _eventMap, TableIndex.EventMap);
		PopulateEncLogTableFieldsOrMethods(_fieldDefs, TableIndex.Field, EditAndContinueOperation.AddField);
		PopulateEncLogTableFieldsOrMethods(_methodDefs, TableIndex.MethodDef, EditAndContinueOperation.AddMethod);
		PopulateEncLogTableEventsOrProperties(_propertyDefs, TableIndex.Property, EditAndContinueOperation.AddProperty, _propertyMap, TableIndex.PropertyMap);
		PopulateEncLogTableParameters(paramEncMapRows);
		PopulateEncLogTableRows(TableIndex.Constant, tableSizes, deltaTableSizes);
		foreach (int customAttributeRowId in _customAttributeRowIds)
		{
			metadata.AddEncLogEntry(MetadataTokens.CustomAttributeHandle(customAttributeRowId), EditAndContinueOperation.Default);
		}
		PopulateEncLogTableRows(TableIndex.DeclSecurity, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.ClassLayout, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.FieldLayout, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MethodSemantics, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.MethodImpl, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.ImplMap, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.FieldRva, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.NestedClass, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.GenericParam, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.InterfaceImpl, tableSizes, deltaTableSizes);
		PopulateEncLogTableRows(TableIndex.GenericParamConstraint, tableSizes, deltaTableSizes);
	}

	private void PopulateEncLogTableEventsOrProperties<T>(DefinitionIndex<T> index, TableIndex table, EditAndContinueOperation addCode, EventOrPropertyMapIndex map, TableIndex mapTable) where T : class, ITypeDefinitionMember
	{
		foreach (T row in index.GetRows())
		{
			if (index.IsAddedNotChanged(row))
			{
				int rowNumber = MetadataTokens.GetRowNumber(GetTypeDefinitionHandle(row.ContainingTypeDefinition));
				int rowId = map.GetRowId(rowNumber);
				metadata.AddEncLogEntry(MetadataTokens.Handle(mapTable, rowId), addCode);
			}
			metadata.AddEncLogEntry(MetadataTokens.Handle(table, index.GetRowId(row)), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncLogTableFieldsOrMethods<T>(DefinitionIndex<T> index, TableIndex tableIndex, EditAndContinueOperation addCode) where T : class, ITypeDefinitionMember
	{
		foreach (T row in index.GetRows())
		{
			if (index.IsAddedNotChanged(row))
			{
				metadata.AddEncLogEntry(GetTypeDefinitionHandle(row.ContainingTypeDefinition), addCode);
			}
			metadata.AddEncLogEntry(MetadataTokens.Handle(tableIndex, index.GetRowId(row)), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncLogTableParameters(ArrayBuilder<int> paramEncMapRows)
	{
		int firstRowId = _parameterDefs.FirstRowId;
		int num = 0;
		foreach (IParameterDefinition parameterDef in GetParameterDefs())
		{
			IMethodDefinition item = _parameterDefList[parameterDef];
			if (_methodDefs.IsAddedNotChanged(item))
			{
				paramEncMapRows.Add(firstRowId + num);
				metadata.AddEncLogEntry(MetadataTokens.MethodDefinitionHandle(_methodDefs.GetRowId(item)), EditAndContinueOperation.AddParameter);
				metadata.AddEncLogEntry(MetadataTokens.ParameterHandle(firstRowId + num), EditAndContinueOperation.Default);
				num++;
			}
			else
			{
				ParameterHandle parameterHandle = GetParameterHandle(parameterDef);
				paramEncMapRows.Add(MetadataTokens.GetRowNumber(parameterHandle));
				metadata.AddEncLogEntry(parameterHandle, EditAndContinueOperation.Default);
			}
		}
	}

	private void PopulateEncLogTableRows<T>(DefinitionIndex<T> index, TableIndex tableIndex) where T : class, IDefinition
	{
		foreach (T row in index.GetRows())
		{
			metadata.AddEncLogEntry(MetadataTokens.Handle(tableIndex, index.GetRowId(row)), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncLogTableRows(TableIndex tableIndex, ImmutableArray<int> previousSizes, ImmutableArray<int> deltaSizes)
	{
		PopulateEncLogTableRows(tableIndex, previousSizes[(int)tableIndex] + 1, deltaSizes[(int)tableIndex]);
	}

	private void PopulateEncLogTableRows(TableIndex tableIndex, int firstRowId, int tokenCount)
	{
		for (int i = 0; i < tokenCount; i++)
		{
			metadata.AddEncLogEntry(MetadataTokens.Handle(tableIndex, firstRowId + i), EditAndContinueOperation.Default);
		}
	}

	private void PopulateEncMapTableRows(ImmutableArray<int> rowCounts, ArrayBuilder<int> paramEncMapRows)
	{
		ArrayBuilder<EntityHandle> instance = ArrayBuilder<EntityHandle>.GetInstance();
		ImmutableArray<int> tableSizes = _previousGeneration.TableSizes;
		ImmutableArray<int> deltaTableSizes = GetDeltaTableSizes(rowCounts);
		TableIndex tableIndex = TableIndex.Module;
		while ((int)tableIndex <= 44)
		{
			switch (tableIndex)
			{
			case TableIndex.TypeRef:
			case TableIndex.InterfaceImpl:
			case TableIndex.MemberRef:
			case TableIndex.Constant:
			case TableIndex.DeclSecurity:
			case TableIndex.ClassLayout:
			case TableIndex.FieldLayout:
			case TableIndex.StandAloneSig:
			case TableIndex.EventMap:
			case TableIndex.PropertyMap:
			case TableIndex.MethodSemantics:
			case TableIndex.MethodImpl:
			case TableIndex.ModuleRef:
			case TableIndex.TypeSpec:
			case TableIndex.ImplMap:
			case TableIndex.FieldRva:
			case TableIndex.AssemblyRef:
			case TableIndex.NestedClass:
			case TableIndex.GenericParam:
			case TableIndex.MethodSpec:
			case TableIndex.GenericParamConstraint:
				AddReferencedTokens(instance, tableIndex, tableSizes, deltaTableSizes);
				break;
			case TableIndex.TypeDef:
				AddDefinitionTokens(instance, tableIndex, _typeDefs);
				break;
			case TableIndex.Field:
				AddDefinitionTokens(instance, tableIndex, _fieldDefs);
				break;
			case TableIndex.MethodDef:
				AddDefinitionTokens(instance, tableIndex, _methodDefs);
				break;
			case TableIndex.Event:
				AddDefinitionTokens(instance, tableIndex, _eventDefs);
				break;
			case TableIndex.Property:
				AddDefinitionTokens(instance, tableIndex, _propertyDefs);
				break;
			case TableIndex.Param:
				AddRowNumberTokens(instance, TableIndex.Param, paramEncMapRows);
				break;
			case TableIndex.CustomAttribute:
				AddRowNumberTokens(instance, TableIndex.CustomAttribute, _customAttributeRowIds);
				break;
			}
			tableIndex++;
		}
		foreach (EntityHandle item in instance)
		{
			metadata.AddEncMapEntry(item);
		}
		instance.Free();
		if (_debugMetadataOpt != null)
		{
			ArrayBuilder<EntityHandle> instance2 = ArrayBuilder<EntityHandle>.GetInstance();
			AddDefinitionTokens(instance2, TableIndex.MethodDebugInformation, _methodDefs);
			instance2.Sort(HandleComparer.Default);
			foreach (EntityHandle item2 in instance2)
			{
				_debugMetadataOpt.AddEncMapEntry(item2);
			}
			instance2.Free();
		}
	}

	private static void AddReferencedTokens(ArrayBuilder<EntityHandle> builder, TableIndex tableIndex, ImmutableArray<int> previousSizes, ImmutableArray<int> deltaSizes)
	{
		AddReferencedTokens(builder, tableIndex, previousSizes[(int)tableIndex] + 1, deltaSizes[(int)tableIndex]);
	}

	private static void AddReferencedTokens(ArrayBuilder<EntityHandle> tokens, TableIndex tableIndex, int firstRowId, int nTokens)
	{
		for (int i = 0; i < nTokens; i++)
		{
			tokens.Add(MetadataTokens.Handle(tableIndex, firstRowId + i));
		}
	}

	private static void AddDefinitionTokens<T>(ArrayBuilder<EntityHandle> tokens, TableIndex tableIndex, DefinitionIndex<T> index) where T : class, IDefinition
	{
		foreach (T row in index.GetRows())
		{
			tokens.Add(MetadataTokens.Handle(tableIndex, index.GetRowId(row)));
		}
	}

	private static void AddRowNumberTokens(ArrayBuilder<EntityHandle> tokens, TableIndex tableIndex, ArrayBuilder<int> rowNumbers)
	{
		foreach (int rowNumber in rowNumbers)
		{
			tokens.Add(MetadataTokens.Handle(tableIndex, rowNumber));
		}
	}

	protected override void PopulateEventMapTableRows()
	{
		foreach (int row in _eventMap.GetRows())
		{
			metadata.AddEventMap(MetadataTokens.TypeDefinitionHandle(row), MetadataTokens.EventDefinitionHandle(_eventMap.GetRowId(row)));
		}
	}

	protected override void PopulatePropertyMapTableRows()
	{
		foreach (int row in _propertyMap.GetRows())
		{
			metadata.AddPropertyMap(MetadataTokens.TypeDefinitionHandle(row), MetadataTokens.PropertyDefinitionHandle(_propertyMap.GetRowId(row)));
		}
	}

	private bool TryGetExistingTypeDefIndex(ITypeDefinition item, out int index)
	{
		if (_previousGeneration.TypesAdded.TryGetValue(item, out index))
		{
			return true;
		}
		EntityHandle initialMetadataHandle = _definitionMap.GetInitialMetadataHandle(item);
		index = MetadataTokens.GetRowNumber(initialMetadataHandle);
		return !initialMetadataHandle.IsNil;
	}

	private bool TryGetExistingEventDefIndex(IEventDefinition item, out int index)
	{
		if (_previousGeneration.EventsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		EntityHandle initialMetadataHandle = _definitionMap.GetInitialMetadataHandle(item);
		index = MetadataTokens.GetRowNumber(initialMetadataHandle);
		return !initialMetadataHandle.IsNil;
	}

	private bool TryGetExistingFieldDefIndex(IFieldDefinition item, out int index)
	{
		if (_previousGeneration.FieldsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		EntityHandle initialMetadataHandle = _definitionMap.GetInitialMetadataHandle(item);
		index = MetadataTokens.GetRowNumber(initialMetadataHandle);
		return !initialMetadataHandle.IsNil;
	}

	private bool TryGetExistingMethodDefIndex(IMethodDefinition item, out int index)
	{
		if (item is IDeletedMethodDefinition { MetadataHandle: var metadataHandle })
		{
			index = MetadataTokens.GetRowNumber(metadataHandle);
			return true;
		}
		if (_previousGeneration.MethodsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		EntityHandle initialMetadataHandle = _definitionMap.GetInitialMetadataHandle(item);
		index = MetadataTokens.GetRowNumber(initialMetadataHandle);
		return !initialMetadataHandle.IsNil;
	}

	private bool TryGetExistingPropertyDefIndex(IPropertyDefinition item, out int index)
	{
		if (_previousGeneration.PropertiesAdded.TryGetValue(item, out index))
		{
			return true;
		}
		EntityHandle initialMetadataHandle = _definitionMap.GetInitialMetadataHandle(item);
		index = MetadataTokens.GetRowNumber(initialMetadataHandle);
		return !initialMetadataHandle.IsNil;
	}

	private bool TryGetExistingParameterDefIndex(IParameterDefinition item, out int index)
	{
		return _existingParameterDefs.TryGetValue(item, out index);
	}

	private bool TryGetExistingEventMapIndex(int item, out int index)
	{
		if (_previousGeneration.EventMapAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_previousGeneration.TypeToEventMap.TryGetValue(item, out index))
		{
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingPropertyMapIndex(int item, out int index)
	{
		if (_previousGeneration.PropertyMapAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_previousGeneration.TypeToPropertyMap.TryGetValue(item, out index))
		{
			return true;
		}
		index = 0;
		return false;
	}

	private bool TryGetExistingMethodImplIndex(MethodImplKey item, out int index)
	{
		if (_previousGeneration.MethodImplsAdded.TryGetValue(item, out index))
		{
			return true;
		}
		if (_previousGeneration.MethodImpls.TryGetValue(item, out index))
		{
			return true;
		}
		index = 0;
		return false;
	}
}
