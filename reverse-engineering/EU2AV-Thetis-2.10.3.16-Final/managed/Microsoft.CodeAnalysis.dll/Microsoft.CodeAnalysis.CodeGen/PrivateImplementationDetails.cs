using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class PrivateImplementationDetails : DefaultTypeDef, INamespaceTypeDefinition, INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, INamespaceTypeReference
{
	private sealed class FieldComparer : IComparer<SynthesizedStaticField>
	{
		public static readonly FieldComparer Instance = new FieldComparer();

		private FieldComparer()
		{
		}

		public int Compare(SynthesizedStaticField? x, SynthesizedStaticField? y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	private sealed class DataAndUShortEqualityComparer : EqualityComparer<(ImmutableArray<byte> Data, ushort Value)>
	{
		public static readonly DataAndUShortEqualityComparer Instance = new DataAndUShortEqualityComparer();

		private DataAndUShortEqualityComparer()
		{
		}

		public override bool Equals((ImmutableArray<byte> Data, ushort Value) x, (ImmutableArray<byte> Data, ushort Value) y)
		{
			if (x.Value == y.Value)
			{
				return ByteSequenceComparer.Equals(x.Data, y.Data);
			}
			return false;
		}

		public override int GetHashCode((ImmutableArray<byte> Data, ushort Value) obj)
		{
			return ByteSequenceComparer.GetHashCode(obj.Data);
		}
	}

	private sealed class ConstantValueAndUShortEqualityComparer : EqualityComparer<(ImmutableArray<ConstantValue> Constants, ushort Value)>
	{
		public static readonly ConstantValueAndUShortEqualityComparer Instance = new ConstantValueAndUShortEqualityComparer();

		private ConstantValueAndUShortEqualityComparer()
		{
		}

		public override bool Equals((ImmutableArray<ConstantValue> Constants, ushort Value) x, (ImmutableArray<ConstantValue> Constants, ushort Value) y)
		{
			if (x.Value != y.Value)
			{
				return false;
			}
			if (x.Constants.Length != y.Constants.Length)
			{
				return false;
			}
			for (int i = 0; i < x.Constants.Length; i++)
			{
				if (x.Constants[i] != y.Constants[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode((ImmutableArray<ConstantValue> Constants, ushort Value) obj)
		{
			int num = 0;
			foreach (ConstantValue item in obj.Constants)
			{
				Hash.Combine(item.GetHashCode(), num);
			}
			return num;
		}
	}

	private const string TypeNamePrefix = "<PrivateImplementationDetails>";

	internal const string SynthesizedStringHashFunctionName = "ComputeStringHash";

	internal const string SynthesizedReadOnlySpanHashFunctionName = "ComputeReadOnlySpanHash";

	internal const string SynthesizedSpanHashFunctionName = "ComputeSpanHash";

	internal const string SynthesizedThrowSwitchExpressionExceptionFunctionName = "ThrowSwitchExpressionException";

	internal const string SynthesizedThrowSwitchExpressionExceptionParameterlessFunctionName = "ThrowSwitchExpressionExceptionParameterless";

	internal const string SynthesizedThrowInvalidOperationExceptionFunctionName = "ThrowInvalidOperationException";

	internal const string SynthesizedInlineArrayAsSpanName = "InlineArrayAsSpan";

	internal const string SynthesizedInlineArrayAsReadOnlySpanName = "InlineArrayAsReadOnlySpan";

	internal const string SynthesizedInlineArrayElementRefName = "InlineArrayElementRef";

	internal const string SynthesizedInlineArrayElementRefReadOnlyName = "InlineArrayElementRefReadOnly";

	internal const string SynthesizedInlineArrayFirstElementRefName = "InlineArrayFirstElementRef";

	internal const string SynthesizedInlineArrayFirstElementRefReadOnlyName = "InlineArrayFirstElementRefReadOnly";

	internal const string SynthesizedBytesToStringFunctionName = "BytesToString";

	internal readonly CommonPEModuleBuilder ModuleBuilder;

	internal readonly ITypeReference SystemObject;

	private readonly ITypeReference _systemValueType;

	private readonly ITypeReference _systemInt8Type;

	private readonly ITypeReference _systemInt16Type;

	private readonly ITypeReference _systemInt32Type;

	private readonly ITypeReference _systemInt64Type;

	private readonly ICustomAttribute? _compilerGeneratedAttribute;

	private readonly string _name;

	private int _frozen;

	private ImmutableArray<SynthesizedStaticField> _orderedSynthesizedFields;

	private readonly ConcurrentDictionary<(ImmutableArray<byte> Data, ushort Alignment), MappedField> _mappedFields = new ConcurrentDictionary<(ImmutableArray<byte>, ushort), MappedField>(DataAndUShortEqualityComparer.Instance);

	private readonly ConcurrentDictionary<(ImmutableArray<byte> Data, ushort ElementType), CachedArrayField> _cachedArrayFields = new ConcurrentDictionary<(ImmutableArray<byte>, ushort), CachedArrayField>(DataAndUShortEqualityComparer.Instance);

	private readonly ConcurrentDictionary<(ImmutableArray<ConstantValue> Constants, ushort ElementType), CachedArrayField> _cachedArrayFieldsForConstants = new ConcurrentDictionary<(ImmutableArray<ConstantValue>, ushort), CachedArrayField>(ConstantValueAndUShortEqualityComparer.Instance);

	private ModuleVersionIdField? _mvidField;

	private ModuleCancellationTokenField? _moduleCancellationTokenField;

	private readonly ConcurrentDictionary<int, InstrumentationPayloadRootField> _instrumentationPayloadRootFields = new ConcurrentDictionary<int, InstrumentationPayloadRootField>();

	private ImmutableArray<IMethodDefinition> _orderedSynthesizedMethods;

	private readonly ConcurrentDictionary<string, IMethodDefinition> _synthesizedMethods = new ConcurrentDictionary<string, IMethodDefinition>();

	private readonly ConcurrentDictionary<(uint Size, ushort Alignment), ITypeReference> _dataFieldTypes = new ConcurrentDictionary<(uint, ushort), ITypeReference>();

	private readonly ConcurrentDictionary<string, DataSectionStringType> _dataSectionStringLiteralTypes = new ConcurrentDictionary<string, DataSectionStringType>();

	private readonly ConcurrentDictionary<string, string> _dataSectionStringLiteralNames = new ConcurrentDictionary<string, string>();

	private ImmutableArray<INestedTypeDefinition> _orderedNestedTypes;

	internal bool IsFrozen => _frozen != 0;

	public override INamespaceTypeReference AsNamespaceTypeReference => this;

	public string Name => _name;

	public bool IsPublic => false;

	public string NamespaceName => string.Empty;

	internal PrivateImplementationDetails(CommonPEModuleBuilder moduleBuilder, string moduleName, int submissionSlotIndex, ITypeReference systemObject, ITypeReference systemValueType, ITypeReference systemInt8Type, ITypeReference systemInt16Type, ITypeReference systemInt32Type, ITypeReference systemInt64Type, ICustomAttribute? compilerGeneratedAttribute)
	{
		CommonPEModuleBuilder moduleBuilder2 = moduleBuilder;
		string moduleName2 = moduleName;
		int submissionSlotIndex2 = submissionSlotIndex;
		base._002Ector();
		ModuleBuilder = moduleBuilder2;
		SystemObject = systemObject;
		_systemValueType = systemValueType;
		_systemInt8Type = systemInt8Type;
		_systemInt16Type = systemInt16Type;
		_systemInt32Type = systemInt32Type;
		_systemInt64Type = systemInt64Type;
		_compilerGeneratedAttribute = compilerGeneratedAttribute;
		_name = getClassName();
		string getClassName()
		{
			string text = ((moduleBuilder2.OutputKind == OutputKind.NetModule) ? ("<PrivateImplementationDetails><" + MetadataHelpers.MangleForTypeNameIfNeeded(moduleName2) + ">") : "<PrivateImplementationDetails>");
			if (submissionSlotIndex2 >= 0)
			{
				text += submissionSlotIndex2.ToString(CultureInfo.InvariantCulture);
			}
			if (moduleBuilder2.CurrentGenerationOrdinal > 0)
			{
				text = text + "#" + moduleBuilder2.CurrentGenerationOrdinal;
			}
			return text;
		}
	}

	internal void Freeze()
	{
		if (Interlocked.Exchange(ref _frozen, 1) != 0)
		{
			throw new InvalidOperationException();
		}
		ArrayBuilder<SynthesizedStaticField> instance = ArrayBuilder<SynthesizedStaticField>.GetInstance(_mappedFields.Count + _cachedArrayFields.Count + _cachedArrayFieldsForConstants.Count + ((_mvidField != null) ? 1 : 0));
		instance.AddRange(_mappedFields.Values);
		instance.AddRange(_cachedArrayFields.Values);
		instance.AddRange(_cachedArrayFieldsForConstants.Values);
		if (_mvidField != null)
		{
			instance.Add(_mvidField);
		}
		if (_moduleCancellationTokenField != null)
		{
			instance.Add(_moduleCancellationTokenField);
		}
		instance.AddRange(_instrumentationPayloadRootFields.Values);
		instance.Sort(FieldComparer.Instance);
		_orderedSynthesizedFields = instance.ToImmutableAndFree();
		_orderedSynthesizedMethods = (from kvp in _synthesizedMethods
			orderby kvp.Key
			select kvp.Value).AsImmutable();
		_orderedNestedTypes = ((IEnumerable<INestedTypeDefinition>)(from kvp in _dataFieldTypes
			orderby kvp.Key.Size, kvp.Key.Alignment
			select kvp.Value).OfType<ExplicitSizeStruct>()).Concat((IEnumerable<INestedTypeDefinition>)(from kvp in _dataSectionStringLiteralTypes
			orderby kvp.Key
			select kvp.Value)).AsImmutable();
	}

	internal IFieldReference CreateArrayCachingField(ImmutableArray<byte> data, IArrayTypeReference arrayType, EmitContext emitContext)
	{
		PrimitiveTypeCode typeCode = arrayType.GetElementType(emitContext).TypeCode;
		return _cachedArrayFields.GetOrAdd((data, (ushort)typeCode), ((ImmutableArray<byte> Data, ushort ElementType) key) => new CachedArrayField($"{DataToHex(key.Data)}_A{key.ElementType}", this, arrayType));
	}

	internal IFieldReference CreateArrayCachingField(ImmutableArray<ConstantValue> constants, IArrayTypeReference arrayType, EmitContext emitContext)
	{
		PrimitiveTypeCode typeCode = arrayType.GetElementType(emitContext).TypeCode;
		return _cachedArrayFieldsForConstants.GetOrAdd((constants, (ushort)typeCode), ((ImmutableArray<ConstantValue> Constants, ushort ElementType) key) => new CachedArrayField($"{ConstantsToHex(key.Constants)}_B{key.ElementType}", this, arrayType));
	}

	private ITypeReference GetOrAddDataFieldType(int length, ushort alignment)
	{
		return _dataFieldTypes.GetOrAdd(((uint)length, alignment), delegate((uint Size, ushort Alignment) key)
		{
			if (key.Alignment == 1)
			{
				switch (key.Size)
				{
				case 1u:
					if (_systemInt8Type != null)
					{
						return _systemInt8Type;
					}
					break;
				case 2u:
					if (_systemInt16Type != null)
					{
						return _systemInt16Type;
					}
					break;
				case 4u:
					if (_systemInt32Type != null)
					{
						return _systemInt32Type;
					}
					break;
				case 8u:
					if (_systemInt64Type != null)
					{
						return _systemInt64Type;
					}
					break;
				}
			}
			return new ExplicitSizeStruct(key.Size, key.Alignment, this, _systemValueType);
		});
	}

	internal MappedField GetOrAddDataField(ImmutableArray<byte> data, ushort alignment)
	{
		return _mappedFields.GetOrAdd<(ImmutableArray<byte>, ushort), PrivateImplementationDetails, MappedField>((data, alignment), delegate((ImmutableArray<byte>, ushort) key, PrivateImplementationDetails @this)
		{
			(ImmutableArray<byte>, ushort) tuple = key;
			ImmutableArray<byte> item = tuple.Item1;
			ushort item2 = tuple.Item2;
			ITypeReference orAddDataFieldType = @this.GetOrAddDataFieldType(item.Length, item2);
			string text = DataToHex(item);
			return new MappedField(item2 switch
			{
				2 => text + "2", 
				4 => text + "4", 
				8 => text + "8", 
				_ => text, 
			}, @this, orAddDataFieldType, item);
		}, this);
	}

	internal static IFieldReference? TryGetOrCreateFieldForStringValue(string text, CommonPEModuleBuilder moduleBuilder, SyntaxNode? syntaxNode, DiagnosticBag diagnostics)
	{
		if (!text.TryGetUtf8ByteRepresentation(out byte[] result, out string _))
		{
			return null;
		}
		PrivateImplementationDetails privateImplClass = moduleBuilder.GetPrivateImplClass(syntaxNode, diagnostics);
		return ConcurrentDictionaryExtensions.GetOrAdd(privateImplClass._dataSectionStringLiteralTypes, text, delegate(string text3, (PrivateImplementationDetails @this, ImmutableArray<byte>, SyntaxNode syntaxNode, DiagnosticBag diagnostics) arg)
		{
			(PrivateImplementationDetails @this, ImmutableArray<byte>, SyntaxNode syntaxNode, DiagnosticBag diagnostics) tuple = arg;
			PrivateImplementationDetails item = tuple.@this;
			ImmutableArray<byte> item2 = tuple.Item2;
			SyntaxNode item3 = tuple.syntaxNode;
			DiagnosticBag item4 = tuple.diagnostics;
			string text2 = "<S>" + item.DataToHexViaXxHash128(item2);
			MappedField orAddDataField = item.GetOrAddDataField(item2, 1);
			IMethodDefinition orSynthesizeBytesToStringHelper = item.GetOrSynthesizeBytesToStringHelper(item4);
			string orAdd = item._dataSectionStringLiteralNames.GetOrAdd(text2, text3);
			if (orAdd != text3)
			{
				CommonMessageProvider messageProvider = item.ModuleBuilder.CommonCompilation.MessageProvider;
				item4.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_DataSectionStringLiteralHashCollision, item3?.GetLocation() ?? Location.None, orAdd.Substring(0, Math.Min(orAdd.Length, 500))));
			}
			return new DataSectionStringType(text2, item, orAddDataField, orSynthesizeBytesToStringHelper, item4);
		}, (privateImplClass, ImmutableCollectionsMarshal.AsImmutableArray(result), syntaxNode, diagnostics)).Field;
	}

	private IMethodDefinition GetOrSynthesizeBytesToStringHelper(DiagnosticBag diagnostics)
	{
		IMethodDefinition method = GetMethod("BytesToString");
		if (method == null)
		{
			Compilation commonCompilation = ModuleBuilder.CommonCompilation;
			IMethodReference encodingUtf = getWellKnownTypeMember(commonCompilation, WellKnownMember.System_Text_Encoding__get_UTF8);
			IMethodReference encodingGetString = getWellKnownTypeMember(commonCompilation, WellKnownMember.System_Text_Encoding__GetString);
			TryAddSynthesizedMethod(_003CPrivateImplementationDetails_003EF6A48037C6D2DEBD2D26BA3D816224076B82F75E13DFA773F51ED478F76FBB02B__BytesToStringHelper.Create(ModuleBuilder, this, encodingUtf, encodingGetString, diagnostics));
			method = GetMethod("BytesToString");
		}
		return method;
		static IMethodReference getWellKnownTypeMember(Compilation compilation, WellKnownMember member)
		{
			return (IMethodReference)compilation.CommonGetWellKnownTypeMember(member).GetCciAdapter();
		}
	}

	internal IFieldReference GetModuleVersionId(ITypeReference mvidType)
	{
		if (_mvidField == null)
		{
			Interlocked.CompareExchange(ref _mvidField, new ModuleVersionIdField(this, mvidType), null);
		}
		return _mvidField;
	}

	internal IFieldReference GetModuleCancellationToken(ITypeReference cancellationTokenType)
	{
		if (_moduleCancellationTokenField == null)
		{
			Interlocked.CompareExchange(ref _moduleCancellationTokenField, new ModuleCancellationTokenField(this, cancellationTokenType), null);
		}
		return _moduleCancellationTokenField;
	}

	internal IFieldReference GetOrAddInstrumentationPayloadRoot(int analysisKind, ITypeReference payloadRootType)
	{
		if (!_instrumentationPayloadRootFields.TryGetValue(analysisKind, out InstrumentationPayloadRootField value))
		{
			return _instrumentationPayloadRootFields.GetOrAdd(analysisKind, (int kind) => new InstrumentationPayloadRootField(this, kind, payloadRootType));
		}
		return value;
	}

	internal IOrderedEnumerable<KeyValuePair<int, InstrumentationPayloadRootField>> GetInstrumentationPayloadRoots()
	{
		return _instrumentationPayloadRootFields.OrderBy<KeyValuePair<int, InstrumentationPayloadRootField>, int>((KeyValuePair<int, InstrumentationPayloadRootField> analysis) => analysis.Key);
	}

	internal bool TryAddSynthesizedMethod(IMethodDefinition method)
	{
		return _synthesizedMethods.TryAdd(method.Name, method);
	}

	public override IEnumerable<IFieldDefinition> GetFields(EmitContext context)
	{
		return _orderedSynthesizedFields;
	}

	public override IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
	{
		return _orderedSynthesizedMethods;
	}

	internal IMethodDefinition? GetMethod(string name)
	{
		_synthesizedMethods.TryGetValue(name, out IMethodDefinition value);
		return value;
	}

	public override IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
	{
		return _orderedNestedTypes;
	}

	public override string ToString()
	{
		return Name;
	}

	public override ITypeReference GetBaseClass(EmitContext context)
	{
		return SystemObject;
	}

	public override IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		if (_compilerGeneratedAttribute != null)
		{
			return SpecializedCollections.SingletonEnumerable(_compilerGeneratedAttribute);
		}
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override INamespaceTypeDefinition AsNamespaceTypeDefinition(EmitContext context)
	{
		return this;
	}

	public IUnitReference GetUnit(EmitContext context)
	{
		return ModuleBuilder;
	}

	private static string DataToHex(ImmutableArray<byte> data)
	{
		return HashToHex(CryptographicHashProvider.ComputeSourceHash(data).AsSpan());
	}

	private string DataToHexViaXxHash128(ImmutableArray<byte> data)
	{
		Func<ImmutableArray<byte>, string> testOnly_DataToHexViaXxHash = ModuleBuilder.EmitOptions.TestOnly_DataToHexViaXxHash128;
		if (testOnly_DataToHexViaXxHash != null)
		{
			return testOnly_DataToHexViaXxHash(data);
		}
		Span<byte> span = stackalloc byte[16];
		XxHash128.Hash(data.AsSpan(), span, 0L);
		return HashToHex(span);
	}

	private static string ConstantsToHex(ImmutableArray<ConstantValue> constants)
	{
		return HashToHex(CryptographicHashProvider.ComputeSourceHash(constants).AsSpan());
	}

	public static string HashToHex(ReadOnlySpan<byte> hash)
	{
		char[] array = new char[hash.Length * 2];
		toHex(hash, array);
		return new string(array);
		static char hexchar(int x)
		{
			return (char)((x <= 9) ? (x + 48) : (x + 55));
		}
		static void toHex(ReadOnlySpan<byte> source, Span<char> destination)
		{
			int num = 0;
			ReadOnlySpan<byte> readOnlySpan = source;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				byte b = readOnlySpan[i];
				destination[num++] = hexchar(b >> 4);
				destination[num++] = hexchar(b & 0xF);
			}
		}
	}
}
