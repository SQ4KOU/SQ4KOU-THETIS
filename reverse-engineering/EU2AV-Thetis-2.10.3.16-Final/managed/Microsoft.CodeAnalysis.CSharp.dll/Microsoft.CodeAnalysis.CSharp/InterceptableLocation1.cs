using System;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class InterceptableLocation1 : InterceptableLocation
{
	internal const int ContentHashLength = 16;

	private static readonly UTF8Encoding s_encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private readonly ImmutableArray<byte> _checksum;

	private readonly string _path;

	private readonly SourceReferenceResolver? _resolver;

	private readonly int _position;

	private readonly int _lineNumberOneIndexed;

	private readonly int _characterNumberOneIndexed;

	private string? _lazyData;

	public override int Version => 1;

	public override string Data
	{
		get
		{
			if (_lazyData == null)
			{
				_lazyData = makeData();
			}
			return _lazyData;
			string makeData()
			{
				PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
				instance.WriteBytes(_checksum, 0, 16);
				instance.WriteInt32(_position);
				string fileName = Path.GetFileName(_path);
				instance.WriteUTF8(fileName);
				byte[] inArray = instance.ToArray();
				instance.Free();
				return Convert.ToBase64String(inArray);
			}
		}
	}

	internal InterceptableLocation1(ImmutableArray<byte> checksum, string path, SourceReferenceResolver? resolver, int position, int lineNumberOneIndexed, int characterNumberOneIndexed)
	{
		_checksum = checksum;
		_path = path;
		_resolver = resolver;
		_position = position;
		_lineNumberOneIndexed = lineNumberOneIndexed;
		_characterNumberOneIndexed = characterNumberOneIndexed;
	}

	public override string GetDisplayLocation()
	{
		string arg = _resolver?.NormalizePath(_path, null) ?? _path;
		return $"{arg}({_lineNumberOneIndexed},{_characterNumberOneIndexed})";
	}

	public override string ToString()
	{
		return GetDisplayLocation();
	}

	internal static (ReadOnlyMemory<byte> checksum, int position, string displayFileName)? Decode(string? data)
	{
		if (data == null)
		{
			return null;
		}
		byte[] array;
		try
		{
			array = Convert.FromBase64String(data);
		}
		catch (FormatException)
		{
			return null;
		}
		if (array.Length < 20)
		{
			return null;
		}
		Memory<byte> memory = System.MemoryExtensions.AsMemory(array, 0, 16);
		int item = BinaryPrimitives.ReadInt32LittleEndian(System.MemoryExtensions.AsSpan(array, 16));
		string item2;
		try
		{
			item2 = s_encoding.GetString(array, 20, array.Length - 20);
		}
		catch (ArgumentException)
		{
			return null;
		}
		return (memory, item, item2);
	}

	public override bool Equals(object? obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is InterceptableLocation other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(BinaryPrimitives.ReadInt32LittleEndian(_checksum.AsSpan()), _position);
	}

	public override bool Equals(InterceptableLocation? obj)
	{
		if (obj is InterceptableLocation1 interceptableLocation && _checksum.SequenceEqual(interceptableLocation._checksum) && _path == interceptableLocation._path && _position == interceptableLocation._position && _lineNumberOneIndexed == interceptableLocation._lineNumberOneIndexed)
		{
			return _characterNumberOneIndexed == interceptableLocation._characterNumberOneIndexed;
		}
		return false;
	}
}
