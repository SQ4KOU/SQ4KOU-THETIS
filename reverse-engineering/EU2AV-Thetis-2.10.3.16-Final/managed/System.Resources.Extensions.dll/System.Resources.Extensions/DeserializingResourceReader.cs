using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Resources.Extensions.BinaryFormat;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace System.Resources.Extensions;

public sealed class DeserializingResourceReader : IResourceReader, IEnumerable, IDisposable
{
	internal sealed class ResourceEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private const int ENUM_DONE = int.MinValue;

		private const int ENUM_NOT_STARTED = -1;

		private readonly DeserializingResourceReader _reader;

		private bool _currentIsValid;

		private int _currentName;

		private int _dataPosition;

		public object Key
		{
			get
			{
				if (_currentName == int.MinValue)
				{
					throw new InvalidOperationException(System.SR.InvalidOperation_EnumEnded);
				}
				if (!_currentIsValid)
				{
					throw new InvalidOperationException(System.SR.InvalidOperation_EnumNotStarted);
				}
				if (_reader._resCache == null)
				{
					throw new InvalidOperationException(System.SR.ResourceReaderIsClosed);
				}
				return _reader.AllocateStringForNameIndex(_currentName, out _dataPosition);
			}
		}

		public object Current => Entry;

		internal int DataPosition => _dataPosition;

		public DictionaryEntry Entry
		{
			get
			{
				if (_currentName == int.MinValue)
				{
					throw new InvalidOperationException(System.SR.InvalidOperation_EnumEnded);
				}
				if (!_currentIsValid)
				{
					throw new InvalidOperationException(System.SR.InvalidOperation_EnumNotStarted);
				}
				if (_reader._resCache == null)
				{
					throw new InvalidOperationException(System.SR.ResourceReaderIsClosed);
				}
				string key = _reader.AllocateStringForNameIndex(_currentName, out _dataPosition);
				object obj = null;
				lock (_reader._resCache)
				{
					if (_reader._resCache.TryGetValue(key, out var value))
					{
						obj = value.Value;
					}
				}
				if (obj == null)
				{
					obj = ((_dataPosition != -1) ? _reader.LoadObject(_dataPosition) : _reader.GetValueForNameIndex(_currentName));
				}
				return new DictionaryEntry(key, obj);
			}
		}

		public object Value
		{
			get
			{
				if (_currentName == int.MinValue)
				{
					throw new InvalidOperationException(System.SR.InvalidOperation_EnumEnded);
				}
				if (!_currentIsValid)
				{
					throw new InvalidOperationException(System.SR.InvalidOperation_EnumNotStarted);
				}
				if (_reader._resCache == null)
				{
					throw new InvalidOperationException(System.SR.ResourceReaderIsClosed);
				}
				return _reader.GetValueForNameIndex(_currentName);
			}
		}

		internal ResourceEnumerator(DeserializingResourceReader reader)
		{
			_currentName = -1;
			_reader = reader;
			_dataPosition = -2;
		}

		public bool MoveNext()
		{
			if (_currentName == _reader._numResources - 1 || _currentName == int.MinValue)
			{
				_currentIsValid = false;
				_currentName = int.MinValue;
				return false;
			}
			_currentIsValid = true;
			_currentName++;
			return true;
		}

		public void Reset()
		{
			if (_reader._resCache == null)
			{
				throw new InvalidOperationException(System.SR.ResourceReaderIsClosed);
			}
			_currentIsValid = false;
			_currentName = -1;
		}
	}

	internal sealed class UndoTruncatedTypeNameSerializationBinder : SerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
			Type result = null;
			if (typeName != null && assemblyName != null && !AreBracketsBalanced(typeName))
			{
				typeName = typeName + ", " + assemblyName;
				result = Type.GetType(typeName, throwOnError: false, ignoreCase: false);
			}
			return result;
		}

		private static bool AreBracketsBalanced(string typeName)
		{
			int num = typeName.IndexOf('[');
			if (num == -1)
			{
				return true;
			}
			int num2 = 1;
			for (int i = num + 1; i < typeName.Length; i++)
			{
				if (typeName[i] == '[')
				{
					num2++;
				}
				else if (typeName[i] == ']')
				{
					num2--;
					if (num2 < 0)
					{
						break;
					}
				}
			}
			return num2 == 0;
		}
	}

	private const int DefaultFileStreamBufferSize = 4096;

	private BinaryReader _store;

	internal Dictionary<string, ResourceLocator> _resCache;

	private long _nameSectionOffset;

	private long _dataSectionOffset;

	private int[] _nameHashes;

	private unsafe int* _nameHashesPtr;

	private int[] _namePositions;

	private unsafe int* _namePositionsPtr;

	private Type[] _typeTable;

	private int[] _typeNamePositions;

	private int _numResources;

	private UnmanagedMemoryStream _ums;

	private int _version;

	private static readonly bool s_useBinaryFormatter = AppContext.TryGetSwitch("System.Resources.Extensions.UseBinaryFormatter", out var isEnabled2) & isEnabled2;

	private bool _assumeBinaryFormatter;

	[FeatureSwitchDefinition("System.Resources.ResourceManager.AllowCustomResourceTypes")]
	internal static bool AllowCustomResourceTypes { get; } = !AppContext.TryGetSwitch("System.Resources.ResourceManager.AllowCustomResourceTypes", out var isEnabled) || isEnabled;

	public DeserializingResourceReader(string fileName)
	{
		_resCache = new Dictionary<string, ResourceLocator>(System.Resources.FastResourceComparer.Default);
		_store = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.RandomAccess), Encoding.UTF8);
		try
		{
			ReadResources();
		}
		catch
		{
			_store.Close();
			throw;
		}
	}

	public DeserializingResourceReader(Stream stream)
	{
		System.ExceptionPolyfills.ThrowIfNull(stream, "stream");
		if (!stream.CanRead)
		{
			throw new ArgumentException(System.SR.Argument_StreamNotReadable);
		}
		_resCache = new Dictionary<string, ResourceLocator>(System.Resources.FastResourceComparer.Default);
		_store = new BinaryReader(stream, Encoding.UTF8);
		_ums = stream as UnmanagedMemoryStream;
		ReadResources();
	}

	public void Close()
	{
		Dispose(disposing: true);
	}

	public void Dispose()
	{
		Close();
	}

	private unsafe void Dispose(bool disposing)
	{
		if (_store != null)
		{
			_resCache = null;
			if (disposing)
			{
				BinaryReader store = _store;
				_store = null;
				store?.Close();
			}
			_store = null;
			_namePositions = null;
			_nameHashes = null;
			_ums = null;
			_namePositionsPtr = null;
			_nameHashesPtr = null;
		}
	}

	private unsafe static int ReadUnalignedI4(int* p)
	{
		return BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(p, 4));
	}

	private void SkipString()
	{
		int num = _store.Read7BitEncodedInt();
		if (num < 0)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_NegativeStringLength);
		}
		_store.BaseStream.Seek(num, SeekOrigin.Current);
	}

	private unsafe int GetNameHash(int index)
	{
		if (_ums == null)
		{
			return _nameHashes[index];
		}
		return ReadUnalignedI4(_nameHashesPtr + index);
	}

	private unsafe int GetNamePosition(int index)
	{
		int num = ((_ums != null) ? ReadUnalignedI4(_namePositionsPtr + index) : _namePositions[index]);
		if (num < 0 || num > _dataSectionOffset - _nameSectionOffset)
		{
			throw new FormatException(System.SR.Format(System.SR.BadImageFormat_ResourcesNameInvalidOffset, num));
		}
		return num;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IDictionaryEnumerator GetEnumerator()
	{
		if (_resCache == null)
		{
			throw new InvalidOperationException(System.SR.ResourceReaderIsClosed);
		}
		return new ResourceEnumerator(this);
	}

	internal ResourceEnumerator GetEnumeratorInternal()
	{
		return new ResourceEnumerator(this);
	}

	internal int FindPosForResource(string name)
	{
		int num = System.Resources.FastResourceComparer.HashFunction(name);
		int num2 = 0;
		int i = _numResources - 1;
		int num3 = -1;
		bool flag = false;
		while (num2 <= i)
		{
			num3 = num2 + i >> 1;
			int nameHash = GetNameHash(num3);
			int num4 = ((nameHash != num) ? ((nameHash >= num) ? 1 : (-1)) : 0);
			if (num4 == 0)
			{
				flag = true;
				break;
			}
			if (num4 < 0)
			{
				num2 = num3 + 1;
			}
			else
			{
				i = num3 - 1;
			}
		}
		if (!flag)
		{
			return -1;
		}
		if (num2 != num3)
		{
			num2 = num3;
			while (num2 > 0 && GetNameHash(num2 - 1) == num)
			{
				num2--;
			}
		}
		if (i != num3)
		{
			for (i = num3; i < _numResources - 1 && GetNameHash(i + 1) == num; i++)
			{
			}
		}
		lock (this)
		{
			for (int j = num2; j <= i; j++)
			{
				_store.BaseStream.Seek(_nameSectionOffset + GetNamePosition(j), SeekOrigin.Begin);
				if (CompareStringEqualsName(name))
				{
					int num5 = _store.ReadInt32();
					if (num5 < 0 || num5 >= _store.BaseStream.Length - _dataSectionOffset)
					{
						throw new FormatException(System.SR.Format(System.SR.BadImageFormat_ResourcesDataInvalidOffset, num5));
					}
					return num5;
				}
			}
		}
		return -1;
	}

	private unsafe bool CompareStringEqualsName(string name)
	{
		int num = _store.Read7BitEncodedInt();
		if (num < 0)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_NegativeStringLength);
		}
		if (_ums != null)
		{
			byte* positionPointer = _ums.PositionPointer;
			_ums.Seek(num, SeekOrigin.Current);
			if (_ums.Position > _ums.Length)
			{
				throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesNameTooLong);
			}
			return System.Resources.FastResourceComparer.CompareOrdinal(positionPointer, num, name) == 0;
		}
		byte[] array = new byte[num];
		int num2 = num;
		while (num2 > 0)
		{
			int num3 = _store.Read(array, num - num2, num2);
			if (num3 == 0)
			{
				throw new BadImageFormatException(System.SR.BadImageFormat_ResourceNameCorrupted);
			}
			num2 -= num3;
		}
		return System.Resources.FastResourceComparer.CompareOrdinal(array, num / 2, name) == 0;
	}

	private unsafe string AllocateStringForNameIndex(int index, out int dataOffset)
	{
		long num = GetNamePosition(index);
		int num2;
		byte[] array2;
		lock (this)
		{
			_store.BaseStream.Seek(num + _nameSectionOffset, SeekOrigin.Begin);
			num2 = _store.Read7BitEncodedInt();
			if (num2 < 0)
			{
				throw new BadImageFormatException(System.SR.BadImageFormat_NegativeStringLength);
			}
			if (_ums != null)
			{
				if (_ums.Position > _ums.Length - num2)
				{
					throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourcesIndexTooLong, index));
				}
				string text = null;
				char* positionPointer = (char*)_ums.PositionPointer;
				if (BitConverter.IsLittleEndian)
				{
					text = new string(positionPointer, 0, num2 / 2);
				}
				else
				{
					char[] array = new char[num2 / 2];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = (char)BinaryPrimitives.ReverseEndianness((short)positionPointer[i]);
					}
					text = new string(array);
				}
				_ums.Position += num2;
				dataOffset = _store.ReadInt32();
				if (dataOffset < 0 || dataOffset >= _store.BaseStream.Length - _dataSectionOffset)
				{
					throw new FormatException(System.SR.Format(System.SR.BadImageFormat_ResourcesDataInvalidOffset, dataOffset));
				}
				return text;
			}
			array2 = new byte[num2];
			int num3 = num2;
			while (num3 > 0)
			{
				int num4 = _store.Read(array2, num2 - num3, num3);
				if (num4 == 0)
				{
					throw new EndOfStreamException(System.SR.Format(System.SR.BadImageFormat_ResourceNameCorrupted_NameIndex, index));
				}
				num3 -= num4;
			}
			dataOffset = _store.ReadInt32();
			if (dataOffset < 0 || dataOffset >= _store.BaseStream.Length - _dataSectionOffset)
			{
				throw new FormatException(System.SR.Format(System.SR.BadImageFormat_ResourcesDataInvalidOffset, dataOffset));
			}
		}
		return Encoding.Unicode.GetString(array2, 0, num2);
	}

	private object GetValueForNameIndex(int index)
	{
		long num = GetNamePosition(index);
		lock (this)
		{
			_store.BaseStream.Seek(num + _nameSectionOffset, SeekOrigin.Begin);
			SkipString();
			int num2 = _store.ReadInt32();
			if (num2 < 0 || num2 >= _store.BaseStream.Length - _dataSectionOffset)
			{
				throw new FormatException(System.SR.Format(System.SR.BadImageFormat_ResourcesDataInvalidOffset, num2));
			}
			if (_version == 1)
			{
				return LoadObjectV1(num2);
			}
			System.Resources.ResourceTypeCode typeCode;
			return LoadObjectV2(num2, out typeCode);
		}
	}

	internal string LoadString(int pos)
	{
		lock (this)
		{
			_store.BaseStream.Seek(_dataSectionOffset + pos, SeekOrigin.Begin);
			string result = null;
			int num = _store.Read7BitEncodedInt();
			if (_version == 1)
			{
				if (num == -1)
				{
					return null;
				}
				if (FindType(num) != typeof(string))
				{
					throw new InvalidOperationException(System.SR.Format(System.SR.InvalidOperation_ResourceNotString_Type, FindType(num).FullName));
				}
				result = _store.ReadString();
			}
			else
			{
				System.Resources.ResourceTypeCode resourceTypeCode = (System.Resources.ResourceTypeCode)num;
				if (resourceTypeCode != System.Resources.ResourceTypeCode.String && resourceTypeCode != System.Resources.ResourceTypeCode.Null)
				{
					throw new InvalidOperationException(System.SR.Format(p1: (resourceTypeCode >= System.Resources.ResourceTypeCode.StartOfUserTypes) ? FindType((int)(resourceTypeCode - 64)).FullName : resourceTypeCode.ToString(), resourceFormat: System.SR.InvalidOperation_ResourceNotString_Type));
				}
				if (resourceTypeCode == System.Resources.ResourceTypeCode.String)
				{
					result = _store.ReadString();
				}
			}
			return result;
		}
	}

	internal object LoadObject(int pos)
	{
		lock (this)
		{
			System.Resources.ResourceTypeCode typeCode;
			return (_version == 1) ? LoadObjectV1(pos) : LoadObjectV2(pos, out typeCode);
		}
	}

	internal object LoadObject(int pos, out System.Resources.ResourceTypeCode typeCode)
	{
		lock (this)
		{
			if (_version == 1)
			{
				object obj = LoadObjectV1(pos);
				typeCode = ((obj is string) ? System.Resources.ResourceTypeCode.String : System.Resources.ResourceTypeCode.StartOfUserTypes);
				return obj;
			}
			return LoadObjectV2(pos, out typeCode);
		}
	}

	private object LoadObjectV1(int pos)
	{
		try
		{
			return _LoadObjectV1(pos);
		}
		catch (EndOfStreamException inner)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_TypeMismatch, inner);
		}
		catch (ArgumentOutOfRangeException inner2)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_TypeMismatch, inner2);
		}
	}

	private object _LoadObjectV1(int pos)
	{
		_store.BaseStream.Seek(_dataSectionOffset + pos, SeekOrigin.Begin);
		int num = _store.Read7BitEncodedInt();
		if (num == -1)
		{
			return null;
		}
		Type type = FindType(num);
		if (type == typeof(string))
		{
			return _store.ReadString();
		}
		if (type == typeof(int))
		{
			return _store.ReadInt32();
		}
		if (type == typeof(byte))
		{
			return _store.ReadByte();
		}
		if (type == typeof(sbyte))
		{
			return _store.ReadSByte();
		}
		if (type == typeof(short))
		{
			return _store.ReadInt16();
		}
		if (type == typeof(long))
		{
			return _store.ReadInt64();
		}
		if (type == typeof(ushort))
		{
			return _store.ReadUInt16();
		}
		if (type == typeof(uint))
		{
			return _store.ReadUInt32();
		}
		if (type == typeof(ulong))
		{
			return _store.ReadUInt64();
		}
		if (type == typeof(float))
		{
			return _store.ReadSingle();
		}
		if (type == typeof(double))
		{
			return _store.ReadDouble();
		}
		if (type == typeof(DateTime))
		{
			return new DateTime(_store.ReadInt64());
		}
		if (type == typeof(TimeSpan))
		{
			return new TimeSpan(_store.ReadInt64());
		}
		if (type == typeof(decimal))
		{
			int[] array = new int[4];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _store.ReadInt32();
			}
			return new decimal(array);
		}
		return DeserializeObject(num);
	}

	private object LoadObjectV2(int pos, out System.Resources.ResourceTypeCode typeCode)
	{
		try
		{
			return _LoadObjectV2(pos, out typeCode);
		}
		catch (EndOfStreamException inner)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_TypeMismatch, inner);
		}
		catch (ArgumentOutOfRangeException inner2)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_TypeMismatch, inner2);
		}
	}

	private unsafe object _LoadObjectV2(int pos, out System.Resources.ResourceTypeCode typeCode)
	{
		_store.BaseStream.Seek(_dataSectionOffset + pos, SeekOrigin.Begin);
		typeCode = (System.Resources.ResourceTypeCode)_store.Read7BitEncodedInt();
		switch (typeCode)
		{
		case System.Resources.ResourceTypeCode.Null:
			return null;
		case System.Resources.ResourceTypeCode.String:
			return _store.ReadString();
		case System.Resources.ResourceTypeCode.Boolean:
			return _store.ReadBoolean();
		case System.Resources.ResourceTypeCode.Char:
			return (char)_store.ReadUInt16();
		case System.Resources.ResourceTypeCode.Byte:
			return _store.ReadByte();
		case System.Resources.ResourceTypeCode.SByte:
			return _store.ReadSByte();
		case System.Resources.ResourceTypeCode.Int16:
			return _store.ReadInt16();
		case System.Resources.ResourceTypeCode.UInt16:
			return _store.ReadUInt16();
		case System.Resources.ResourceTypeCode.Int32:
			return _store.ReadInt32();
		case System.Resources.ResourceTypeCode.UInt32:
			return _store.ReadUInt32();
		case System.Resources.ResourceTypeCode.Int64:
			return _store.ReadInt64();
		case System.Resources.ResourceTypeCode.UInt64:
			return _store.ReadUInt64();
		case System.Resources.ResourceTypeCode.Single:
			return _store.ReadSingle();
		case System.Resources.ResourceTypeCode.Double:
			return _store.ReadDouble();
		case System.Resources.ResourceTypeCode.Decimal:
			return _store.ReadDecimal();
		case System.Resources.ResourceTypeCode.DateTime:
			return DateTime.FromBinary(_store.ReadInt64());
		case System.Resources.ResourceTypeCode.TimeSpan:
			return new TimeSpan(_store.ReadInt64());
		case System.Resources.ResourceTypeCode.ByteArray:
		{
			int num2 = _store.ReadInt32();
			if (num2 < 0)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num2));
			}
			if (_ums == null)
			{
				if (num2 > _store.BaseStream.Length)
				{
					throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num2));
				}
				return _store.ReadBytes(num2);
			}
			if (num2 > _ums.Length - _ums.Position)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num2));
			}
			byte[] array = new byte[num2];
			_ums.Read(array, 0, num2);
			return array;
		}
		case System.Resources.ResourceTypeCode.Stream:
		{
			int num = _store.ReadInt32();
			if (num < 0)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num));
			}
			if (_ums == null)
			{
				return new System.IO.PinnedBufferMemoryStream(_store.ReadBytes(num));
			}
			if (num > _ums.Length - _ums.Position)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num));
			}
			return new UnmanagedMemoryStream(_ums.PositionPointer, num, num, FileAccess.Read);
		}
		default:
		{
			if (typeCode < System.Resources.ResourceTypeCode.StartOfUserTypes)
			{
				throw new BadImageFormatException(System.SR.BadImageFormat_TypeMismatch);
			}
			int typeIndex = (int)(typeCode - 64);
			return DeserializeObject(typeIndex);
		}
		}
	}

	[MemberNotNull("_typeTable")]
	[MemberNotNull("_typeNamePositions")]
	private void ReadResources()
	{
		try
		{
			_ReadResources();
		}
		catch (EndOfStreamException inner)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted, inner);
		}
		catch (IndexOutOfRangeException inner2)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted, inner2);
		}
	}

	[MemberNotNull("_typeTable")]
	[MemberNotNull("_typeNamePositions")]
	private unsafe void _ReadResources()
	{
		if (_store.ReadInt32() != ResourceManager.MagicNumber)
		{
			throw new ArgumentException(System.SR.Resources_StreamNotValid);
		}
		int num = _store.ReadInt32();
		int num2 = _store.ReadInt32();
		if (num2 < 0 || num < 0)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
		}
		if (num > 1)
		{
			_store.BaseStream.Seek(num2, SeekOrigin.Current);
		}
		else
		{
			string text = _store.ReadString();
			if (!ValidateReaderType(text))
			{
				throw new NotSupportedException(System.SR.Format(System.SR.NotSupported_WrongResourceReader_Type, text));
			}
			SkipString();
		}
		int num3 = _store.ReadInt32();
		if (num3 != 2 && num3 != 1)
		{
			throw new ArgumentException(System.SR.Format(System.SR.Arg_ResourceFileUnsupportedVersion, 2, num3));
		}
		_version = num3;
		_numResources = _store.ReadInt32();
		if (_numResources < 0)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
		}
		int num4 = _store.ReadInt32();
		if (num4 < 0)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
		}
		_typeTable = new Type[num4];
		_typeNamePositions = new int[num4];
		for (int i = 0; i < num4; i++)
		{
			_typeNamePositions[i] = (int)_store.BaseStream.Position;
			SkipString();
		}
		int num5 = (int)_store.BaseStream.Position & 7;
		if (num5 != 0)
		{
			for (int j = 0; j < 8 - num5; j++)
			{
				_store.ReadByte();
			}
		}
		if (_ums == null)
		{
			_nameHashes = new int[_numResources];
			for (int k = 0; k < _numResources; k++)
			{
				_nameHashes[k] = _store.ReadInt32();
			}
		}
		else
		{
			int num6 = 4 * _numResources;
			if (num6 < 0)
			{
				throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
			}
			_nameHashesPtr = (int*)_ums.PositionPointer;
			_ums.Seek(num6, SeekOrigin.Current);
			_ = _ums.PositionPointer;
		}
		if (_ums == null)
		{
			_namePositions = new int[_numResources];
			for (int l = 0; l < _numResources; l++)
			{
				int num7 = _store.ReadInt32();
				if (num7 < 0)
				{
					throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
				}
				_namePositions[l] = num7;
			}
		}
		else
		{
			int num8 = 4 * _numResources;
			if (num8 < 0)
			{
				throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
			}
			_namePositionsPtr = (int*)_ums.PositionPointer;
			_ums.Seek(num8, SeekOrigin.Current);
			_ = _ums.PositionPointer;
		}
		_dataSectionOffset = _store.ReadInt32();
		if (_dataSectionOffset < 0)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
		}
		_nameSectionOffset = _store.BaseStream.Position;
		if (_dataSectionOffset < _nameSectionOffset)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_ResourcesHeaderCorrupted);
		}
	}

	private Type FindType(int typeIndex)
	{
		if (!AllowCustomResourceTypes)
		{
			throw new NotSupportedException(System.SR.ResourceManager_ReflectionNotAllowed);
		}
		if (typeIndex < 0 || typeIndex >= _typeTable.Length)
		{
			throw new BadImageFormatException(System.SR.BadImageFormat_InvalidType);
		}
		return _typeTable[typeIndex] ?? UseReflectionToGetType(typeIndex);
	}

	[RequiresUnreferencedCode("The CustomResourceTypesSupport feature switch has been enabled for this app which is being trimmed. Custom readers as well as custom objects on the resources file are not observable by the trimmer and so required assemblies, types and members may be removed.")]
	private Type UseReflectionToGetType(int typeIndex)
	{
		long position = _store.BaseStream.Position;
		try
		{
			_store.BaseStream.Position = _typeNamePositions[typeIndex];
			string typeName = _store.ReadString();
			_typeTable[typeIndex] = Type.GetType(typeName, throwOnError: true);
			return _typeTable[typeIndex];
		}
		finally
		{
			_store.BaseStream.Position = position;
		}
	}

	private string TypeNameFromTypeCode(System.Resources.ResourceTypeCode typeCode)
	{
		if (typeCode < System.Resources.ResourceTypeCode.StartOfUserTypes)
		{
			return "ResourceTypeCode." + typeCode;
		}
		int num = (int)(typeCode - 64);
		long position = _store.BaseStream.Position;
		try
		{
			_store.BaseStream.Position = _typeNamePositions[num];
			return _store.ReadString();
		}
		finally
		{
			_store.BaseStream.Position = position;
		}
	}

	private bool ValidateReaderType(string readerType)
	{
		if (TypeNameComparer.Instance.Equals(readerType, "System.Resources.Extensions.DeserializingResourceReader, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51"))
		{
			return true;
		}
		if (TypeNameComparer.Instance.Equals(readerType, "System.Resources.ResourceReader, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))
		{
			_assumeBinaryFormatter = true;
			return true;
		}
		return false;
	}

	private object ReadBinaryFormattedObject()
	{
		if (!s_useBinaryFormatter)
		{
			return new BinaryFormattedObject(_store.BaseStream).Deserialize();
		}
		return new BinaryFormatter
		{
			Binder = new UndoTruncatedTypeNameSerializationBinder()
		}.Deserialize(_store.BaseStream);
	}

	private unsafe object DeserializeObject(int typeIndex)
	{
		Type type = FindType(typeIndex);
		if (_assumeBinaryFormatter)
		{
			return ReadBinaryFormattedObject();
		}
		object obj;
		switch ((SerializationFormat)_store.Read7BitEncodedInt())
		{
		case SerializationFormat.BinaryFormatter:
		{
			int num3 = _store.Read7BitEncodedInt();
			if (num3 < 0)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num3));
			}
			long position = _store.BaseStream.Position;
			obj = ReadBinaryFormattedObject();
			if (type == typeof(UnknownType))
			{
				type = obj.GetType();
			}
			if (_store.BaseStream.Position - position != num3)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num3));
			}
			break;
		}
		case SerializationFormat.TypeConverterByteArray:
		{
			int num2 = _store.Read7BitEncodedInt();
			if (num2 < 0)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num2));
			}
			byte[] value = _store.ReadBytes(num2);
			obj = (TypeDescriptor.GetConverter(type) ?? throw new TypeLoadException(System.SR.Format(System.SR.TypeLoadException_CannotLoadConverter, type))).ConvertFrom(value);
			break;
		}
		case SerializationFormat.TypeConverterString:
		{
			string text = _store.ReadString();
			obj = (TypeDescriptor.GetConverter(type) ?? throw new TypeLoadException(System.SR.Format(System.SR.TypeLoadException_CannotLoadConverter, type))).ConvertFromInvariantString(text);
			break;
		}
		case SerializationFormat.ActivatorStream:
		{
			int num = _store.Read7BitEncodedInt();
			if (num < 0)
			{
				throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResourceDataLengthInvalid, num));
			}
			Stream stream = ((!(_store.BaseStream is UnmanagedMemoryStream unmanagedMemoryStream)) ? ((Stream)new MemoryStream(_store.ReadBytes(num), writable: false)) : ((Stream)new UnmanagedMemoryStream(unmanagedMemoryStream.PositionPointer, num, num, FileAccess.Read)));
			obj = Activator.CreateInstance(type, stream);
			break;
		}
		default:
			throw new BadImageFormatException(System.SR.BadImageFormat_TypeMismatch);
		}
		if (obj.GetType() != type)
		{
			throw new BadImageFormatException(System.SR.Format(System.SR.BadImageFormat_ResType_SerBlobMismatch, type.FullName, obj.GetType().FullName));
		}
		return obj;
	}
}
