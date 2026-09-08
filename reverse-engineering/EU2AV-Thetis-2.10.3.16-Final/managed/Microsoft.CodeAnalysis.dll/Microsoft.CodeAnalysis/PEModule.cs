using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class PEModule : IDisposable
{
	private delegate bool AttributeValueExtractor<T>(out T value, ref BlobReader sigReader);

	internal readonly struct BoolAndStringArrayData(bool sense, ImmutableArray<string?> strings)
	{
		public readonly bool Sense = sense;

		public readonly ImmutableArray<string?> Strings = strings;
	}

	internal readonly struct BoolAndStringData(bool sense, string? @string)
	{
		public readonly bool Sense = sense;

		public readonly string? String = @string;
	}

	private sealed class PEHashProvider : CryptographicHashProvider
	{
		private readonly PEReader _peReader;

		public PEHashProvider(PEReader peReader)
		{
			_peReader = peReader;
		}

		internal unsafe override ImmutableArray<byte> ComputeHash(HashAlgorithm algorithm)
		{
			PEMemoryBlock entireImage = _peReader.GetEntireImage();
			byte[] items;
			using (ReadOnlyUnmanagedMemoryStream inputStream = new ReadOnlyUnmanagedMemoryStream(_peReader, (IntPtr)entireImage.Pointer, entireImage.Length))
			{
				items = algorithm.ComputeHash(inputStream);
			}
			return ImmutableArray.Create(items);
		}
	}

	private readonly struct TypeDefToNamespace
	{
		internal readonly TypeDefinitionHandle TypeDef;

		internal readonly NamespaceDefinitionHandle NamespaceHandle;

		internal TypeDefToNamespace(TypeDefinitionHandle typeDef, NamespaceDefinitionHandle namespaceHandle)
		{
			TypeDef = typeDef;
			NamespaceHandle = namespaceHandle;
		}
	}

	internal sealed class TypesByNamespaceSortComparer : IComparer<IGrouping<string, TypeDefinitionHandle>>
	{
		private readonly StringComparer _nameComparer;

		public TypesByNamespaceSortComparer(StringComparer nameComparer)
		{
			_nameComparer = nameComparer;
		}

		public int Compare(IGrouping<string, TypeDefinitionHandle>? left, IGrouping<string, TypeDefinitionHandle>? right)
		{
			if (left == right)
			{
				return 0;
			}
			if (left == null)
			{
				return -1;
			}
			if (right == null)
			{
				return 1;
			}
			int num = _nameComparer.Compare(left.Key, right.Key);
			if (num == 0)
			{
				TypeDefinitionHandle typeDefinitionHandle = left.FirstOrDefault();
				TypeDefinitionHandle typeDefinitionHandle2 = right.FirstOrDefault();
				num = ((!(typeDefinitionHandle.IsNil ^ typeDefinitionHandle2.IsNil)) ? HandleComparer.Default.Compare(typeDefinitionHandle, typeDefinitionHandle2) : (typeDefinitionHandle.IsNil ? 1 : (-1)));
				if (num == 0)
				{
					num = string.CompareOrdinal(left.Key, right.Key);
				}
			}
			return num;
		}
	}

	private class NamespaceHandleEqualityComparer : IEqualityComparer<NamespaceDefinitionHandle>
	{
		public static readonly NamespaceHandleEqualityComparer Singleton = new NamespaceHandleEqualityComparer();

		private NamespaceHandleEqualityComparer()
		{
		}

		public bool Equals(NamespaceDefinitionHandle x, NamespaceDefinitionHandle y)
		{
			return x == y;
		}

		public int GetHashCode(NamespaceDefinitionHandle obj)
		{
			return obj.GetHashCode();
		}
	}

	private struct StringAndInt
	{
		public string? StringValue;

		public int IntValue;
	}

	internal readonly struct AttributeInfo(CustomAttributeHandle handle, int signatureIndex)
	{
		public readonly CustomAttributeHandle Handle = handle;

		public readonly byte SignatureIndex = (byte)signatureIndex;

		public bool HasValue => !Handle.IsNil;
	}

	private sealed class StringTableDecoder : MetadataStringDecoder
	{
		public static readonly StringTableDecoder Instance = new StringTableDecoder();

		private StringTableDecoder()
			: base(System.Text.Encoding.UTF8)
		{
		}

		public unsafe override string GetString(byte* bytes, int byteCount)
		{
			return StringTable.AddSharedUtf8(new ReadOnlySpan<byte>(bytes, byteCount));
		}
	}

	private readonly ModuleMetadata _owner;

	private readonly PEReader _peReaderOpt;

	private readonly IntPtr _metadataPointerOpt;

	private readonly int _metadataSizeOpt;

	private MetadataReader _lazyMetadataReader;

	private ImmutableArray<AssemblyIdentity> _lazyAssemblyReferences;

	private static readonly Dictionary<string, (int FirstIndex, int SecondIndex)> s_sharedEmptyForwardedTypes = new Dictionary<string, (int, int)>();

	private static readonly Dictionary<string, (string OriginalName, int FirstIndex, int SecondIndex)> s_sharedEmptyCaseInsensitiveForwardedTypes = new Dictionary<string, (string, int, int)>();

	private Dictionary<string, (int FirstIndex, int SecondIndex)> _lazyForwardedTypesToAssemblyIndexMap;

	private Dictionary<string, (string OriginalName, int FirstIndex, int SecondIndex)> _lazyCaseInsensitiveForwardedTypesToAssemblyIndexMap;

	private readonly Lazy<IdentifierCollection> _lazyTypeNameCollection;

	private readonly Lazy<IdentifierCollection> _lazyNamespaceNameCollection;

	private string _lazyName;

	private bool _isDisposed;

	private ThreeState _lazyContainsNoPiaLocalTypes;

	private int[] _lazyNoPiaLocalTypeCheckBitMap;

	private ConcurrentDictionary<TypeDefinitionHandle, AttributeInfo> _lazyTypeDefToTypeIdentifierMap;

	private readonly CryptographicHashProvider _hashesOpt;

	private static readonly AttributeValueExtractor<string?> s_attributeStringValueExtractor = CrackStringInAttributeValue;

	private static readonly AttributeValueExtractor<(int, int)> s_attributeIntAndIntValueExtractor = CrackIntAndIntInAttributeValue;

	private static readonly AttributeValueExtractor<StringAndInt> s_attributeStringAndIntValueExtractor = CrackStringAndIntInAttributeValue;

	private static readonly AttributeValueExtractor<(string?, string?)> s_attributeStringAndStringValueExtractor = CrackStringAndStringInAttributeValue;

	private static readonly AttributeValueExtractor<bool> s_attributeBooleanValueExtractor = CrackBooleanInAttributeValue;

	private static readonly AttributeValueExtractor<byte> s_attributeByteValueExtractor = CrackByteInAttributeValue;

	private static readonly AttributeValueExtractor<short> s_attributeShortValueExtractor = CrackShortInAttributeValue;

	private static readonly AttributeValueExtractor<int> s_attributeIntValueExtractor = CrackIntInAttributeValue;

	private static readonly AttributeValueExtractor<long> s_attributeLongValueExtractor = CrackLongInAttributeValue;

	private static readonly AttributeValueExtractor<decimal> s_decimalValueInDecimalConstantAttributeExtractor = CrackDecimalInDecimalConstantAttribute;

	private static readonly AttributeValueExtractor<ImmutableArray<bool>> s_attributeBoolArrayValueExtractor = CrackBoolArrayInAttributeValue;

	private static readonly AttributeValueExtractor<ImmutableArray<byte>> s_attributeByteArrayValueExtractor = CrackByteArrayInAttributeValue;

	private static readonly AttributeValueExtractor<ImmutableArray<string?>> s_attributeStringArrayValueExtractor = CrackStringArrayInAttributeValue;

	private static readonly AttributeValueExtractor<ObsoleteAttributeData?> s_attributeDeprecatedDataExtractor = CrackDeprecatedAttributeData;

	private static readonly AttributeValueExtractor<BoolAndStringArrayData> s_attributeBoolAndStringArrayValueExtractor = CrackBoolAndStringArrayInAttributeValue;

	private static readonly AttributeValueExtractor<BoolAndStringData> s_attributeBoolAndStringValueExtractor = CrackBoolAndStringInAttributeValue;

	private static readonly ImmutableArray<bool> s_simpleTransformFlags = ImmutableArray.Create(item: true);

	internal const string ByRefLikeMarker = "Types with embedded references are not supported in this version of your compiler.";

	internal const string RequiredMembersMarker = "Constructors of types with required members are not supported in this version of your compiler.";

	internal bool IsDisposed => _isDisposed;

	internal PEReader PEReaderOpt => _peReaderOpt;

	internal MetadataReader MetadataReader
	{
		get
		{
			if (_lazyMetadataReader == null)
			{
				InitializeMetadataReader();
			}
			if (_isDisposed)
			{
				ThrowMetadataDisposed();
			}
			return _lazyMetadataReader;
		}
	}

	internal bool IsManifestModule => MetadataReader.IsAssembly;

	internal bool IsLinkedModule => !MetadataReader.IsAssembly;

	internal bool IsCOFFOnly
	{
		get
		{
			if (_peReaderOpt == null)
			{
				return false;
			}
			return _peReaderOpt.PEHeaders.IsCoffOnly;
		}
	}

	internal Machine Machine
	{
		get
		{
			if (_peReaderOpt == null)
			{
				return Machine.I386;
			}
			return _peReaderOpt.PEHeaders.CoffHeader.Machine;
		}
	}

	internal bool Bit32Required
	{
		get
		{
			if (_peReaderOpt == null)
			{
				return false;
			}
			return (_peReaderOpt.PEHeaders.CorHeader.Flags & CorFlags.Requires32Bit) != 0;
		}
	}

	internal string Name
	{
		get
		{
			if (_lazyName == null)
			{
				_lazyName = MetadataReader.GetString(MetadataReader.GetModuleDefinition().Name);
			}
			return _lazyName;
		}
	}

	public ImmutableArray<AssemblyIdentity> ReferencedAssemblies
	{
		get
		{
			if (_lazyAssemblyReferences == null)
			{
				_lazyAssemblyReferences = MetadataReader.GetReferencedAssembliesOrThrow();
			}
			return _lazyAssemblyReferences;
		}
	}

	internal string MetadataVersion => MetadataReader.MetadataVersion;

	internal IdentifierCollection TypeNames => _lazyTypeNameCollection.Value;

	internal IdentifierCollection NamespaceNames => _lazyNamespaceNameCollection.Value;

	internal bool HasIL => IsEntireImageAvailable;

	internal bool IsEntireImageAvailable
	{
		get
		{
			if (_peReaderOpt != null)
			{
				return _peReaderOpt.IsEntireImageAvailable;
			}
			return false;
		}
	}

	internal PEModule(ModuleMetadata owner, PEReader peReader, IntPtr metadataOpt, int metadataSizeOpt, bool includeEmbeddedInteropTypes, bool ignoreAssemblyRefs)
	{
		_owner = owner;
		_peReaderOpt = peReader;
		_metadataPointerOpt = metadataOpt;
		_metadataSizeOpt = metadataSizeOpt;
		_lazyTypeNameCollection = new Lazy<IdentifierCollection>(ComputeTypeNameCollection);
		_lazyNamespaceNameCollection = new Lazy<IdentifierCollection>(ComputeNamespaceNameCollection);
		_hashesOpt = ((peReader != null) ? new PEHashProvider(peReader) : null);
		_lazyContainsNoPiaLocalTypes = (includeEmbeddedInteropTypes ? ThreeState.False : ThreeState.Unknown);
		if (ignoreAssemblyRefs)
		{
			_lazyAssemblyReferences = ImmutableArray<AssemblyIdentity>.Empty;
		}
	}

	public void Dispose()
	{
		_isDisposed = true;
		_peReaderOpt?.Dispose();
	}

	private unsafe void InitializeMetadataReader()
	{
		MetadataReader value;
		if (_metadataPointerOpt != IntPtr.Zero)
		{
			value = new MetadataReader((byte*)(void*)_metadataPointerOpt, _metadataSizeOpt, MetadataReaderOptions.Default, StringTableDecoder.Instance);
		}
		else
		{
			bool flag;
			try
			{
				flag = _peReaderOpt.HasMetadata;
			}
			catch
			{
				flag = false;
			}
			if (!flag)
			{
				throw new BadImageFormatException(CodeAnalysisResources.PEImageDoesntContainManagedMetadata);
			}
			value = _peReaderOpt.GetMetadataReader(MetadataReaderOptions.Default, StringTableDecoder.Instance);
		}
		Interlocked.CompareExchange(ref _lazyMetadataReader, value, null);
	}

	private static void ThrowMetadataDisposed()
	{
		throw new ObjectDisposedException("ModuleMetadata");
	}

	internal ImmutableArray<byte> GetHash(AssemblyHashAlgorithm algorithmId)
	{
		return _hashesOpt.GetHash(algorithmId);
	}

	internal Guid GetModuleVersionIdOrThrow()
	{
		return MetadataReader.GetModuleVersionIdOrThrow();
	}

	internal ImmutableArray<string> GetMetadataModuleNamesOrThrow()
	{
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		try
		{
			foreach (AssemblyFileHandle assemblyFile2 in MetadataReader.AssemblyFiles)
			{
				AssemblyFile assemblyFile = MetadataReader.GetAssemblyFile(assemblyFile2);
				if (assemblyFile.ContainsMetadata)
				{
					string text = MetadataReader.GetString(assemblyFile.Name);
					if (!MetadataHelpers.IsValidMetadataFileName(text))
					{
						throw new BadImageFormatException(string.Format(CodeAnalysisResources.InvalidModuleName, Name, text));
					}
					instance.Add(text);
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	internal IEnumerable<string> GetReferencedManagedModulesOrThrow()
	{
		HashSet<EntityHandle> hashSet = new HashSet<EntityHandle>();
		foreach (TypeReferenceHandle typeReference in MetadataReader.TypeReferences)
		{
			EntityHandle resolutionScope = MetadataReader.GetTypeReference(typeReference).ResolutionScope;
			if (resolutionScope.Kind == HandleKind.ModuleReference)
			{
				hashSet.Add(resolutionScope);
			}
		}
		foreach (EntityHandle item in hashSet)
		{
			yield return GetModuleRefNameOrThrow((ModuleReferenceHandle)item);
		}
	}

	internal ImmutableArray<EmbeddedResource> GetEmbeddedResourcesOrThrow()
	{
		if (MetadataReader.ManifestResources.Count == 0)
		{
			return ImmutableArray<EmbeddedResource>.Empty;
		}
		ImmutableArray<EmbeddedResource>.Builder builder = ImmutableArray.CreateBuilder<EmbeddedResource>();
		foreach (ManifestResourceHandle manifestResource2 in MetadataReader.ManifestResources)
		{
			ManifestResource manifestResource = MetadataReader.GetManifestResource(manifestResource2);
			if (manifestResource.Implementation.IsNil)
			{
				string name = MetadataReader.GetString(manifestResource.Name);
				builder.Add(new EmbeddedResource((uint)manifestResource.Offset, manifestResource.Attributes, name));
			}
		}
		return builder.ToImmutable();
	}

	public string GetModuleRefNameOrThrow(ModuleReferenceHandle moduleRef)
	{
		return MetadataReader.GetString(MetadataReader.GetModuleReference(moduleRef).Name);
	}

	internal BlobReader GetMemoryReaderOrThrow(BlobHandle blob)
	{
		return MetadataReader.GetBlobReader(blob);
	}

	internal string GetFullNameOrThrow(StringHandle namespaceHandle, StringHandle nameHandle)
	{
		string name = MetadataReader.GetString(nameHandle);
		return MetadataHelpers.BuildQualifiedName(MetadataReader.GetString(namespaceHandle), name);
	}

	internal AssemblyIdentity ReadAssemblyIdentityOrThrow()
	{
		return MetadataReader.ReadAssemblyIdentityOrThrow();
	}

	public TypeDefinitionHandle GetContainingTypeOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetDeclaringType();
	}

	public string GetTypeDefNameOrThrow(TypeDefinitionHandle typeDef)
	{
		TypeDefinition typeDefinition = MetadataReader.GetTypeDefinition(typeDef);
		string text = MetadataReader.GetString(typeDefinition.Name);
		if (IsNestedTypeDefOrThrow(typeDef))
		{
			string text2 = MetadataReader.GetString(typeDefinition.Namespace);
			if (text2.Length > 0)
			{
				text = text2 + "." + text;
			}
		}
		return text;
	}

	public string GetTypeDefNamespaceOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetString(MetadataReader.GetTypeDefinition(typeDef).Namespace);
	}

	public EntityHandle GetTypeDefExtendsOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).BaseType;
	}

	public TypeAttributes GetTypeDefFlagsOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).Attributes;
	}

	public GenericParameterHandleCollection GetTypeDefGenericParamsOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetGenericParameters();
	}

	public bool HasGenericParametersOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetGenericParameters().Count > 0;
	}

	public void GetTypeDefPropsOrThrow(TypeDefinitionHandle typeDef, out string name, out string @namespace, out TypeAttributes flags, out EntityHandle extends)
	{
		TypeDefinition typeDefinition = MetadataReader.GetTypeDefinition(typeDef);
		name = MetadataReader.GetString(typeDefinition.Name);
		@namespace = MetadataReader.GetString(typeDefinition.Namespace);
		flags = typeDefinition.Attributes;
		extends = typeDefinition.BaseType;
	}

	internal bool IsNestedTypeDefOrThrow(TypeDefinitionHandle typeDef)
	{
		return IsNestedTypeDefOrThrow(MetadataReader, typeDef);
	}

	private static bool IsNestedTypeDefOrThrow(MetadataReader metadataReader, TypeDefinitionHandle typeDef)
	{
		return IsNested(metadataReader.GetTypeDefinition(typeDef).Attributes);
	}

	internal bool IsInterfaceOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).Attributes.IsInterface();
	}

	private IEnumerable<TypeDefToNamespace> GetTypeDefsOrThrow(bool topLevelOnly)
	{
		foreach (TypeDefinitionHandle typeDefinition2 in MetadataReader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = MetadataReader.GetTypeDefinition(typeDefinition2);
			if (!topLevelOnly || !IsNested(typeDefinition.Attributes))
			{
				yield return new TypeDefToNamespace(typeDefinition2, typeDefinition.NamespaceDefinition);
			}
		}
	}

	internal IEnumerable<IGrouping<string, TypeDefinitionHandle>> GroupTypesByNamespaceOrThrow(StringComparer nameComparer)
	{
		Dictionary<string, ArrayBuilder<TypeDefinitionHandle>> dictionary = new Dictionary<string, ArrayBuilder<TypeDefinitionHandle>>();
		GetTypeNamespaceNamesOrThrow(dictionary);
		GetForwardedTypeNamespaceNamesOrThrow(dictionary);
		ArrayBuilder<IGrouping<string, TypeDefinitionHandle>> arrayBuilder = new ArrayBuilder<IGrouping<string, TypeDefinitionHandle>>(dictionary.Count);
		foreach (KeyValuePair<string, ArrayBuilder<TypeDefinitionHandle>> item in dictionary)
		{
			string key = item.Key;
			IEnumerable<TypeDefinitionHandle> value = item.Value;
			arrayBuilder.Add(new Grouping<string, TypeDefinitionHandle>(key, value ?? SpecializedCollections.EmptyEnumerable<TypeDefinitionHandle>()));
		}
		arrayBuilder.Sort(new TypesByNamespaceSortComparer(nameComparer));
		return arrayBuilder;
	}

	private void GetTypeNamespaceNamesOrThrow(Dictionary<string, ArrayBuilder<TypeDefinitionHandle>> namespaces)
	{
		Dictionary<NamespaceDefinitionHandle, ArrayBuilder<TypeDefinitionHandle>> dictionary = new Dictionary<NamespaceDefinitionHandle, ArrayBuilder<TypeDefinitionHandle>>(NamespaceHandleEqualityComparer.Singleton);
		foreach (TypeDefToNamespace item in GetTypeDefsOrThrow(topLevelOnly: true))
		{
			NamespaceDefinitionHandle namespaceHandle = item.NamespaceHandle;
			TypeDefinitionHandle typeDef = item.TypeDef;
			if (dictionary.TryGetValue(namespaceHandle, out var value))
			{
				value.Add(typeDef);
				continue;
			}
			dictionary.Add(namespaceHandle, new ArrayBuilder<TypeDefinitionHandle> { typeDef });
		}
		foreach (KeyValuePair<NamespaceDefinitionHandle, ArrayBuilder<TypeDefinitionHandle>> item2 in dictionary)
		{
			string key = MetadataReader.GetString(item2.Key);
			if (namespaces.TryGetValue(key, out ArrayBuilder<TypeDefinitionHandle> value2))
			{
				value2.AddRange(item2.Value);
			}
			else
			{
				namespaces.Add(key, item2.Value);
			}
		}
	}

	private void GetForwardedTypeNamespaceNamesOrThrow(Dictionary<string, ArrayBuilder<TypeDefinitionHandle>?> namespaces)
	{
		EnsureForwardTypeToAssemblyMap();
		foreach (string key2 in _lazyForwardedTypesToAssemblyIndexMap.Keys)
		{
			int num = key2.LastIndexOf('.');
			string key = ((num >= 0) ? key2.Substring(0, num) : "");
			if (!namespaces.ContainsKey(key))
			{
				namespaces.Add(key, null);
			}
		}
	}

	private IdentifierCollection ComputeTypeNameCollection()
	{
		try
		{
			return new IdentifierCollection(from typeDef in GetTypeDefsOrThrow(topLevelOnly: false)
				let metadataName = GetTypeDefNameOrThrow(typeDef.TypeDef)
				let backtickIndex = metadataName.IndexOf('`')
				select (backtickIndex >= 0) ? metadataName.Substring(0, backtickIndex) : metadataName);
		}
		catch (BadImageFormatException)
		{
			return new IdentifierCollection();
		}
	}

	private IdentifierCollection ComputeNamespaceNameCollection()
	{
		try
		{
			return new IdentifierCollection(from fullName in (from id in GetTypeDefsOrThrow(topLevelOnly: true).Where(delegate(TypeDefToNamespace id)
					{
						TypeDefToNamespace typeDefToNamespace = id;
						return !typeDefToNamespace.NamespaceHandle.IsNil;
					})
					select MetadataReader.GetString(id.NamespaceHandle)).Distinct()
				from name in fullName.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries)
				select name);
		}
		catch (BadImageFormatException)
		{
			return new IdentifierCollection();
		}
	}

	internal ImmutableArray<TypeDefinitionHandle> GetNestedTypeDefsOrThrow(TypeDefinitionHandle container)
	{
		return MetadataReader.GetTypeDefinition(container).GetNestedTypes();
	}

	internal MethodImplementationHandleCollection GetMethodImplementationsOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetMethodImplementations();
	}

	internal InterfaceImplementationHandleCollection GetInterfaceImplementationsOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetInterfaceImplementations();
	}

	internal MethodDefinitionHandleCollection GetMethodsOfTypeOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetMethods();
	}

	internal PropertyDefinitionHandleCollection GetPropertiesOfTypeOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetProperties();
	}

	internal EventDefinitionHandleCollection GetEventsOfTypeOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetEvents();
	}

	internal FieldDefinitionHandleCollection GetFieldsOfTypeOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).GetFields();
	}

	internal EntityHandle GetBaseTypeOfTypeOrThrow(TypeDefinitionHandle typeDef)
	{
		return MetadataReader.GetTypeDefinition(typeDef).BaseType;
	}

	internal TypeLayout GetTypeLayout(TypeDefinitionHandle typeDef)
	{
		try
		{
			TypeDefinition typeDefinition = MetadataReader.GetTypeDefinition(typeDef);
			LayoutKind kind;
			switch (typeDefinition.Attributes & TypeAttributes.LayoutMask)
			{
			case TypeAttributes.SequentialLayout:
				kind = LayoutKind.Sequential;
				break;
			case TypeAttributes.ExplicitLayout:
				kind = LayoutKind.Explicit;
				break;
			case TypeAttributes.NotPublic:
				return default(TypeLayout);
			default:
				return default(TypeLayout);
			}
			System.Reflection.Metadata.TypeLayout layout = typeDefinition.GetLayout();
			int num = layout.Size;
			int num2 = layout.PackingSize;
			if (num2 > 255)
			{
				num2 = 0;
			}
			if (num < 0)
			{
				num = 0;
			}
			return new TypeLayout(kind, num, (byte)num2);
		}
		catch (BadImageFormatException)
		{
			return default(TypeLayout);
		}
	}

	internal bool IsNoPiaLocalType(TypeDefinitionHandle typeDef)
	{
		AttributeInfo attributeInfo;
		return IsNoPiaLocalType(typeDef, out attributeInfo);
	}

	internal bool HasParamArrayAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.ParamArrayAttribute).HasValue;
	}

	internal bool HasParamCollectionAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.ParamCollectionAttribute).HasValue;
	}

	internal bool HasIsReadOnlyAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.IsReadOnlyAttribute).HasValue;
	}

	internal bool HasDoesNotReturnAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.DoesNotReturnAttribute).HasValue;
	}

	internal bool HasIsUnmanagedAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.IsUnmanagedAttribute).HasValue;
	}

	internal bool HasExtensionAttribute(EntityHandle token, bool ignoreCase)
	{
		return FindTargetAttribute(token, ignoreCase ? AttributeDescription.CaseInsensitiveExtensionAttribute : AttributeDescription.CaseSensitiveExtensionAttribute).HasValue;
	}

	internal bool HasVisualBasicEmbeddedAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.VisualBasicEmbeddedAttribute).HasValue;
	}

	internal bool HasCodeAnalysisEmbeddedAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.CodeAnalysisEmbeddedAttribute).HasValue;
	}

	internal bool HasCompilerLoweringPreserveAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.CompilerLoweringPreserveAttribute).HasValue;
	}

	internal bool HasInterpolatedStringHandlerAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.InterpolatedStringHandlerAttribute).HasValue;
	}

	internal bool HasDefaultMemberAttribute(EntityHandle token, out string memberName)
	{
		return HasStringValuedAttribute(token, AttributeDescription.DefaultMemberAttribute, out memberName);
	}

	internal bool HasExtensionMarkerAttribute(EntityHandle token, out string markerName)
	{
		return HasStringValuedAttribute(token, AttributeDescription.ExtensionMarkerAttribute, out markerName);
	}

	internal bool HasGuidAttribute(EntityHandle token, out string guidValue)
	{
		return HasStringValuedAttribute(token, AttributeDescription.GuidAttribute, out guidValue);
	}

	internal bool HasImportedFromTypeLibAttribute(EntityHandle token, out string libValue)
	{
		return HasStringValuedAttribute(token, AttributeDescription.ImportedFromTypeLibAttribute, out libValue);
	}

	internal bool HasPrimaryInteropAssemblyAttribute(EntityHandle token, out int majorValue, out int minorValue)
	{
		return HasIntAndIntValuedAttribute(token, AttributeDescription.PrimaryInteropAssemblyAttribute, out majorValue, out minorValue);
	}

	internal bool HasFixedBufferAttribute(EntityHandle token, out string elementTypeName, out int bufferSize)
	{
		return HasStringAndIntValuedAttribute(token, AttributeDescription.FixedBufferAttribute, out elementTypeName, out bufferSize);
	}

	internal bool HasAccessedThroughPropertyAttribute(EntityHandle token, out string propertyName)
	{
		return HasStringValuedAttribute(token, AttributeDescription.AccessedThroughPropertyAttribute, out propertyName);
	}

	internal bool HasRequiredAttributeAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.RequiredAttributeAttribute).HasValue;
	}

	internal bool HasCollectionBuilderAttribute(EntityHandle token, out string builderTypeName, out string methodName)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.CollectionBuilderAttribute);
		if (attributeInfo.HasValue)
		{
			return TryExtractStringAndStringValueFromAttribute(attributeInfo.Handle, out builderTypeName, out methodName);
		}
		builderTypeName = null;
		methodName = null;
		return false;
	}

	internal bool HasAttribute(EntityHandle token, AttributeDescription description)
	{
		return FindTargetAttribute(token, description).HasValue;
	}

	internal CustomAttributeHandle GetAttributeHandle(EntityHandle token, AttributeDescription description)
	{
		return FindTargetAttribute(token, description).Handle;
	}

	internal bool HasDynamicAttribute(EntityHandle token, out ImmutableArray<bool> transformFlags)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.DynamicAttribute);
		if (!attributeInfo.HasValue)
		{
			transformFlags = default(ImmutableArray<bool>);
			return false;
		}
		if (attributeInfo.SignatureIndex == 0)
		{
			transformFlags = s_simpleTransformFlags;
			return true;
		}
		return TryExtractBoolArrayValueFromAttribute(attributeInfo.Handle, out transformFlags);
	}

	internal bool HasNativeIntegerAttribute(EntityHandle token, out ImmutableArray<bool> transformFlags)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.NativeIntegerAttribute);
		if (!attributeInfo.HasValue)
		{
			transformFlags = default(ImmutableArray<bool>);
			return false;
		}
		if (attributeInfo.SignatureIndex == 0)
		{
			transformFlags = s_simpleTransformFlags;
			return true;
		}
		return TryExtractBoolArrayValueFromAttribute(attributeInfo.Handle, out transformFlags);
	}

	internal bool HasScopedRefAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.ScopedRefAttribute).HasValue;
	}

	internal bool HasUnscopedRefAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.UnscopedRefAttribute).HasValue;
	}

	internal bool HasRefSafetyRulesAttribute(EntityHandle token, out int version, out bool foundAttributeType)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(MetadataReader, token, AttributeDescription.RefSafetyRulesAttribute, out foundAttributeType);
		if (attributeInfo.HasValue && TryExtractValueFromAttribute(attributeInfo.Handle, out var value, s_attributeIntValueExtractor))
		{
			version = value;
			return true;
		}
		version = 0;
		return false;
	}

	internal bool HasInlineArrayAttribute(TypeDefinitionHandle token, out int length)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.InlineArrayAttribute);
		if (attributeInfo.HasValue && TryExtractValueFromAttribute(attributeInfo.Handle, out var value, s_attributeIntValueExtractor))
		{
			length = value;
			return true;
		}
		length = 0;
		return false;
	}

	internal bool HasTupleElementNamesAttribute(EntityHandle token, out ImmutableArray<string> tupleElementNames)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.TupleElementNamesAttribute);
		if (!attributeInfo.HasValue)
		{
			tupleElementNames = default(ImmutableArray<string>);
			return false;
		}
		return TryExtractStringArrayValueFromAttribute(attributeInfo.Handle, out tupleElementNames);
	}

	internal bool HasIsByRefLikeAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.IsByRefLikeAttribute).HasValue;
	}

	internal bool HasRequiresLocationAttribute(EntityHandle token)
	{
		return FindTargetAttribute(token, AttributeDescription.RequiresLocationAttribute).HasValue;
	}

	internal ObsoleteAttributeData TryGetDeprecatedOrExperimentalOrObsoleteAttribute(EntityHandle token, IAttributeNamedArgumentDecoder decoder, bool ignoreByRefLikeMarker, bool ignoreRequiredMemberMarker)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.DeprecatedAttribute);
		if (attributeInfo.HasValue)
		{
			return TryExtractDeprecatedDataFromAttribute(attributeInfo);
		}
		attributeInfo = FindTargetAttribute(token, AttributeDescription.ObsoleteAttribute);
		if (attributeInfo.HasValue)
		{
			ObsoleteAttributeData obsoleteAttributeData = TryExtractObsoleteDataFromAttribute(attributeInfo, decoder);
			string text = obsoleteAttributeData?.Message;
			if (!(text == "Types with embedded references are not supported in this version of your compiler."))
			{
				if (text == "Constructors of types with required members are not supported in this version of your compiler." && ignoreRequiredMemberMarker)
				{
					return null;
				}
			}
			else if (ignoreByRefLikeMarker)
			{
				return null;
			}
			return obsoleteAttributeData;
		}
		attributeInfo = FindTargetAttribute(token, AttributeDescription.WindowsExperimentalAttribute);
		if (attributeInfo.HasValue)
		{
			return TryExtractWindowsExperimentalDataFromAttribute(attributeInfo);
		}
		attributeInfo = FindTargetAttribute(token, AttributeDescription.ExperimentalAttribute);
		if (attributeInfo.HasValue)
		{
			return TryExtractExperimentalDataFromAttribute(attributeInfo, decoder);
		}
		return null;
	}

	internal static bool IsMoreImportantObsoleteKind(ObsoleteAttributeKind firstKind, ObsoleteAttributeKind secondKind)
	{
		return getPriority(firstKind) <= getPriority(secondKind);
		static int getPriority(ObsoleteAttributeKind kind)
		{
			return kind switch
			{
				ObsoleteAttributeKind.Deprecated => 0, 
				ObsoleteAttributeKind.Obsolete => 1, 
				ObsoleteAttributeKind.WindowsExperimental => 2, 
				ObsoleteAttributeKind.Experimental => 3, 
				ObsoleteAttributeKind.Uninitialized => 4, 
				_ => throw ExceptionUtilities.UnexpectedValue(kind), 
			};
		}
	}

	internal ObsoleteAttributeData? TryDecodeExperimentalAttributeData(EntityHandle handle, IAttributeNamedArgumentDecoder decoder)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(handle, AttributeDescription.ExperimentalAttribute);
		if (!attributeInfo.HasValue)
		{
			return null;
		}
		return TryExtractExperimentalDataFromAttribute(attributeInfo, decoder);
	}

	private ObsoleteAttributeData? TryExtractExperimentalDataFromAttribute(AttributeInfo attributeInfo, IAttributeNamedArgumentDecoder decoder)
	{
		if (!TryGetAttributeReader(attributeInfo.Handle, out var blobReader))
		{
			return null;
		}
		if (attributeInfo.SignatureIndex != 0)
		{
			throw ExceptionUtilities.UnexpectedValue(attributeInfo.SignatureIndex);
		}
		if (blobReader.RemainingBytes <= 0 || !CrackStringInAttributeValue(out string value, ref blobReader))
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(value))
		{
			value = null;
		}
		var (urlFormat, message) = crackUrlFormatAndMessage(decoder, ref blobReader);
		return new ObsoleteAttributeData(ObsoleteAttributeKind.Experimental, message, isError: false, value, urlFormat);
		static (string? urlFormat, string? message) crackUrlFormatAndMessage(IAttributeNamedArgumentDecoder attributeNamedArgumentDecoder, ref BlobReader sig)
		{
			if (sig.RemainingBytes <= 0)
			{
				return default((string, string));
			}
			string text = null;
			string text2 = null;
			try
			{
				ushort num = sig.ReadUInt16();
				for (int i = 0; i < num; i++)
				{
					if (text != null && text2 != null)
					{
						break;
					}
					(KeyValuePair<string, TypedConstant> nameValuePair, bool isProperty, SerializationTypeCode typeCode, SerializationTypeCode elementTypeCode) tuple2 = attributeNamedArgumentDecoder.DecodeCustomAttributeNamedArgumentOrThrow(ref sig);
					var (text4, typedConstant2) = tuple2.nameValuePair;
					bool item = tuple2.isProperty;
					if (((tuple2.typeCode == SerializationTypeCode.String) & item) && typedConstant2.ValueInternal is string text5)
					{
						if (text == null && text4 == "UrlFormat")
						{
							text = text5;
						}
						else if (text2 == null && text4 == "Message")
						{
							text2 = text5;
						}
					}
				}
			}
			catch (BadImageFormatException)
			{
			}
			catch (UnsupportedSignatureContent)
			{
			}
			return (urlFormat: text, message: text2);
		}
	}

	internal string? GetFirstUnsupportedCompilerFeatureFromToken(EntityHandle token, IAttributeNamedArgumentDecoder attributeNamedArgumentDecoder, CompilerFeatureRequiredFeatures allowedFeatures)
	{
		List<AttributeInfo> list = FindTargetAttributes(token, AttributeDescription.CompilerFeatureRequiredAttribute);
		if (list == null)
		{
			return null;
		}
		foreach (AttributeInfo item in list)
		{
			if (!item.HasValue || !TryGetAttributeReader(item.Handle, out var argReader) || !CrackStringInAttributeValue(out string value, ref argReader))
			{
				continue;
			}
			bool flag = false;
			if (argReader.RemainingBytes >= 2)
			{
				try
				{
					ushort num = argReader.ReadUInt16();
					for (uint num2 = 0u; num2 < num; num2++)
					{
						(KeyValuePair<string, TypedConstant>, bool, SerializationTypeCode, SerializationTypeCode) tuple = attributeNamedArgumentDecoder.DecodeCustomAttributeNamedArgumentOrThrow(ref argReader);
						var (keyValuePair, _, _, _) = tuple;
						if (keyValuePair.Key == "IsOptional" && tuple.Item2 && tuple.Item3 == SerializationTypeCode.Boolean)
						{
							flag = (bool)tuple.Item1.Value.ValueInternal;
							break;
						}
					}
				}
				catch (Exception ex) when (((ex is UnsupportedSignatureContent || ex is BadImageFormatException) ? 1 : 0) != 0)
				{
				}
			}
			if (!flag && (allowedFeatures & getFeatureKind(value)) == 0)
			{
				return value;
			}
		}
		return null;
		static CompilerFeatureRequiredFeatures getFeatureKind(string? feature)
		{
			return feature switch
			{
				"RefStructs" => CompilerFeatureRequiredFeatures.RefStructs, 
				"RequiredMembers" => CompilerFeatureRequiredFeatures.RequiredMembers, 
				"UserDefinedCompoundAssignmentOperators" => CompilerFeatureRequiredFeatures.UserDefinedCompoundAssignmentOperators, 
				_ => CompilerFeatureRequiredFeatures.None, 
			};
		}
	}

	internal UnmanagedCallersOnlyAttributeData? TryGetUnmanagedCallersOnlyAttribute(EntityHandle token, IAttributeNamedArgumentDecoder attributeArgumentDecoder, Func<string, TypedConstant, bool, (bool IsCallConvs, ImmutableHashSet<INamedTypeSymbolInternal>? CallConvs)> unmanagedCallersOnlyDecoder)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.UnmanagedCallersOnlyAttribute);
		if (!attributeInfo.HasValue || attributeInfo.SignatureIndex != 0 || !TryGetAttributeReader(attributeInfo.Handle, out var argReader))
		{
			return null;
		}
		ImmutableHashSet<INamedTypeSymbolInternal> callingConventionTypes = ImmutableHashSet<INamedTypeSymbolInternal>.Empty;
		if (argReader.RemainingBytes > 0)
		{
			try
			{
				ushort num = argReader.ReadUInt16();
				for (int i = 0; i < num; i++)
				{
					(KeyValuePair<string, TypedConstant> nameValuePair, bool isProperty, SerializationTypeCode typeCode, SerializationTypeCode elementTypeCode) tuple = attributeArgumentDecoder.DecodeCustomAttributeNamedArgumentOrThrow(ref argReader);
					var (arg, arg2) = tuple.nameValuePair;
					bool item = tuple.isProperty;
					SerializationTypeCode item2 = tuple.typeCode;
					SerializationTypeCode item3 = tuple.elementTypeCode;
					if (item2 == SerializationTypeCode.SZArray && item3 == SerializationTypeCode.Type)
					{
						(bool, ImmutableHashSet<INamedTypeSymbolInternal>) tuple2 = unmanagedCallersOnlyDecoder(arg, arg2, !item);
						if (tuple2.Item1)
						{
							callingConventionTypes = tuple2.Item2;
							break;
						}
					}
				}
			}
			catch (Exception ex) when (((ex is BadImageFormatException || ex is UnsupportedSignatureContent) ? 1 : 0) != 0)
			{
			}
		}
		return UnmanagedCallersOnlyAttributeData.Create(callingConventionTypes);
	}

	internal (ImmutableArray<string?> Names, bool FoundAttribute) GetInterpolatedStringHandlerArgumentAttributeValues(EntityHandle token)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.InterpolatedStringHandlerArgumentAttribute);
		if (!attributeInfo.HasValue)
		{
			return (Names: default(ImmutableArray<string>), FoundAttribute: false);
		}
		ImmutableArray<string> value2;
		if (attributeInfo.SignatureIndex == 0)
		{
			if (TryExtractStringValueFromAttribute(attributeInfo.Handle, out string value))
			{
				return (Names: ImmutableArray.Create(value), FoundAttribute: true);
			}
		}
		else if (TryExtractStringArrayValueFromAttribute(attributeInfo.Handle, out value2))
		{
			return (Names: value2.NullToEmpty(), FoundAttribute: true);
		}
		return (Names: default(ImmutableArray<string>), FoundAttribute: true);
	}

	internal bool HasMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(EntityHandle token, AttributeDescription description, out bool when)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, description);
		if (attributeInfo.HasValue && attributeInfo.SignatureIndex == 0)
		{
			return TryExtractValueFromAttribute(attributeInfo.Handle, out when, s_attributeBooleanValueExtractor);
		}
		when = false;
		return false;
	}

	internal ImmutableHashSet<string> GetStringValuesOfNotNullIfNotNullAttribute(EntityHandle token)
	{
		List<AttributeInfo> list = FindTargetAttributes(token, AttributeDescription.NotNullIfNotNullAttribute);
		ImmutableHashSet<string> immutableHashSet = ImmutableHashSet<string>.Empty;
		if (list == null)
		{
			return immutableHashSet;
		}
		foreach (AttributeInfo item in list)
		{
			if (TryExtractStringValueFromAttribute(item.Handle, out string value))
			{
				immutableHashSet = immutableHashSet.Add(value);
			}
		}
		return immutableHashSet;
	}

	internal bool HasAttributeUsageAttribute(EntityHandle token, IAttributeNamedArgumentDecoder attributeNamedArgumentDecoder, out AttributeUsageInfo usageInfo)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.AttributeUsageAttribute);
		if (attributeInfo.HasValue && TryGetAttributeReader(attributeInfo.Handle, out var blobReader) && CrackIntInAttributeValue(out var value, ref blobReader))
		{
			bool allowMultiple = false;
			bool inherited = true;
			if (blobReader.RemainingBytes >= 2)
			{
				try
				{
					ushort num = blobReader.ReadUInt16();
					for (uint num2 = 0u; num2 < num; num2++)
					{
						(KeyValuePair<string, TypedConstant>, bool, SerializationTypeCode, SerializationTypeCode) tuple = attributeNamedArgumentDecoder.DecodeCustomAttributeNamedArgumentOrThrow(ref blobReader);
						if (!tuple.Item2 || tuple.Item3 != SerializationTypeCode.Boolean)
						{
							continue;
						}
						string key = tuple.Item1.Key;
						if (!(key == "AllowMultiple"))
						{
							if (key == "Inherited")
							{
								inherited = (bool)tuple.Item1.Value.ValueInternal;
							}
						}
						else
						{
							allowMultiple = (bool)tuple.Item1.Value.ValueInternal;
						}
					}
				}
				catch (Exception ex) when (((ex is UnsupportedSignatureContent || ex is BadImageFormatException) ? 1 : 0) != 0)
				{
				}
			}
			usageInfo = new AttributeUsageInfo((AttributeTargets)value, allowMultiple, inherited);
			return true;
		}
		usageInfo = default(AttributeUsageInfo);
		return false;
	}

	internal bool HasInterfaceTypeAttribute(EntityHandle token, out ComInterfaceType interfaceType)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.InterfaceTypeAttribute);
		if (attributeInfo.HasValue && TryExtractInterfaceTypeFromAttribute(attributeInfo, out interfaceType))
		{
			return true;
		}
		interfaceType = ComInterfaceType.InterfaceIsDual;
		return false;
	}

	internal bool HasTypeLibTypeAttribute(EntityHandle token, out Microsoft.Cci.TypeLibTypeFlags flags)
	{
		AttributeInfo info = FindTargetAttribute(token, AttributeDescription.TypeLibTypeAttribute);
		if (info.HasValue && TryExtractTypeLibTypeFromAttribute(info, out flags))
		{
			return true;
		}
		flags = (Microsoft.Cci.TypeLibTypeFlags)0;
		return false;
	}

	internal bool HasDateTimeConstantAttribute(EntityHandle token, out ConstantValue defaultValue)
	{
		AttributeInfo attributeInfo = FindLastTargetAttribute(token, AttributeDescription.DateTimeConstantAttribute);
		if (attributeInfo.HasValue && TryExtractLongValueFromAttribute(attributeInfo.Handle, out var value))
		{
			long num = value;
			DateTime minValue = DateTime.MinValue;
			if (num >= minValue.Ticks)
			{
				long num2 = value;
				minValue = DateTime.MaxValue;
				if (num2 <= minValue.Ticks)
				{
					defaultValue = ConstantValue.Create(new DateTime(value));
					goto IL_005c;
				}
			}
			defaultValue = ConstantValue.Bad;
			goto IL_005c;
		}
		defaultValue = null;
		return false;
		IL_005c:
		return true;
	}

	internal bool HasDecimalConstantAttribute(EntityHandle token, out ConstantValue defaultValue)
	{
		AttributeInfo attributeInfo = FindLastTargetAttribute(token, AttributeDescription.DecimalConstantAttribute);
		if (attributeInfo.HasValue && TryExtractDecimalValueFromDecimalConstantAttribute(attributeInfo.Handle, out var value))
		{
			defaultValue = ConstantValue.Create(value);
			return true;
		}
		defaultValue = null;
		return false;
	}

	internal bool HasNullablePublicOnlyAttribute(EntityHandle token, out bool includesInternals)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.NullablePublicOnlyAttribute);
		if (attributeInfo.HasValue && TryExtractValueFromAttribute(attributeInfo.Handle, out var value, s_attributeBooleanValueExtractor))
		{
			includesInternals = value;
			return true;
		}
		includesInternals = false;
		return false;
	}

	internal ImmutableArray<string> GetInternalsVisibleToAttributeValues(EntityHandle token)
	{
		List<AttributeInfo> attrInfos = FindTargetAttributes(token, AttributeDescription.InternalsVisibleToAttribute);
		return ExtractStringValuesFromAttributes(attrInfos)?.ToImmutableAndFree() ?? ImmutableArray<string>.Empty;
	}

	internal ImmutableArray<string> GetConditionalAttributeValues(EntityHandle token)
	{
		List<AttributeInfo> attrInfos = FindTargetAttributes(token, AttributeDescription.ConditionalAttribute);
		return ExtractStringValuesFromAttributes(attrInfos)?.ToImmutableAndFree() ?? ImmutableArray<string>.Empty;
	}

	internal ImmutableArray<string> GetMemberNotNullAttributeValues(EntityHandle token)
	{
		List<AttributeInfo> list = FindTargetAttributes(token, AttributeDescription.MemberNotNullAttribute);
		if (list == null || list.Count == 0)
		{
			return ImmutableArray<string>.Empty;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(list.Count);
		foreach (AttributeInfo item in list)
		{
			if (item.SignatureIndex == 0)
			{
				if (TryExtractStringValueFromAttribute(item.Handle, out string value) && value != null)
				{
					instance.Add(value);
				}
			}
			else
			{
				if (!TryExtractStringArrayValueFromAttribute(item.Handle, out ImmutableArray<string> value2))
				{
					continue;
				}
				foreach (string item2 in value2)
				{
					if (item2 != null)
					{
						instance.Add(item2);
					}
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal (ImmutableArray<string> whenTrue, ImmutableArray<string> whenFalse) GetMemberNotNullWhenAttributeValues(EntityHandle token)
	{
		List<AttributeInfo> list = FindTargetAttributes(token, AttributeDescription.MemberNotNullWhenAttribute);
		if (list == null || list.Count == 0)
		{
			return (whenTrue: ImmutableArray<string>.Empty, whenFalse: ImmutableArray<string>.Empty);
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(list.Count);
		ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance(list.Count);
		foreach (AttributeInfo item in list)
		{
			if (item.SignatureIndex == 0)
			{
				if (TryExtractValueFromAttribute(item.Handle, out var value, s_attributeBoolAndStringValueExtractor) && value.String != null)
				{
					(value.Sense ? instance : instance2).Add(value.String);
				}
			}
			else
			{
				if (!TryExtractValueFromAttribute(item.Handle, out var value2, s_attributeBoolAndStringArrayValueExtractor))
				{
					continue;
				}
				ArrayBuilder<string> arrayBuilder = (value2.Sense ? instance : instance2);
				foreach (string @string in value2.Strings)
				{
					if (@string != null)
					{
						arrayBuilder.Add(@string);
					}
				}
			}
		}
		return (whenTrue: instance.ToImmutableAndFree(), whenFalse: instance2.ToImmutableAndFree());
	}

	private ArrayBuilder<string> ExtractStringValuesFromAttributes(List<AttributeInfo> attrInfos)
	{
		if (attrInfos == null)
		{
			return null;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(attrInfos.Count);
		foreach (AttributeInfo attrInfo in attrInfos)
		{
			if (TryExtractStringValueFromAttribute(attrInfo.Handle, out string value) && value != null)
			{
				instance.Add(value);
			}
		}
		return instance;
	}

	private ObsoleteAttributeData? TryExtractObsoleteDataFromAttribute(AttributeInfo attributeInfo, IAttributeNamedArgumentDecoder decoder)
	{
		if (!TryGetAttributeReader(attributeInfo.Handle, out var blobReader))
		{
			return null;
		}
		string value = null;
		bool value2 = false;
		switch (attributeInfo.SignatureIndex)
		{
		case 1:
			if (blobReader.RemainingBytes <= 0 || !CrackStringInAttributeValue(out value, ref blobReader))
			{
				return null;
			}
			break;
		case 2:
			if (blobReader.RemainingBytes <= 0 || !CrackStringInAttributeValue(out value, ref blobReader) || blobReader.RemainingBytes <= 0 || !CrackBooleanInAttributeValue(out value2, ref blobReader))
			{
				return null;
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(attributeInfo.SignatureIndex);
		case 0:
			break;
		}
		string diagnosticId;
		string urlFormat;
		if (blobReader.RemainingBytes > 0)
		{
			(diagnosticId, urlFormat) = CrackObsoleteProperties(ref blobReader, decoder);
		}
		else
		{
			diagnosticId = null;
			urlFormat = null;
		}
		return new ObsoleteAttributeData(ObsoleteAttributeKind.Obsolete, value, value2, diagnosticId, urlFormat);
	}

	private bool TryGetAttributeReader(CustomAttributeHandle handle, out BlobReader blobReader)
	{
		try
		{
			BlobHandle customAttributeValueOrThrow = GetCustomAttributeValueOrThrow(handle);
			if (!customAttributeValueOrThrow.IsNil)
			{
				blobReader = MetadataReader.GetBlobReader(customAttributeValueOrThrow);
				if (blobReader.Length >= 4 && blobReader.ReadInt16() == 1)
				{
					return true;
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		blobReader = default(BlobReader);
		return false;
	}

	private ObsoleteAttributeData TryExtractDeprecatedDataFromAttribute(AttributeInfo attributeInfo)
	{
		byte signatureIndex = attributeInfo.SignatureIndex;
		if ((uint)signatureIndex <= 3u)
		{
			if (!TryExtractValueFromAttribute(attributeInfo.Handle, out ObsoleteAttributeData value, s_attributeDeprecatedDataExtractor))
			{
				return null;
			}
			return value;
		}
		throw ExceptionUtilities.UnexpectedValue(attributeInfo.SignatureIndex);
	}

	private ObsoleteAttributeData TryExtractWindowsExperimentalDataFromAttribute(AttributeInfo attributeInfo)
	{
		if (attributeInfo.SignatureIndex == 0)
		{
			return ObsoleteAttributeData.WindowsExperimental;
		}
		throw ExceptionUtilities.UnexpectedValue(attributeInfo.SignatureIndex);
	}

	private bool TryExtractInterfaceTypeFromAttribute(AttributeInfo attributeInfo, out ComInterfaceType interfaceType)
	{
		switch (attributeInfo.SignatureIndex)
		{
		case 0:
		{
			if (TryExtractValueFromAttribute(attributeInfo.Handle, out var value2, s_attributeShortValueExtractor) && IsValidComInterfaceType(value2))
			{
				interfaceType = (ComInterfaceType)value2;
				return true;
			}
			break;
		}
		case 1:
		{
			if (TryExtractValueFromAttribute(attributeInfo.Handle, out var value, s_attributeIntValueExtractor) && IsValidComInterfaceType(value))
			{
				interfaceType = (ComInterfaceType)value;
				return true;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(attributeInfo.SignatureIndex);
		}
		interfaceType = ComInterfaceType.InterfaceIsDual;
		return false;
	}

	private static bool IsValidComInterfaceType(int comInterfaceType)
	{
		if ((uint)comInterfaceType <= 3u)
		{
			return true;
		}
		return false;
	}

	private bool TryExtractTypeLibTypeFromAttribute(AttributeInfo info, out Microsoft.Cci.TypeLibTypeFlags flags)
	{
		switch (info.SignatureIndex)
		{
		case 0:
		{
			if (TryExtractValueFromAttribute(info.Handle, out var value2, s_attributeShortValueExtractor))
			{
				flags = (Microsoft.Cci.TypeLibTypeFlags)value2;
				return true;
			}
			break;
		}
		case 1:
		{
			if (TryExtractValueFromAttribute(info.Handle, out var value, s_attributeIntValueExtractor))
			{
				flags = (Microsoft.Cci.TypeLibTypeFlags)value;
				return true;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(info.SignatureIndex);
		}
		flags = (Microsoft.Cci.TypeLibTypeFlags)0;
		return false;
	}

	internal bool TryExtractStringValueFromAttribute(CustomAttributeHandle handle, out string? value)
	{
		return TryExtractValueFromAttribute(handle, out value, s_attributeStringValueExtractor);
	}

	internal bool TryExtractLongValueFromAttribute(CustomAttributeHandle handle, out long value)
	{
		return TryExtractValueFromAttribute(handle, out value, s_attributeLongValueExtractor);
	}

	private bool TryExtractDecimalValueFromDecimalConstantAttribute(CustomAttributeHandle handle, out decimal value)
	{
		return TryExtractValueFromAttribute(handle, out value, s_decimalValueInDecimalConstantAttributeExtractor);
	}

	private bool TryExtractIntAndIntValueFromAttribute(CustomAttributeHandle handle, out int value1, out int value2)
	{
		bool result = TryExtractValueFromAttribute(handle, out var value3, s_attributeIntAndIntValueExtractor);
		(value1, value2) = value3;
		return result;
	}

	private bool TryExtractStringAndIntValueFromAttribute(CustomAttributeHandle handle, out string? stringValue, out int intValue)
	{
		bool result = TryExtractValueFromAttribute(handle, out var value, s_attributeStringAndIntValueExtractor);
		stringValue = value.StringValue;
		intValue = value.IntValue;
		return result;
	}

	private bool TryExtractStringAndStringValueFromAttribute(CustomAttributeHandle handle, out string? string1Value, out string? string2Value)
	{
		bool result = TryExtractValueFromAttribute<(string, string)>(handle, out var value, s_attributeStringAndStringValueExtractor);
		(string1Value, string2Value) = value;
		return result;
	}

	private bool TryExtractBoolArrayValueFromAttribute(CustomAttributeHandle handle, out ImmutableArray<bool> value)
	{
		return TryExtractValueFromAttribute(handle, out value, s_attributeBoolArrayValueExtractor);
	}

	private bool TryExtractByteArrayValueFromAttribute(CustomAttributeHandle handle, out ImmutableArray<byte> value)
	{
		return TryExtractValueFromAttribute(handle, out value, s_attributeByteArrayValueExtractor);
	}

	private bool TryExtractStringArrayValueFromAttribute(CustomAttributeHandle handle, out ImmutableArray<string?> value)
	{
		return TryExtractValueFromAttribute(handle, out value, s_attributeStringArrayValueExtractor);
	}

	private bool TryExtractValueFromAttribute<T>(CustomAttributeHandle handle, out T? value, AttributeValueExtractor<T?> valueExtractor)
	{
		try
		{
			BlobHandle customAttributeValueOrThrow = GetCustomAttributeValueOrThrow(handle);
			if (!customAttributeValueOrThrow.IsNil)
			{
				BlobReader sigReader = MetadataReader.GetBlobReader(customAttributeValueOrThrow);
				if (sigReader.Length > 4 && sigReader.ReadByte() == 1 && sigReader.ReadByte() == 0)
				{
					return valueExtractor(out value, ref sigReader);
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		value = default(T);
		return false;
	}

	internal bool HasStateMachineAttribute(MethodDefinitionHandle handle, out string stateMachineTypeName)
	{
		if (!HasStringValuedAttribute(handle, AttributeDescription.AsyncStateMachineAttribute, out stateMachineTypeName) && !HasStringValuedAttribute(handle, AttributeDescription.IteratorStateMachineAttribute, out stateMachineTypeName))
		{
			return HasStringValuedAttribute(handle, AttributeDescription.AsyncIteratorStateMachineAttribute, out stateMachineTypeName);
		}
		return true;
	}

	internal bool HasStringValuedAttribute(EntityHandle token, AttributeDescription description, out string value)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, description);
		if (attributeInfo.HasValue)
		{
			return TryExtractStringValueFromAttribute(attributeInfo.Handle, out value);
		}
		value = null;
		return false;
	}

	private bool HasIntAndIntValuedAttribute(EntityHandle token, AttributeDescription description, out int value1, out int value2)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, description);
		if (attributeInfo.HasValue)
		{
			return TryExtractIntAndIntValueFromAttribute(attributeInfo.Handle, out value1, out value2);
		}
		value1 = 0;
		value2 = 0;
		return false;
	}

	private bool HasStringAndIntValuedAttribute(EntityHandle token, AttributeDescription description, out string stringValue, out int intValue)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, description);
		if (attributeInfo.HasValue)
		{
			return TryExtractStringAndIntValueFromAttribute(attributeInfo.Handle, out stringValue, out intValue);
		}
		stringValue = null;
		intValue = 0;
		return false;
	}

	internal bool IsNoPiaLocalType(TypeDefinitionHandle typeDef, out string interfaceGuid, out string scope, out string identifier)
	{
		if (!IsNoPiaLocalType(typeDef, out var attributeInfo))
		{
			interfaceGuid = null;
			scope = null;
			identifier = null;
			return false;
		}
		interfaceGuid = null;
		scope = null;
		identifier = null;
		try
		{
			if (GetTypeDefFlagsOrThrow(typeDef).IsInterface())
			{
				HasGuidAttribute(typeDef, out interfaceGuid);
			}
			if (attributeInfo.SignatureIndex == 1)
			{
				BlobHandle customAttributeValueOrThrow = GetCustomAttributeValueOrThrow(attributeInfo.Handle);
				if (!customAttributeValueOrThrow.IsNil)
				{
					BlobReader sig = MetadataReader.GetBlobReader(customAttributeValueOrThrow);
					if (sig.Length > 4 && sig.ReadInt16() == 1 && (!CrackStringInAttributeValue(out scope, ref sig) || !CrackStringInAttributeValue(out identifier, ref sig)))
					{
						return false;
					}
				}
			}
			return true;
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}

	private static (string? diagnosticId, string? urlFormat) CrackObsoleteProperties(ref BlobReader sig, IAttributeNamedArgumentDecoder decoder)
	{
		string text = null;
		string text2 = null;
		try
		{
			ushort num = sig.ReadUInt16();
			for (int i = 0; i < num; i++)
			{
				if (text != null && text2 != null)
				{
					break;
				}
				(KeyValuePair<string, TypedConstant> nameValuePair, bool isProperty, SerializationTypeCode typeCode, SerializationTypeCode elementTypeCode) tuple = decoder.DecodeCustomAttributeNamedArgumentOrThrow(ref sig);
				var (text4, typedConstant2) = tuple.nameValuePair;
				bool item = tuple.isProperty;
				if (((tuple.typeCode == SerializationTypeCode.String) & item) && typedConstant2.ValueInternal is string text5)
				{
					if (text == null && text4 == "DiagnosticId")
					{
						text = text5;
					}
					else if (text2 == null && text4 == "UrlFormat")
					{
						text2 = text5;
					}
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		catch (UnsupportedSignatureContent)
		{
		}
		return (diagnosticId: text, urlFormat: text2);
	}

	private static bool CrackDeprecatedAttributeData([NotNullWhen(true)] out ObsoleteAttributeData? value, ref BlobReader sig)
	{
		if (CrackStringAndIntInAttributeValue(out var value2, ref sig))
		{
			value = new ObsoleteAttributeData(ObsoleteAttributeKind.Deprecated, value2.StringValue, value2.IntValue == 1, null, null);
			return true;
		}
		value = null;
		return false;
	}

	private static bool CrackIntAndIntInAttributeValue(out (int, int) value, ref BlobReader sig)
	{
		if (CrackIntInAttributeValue(out var value2, ref sig) && CrackIntInAttributeValue(out var value3, ref sig))
		{
			value = (value2, value3);
			return true;
		}
		value = default((int, int));
		return false;
	}

	private static bool CrackStringAndIntInAttributeValue(out StringAndInt value, ref BlobReader sig)
	{
		value = default(StringAndInt);
		if (CrackStringInAttributeValue(out value.StringValue, ref sig))
		{
			return CrackIntInAttributeValue(out value.IntValue, ref sig);
		}
		return false;
	}

	private static bool CrackStringAndStringInAttributeValue(out (string?, string?) value, ref BlobReader sig)
	{
		if (CrackStringInAttributeValue(out string value2, ref sig) && CrackStringInAttributeValue(out string value3, ref sig))
		{
			value = (value2, value3);
			return true;
		}
		value = default((string, string));
		return false;
	}

	internal static bool CrackStringInAttributeValue(out string? value, ref BlobReader sig)
	{
		try
		{
			if (sig.TryReadCompressedInteger(out var value2) && sig.RemainingBytes >= value2)
			{
				value = sig.ReadUTF8(value2);
				value = value.TrimEnd(new char[1]);
				return true;
			}
			value = null;
			return sig.RemainingBytes >= 1 && sig.ReadByte() == byte.MaxValue;
		}
		catch (BadImageFormatException)
		{
			value = null;
			return false;
		}
	}

	internal static bool CrackStringArrayInAttributeValue(out ImmutableArray<string?> value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 4)
		{
			uint num = sig.ReadUInt32();
			if (IsArrayNull(num))
			{
				value = default(ImmutableArray<string>);
				return false;
			}
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				if (!CrackStringInAttributeValue(out array[i], ref sig))
				{
					value = array.AsImmutableOrNull();
					return false;
				}
			}
			value = array.AsImmutableOrNull();
			return true;
		}
		value = default(ImmutableArray<string>);
		return false;
	}

	private static bool IsArrayNull(uint length)
	{
		if (length == uint.MaxValue)
		{
			return true;
		}
		return false;
	}

	private static bool CrackBoolAndStringArrayInAttributeValue(out BoolAndStringArrayData value, ref BlobReader sig)
	{
		if (CrackBooleanInAttributeValue(out var value2, ref sig) && CrackStringArrayInAttributeValue(out ImmutableArray<string> value3, ref sig))
		{
			value = new BoolAndStringArrayData(value2, value3);
			return true;
		}
		value = default(BoolAndStringArrayData);
		return false;
	}

	private static bool CrackBoolAndStringInAttributeValue(out BoolAndStringData value, ref BlobReader sig)
	{
		if (CrackBooleanInAttributeValue(out var value2, ref sig) && CrackStringInAttributeValue(out string value3, ref sig))
		{
			value = new BoolAndStringData(value2, value3);
			return true;
		}
		value = default(BoolAndStringData);
		return false;
	}

	private static bool CrackBooleanInAttributeValue(out bool value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 1)
		{
			value = sig.ReadBoolean();
			return true;
		}
		value = false;
		return false;
	}

	private static bool CrackByteInAttributeValue(out byte value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 1)
		{
			value = sig.ReadByte();
			return true;
		}
		value = byte.MaxValue;
		return false;
	}

	private static bool CrackShortInAttributeValue(out short value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 2)
		{
			value = sig.ReadInt16();
			return true;
		}
		value = -1;
		return false;
	}

	private static bool CrackIntInAttributeValue(out int value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 4)
		{
			value = sig.ReadInt32();
			return true;
		}
		value = -1;
		return false;
	}

	private static bool CrackLongInAttributeValue(out long value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 8)
		{
			value = sig.ReadInt64();
			return true;
		}
		value = -1L;
		return false;
	}

	private static bool CrackDecimalInDecimalConstantAttribute(out decimal value, ref BlobReader sig)
	{
		if (CrackByteInAttributeValue(out var value2, ref sig) && CrackByteInAttributeValue(out var value3, ref sig) && CrackIntInAttributeValue(out var value4, ref sig) && CrackIntInAttributeValue(out var value5, ref sig) && CrackIntInAttributeValue(out var value6, ref sig))
		{
			value = new decimal(value6, value5, value4, value3 != 0, value2);
			return true;
		}
		value = -1m;
		return false;
	}

	private static bool CrackBoolArrayInAttributeValue(out ImmutableArray<bool> value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 4)
		{
			uint num = sig.ReadUInt32();
			if (IsArrayNull(num))
			{
				value = default(ImmutableArray<bool>);
				return false;
			}
			if (sig.RemainingBytes >= num)
			{
				ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance((int)num);
				for (int i = 0; i < num; i++)
				{
					instance.Add(sig.ReadByte() == 1);
				}
				value = instance.ToImmutableAndFree();
				return true;
			}
		}
		value = default(ImmutableArray<bool>);
		return false;
	}

	private static bool CrackByteArrayInAttributeValue(out ImmutableArray<byte> value, ref BlobReader sig)
	{
		if (sig.RemainingBytes >= 4)
		{
			uint num = sig.ReadUInt32();
			if (IsArrayNull(num))
			{
				value = default(ImmutableArray<byte>);
				return false;
			}
			if (sig.RemainingBytes >= num)
			{
				ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance((int)num);
				for (int i = 0; i < num; i++)
				{
					instance.Add(sig.ReadByte());
				}
				value = instance.ToImmutableAndFree();
				return true;
			}
		}
		value = default(ImmutableArray<byte>);
		return false;
	}

	internal List<AttributeInfo>? FindTargetAttributes(EntityHandle hasAttribute, AttributeDescription description)
	{
		List<AttributeInfo> list = null;
		try
		{
			foreach (CustomAttributeHandle customAttribute in MetadataReader.GetCustomAttributes(hasAttribute))
			{
				int targetAttributeSignatureIndex = GetTargetAttributeSignatureIndex(customAttribute, description);
				if (targetAttributeSignatureIndex != -1)
				{
					if (list == null)
					{
						list = new List<AttributeInfo>();
					}
					list.Add(new AttributeInfo(customAttribute, targetAttributeSignatureIndex));
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return list;
	}

	internal AttributeInfo FindTargetAttribute(EntityHandle hasAttribute, AttributeDescription description)
	{
		bool foundAttributeType;
		return FindTargetAttribute(MetadataReader, hasAttribute, description, out foundAttributeType);
	}

	internal static AttributeInfo FindTargetAttribute(MetadataReader metadataReader, EntityHandle hasAttribute, AttributeDescription description, out bool foundAttributeType)
	{
		foundAttributeType = false;
		try
		{
			foreach (CustomAttributeHandle customAttribute in metadataReader.GetCustomAttributes(hasAttribute))
			{
				int targetAttributeSignatureIndex = GetTargetAttributeSignatureIndex(metadataReader, customAttribute, description, out var matchedAttributeType);
				if (matchedAttributeType)
				{
					foundAttributeType = true;
				}
				if (targetAttributeSignatureIndex != -1)
				{
					return new AttributeInfo(customAttribute, targetAttributeSignatureIndex);
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return default(AttributeInfo);
	}

	internal AttributeInfo FindLastTargetAttribute(EntityHandle hasAttribute, AttributeDescription description)
	{
		try
		{
			AttributeInfo result = default(AttributeInfo);
			foreach (CustomAttributeHandle customAttribute in MetadataReader.GetCustomAttributes(hasAttribute))
			{
				int targetAttributeSignatureIndex = GetTargetAttributeSignatureIndex(customAttribute, description);
				if (targetAttributeSignatureIndex != -1)
				{
					result = new AttributeInfo(customAttribute, targetAttributeSignatureIndex);
				}
			}
			return result;
		}
		catch (BadImageFormatException)
		{
		}
		return default(AttributeInfo);
	}

	internal int GetParamArrayCountOrThrow(EntityHandle hasAttribute)
	{
		int num = 0;
		foreach (CustomAttributeHandle customAttribute in MetadataReader.GetCustomAttributes(hasAttribute))
		{
			if (GetTargetAttributeSignatureIndex(customAttribute, AttributeDescription.ParamArrayAttribute) != -1)
			{
				num++;
			}
		}
		return num;
	}

	private bool IsNoPiaLocalType(TypeDefinitionHandle typeDef, out AttributeInfo attributeInfo)
	{
		if (_lazyContainsNoPiaLocalTypes == ThreeState.False)
		{
			attributeInfo = default(AttributeInfo);
			return false;
		}
		if (_lazyNoPiaLocalTypeCheckBitMap != null && _lazyTypeDefToTypeIdentifierMap != null)
		{
			int rowNumber = MetadataReader.GetRowNumber(typeDef);
			int num = rowNumber / 32;
			int num2 = 1 << rowNumber % 32;
			if ((_lazyNoPiaLocalTypeCheckBitMap[num] & num2) != 0)
			{
				return _lazyTypeDefToTypeIdentifierMap.TryGetValue(typeDef, out attributeInfo);
			}
		}
		try
		{
			foreach (CustomAttributeHandle customAttribute in MetadataReader.GetCustomAttributes(typeDef))
			{
				int num3 = IsTypeIdentifierAttribute(customAttribute);
				if (num3 != -1)
				{
					_lazyContainsNoPiaLocalTypes = ThreeState.True;
					RegisterNoPiaLocalType(typeDef, customAttribute, num3);
					attributeInfo = new AttributeInfo(customAttribute, num3);
					return true;
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		RecordNoPiaLocalTypeCheck(typeDef);
		attributeInfo = default(AttributeInfo);
		return false;
	}

	private void RegisterNoPiaLocalType(TypeDefinitionHandle typeDef, CustomAttributeHandle customAttribute, int signatureIndex)
	{
		if (_lazyNoPiaLocalTypeCheckBitMap == null)
		{
			Interlocked.CompareExchange(ref _lazyNoPiaLocalTypeCheckBitMap, new int[(MetadataReader.TypeDefinitions.Count + 32) / 32], null);
		}
		if (_lazyTypeDefToTypeIdentifierMap == null)
		{
			Interlocked.CompareExchange(ref _lazyTypeDefToTypeIdentifierMap, new ConcurrentDictionary<TypeDefinitionHandle, AttributeInfo>(), null);
		}
		_lazyTypeDefToTypeIdentifierMap.TryAdd(typeDef, new AttributeInfo(customAttribute, signatureIndex));
		RecordNoPiaLocalTypeCheck(typeDef);
	}

	private void RecordNoPiaLocalTypeCheck(TypeDefinitionHandle typeDef)
	{
		if (_lazyNoPiaLocalTypeCheckBitMap != null)
		{
			int rowNumber = MetadataTokens.GetRowNumber(typeDef);
			int num = rowNumber / 32;
			int num2 = 1 << rowNumber % 32;
			int num3;
			do
			{
				num3 = _lazyNoPiaLocalTypeCheckBitMap[num];
			}
			while (Interlocked.CompareExchange(ref _lazyNoPiaLocalTypeCheckBitMap[num], num3 | num2, num3) != num3);
		}
	}

	private int IsTypeIdentifierAttribute(CustomAttributeHandle customAttribute)
	{
		try
		{
			if (MetadataReader.GetCustomAttribute(customAttribute).Parent.Kind != HandleKind.TypeDefinition)
			{
				return -1;
			}
			return GetTargetAttributeSignatureIndex(customAttribute, AttributeDescription.TypeIdentifierAttribute);
		}
		catch (BadImageFormatException)
		{
			return -1;
		}
	}

	internal bool IsTargetAttribute(CustomAttributeHandle customAttribute, string namespaceName, string typeName, out EntityHandle ctor, bool ignoreCase = false)
	{
		return IsTargetAttribute(MetadataReader, customAttribute, namespaceName, typeName, out ctor, ignoreCase);
	}

	private static bool IsTargetAttribute(MetadataReader metadataReader, CustomAttributeHandle customAttribute, string namespaceName, string typeName, out EntityHandle ctor, bool ignoreCase)
	{
		if (!GetTypeAndConstructor(metadataReader, customAttribute, out var ctorType, out ctor))
		{
			return false;
		}
		if (!GetAttributeNamespaceAndName(metadataReader, ctorType, out var namespaceHandle, out var nameHandle))
		{
			return false;
		}
		try
		{
			return StringEquals(metadataReader, nameHandle, typeName, ignoreCase) && StringEquals(metadataReader, namespaceHandle, namespaceName, ignoreCase);
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}

	internal AssemblyReferenceHandle GetAssemblyRef(string assemblyName)
	{
		try
		{
			foreach (AssemblyReferenceHandle assemblyReference in MetadataReader.AssemblyReferences)
			{
				if (MetadataReader.StringComparer.Equals(MetadataReader.GetAssemblyReference(assemblyReference).Name, assemblyName))
				{
					return assemblyReference;
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return default(AssemblyReferenceHandle);
	}

	internal AssemblyReference GetAssemblyRef(AssemblyReferenceHandle assemblyRef)
	{
		return MetadataReader.GetAssemblyReference(assemblyRef);
	}

	internal EntityHandle GetTypeRef(EntityHandle resolutionScope, string namespaceName, string typeName)
	{
		try
		{
			foreach (TypeReferenceHandle typeReference2 in MetadataReader.TypeReferences)
			{
				TypeReference typeReference = MetadataReader.GetTypeReference(typeReference2);
				if (!(typeReference.ResolutionScope != resolutionScope) && MetadataReader.StringComparer.Equals(typeReference.Name, typeName) && MetadataReader.StringComparer.Equals(typeReference.Namespace, namespaceName))
				{
					return typeReference2;
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return default(TypeReferenceHandle);
	}

	public void GetTypeRefPropsOrThrow(TypeReferenceHandle handle, out string name, out string @namespace, out EntityHandle resolutionScope)
	{
		TypeReference typeReference = MetadataReader.GetTypeReference(handle);
		resolutionScope = typeReference.ResolutionScope;
		name = MetadataReader.GetString(typeReference.Name);
		@namespace = MetadataReader.GetString(typeReference.Namespace);
	}

	internal int GetTargetAttributeSignatureIndex(CustomAttributeHandle customAttribute, AttributeDescription description)
	{
		bool matchedAttributeType;
		return GetTargetAttributeSignatureIndex(MetadataReader, customAttribute, description, out matchedAttributeType);
	}

	private static int GetTargetAttributeSignatureIndex(MetadataReader metadataReader, CustomAttributeHandle customAttribute, AttributeDescription description, out bool matchedAttributeType)
	{
		if (!IsTargetAttribute(metadataReader, customAttribute, description.Namespace, description.Name, out var ctor, description.MatchIgnoringCase))
		{
			matchedAttributeType = false;
			return -1;
		}
		matchedAttributeType = true;
		try
		{
			BlobReader blobReader = metadataReader.GetBlobReader(GetMethodSignatureOrThrow(metadataReader, ctor));
			for (int i = 0; i < description.Signatures.Length; i++)
			{
				byte[] array = description.Signatures[i];
				blobReader.Reset();
				if (blobReader.RemainingBytes < 3 || blobReader.ReadByte() != array[0] || blobReader.ReadByte() != array[1] || blobReader.ReadByte() != array[2])
				{
					continue;
				}
				int j;
				for (j = 3; j < array.Length; j++)
				{
					if (blobReader.RemainingBytes == 0)
					{
						break;
					}
					SignatureTypeCode signatureTypeCode = blobReader.ReadSignatureTypeCode();
					if ((uint)array[j] != (uint)signatureTypeCode)
					{
						break;
					}
					if (signatureTypeCode == SignatureTypeCode.SZArray || signatureTypeCode != SignatureTypeCode.TypeHandle)
					{
						continue;
					}
					EntityHandle entityHandle = blobReader.ReadTypeHandle();
					HandleKind kind = entityHandle.Kind;
					StringHandle name;
					StringHandle nameHandle;
					if (kind == HandleKind.TypeDefinition)
					{
						TypeDefinitionHandle typeDefinitionHandle = (TypeDefinitionHandle)entityHandle;
						if (IsNestedTypeDefOrThrow(metadataReader, typeDefinitionHandle))
						{
							break;
						}
						TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinitionHandle);
						name = typeDefinition.Name;
						nameHandle = typeDefinition.Namespace;
					}
					else
					{
						if (kind != HandleKind.TypeReference)
						{
							break;
						}
						TypeReference typeReference = metadataReader.GetTypeReference((TypeReferenceHandle)entityHandle);
						if (typeReference.ResolutionScope.Kind == HandleKind.TypeReference)
						{
							break;
						}
						name = typeReference.Name;
						nameHandle = typeReference.Namespace;
					}
					AttributeDescription.TypeHandleTargetInfo typeHandleTargetInfo = AttributeDescription.TypeHandleTargets[array[j + 1]];
					if (!StringEquals(metadataReader, nameHandle, typeHandleTargetInfo.Namespace, ignoreCase: false) || !StringEquals(metadataReader, name, typeHandleTargetInfo.Name, ignoreCase: false))
					{
						break;
					}
					j++;
				}
				if (blobReader.RemainingBytes == 0 && j == array.Length)
				{
					return i;
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return -1;
	}

	internal bool GetTypeAndConstructor(CustomAttributeHandle customAttribute, out EntityHandle ctorType, out EntityHandle attributeCtor)
	{
		return GetTypeAndConstructor(MetadataReader, customAttribute, out ctorType, out attributeCtor);
	}

	private static bool GetTypeAndConstructor(MetadataReader metadataReader, CustomAttributeHandle customAttribute, out EntityHandle ctorType, out EntityHandle attributeCtor)
	{
		try
		{
			ctorType = default(EntityHandle);
			attributeCtor = metadataReader.GetCustomAttribute(customAttribute).Constructor;
			if (attributeCtor.Kind == HandleKind.MemberReference)
			{
				MemberReference memberReference = metadataReader.GetMemberReference((MemberReferenceHandle)attributeCtor);
				StringHandle name = memberReference.Name;
				if (!metadataReader.StringComparer.Equals(name, ".ctor"))
				{
					return false;
				}
				ctorType = memberReference.Parent;
			}
			else
			{
				if (attributeCtor.Kind != HandleKind.MethodDefinition)
				{
					return false;
				}
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition((MethodDefinitionHandle)attributeCtor);
				if (!metadataReader.StringComparer.Equals(methodDefinition.Name, ".ctor"))
				{
					return false;
				}
				ctorType = methodDefinition.GetDeclaringType();
			}
			return true;
		}
		catch (BadImageFormatException)
		{
			ctorType = default(EntityHandle);
			attributeCtor = default(EntityHandle);
			return false;
		}
	}

	internal bool GetAttributeNamespaceAndName(EntityHandle typeDefOrRef, out StringHandle namespaceHandle, out StringHandle nameHandle)
	{
		return GetAttributeNamespaceAndName(MetadataReader, typeDefOrRef, out namespaceHandle, out nameHandle);
	}

	private static bool GetAttributeNamespaceAndName(MetadataReader metadataReader, EntityHandle typeDefOrRef, out StringHandle namespaceHandle, out StringHandle nameHandle)
	{
		nameHandle = default(StringHandle);
		namespaceHandle = default(StringHandle);
		try
		{
			if (typeDefOrRef.Kind == HandleKind.TypeReference)
			{
				TypeReference typeReference = metadataReader.GetTypeReference((TypeReferenceHandle)typeDefOrRef);
				HandleKind kind = typeReference.ResolutionScope.Kind;
				if (kind == HandleKind.TypeReference || kind == HandleKind.TypeDefinition)
				{
					return false;
				}
				nameHandle = typeReference.Name;
				namespaceHandle = typeReference.Namespace;
			}
			else
			{
				if (typeDefOrRef.Kind != HandleKind.TypeDefinition)
				{
					return false;
				}
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition((TypeDefinitionHandle)typeDefOrRef);
				if (IsNested(typeDefinition.Attributes))
				{
					return false;
				}
				nameHandle = typeDefinition.Name;
				namespaceHandle = typeDefinition.Namespace;
			}
			return true;
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}

	internal void PretendThereArentNoPiaLocalTypes()
	{
		_lazyContainsNoPiaLocalTypes = ThreeState.False;
	}

	internal bool ContainsNoPiaLocalTypes()
	{
		if (_lazyContainsNoPiaLocalTypes == ThreeState.Unknown)
		{
			try
			{
				foreach (CustomAttributeHandle customAttribute in MetadataReader.CustomAttributes)
				{
					int num = IsTypeIdentifierAttribute(customAttribute);
					if (num != -1)
					{
						_lazyContainsNoPiaLocalTypes = ThreeState.True;
						TypeDefinitionHandle typeDef = (TypeDefinitionHandle)MetadataReader.GetCustomAttribute(customAttribute).Parent;
						RegisterNoPiaLocalType(typeDef, customAttribute, num);
						return true;
					}
				}
			}
			catch (BadImageFormatException)
			{
			}
			_lazyContainsNoPiaLocalTypes = ThreeState.False;
		}
		return _lazyContainsNoPiaLocalTypes == ThreeState.True;
	}

	internal bool HasNullableContextAttribute(EntityHandle token, out byte value)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.NullableContextAttribute);
		if (!attributeInfo.HasValue)
		{
			value = 0;
			return false;
		}
		return TryExtractValueFromAttribute(attributeInfo.Handle, out value, s_attributeByteValueExtractor);
	}

	internal bool HasNullableAttribute(EntityHandle token, out byte defaultTransform, out ImmutableArray<byte> nullableTransforms)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.NullableAttribute);
		defaultTransform = 0;
		nullableTransforms = default(ImmutableArray<byte>);
		if (!attributeInfo.HasValue)
		{
			return false;
		}
		if (attributeInfo.SignatureIndex == 0)
		{
			return TryExtractValueFromAttribute(attributeInfo.Handle, out defaultTransform, s_attributeByteValueExtractor);
		}
		return TryExtractByteArrayValueFromAttribute(attributeInfo.Handle, out nullableTransforms);
	}

	internal bool TryGetOverloadResolutionPriorityValue(EntityHandle token, out int decodedPriority)
	{
		AttributeInfo attributeInfo = FindTargetAttribute(token, AttributeDescription.OverloadResolutionPriorityAttribute);
		if (!attributeInfo.HasValue)
		{
			decodedPriority = 0;
			return false;
		}
		return TryExtractValueFromAttribute(attributeInfo.Handle, out decodedPriority, s_attributeIntValueExtractor);
	}

	internal BlobReader GetTypeSpecificationSignatureReaderOrThrow(TypeSpecificationHandle typeSpec)
	{
		BlobHandle signature = MetadataReader.GetTypeSpecification(typeSpec).Signature;
		return MetadataReader.GetBlobReader(signature);
	}

	internal void GetMethodSpecificationOrThrow(MethodSpecificationHandle handle, out EntityHandle method, out BlobHandle instantiation)
	{
		MethodSpecification methodSpecification = MetadataReader.GetMethodSpecification(handle);
		method = methodSpecification.Method;
		instantiation = methodSpecification.Signature;
	}

	internal void GetGenericParamPropsOrThrow(GenericParameterHandle handle, out string name, out GenericParameterAttributes flags)
	{
		GenericParameter genericParameter = MetadataReader.GetGenericParameter(handle);
		name = MetadataReader.GetString(genericParameter.Name);
		flags = genericParameter.Attributes;
	}

	internal string GetMethodDefNameOrThrow(MethodDefinitionHandle methodDef)
	{
		return MetadataReader.GetString(MetadataReader.GetMethodDefinition(methodDef).Name);
	}

	internal BlobHandle GetMethodSignatureOrThrow(MethodDefinitionHandle methodDef)
	{
		return GetMethodSignatureOrThrow(MetadataReader, methodDef);
	}

	private static BlobHandle GetMethodSignatureOrThrow(MetadataReader metadataReader, MethodDefinitionHandle methodDef)
	{
		return metadataReader.GetMethodDefinition(methodDef).Signature;
	}

	internal BlobHandle GetMethodSignatureOrThrow(EntityHandle methodDefOrRef)
	{
		return GetMethodSignatureOrThrow(MetadataReader, methodDefOrRef);
	}

	private static BlobHandle GetMethodSignatureOrThrow(MetadataReader metadataReader, EntityHandle methodDefOrRef)
	{
		return methodDefOrRef.Kind switch
		{
			HandleKind.MethodDefinition => GetMethodSignatureOrThrow(metadataReader, (MethodDefinitionHandle)methodDefOrRef), 
			HandleKind.MemberReference => GetSignatureOrThrow(metadataReader, (MemberReferenceHandle)methodDefOrRef), 
			_ => throw ExceptionUtilities.UnexpectedValue(methodDefOrRef.Kind), 
		};
	}

	public MethodAttributes GetMethodDefFlagsOrThrow(MethodDefinitionHandle methodDef)
	{
		return MetadataReader.GetMethodDefinition(methodDef).Attributes;
	}

	internal TypeDefinitionHandle FindContainingTypeOrThrow(MethodDefinitionHandle methodDef)
	{
		return MetadataReader.GetMethodDefinition(methodDef).GetDeclaringType();
	}

	internal TypeDefinitionHandle FindContainingTypeOrThrow(FieldDefinitionHandle fieldDef)
	{
		return MetadataReader.GetFieldDefinition(fieldDef).GetDeclaringType();
	}

	internal EntityHandle GetContainingTypeOrThrow(MemberReferenceHandle memberRef)
	{
		return MetadataReader.GetMemberReference(memberRef).Parent;
	}

	public void GetMethodDefPropsOrThrow(MethodDefinitionHandle methodDef, out string name, out MethodImplAttributes implFlags, out MethodAttributes flags, out int rva)
	{
		MethodDefinition methodDefinition = MetadataReader.GetMethodDefinition(methodDef);
		name = MetadataReader.GetString(methodDefinition.Name);
		implFlags = methodDefinition.ImplAttributes;
		flags = methodDefinition.Attributes;
		rva = methodDefinition.RelativeVirtualAddress;
	}

	internal void GetMethodImplPropsOrThrow(MethodImplementationHandle methodImpl, out EntityHandle body, out EntityHandle declaration)
	{
		System.Reflection.Metadata.MethodImplementation methodImplementation = MetadataReader.GetMethodImplementation(methodImpl);
		body = methodImplementation.MethodBody;
		declaration = methodImplementation.MethodDeclaration;
	}

	internal GenericParameterHandleCollection GetGenericParametersForMethodOrThrow(MethodDefinitionHandle methodDef)
	{
		return MetadataReader.GetMethodDefinition(methodDef).GetGenericParameters();
	}

	internal ParameterHandleCollection GetParametersOfMethodOrThrow(MethodDefinitionHandle methodDef)
	{
		return MetadataReader.GetMethodDefinition(methodDef).GetParameters();
	}

	internal DllImportData GetDllImportData(MethodDefinitionHandle methodDef)
	{
		try
		{
			MethodImport import = MetadataReader.GetMethodDefinition(methodDef).GetImport();
			if (import.Module.IsNil)
			{
				return null;
			}
			string moduleRefNameOrThrow = GetModuleRefNameOrThrow(import.Module);
			string entryPointName = MetadataReader.GetString(import.Name);
			MethodImportAttributes attributes = import.Attributes;
			return new DllImportData(moduleRefNameOrThrow, entryPointName, attributes);
		}
		catch (BadImageFormatException)
		{
			return null;
		}
	}

	public string GetMemberRefNameOrThrow(MemberReferenceHandle memberRef)
	{
		return GetMemberRefNameOrThrow(MetadataReader, memberRef);
	}

	private static string GetMemberRefNameOrThrow(MetadataReader metadataReader, MemberReferenceHandle memberRef)
	{
		return metadataReader.GetString(metadataReader.GetMemberReference(memberRef).Name);
	}

	internal BlobHandle GetSignatureOrThrow(MemberReferenceHandle memberRef)
	{
		return GetSignatureOrThrow(MetadataReader, memberRef);
	}

	private static BlobHandle GetSignatureOrThrow(MetadataReader metadataReader, MemberReferenceHandle memberRef)
	{
		return metadataReader.GetMemberReference(memberRef).Signature;
	}

	public void GetMemberRefPropsOrThrow(MemberReferenceHandle memberRef, out EntityHandle @class, out string name, out byte[] signature)
	{
		MemberReference memberReference = MetadataReader.GetMemberReference(memberRef);
		@class = memberReference.Parent;
		name = MetadataReader.GetString(memberReference.Name);
		signature = MetadataReader.GetBlobBytes(memberReference.Signature);
	}

	internal void GetParamPropsOrThrow(ParameterHandle parameterDef, out string name, out ParameterAttributes flags)
	{
		Parameter parameter = MetadataReader.GetParameter(parameterDef);
		name = MetadataReader.GetString(parameter.Name);
		flags = parameter.Attributes;
	}

	internal string GetParamNameOrThrow(ParameterHandle parameterDef)
	{
		Parameter parameter = MetadataReader.GetParameter(parameterDef);
		return MetadataReader.GetString(parameter.Name);
	}

	internal int GetParameterSequenceNumberOrThrow(ParameterHandle param)
	{
		return MetadataReader.GetParameter(param).SequenceNumber;
	}

	internal string GetPropertyDefNameOrThrow(PropertyDefinitionHandle propertyDef)
	{
		return MetadataReader.GetString(MetadataReader.GetPropertyDefinition(propertyDef).Name);
	}

	internal BlobHandle GetPropertySignatureOrThrow(PropertyDefinitionHandle propertyDef)
	{
		return MetadataReader.GetPropertyDefinition(propertyDef).Signature;
	}

	internal void GetPropertyDefPropsOrThrow(PropertyDefinitionHandle propertyDef, out string name, out PropertyAttributes flags)
	{
		PropertyDefinition propertyDefinition = MetadataReader.GetPropertyDefinition(propertyDef);
		name = MetadataReader.GetString(propertyDefinition.Name);
		flags = propertyDefinition.Attributes;
	}

	internal string GetEventDefNameOrThrow(EventDefinitionHandle eventDef)
	{
		return MetadataReader.GetString(MetadataReader.GetEventDefinition(eventDef).Name);
	}

	internal void GetEventDefPropsOrThrow(EventDefinitionHandle eventDef, out string name, out EventAttributes flags, out EntityHandle type)
	{
		EventDefinition eventDefinition = MetadataReader.GetEventDefinition(eventDef);
		name = MetadataReader.GetString(eventDefinition.Name);
		flags = eventDefinition.Attributes;
		type = eventDefinition.Type;
	}

	public string GetFieldDefNameOrThrow(FieldDefinitionHandle fieldDef)
	{
		return MetadataReader.GetString(MetadataReader.GetFieldDefinition(fieldDef).Name);
	}

	internal BlobHandle GetFieldSignatureOrThrow(FieldDefinitionHandle fieldDef)
	{
		return MetadataReader.GetFieldDefinition(fieldDef).Signature;
	}

	public FieldAttributes GetFieldDefFlagsOrThrow(FieldDefinitionHandle fieldDef)
	{
		return MetadataReader.GetFieldDefinition(fieldDef).Attributes;
	}

	public void GetFieldDefPropsOrThrow(FieldDefinitionHandle fieldDef, out string name, out FieldAttributes flags)
	{
		FieldDefinition fieldDefinition = MetadataReader.GetFieldDefinition(fieldDef);
		name = MetadataReader.GetString(fieldDefinition.Name);
		flags = fieldDefinition.Attributes;
	}

	internal ConstantValue GetParamDefaultValue(ParameterHandle param)
	{
		try
		{
			ConstantHandle defaultValue = MetadataReader.GetParameter(param).GetDefaultValue();
			return defaultValue.IsNil ? ConstantValue.Bad : GetConstantValueOrThrow(defaultValue);
		}
		catch (BadImageFormatException)
		{
			return ConstantValue.Bad;
		}
	}

	internal ConstantValue GetConstantFieldValue(FieldDefinitionHandle fieldDef)
	{
		try
		{
			ConstantHandle defaultValue = MetadataReader.GetFieldDefinition(fieldDef).GetDefaultValue();
			return defaultValue.IsNil ? ConstantValue.Bad : GetConstantValueOrThrow(defaultValue);
		}
		catch (BadImageFormatException)
		{
			return ConstantValue.Bad;
		}
	}

	public CustomAttributeHandleCollection GetCustomAttributesOrThrow(EntityHandle handle)
	{
		return MetadataReader.GetCustomAttributes(handle);
	}

	public BlobHandle GetCustomAttributeValueOrThrow(CustomAttributeHandle handle)
	{
		return MetadataReader.GetCustomAttribute(handle).Value;
	}

	private BlobHandle GetMarshallingDescriptorHandleOrThrow(EntityHandle fieldOrParameterToken)
	{
		if (fieldOrParameterToken.Kind != HandleKind.FieldDefinition)
		{
			return MetadataReader.GetParameter((ParameterHandle)fieldOrParameterToken).GetMarshallingDescriptor();
		}
		return MetadataReader.GetFieldDefinition((FieldDefinitionHandle)fieldOrParameterToken).GetMarshallingDescriptor();
	}

	internal UnmanagedType GetMarshallingType(EntityHandle fieldOrParameterToken)
	{
		try
		{
			BlobHandle marshallingDescriptorHandleOrThrow = GetMarshallingDescriptorHandleOrThrow(fieldOrParameterToken);
			if (marshallingDescriptorHandleOrThrow.IsNil)
			{
				return (UnmanagedType)0;
			}
			byte b = MetadataReader.GetBlobReader(marshallingDescriptorHandleOrThrow).ReadByte();
			return (UnmanagedType)((b <= 80) ? b : 0);
		}
		catch (BadImageFormatException)
		{
			return (UnmanagedType)0;
		}
	}

	internal ImmutableArray<byte> GetMarshallingDescriptor(EntityHandle fieldOrParameterToken)
	{
		try
		{
			BlobHandle marshallingDescriptorHandleOrThrow = GetMarshallingDescriptorHandleOrThrow(fieldOrParameterToken);
			if (marshallingDescriptorHandleOrThrow.IsNil)
			{
				return ImmutableArray<byte>.Empty;
			}
			return MetadataReader.GetBlobBytes(marshallingDescriptorHandleOrThrow).AsImmutableOrNull();
		}
		catch (BadImageFormatException)
		{
			return ImmutableArray<byte>.Empty;
		}
	}

	internal int? GetFieldOffset(FieldDefinitionHandle fieldDef)
	{
		try
		{
			int offset = MetadataReader.GetFieldDefinition(fieldDef).GetOffset();
			if (offset == -1)
			{
				return null;
			}
			return offset;
		}
		catch (BadImageFormatException)
		{
			return null;
		}
	}

	private ConstantValue GetConstantValueOrThrow(ConstantHandle handle)
	{
		Constant constant = MetadataReader.GetConstant(handle);
		BlobReader blobReader = MetadataReader.GetBlobReader(constant.Value);
		switch (constant.TypeCode)
		{
		case ConstantTypeCode.Boolean:
			return ConstantValue.Create(blobReader.ReadBoolean());
		case ConstantTypeCode.Char:
			return ConstantValue.Create(blobReader.ReadChar());
		case ConstantTypeCode.SByte:
			return ConstantValue.Create(blobReader.ReadSByte());
		case ConstantTypeCode.Int16:
			return ConstantValue.Create(blobReader.ReadInt16());
		case ConstantTypeCode.Int32:
			return ConstantValue.Create(blobReader.ReadInt32());
		case ConstantTypeCode.Int64:
			return ConstantValue.Create(blobReader.ReadInt64());
		case ConstantTypeCode.Byte:
			return ConstantValue.Create(blobReader.ReadByte());
		case ConstantTypeCode.UInt16:
			return ConstantValue.Create(blobReader.ReadUInt16());
		case ConstantTypeCode.UInt32:
			return ConstantValue.Create(blobReader.ReadUInt32());
		case ConstantTypeCode.UInt64:
			return ConstantValue.Create(blobReader.ReadUInt64());
		case ConstantTypeCode.Single:
			return ConstantValue.Create(blobReader.ReadSingle());
		case ConstantTypeCode.Double:
			return ConstantValue.Create(blobReader.ReadDouble());
		case ConstantTypeCode.String:
			return ConstantValue.Create(blobReader.ReadUTF16(blobReader.Length));
		case ConstantTypeCode.NullReference:
			if (blobReader.ReadUInt32() == 0)
			{
				return ConstantValue.Null;
			}
			break;
		}
		return ConstantValue.Bad;
	}

	internal (int FirstIndex, int SecondIndex) GetAssemblyRefsForForwardedType(string fullName, bool ignoreCase, out string matchedName)
	{
		EnsureForwardTypeToAssemblyMap();
		(int, int) value2;
		if (ignoreCase)
		{
			ensureCaseInsensitiveDictionary();
			if (_lazyCaseInsensitiveForwardedTypesToAssemblyIndexMap.TryGetValue(fullName, out (string, int, int) value))
			{
				(matchedName, _, _) = value;
				return (FirstIndex: value.Item2, SecondIndex: value.Item3);
			}
		}
		else if (_lazyForwardedTypesToAssemblyIndexMap.TryGetValue(fullName, out value2))
		{
			matchedName = fullName;
			return value2;
		}
		matchedName = null;
		return (FirstIndex: -1, SecondIndex: -1);
		void ensureCaseInsensitiveDictionary()
		{
			if (_lazyCaseInsensitiveForwardedTypesToAssemblyIndexMap == null)
			{
				if (_lazyForwardedTypesToAssemblyIndexMap.Count == 0)
				{
					_lazyCaseInsensitiveForwardedTypesToAssemblyIndexMap = s_sharedEmptyCaseInsensitiveForwardedTypes;
				}
				else
				{
					Dictionary<string, (string, int, int)> dictionary = new Dictionary<string, (string, int, int)>(StringComparer.OrdinalIgnoreCase);
					foreach (KeyValuePair<string, (int, int)> item3 in _lazyForwardedTypesToAssemblyIndexMap)
					{
						RoslynKeyValuePairExtensions.Deconstruct(item3, out var key, out var value3);
						(int, int) tuple2 = value3;
						string text = key;
						var (item, item2) = tuple2;
						DictionaryExtensions.TryAdd(dictionary, text, (text, item, item2));
					}
					_lazyCaseInsensitiveForwardedTypesToAssemblyIndexMap = dictionary;
				}
			}
		}
	}

	internal IEnumerable<KeyValuePair<string, (int FirstIndex, int SecondIndex)>> GetForwardedTypes()
	{
		EnsureForwardTypeToAssemblyMap();
		return _lazyForwardedTypesToAssemblyIndexMap;
	}

	[MemberNotNull("_lazyForwardedTypesToAssemblyIndexMap")]
	private void EnsureForwardTypeToAssemblyMap()
	{
		if (_lazyForwardedTypesToAssemblyIndexMap != null)
		{
			return;
		}
		Dictionary<string, (int, int)> dictionary = null;
		try
		{
			foreach (ExportedTypeHandle exportedType2 in MetadataReader.ExportedTypes)
			{
				System.Reflection.Metadata.ExportedType exportedType = MetadataReader.GetExportedType(exportedType2);
				if (!exportedType.IsForwarder)
				{
					continue;
				}
				AssemblyReferenceHandle assemblyRef = (AssemblyReferenceHandle)exportedType.Implementation;
				if (assemblyRef.IsNil)
				{
					continue;
				}
				int assemblyReferenceIndexOrThrow;
				try
				{
					assemblyReferenceIndexOrThrow = GetAssemblyReferenceIndexOrThrow(assemblyRef);
				}
				catch (BadImageFormatException)
				{
					continue;
				}
				if (assemblyReferenceIndexOrThrow < 0 || assemblyReferenceIndexOrThrow >= ReferencedAssemblies.Length)
				{
					continue;
				}
				string text = MetadataReader.GetString(exportedType.Name);
				StringHandle handle = exportedType.Namespace;
				if (!handle.IsNil)
				{
					string text2 = MetadataReader.GetString(handle);
					if (text2.Length > 0)
					{
						text = text2 + "." + text;
					}
				}
				if (dictionary == null)
				{
					dictionary = new Dictionary<string, (int, int)>();
				}
				if (dictionary.TryGetValue(text, out var value))
				{
					if (value.Item1 != assemblyReferenceIndexOrThrow && value.Item2 < 0)
					{
						value.Item2 = assemblyReferenceIndexOrThrow;
						dictionary[text] = value;
					}
				}
				else
				{
					dictionary.Add(text, (assemblyReferenceIndexOrThrow, -1));
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		if (dictionary == null)
		{
			_lazyForwardedTypesToAssemblyIndexMap = s_sharedEmptyForwardedTypes;
		}
		else
		{
			_lazyForwardedTypesToAssemblyIndexMap = dictionary;
		}
	}

	internal PropertyAccessors GetPropertyMethodsOrThrow(PropertyDefinitionHandle propertyDef)
	{
		return MetadataReader.GetPropertyDefinition(propertyDef).GetAccessors();
	}

	internal EventAccessors GetEventMethodsOrThrow(EventDefinitionHandle eventDef)
	{
		return MetadataReader.GetEventDefinition(eventDef).GetAccessors();
	}

	internal int GetAssemblyReferenceIndexOrThrow(AssemblyReferenceHandle assemblyRef)
	{
		return MetadataReader.GetRowNumber(assemblyRef) - 1;
	}

	internal static bool IsNested(TypeAttributes flags)
	{
		return (flags & TypeAttributes.NestedFamANDAssem) != 0;
	}

	internal MethodBodyBlock GetMethodBodyOrThrow(MethodDefinitionHandle methodHandle)
	{
		MethodDefinition methodDefinition = MetadataReader.GetMethodDefinition(methodHandle);
		if ((methodDefinition.ImplAttributes & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL || methodDefinition.RelativeVirtualAddress == 0)
		{
			return null;
		}
		return _peReaderOpt.GetMethodBody(methodDefinition.RelativeVirtualAddress);
	}

	private static bool StringEquals(MetadataReader metadataReader, StringHandle nameHandle, string name, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return string.Equals(metadataReader.GetString(nameHandle), name, StringComparison.OrdinalIgnoreCase);
		}
		return metadataReader.StringComparer.Equals(nameHandle, name);
	}

	public ModuleMetadata GetNonDisposableMetadata()
	{
		return _owner.Copy();
	}
}
