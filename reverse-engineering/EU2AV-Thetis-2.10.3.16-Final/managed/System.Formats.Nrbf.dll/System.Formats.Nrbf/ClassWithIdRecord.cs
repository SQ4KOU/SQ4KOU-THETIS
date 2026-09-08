using System.Formats.Nrbf.Utils;
using System.IO;
using System.Runtime.Serialization;

namespace System.Formats.Nrbf;

internal sealed class ClassWithIdRecord : ClassRecord
{
	public override SerializationRecordType RecordType => SerializationRecordType.ClassWithId;

	public override SerializationRecordId Id { get; }

	internal ClassRecord MetadataClass { get; }

	private ClassWithIdRecord(SerializationRecordId id, ClassRecord metadataClass)
		: base(metadataClass.ClassInfo, metadataClass.MemberTypeInfo)
	{
		Id = id;
		MetadataClass = metadataClass;
	}

	internal static SerializationRecord Decode(BinaryReader reader, RecordMap recordMap)
	{
		SerializationRecordId id = SerializationRecordId.Decode(reader);
		SerializationRecordId recordId = SerializationRecordId.Decode(reader);
		SerializationRecord record = recordMap.GetRecord(recordId);
		if (record is ClassRecord metadataClass)
		{
			return new ClassWithIdRecord(id, metadataClass);
		}
		if (record is PrimitiveTypeRecord { Id: var id2 } primitiveTypeRecord && !id2.Equals(default(SerializationRecordId)) && !(record is BinaryObjectStringRecord))
		{
			if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<bool>))
			{
				if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<byte>))
				{
					if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<sbyte>))
					{
						if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<char>))
						{
							if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<short>))
							{
								if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<ushort>))
								{
									if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<int>))
									{
										if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<uint>))
										{
											if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<long>))
											{
												if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<ulong>))
												{
													if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<float>))
													{
														if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<double>))
														{
															if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<IntPtr>))
															{
																if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<UIntPtr>))
																{
																	if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<TimeSpan>))
																	{
																		if (!(primitiveTypeRecord is MemberPrimitiveTypedRecord<DateTime>))
																		{
																			if (primitiveTypeRecord is MemberPrimitiveTypedRecord<decimal>)
																			{
																				return SystemClassWithMembersAndTypesRecord.DecodeDecimal(reader, id);
																			}
																			throw new InvalidOperationException();
																		}
																		return SystemClassWithMembersAndTypesRecord.DecodeDateTime(reader, id);
																	}
																	return Create<TimeSpan>(new TimeSpan(reader.ReadInt64()));
																}
																return Create<UIntPtr>(new UIntPtr(reader.ReadUInt64()));
															}
															return Create<IntPtr>(new IntPtr(reader.ReadInt64()));
														}
														return Create<double>(reader.ReadDouble());
													}
													return Create<float>(reader.ReadSingle());
												}
												return Create<ulong>(reader.ReadUInt64());
											}
											return Create<long>(reader.ReadInt64());
										}
										return Create<uint>(reader.ReadUInt32());
									}
									return Create<int>(reader.ReadInt32());
								}
								return Create<ushort>(reader.ReadUInt16());
							}
							return Create<short>(reader.ReadInt16());
						}
						return Create<char>(reader.ParseChar());
					}
					return Create<sbyte>(reader.ReadSByte());
				}
				return Create<byte>(reader.ReadByte());
			}
			return Create<bool>(reader.ReadBoolean());
		}
		throw new SerializationException(System.SR.Serialization_InvalidReference);
		SerializationRecord Create<T>(T value) where T : unmanaged
		{
			return new MemberPrimitiveTypedRecord<T>(value, id);
		}
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetNextAllowedRecordType()
	{
		return MetadataClass.MemberTypeInfo.GetNextAllowedRecordType(base.MemberValues.Count);
	}
}
