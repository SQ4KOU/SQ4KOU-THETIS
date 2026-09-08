using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf;
using System.Linq;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal sealed class ArrayRecordDeserializer : ObjectRecordDeserializer
{
	private readonly ArrayRecord _arrayRecord;

	private readonly Type _elementType;

	private readonly Array _arrayOfClassRecords;

	private readonly Array _arrayOfT;

	private readonly int[] _lengths;

	private readonly int[] _indices;

	private bool _hasFixups;

	private bool _canIterate;

	[RequiresUnreferencedCode("Calls System.Windows.Forms.BinaryFormat.BinaryFormattedObject.TypeResolver.GetType(TypeName)")]
	internal ArrayRecordDeserializer(ArrayRecord arrayRecord, IDeserializer deserializer)
		: base(arrayRecord, deserializer)
	{
		_arrayRecord = arrayRecord;
		_elementType = deserializer.TypeResolver.GetType(arrayRecord.TypeName.GetElementType());
		_arrayOfClassRecords = arrayRecord.GetArray((arrayRecord.Rank != 1) ? _elementType.MakeArrayType(arrayRecord.Rank) : _elementType.MakeArrayType());
		Type type = _arrayOfClassRecords.GetType();
		while (type.IsArray)
		{
			type = type.GetElementType();
		}
		_lengths = arrayRecord.Lengths.ToArray();
		base.Object = (_arrayOfT = Array.CreateInstance(_elementType, _lengths));
		_indices = new int[_lengths.Length];
		_canIterate = _arrayOfT.Length > 0;
	}

	internal override SerializationRecordId Continue()
	{
		int[] indices = _indices;
		int[] lengths = _lengths;
		while (_canIterate)
		{
			var (obj, serializationRecordId) = UnwrapMemberValue(_arrayOfClassRecords.GetValue(indices));
			if (ObjectRecordDeserializer.s_missingValueSentinel == obj)
			{
				return serializationRecordId;
			}
			if (obj != null && DoesValueNeedUpdated(obj, serializationRecordId))
			{
				_hasFixups = true;
				base.Deserializer.PendValueUpdater(new ArrayUpdater(_arrayRecord.Id, serializationRecordId, indices.ToArray()));
			}
			_arrayOfT.SetValue(obj, indices);
			int num;
			for (num = indices.Length - 1; num >= 0; num--)
			{
				indices[num]++;
				if (indices[num] < lengths[num])
				{
					break;
				}
				indices[num] = 0;
			}
			if (num < 0)
			{
				_canIterate = false;
			}
		}
		if (!_hasFixups)
		{
			base.Deserializer.CompleteObject(_arrayRecord.Id);
		}
		return default(SerializationRecordId);
	}

	internal static Array GetArraySinglePrimitive(SerializationRecord record)
	{
		if (!(record is SZArrayRecord<bool> sZArrayRecord))
		{
			if (!(record is SZArrayRecord<byte> sZArrayRecord2))
			{
				if (!(record is SZArrayRecord<sbyte> sZArrayRecord3))
				{
					if (!(record is SZArrayRecord<char> sZArrayRecord4))
					{
						if (!(record is SZArrayRecord<short> sZArrayRecord5))
						{
							if (!(record is SZArrayRecord<ushort> sZArrayRecord6))
							{
								if (!(record is SZArrayRecord<int> sZArrayRecord7))
								{
									if (!(record is SZArrayRecord<uint> sZArrayRecord8))
									{
										if (!(record is SZArrayRecord<long> sZArrayRecord9))
										{
											if (!(record is SZArrayRecord<ulong> sZArrayRecord10))
											{
												if (!(record is SZArrayRecord<float> sZArrayRecord11))
												{
													if (!(record is SZArrayRecord<double> sZArrayRecord12))
													{
														if (!(record is SZArrayRecord<decimal> sZArrayRecord13))
														{
															if (!(record is SZArrayRecord<DateTime> sZArrayRecord14))
															{
																if (record is SZArrayRecord<TimeSpan> sZArrayRecord15)
																{
																	return sZArrayRecord15.GetArray();
																}
																throw new NotSupportedException();
															}
															return sZArrayRecord14.GetArray();
														}
														return sZArrayRecord13.GetArray();
													}
													return sZArrayRecord12.GetArray();
												}
												return sZArrayRecord11.GetArray();
											}
											return sZArrayRecord10.GetArray();
										}
										return sZArrayRecord9.GetArray();
									}
									return sZArrayRecord8.GetArray();
								}
								return sZArrayRecord7.GetArray();
							}
							return sZArrayRecord6.GetArray();
						}
						return sZArrayRecord5.GetArray();
					}
					return sZArrayRecord4.GetArray();
				}
				return sZArrayRecord3.GetArray();
			}
			return sZArrayRecord2.GetArray();
		}
		return sZArrayRecord.GetArray();
	}

	[RequiresUnreferencedCode("Calls System.Windows.Forms.BinaryFormat.BinaryFormattedObject.TypeResolver.GetType(TypeName)")]
	internal static Array GetRectangularArrayOfPrimitives(ArrayRecord arrayRecord, BinaryFormattedObject.ITypeResolver typeResolver)
	{
		if (arrayRecord.Rank <= 1 || arrayRecord.TypeName.GetElementType().IsArray)
		{
			return null;
		}
		Type type = typeResolver.GetType(arrayRecord.TypeName.GetElementType());
		Type type2 = type;
		while (type2.IsArray)
		{
			type2 = type2.GetElementType();
		}
		if (!HasBuiltInSupport(type2))
		{
			return null;
		}
		Type expectedArrayType = type.MakeArrayType(arrayRecord.Rank);
		return arrayRecord.GetArray(expectedArrayType);
		static bool HasBuiltInSupport(Type elementType)
		{
			if (!(elementType == typeof(string)) && !(elementType == typeof(bool)) && !(elementType == typeof(byte)) && !(elementType == typeof(sbyte)) && !(elementType == typeof(char)) && !(elementType == typeof(short)) && !(elementType == typeof(ushort)) && !(elementType == typeof(int)) && !(elementType == typeof(uint)) && !(elementType == typeof(long)) && !(elementType == typeof(ulong)) && !(elementType == typeof(float)) && !(elementType == typeof(double)) && !(elementType == typeof(decimal)) && !(elementType == typeof(DateTime)))
			{
				return elementType == typeof(TimeSpan);
			}
			return true;
		}
	}
}
