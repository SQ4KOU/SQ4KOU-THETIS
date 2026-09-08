using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf.Utils;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class ArrayRectangularPrimitiveRecord<T> : ArrayRecord where T : unmanaged
{
	private readonly int[] _lengths;

	private readonly IReadOnlyList<T> _values;

	private TypeName _typeName;

	public override ReadOnlySpan<int> Lengths => _lengths;

	public override SerializationRecordType RecordType => SerializationRecordType.BinaryArray;

	public override TypeName TypeName => _typeName ?? (_typeName = TypeNameHelpers.GetPrimitiveTypeName(TypeNameHelpers.GetPrimitiveType<T>()).MakeArrayTypeName(base.Rank));

	internal ArrayRectangularPrimitiveRecord(ArrayInfo arrayInfo, int[] lengths, IReadOnlyList<T> values)
		: base(arrayInfo)
	{
		_lengths = lengths;
		_values = values;
		base.ValuesToRead = 0L;
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetAllowedRecordType()
	{
		throw new InvalidOperationException();
	}

	private protected override void AddValue(object value)
	{
		throw new InvalidOperationException();
	}

	[RequiresDynamicCode("May call Array.CreateInstance().")]
	private protected override Array Deserialize(Type arrayType, bool allowNulls)
	{
		Array array = Array.CreateInstance(typeof(T), _lengths);
		int[] array2 = new int[_lengths.Length];
		nuint num = 0u;
		for (int i = 0; i < _values.Count; i++)
		{
			array.SetValue(_values[i], array2);
			num++;
			int num2;
			for (num2 = array2.Length - 1; num2 >= 0; num2--)
			{
				array2[num2]++;
				if (array2[num2] < Lengths[num2])
				{
					break;
				}
				array2[num2] = 0;
			}
			if (num2 < 0)
			{
				break;
			}
		}
		return array;
	}
}
