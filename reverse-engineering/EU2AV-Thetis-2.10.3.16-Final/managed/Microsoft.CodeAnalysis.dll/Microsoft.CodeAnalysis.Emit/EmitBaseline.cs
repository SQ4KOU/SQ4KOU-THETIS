using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

public sealed class EmitBaseline
{
	internal sealed class MetadataSymbols(SynthesizedTypeMaps synthesizedTypes, object metadataDecoder, ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> assemblyReferenceIdentityMap)
	{
		public readonly SynthesizedTypeMaps SynthesizedTypes = synthesizedTypes;

		public readonly object MetadataDecoder = metadataDecoder;

		public readonly ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> AssemblyReferenceIdentityMap = assemblyReferenceIdentityMap;
	}

	private static readonly ImmutableArray<int> s_emptyTableSizes = ImmutableArray.Create(new int[MetadataTokens.TableCount]);

	internal MetadataSymbols? LazyMetadataSymbols;

	internal readonly Compilation Compilation;

	internal readonly CommonPEModuleBuilder? PEModuleBuilder;

	internal readonly Guid ModuleVersionId;

	internal readonly bool HasPortablePdb;

	internal readonly int Ordinal;

	internal readonly Guid EncId;

	internal readonly IReadOnlyDictionary<IDefinition, int> GenerationOrdinals;

	internal readonly IReadOnlyDictionary<ITypeDefinition, int> TypesAdded;

	internal readonly IReadOnlyDictionary<IEventDefinition, int> EventsAdded;

	internal readonly IReadOnlyDictionary<IFieldDefinition, int> FieldsAdded;

	internal readonly IReadOnlyDictionary<IMethodDefinition, int> MethodsAdded;

	internal readonly IReadOnlyDictionary<MethodDefinitionHandle, int> FirstParamRowMap;

	internal readonly IReadOnlyDictionary<IPropertyDefinition, int> PropertiesAdded;

	internal readonly IReadOnlyDictionary<int, int> EventMapAdded;

	internal readonly IReadOnlyDictionary<int, int> PropertyMapAdded;

	internal readonly IReadOnlyDictionary<MethodImplKey, int> MethodImplsAdded;

	internal readonly IReadOnlyDictionary<EntityHandle, ImmutableArray<int>> CustomAttributesAdded;

	internal readonly ImmutableArray<int> TableEntriesAdded;

	internal readonly int BlobStreamLengthAdded;

	internal readonly int StringStreamLengthAdded;

	internal readonly int UserStringStreamLengthAdded;

	internal readonly int GuidStreamLengthAdded;

	internal readonly IReadOnlyDictionary<int, AddedOrChangedMethodInfo> AddedOrChangedMethods;

	internal readonly Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> DebugInformationProvider;

	internal readonly Func<MethodDefinitionHandle, StandaloneSignatureHandle> LocalSignatureProvider;

	internal readonly ImmutableArray<int> TableSizes;

	internal readonly IReadOnlyDictionary<int, int> TypeToEventMap;

	internal readonly IReadOnlyDictionary<int, int> TypeToPropertyMap;

	internal readonly IReadOnlyDictionary<MethodImplKey, int> MethodImpls;

	private readonly SynthesizedTypeMaps _synthesizedTypes;

	internal readonly IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> SynthesizedMembers;

	internal readonly IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> DeletedMembers;

	internal EmitBaseline InitialBaseline { get; }

	public ModuleMetadata OriginalMetadata { get; }

	internal SynthesizedTypeMaps SynthesizedTypes
	{
		get
		{
			if (Ordinal > 0)
			{
				return _synthesizedTypes;
			}
			return LazyMetadataSymbols.SynthesizedTypes;
		}
	}

	internal MetadataReader MetadataReader => OriginalMetadata.MetadataReader;

	internal int BlobStreamLength => BlobStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.Blob);

	internal int StringStreamLength => StringStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.String);

	internal int UserStringStreamLength => UserStringStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.UserString);

	internal int GuidStreamLength => GuidStreamLengthAdded + MetadataReader.GetHeapSize(HeapIndex.Guid);

	[Obsolete("This overload is no longer supported", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static EmitBaseline CreateInitialBaseline(ModuleMetadata module, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider)
	{
		throw new NotSupportedException();
	}

	[Obsolete("This overload is no longer supported", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static EmitBaseline CreateInitialBaseline(ModuleMetadata module, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider, Func<MethodDefinitionHandle, StandaloneSignatureHandle> localSignatureProvider, bool hasPortableDebugInformation)
	{
		throw new NotSupportedException();
	}

	public static EmitBaseline CreateInitialBaseline(Compilation compilation, ModuleMetadata module, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider, Func<MethodDefinitionHandle, StandaloneSignatureHandle> localSignatureProvider, bool hasPortableDebugInformation)
	{
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		if (debugInformationProvider == null)
		{
			throw new ArgumentNullException("debugInformationProvider");
		}
		if (localSignatureProvider == null)
		{
			throw new ArgumentNullException("localSignatureProvider");
		}
		MetadataReader metadataReader = module.MetadataReader;
		return new EmitBaseline(null, module, compilation, null, module.GetModuleVersionId(), 0, default(Guid), hasPortableDebugInformation, new Dictionary<IDefinition, int>(), new Dictionary<ITypeDefinition, int>(), new Dictionary<IEventDefinition, int>(), new Dictionary<IFieldDefinition, int>(), new Dictionary<IMethodDefinition, int>(), new Dictionary<MethodDefinitionHandle, int>(), new Dictionary<IPropertyDefinition, int>(), new Dictionary<int, int>(), new Dictionary<int, int>(), new Dictionary<MethodImplKey, int>(), new Dictionary<EntityHandle, ImmutableArray<int>>(), s_emptyTableSizes, 0, 0, 0, 0, SynthesizedTypeMaps.Empty, ImmutableSegmentedDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>.Empty, ImmutableSegmentedDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>.Empty, new Dictionary<int, AddedOrChangedMethodInfo>(), debugInformationProvider, localSignatureProvider, CalculateTypeEventMap(metadataReader), CalculateTypePropertyMap(metadataReader), CalculateMethodImpls(metadataReader));
	}

	private EmitBaseline(EmitBaseline? initialBaseline, ModuleMetadata module, Compilation compilation, CommonPEModuleBuilder? moduleBuilder, Guid moduleVersionId, int ordinal, Guid encId, bool hasPortablePdb, IReadOnlyDictionary<IDefinition, int> generationOrdinals, IReadOnlyDictionary<ITypeDefinition, int> typesAdded, IReadOnlyDictionary<IEventDefinition, int> eventsAdded, IReadOnlyDictionary<IFieldDefinition, int> fieldsAdded, IReadOnlyDictionary<IMethodDefinition, int> methodsAdded, IReadOnlyDictionary<MethodDefinitionHandle, int> firstParamRowMap, IReadOnlyDictionary<IPropertyDefinition, int> propertiesAdded, IReadOnlyDictionary<int, int> eventMapAdded, IReadOnlyDictionary<int, int> propertyMapAdded, IReadOnlyDictionary<MethodImplKey, int> methodImplsAdded, IReadOnlyDictionary<EntityHandle, ImmutableArray<int>> customAttributesAdded, ImmutableArray<int> tableEntriesAdded, int blobStreamLengthAdded, int stringStreamLengthAdded, int userStringStreamLengthAdded, int guidStreamLengthAdded, SynthesizedTypeMaps synthesizedTypes, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> synthesizedMembers, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> deletedMembers, IReadOnlyDictionary<int, AddedOrChangedMethodInfo> methodsAddedOrChanged, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider, Func<MethodDefinitionHandle, StandaloneSignatureHandle> localSignatureProvider, IReadOnlyDictionary<int, int> typeToEventMap, IReadOnlyDictionary<int, int> typeToPropertyMap, IReadOnlyDictionary<MethodImplKey, int> methodImpls)
	{
		MetadataReader metadataReader = module.Module.MetadataReader;
		InitialBaseline = initialBaseline ?? this;
		OriginalMetadata = module;
		Compilation = compilation;
		PEModuleBuilder = moduleBuilder;
		ModuleVersionId = moduleVersionId;
		Ordinal = ordinal;
		EncId = encId;
		HasPortablePdb = hasPortablePdb;
		GenerationOrdinals = generationOrdinals;
		TypesAdded = typesAdded;
		EventsAdded = eventsAdded;
		FieldsAdded = fieldsAdded;
		MethodsAdded = methodsAdded;
		FirstParamRowMap = firstParamRowMap;
		PropertiesAdded = propertiesAdded;
		EventMapAdded = eventMapAdded;
		PropertyMapAdded = propertyMapAdded;
		MethodImplsAdded = methodImplsAdded;
		CustomAttributesAdded = customAttributesAdded;
		TableEntriesAdded = tableEntriesAdded;
		BlobStreamLengthAdded = blobStreamLengthAdded;
		StringStreamLengthAdded = stringStreamLengthAdded;
		UserStringStreamLengthAdded = userStringStreamLengthAdded;
		GuidStreamLengthAdded = guidStreamLengthAdded;
		_synthesizedTypes = synthesizedTypes;
		SynthesizedMembers = synthesizedMembers;
		DeletedMembers = deletedMembers;
		AddedOrChangedMethods = methodsAddedOrChanged;
		DebugInformationProvider = debugInformationProvider;
		LocalSignatureProvider = localSignatureProvider;
		TableSizes = CalculateTableSizes(metadataReader, TableEntriesAdded);
		TypeToEventMap = typeToEventMap;
		TypeToPropertyMap = typeToPropertyMap;
		MethodImpls = methodImpls;
	}

	internal EmitBaseline With(Compilation compilation, CommonPEModuleBuilder moduleBuilder, int ordinal, Guid encId, IReadOnlyDictionary<IDefinition, int> generationOrdinals, IReadOnlyDictionary<ITypeDefinition, int> typesAdded, IReadOnlyDictionary<IEventDefinition, int> eventsAdded, IReadOnlyDictionary<IFieldDefinition, int> fieldsAdded, IReadOnlyDictionary<IMethodDefinition, int> methodsAdded, IReadOnlyDictionary<MethodDefinitionHandle, int> firstParamRowMap, IReadOnlyDictionary<IPropertyDefinition, int> propertiesAdded, IReadOnlyDictionary<int, int> eventMapAdded, IReadOnlyDictionary<int, int> propertyMapAdded, IReadOnlyDictionary<MethodImplKey, int> methodImplsAdded, IReadOnlyDictionary<EntityHandle, ImmutableArray<int>> customAttributesAdded, ImmutableArray<int> tableEntriesAdded, int blobStreamLengthAdded, int stringStreamLengthAdded, int userStringStreamLengthAdded, int guidStreamLengthAdded, SynthesizedTypeMaps synthesizedTypes, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> synthesizedMembers, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> deletedMembers, IReadOnlyDictionary<int, AddedOrChangedMethodInfo> addedOrChangedMethods, Func<MethodDefinitionHandle, EditAndContinueMethodDebugInformation> debugInformationProvider, Func<MethodDefinitionHandle, StandaloneSignatureHandle> localSignatureProvider)
	{
		return new EmitBaseline(InitialBaseline, OriginalMetadata, compilation, moduleBuilder, ModuleVersionId, ordinal, encId, HasPortablePdb, generationOrdinals, typesAdded, eventsAdded, fieldsAdded, methodsAdded, firstParamRowMap, propertiesAdded, eventMapAdded, propertyMapAdded, methodImplsAdded, customAttributesAdded, tableEntriesAdded, blobStreamLengthAdded, stringStreamLengthAdded, userStringStreamLengthAdded, guidStreamLengthAdded, synthesizedTypes, synthesizedMembers, deletedMembers, addedOrChangedMethods, debugInformationProvider, localSignatureProvider, TypeToEventMap, TypeToPropertyMap, MethodImpls);
	}

	private static ImmutableArray<int> CalculateTableSizes(MetadataReader reader, ImmutableArray<int> delta)
	{
		int[] array = new int[MetadataTokens.TableCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = reader.GetTableRowCount((TableIndex)i) + delta[i];
		}
		return ImmutableArray.Create(array);
	}

	private static Dictionary<int, int> CalculateTypePropertyMap(MetadataReader reader)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 1;
		foreach (TypeDefinitionHandle typesWithProperty in reader.GetTypesWithProperties())
		{
			dictionary.Add(reader.GetRowNumber(typesWithProperty), num);
			num++;
		}
		return dictionary;
	}

	private static Dictionary<int, int> CalculateTypeEventMap(MetadataReader reader)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 1;
		foreach (TypeDefinitionHandle typesWithEvent in reader.GetTypesWithEvents())
		{
			dictionary.Add(reader.GetRowNumber(typesWithEvent), num);
			num++;
		}
		return dictionary;
	}

	private static Dictionary<MethodImplKey, int> CalculateMethodImpls(MetadataReader reader)
	{
		Dictionary<MethodImplKey, int> dictionary = new Dictionary<MethodImplKey, int>();
		int tableRowCount = reader.GetTableRowCount(TableIndex.MethodImpl);
		for (int i = 1; i <= tableRowCount; i++)
		{
			int rowNumber = MetadataTokens.GetRowNumber(reader.GetMethodImplementation(MetadataTokens.MethodImplementationHandle(i)).MethodBody);
			int num = 1;
			MethodImplKey key;
			while (true)
			{
				key = new MethodImplKey(rowNumber, num);
				if (!dictionary.ContainsKey(key))
				{
					break;
				}
				num++;
			}
			dictionary.Add(key, i);
		}
		return dictionary;
	}

	internal int GetNextAnonymousTypeIndex(bool fromDelegates = false)
	{
		int num = 0;
		foreach (var (anonymousTypeKey2, anonymousTypeValue2) in SynthesizedTypes.AnonymousTypes)
		{
			if (fromDelegates == anonymousTypeKey2.IsDelegate)
			{
				int uniqueIndex = anonymousTypeValue2.UniqueIndex;
				if (uniqueIndex >= num)
				{
					num = uniqueIndex + 1;
				}
			}
		}
		return num;
	}

	internal int GetNextAnonymousDelegateIndex()
	{
		int num = 0;
		foreach (var (_, immutableArray2) in SynthesizedTypes.AnonymousDelegatesWithIndexedNames)
		{
			foreach (AnonymousTypeValue item in immutableArray2)
			{
				int uniqueIndex = item.UniqueIndex;
				if (uniqueIndex >= num)
				{
					num = uniqueIndex + 1;
				}
			}
		}
		return num;
	}
}
